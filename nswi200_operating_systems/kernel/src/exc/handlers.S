// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <abi.h>
#include <drivers/cp0.h>
#include <exc/context.h>

.data

/*
 * Pointer to the top of the kernel stack of the current thread.
 *
 * The value is only read by user-space threads entering the kernel to
 * service an exception, because they need to switch from a user space
 * stack to their (original) kernel stack.
 *
 * However, the value is written by all threads in two places. The first
 * is the cpu_switch_context function, which stores the stack top of the
 * next thread here. This does not affect kernel-only threads. The other
 * place is near the end of the exception handler, where we need to make
 * sure that the value for the current thread remains valid.
 *
 * This is not very nice, but we don't need to know the layout of the
 * thread_t structure and we can do it transparently. Alternatively, the
 * stack top would have to be supplied explicitly before each voluntary
 * context switch.
 */
.align 4
.globl kernel_stack_top
kernel_stack_top:
    .word 0


/*
 * Exception handler template.
 *
 * When the CPU reaches this code, bit 1 (EXL) of the CP0 Status register
 * will be set, the CP0 Cause register (#13) will indicate the cause of the
 * exception, and the CP0 EPC register (#14) will contain the value of the
 * PC register at which execution should resume (unless the exception
 * occurred in a branch delay slot).
 *
 * Because EXL=1, the CPU is in kernel mode with interrupts disabled, and
 * because the handler code is in direct-mapped memory (KSEG0), it should
 * not trigger TLB-related exceptions -- unless it touches a TLB-mapped
 * address (KUSEG and others).
 *
 * The handler needs to save the context of the interrupted thread. The
 * simplest approach is to save the context on the stack of the interrupted
 * thread, but we need to take into account the location of the stack in
 * the virtual address space.
 *
 * If the stack is in direct-mapped memory (KSEG0), accessing it will not
 * trigger TLB exceptions. On the other hand, if we run out of stack space,
 * we will not be able to detect it (before we overwrite something).
 *
 * If the stack is in TLB-mapped memory (KUSEG and others), we will be able
 * to detect running out of stack space, but we will also need to take into
 * account that accessing the stack can trigger TLB exceptions that need
 * to be handled.
 *
 * This handler assumes that kernel-mode threads use direct-mapped stack,
 * which means that it does not need to care about causing TLB exceptions
 * for kernel-mode threads. It also assumes that even user-mode threads
 * using a TLB-mapped stack keep their initial direct-mapped stack which
 * they used before jumping to user-mode. For these threads, the handler
 * simply switches to the direct-mapped stack when handling exceptions,
 * which means that it does not have to care about TLB exceptions either.
 *
 * The handler code first switches the CPU from exception level to plain
 * kernel mode with disabled interrupts. We keep interrupts disabled for
 * simplicity, but if we wanted to enable them, an active exception level
 * would keep blocking them.
 *
 * Then we store the general CPU registers, the special multiply/divide
 * registers, and selected CP0 registers on the stack and jump into C
 * code implementing the actual exception handling. After handling
 * the exception, we repeat similar steps in reverse order and resume
 * execution of the interrupted thread.
 */
.macro template_exception_handler handler_c_code
.set push
.set noreorder

    /*
     * We switch from exception level to plain kernel mode by clearing
     * bit 1 (EXL) and bits 4:3 (KSU) of the CP0 Status register, while
     * simultaneously disabling interrupts by clearing bit 0 (IE).
     *
     * We keep original status register in $k0.
     */
    la $k1, ~(CP0_STATUS_KSU_MASK | CP0_STATUS_EXL_BIT | CP0_STATUS_IE_BIT)
    mfc0 $k0, $REG_CP0_STATUS
    and $k1, $k0, $k1
    mtc0 $k1, $REG_CP0_STATUS

    /*
     * Check if we are coming from kernel space or user space. If we are
     * coming from user space, we need to switch to the thread's kernel
     * stack so that we can store the full exception context there without
     * having to worry about triggering TLB exceptions.
     *
     * We check the KSU bits of the original status register and if they
     * don't equal KSU value for user mode, we are coming from kernel
     * space and can use the current stack ($sp) to store context.
     */
    andi $k1, $k0, CP0_STATUS_KSU_MASK
    subu $k1, CP0_STATUS_KSU_USER
    bnez $k1, 1f
    move $k1, $sp

    /*
     * We are coming from user space. Switch to kernel stack.
     */
    la $k1, kernel_stack_top
    lw $k1, 0($k1)

1:
    /*
     * At this point, the target stack for the context is in $k1. Next we
     * allocate space for the exception context structure on the stack and
     * save the CPU registers, the special multiply/divide registers, and
     * selected CP0 registers into it.
     */
    subu $k1, EXC_CONTEXT_SIZE
    EXC_CONTEXT_SAVE_GENERAL_REGISTERS $k1

    /*
     * From now on, we can clobber the general registers (except $s0-$s7).
     */
    mfhi $t0
    mflo $t1
    sw $t0, EXC_CONTEXT_MD_HI_OFFSET($k1)
    sw $t1, EXC_CONTEXT_MD_LO_OFFSET($k1)

    /*
     * Save the original status register first and then
     * the rest of selected CP0 registers.
     *
     * We save EntryHi for debugging purposes, because it contains useful
     * information about the VPN2/ASID pair the CPU was looking for in the
     * TLB when a TLB exception occurred. However, EntryHi should not be
     * restored on return, see below for details.
     */
    sw $k0, EXC_CONTEXT_CP0_STATUS_OFFSET($k1)

    mfc0 $t0, $REG_CP0_CAUSE
    sw $t0, EXC_CONTEXT_CP0_CAUSE_OFFSET($k1)

    mfc0 $t0, $REG_CP0_EPC
    sw $t0, EXC_CONTEXT_CP0_EPC_OFFSET($k1)

    mfc0 $t0, $REG_CP0_BADVADDR
    sw $t0, EXC_CONTEXT_CP0_BADVADDR_OFFSET($k1)

    mfc0 $t0, $REG_CP0_ENTRYHI
    sw $t0, EXC_CONTEXT_CP0_ENTRYHI_OFFSET($k1)

    /*
     * Once the context is stored on the stack, we could re-enable interrupts
     * to make the handler re-entrant. For simplicity, we keep interrupts
     * disabled (re-enabling them would also require a bit more complex
     * epilogue in this handler).
     *
     * We jump to C code to actually handle the exception.
     *
     * Because the C function expects a pointer to the exception
     * context as a parameter, we pass it in register $a0.
     *
     * Note that the MIPS ABI requires the caller to reserve stack
     * space (to be used by the callee) for all parameters passed in
     * registers. For simplicity, here we reserve a fixed amount that
     * is sufficient for all situations (and clean up the stack after
     * the function returns).
     */
    move $a0, $k1
    jal \handler_c_code
    subu $sp, $k1, ABI_STACK_FRAME_SIZE

    addiu $k1, $sp, ABI_STACK_FRAME_SIZE

    /*
     * The value of the kernel_stack_top variable might have been changed
     * if this thread was rescheduled while handling the exception. We must
     * restore it to the same value used to switch to the kernel stack on the
     * entry to the exception handler, otherwise it could steadily move past
     * the stack boundary and we would start overwriting memory!
     */
    la $t0, kernel_stack_top
    addiu $t1, $k1, EXC_CONTEXT_SIZE
    sw $t1, 0($t0)

    /*
     * After the C code returns, we restore the registers from the context
     * structure on the stack and resume execution of the interrupted code.
     *
     * Note that even though restoring the value of the Status register
     * could re-enable interrupts, recall that we are restoring a value
     * from the entry to this handler, at which point bit 1 (EXL) was
     * set, blocking interrupts from occurring.
     */
    lw $k0, EXC_CONTEXT_CP0_STATUS_OFFSET($k1)

    lw $t0, EXC_CONTEXT_CP0_CAUSE_OFFSET($k1)
    mtc0 $t0, $REG_CP0_CAUSE

    lw $t0, EXC_CONTEXT_CP0_EPC_OFFSET($k1)
    mtc0 $t0, $REG_CP0_EPC

    lw $t0, EXC_CONTEXT_CP0_BADVADDR_OFFSET($k1)
    mtc0 $t0, $REG_CP0_BADVADDR

    /*
     * Just to reiterate, we don't restore EntryHi here, because the ASID
     * of the address space belonging to a thread may change between
     * context switches if ASID stealing is implemented. Moreover, the
     * value of the VPN2 bits in EntryHi is only useful for exception
     * handling, but here we are already returning.
     */

    lw $t0, EXC_CONTEXT_MD_HI_OFFSET($k1)
    lw $t1, EXC_CONTEXT_MD_LO_OFFSET($k1)
    mthi $t0
    mtlo $t1

    EXC_CONTEXT_LOAD_GENERAL_REGISTERS $k1

    /*
     * Restoring the general registers also restored the original stack
     * pointer. Note that from now on, we cannot clobber any registers
     * (except $k0-$k1).
     *
     * Next we can restore the status register.
     */
    mtc0 $k0, $REG_CP0_STATUS

    /*
     * Finally, we use the ERET exception to return from the exception
     * handler, which clears bit 1 (EXL) in the CP0 Status register
     * (stops blocking interrupts) and resume resumes execution from
     * the address in EPC.
     *
     * Note that the return can happen after a relatively long time,
     * because a timer interrupt may cause a thread to be rescheduled.
     * Hence the context could be switched several times before we
     * return here.
     */
    eret
    nop

.set pop
.endm template_exception_handler


.text

/*
 * General exception handler.
 *
 * The handler is implemented using a macro in which we substitute
 * the C function that is called to handle the exception.
 */
.globl handle_exception_general_asm
.ent handle_exception_general_asm
handle_exception_general_asm:
    template_exception_handler handle_exception_general
.end handle_exception_general_asm


/*
 * TLB refill exception handler.
 *
 * This handler is the same as the general exception handler, but
 * it jumps into a different C function to handle the exception.
 *
 * Please note that this handler is only called for non-nested TLB
 * Refill exception, i.e., when the exception is triggered in normal
 * execution mode (EXL=0). Other TLB exceptions (and TLB Refill
 * triggered with EXL=1) go to the general exception handler.
 */
.globl handle_tlb_refill_asm
.ent handle_tlb_refill_asm
handle_tlb_refill_asm:
    template_exception_handler handle_tlb_refill
.end handle_tlb_refill_asm
