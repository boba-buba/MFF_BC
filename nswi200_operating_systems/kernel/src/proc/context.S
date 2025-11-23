// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <proc/context.h>

#define cp0_status 12

.text
.set noreorder

/*
 * void cpu_switch_context(
 *     context_t * a0_this_context,
 *     context_t * a1_next_context
 * )
 *
 * Switches processor to another context. The first argument points to
 * "this" context, i.e., the context structure where the current state
 * of the CPU will be stored.
 *
 * The second argument points to the "next" context, from which the
 * state of the CPU will be restored to resume execution.
 *
 * Note that from the perspective of the MIPS ABI, the caller can only
 * assume that this function will preserve the callee-saved registers
 * $s0-$s7 along with $gp, $fp, $sp, and $ra. We do not need to save
 * them here, because whoever needed them preserved had to save them on
 * the stack before. This is why the context_t structure only contains
 * the essential registers and the CP0 Status registers, which contains
 * the interrupt status.
 */
.globl cpu_switch_context

.ent cpu_switch_context
cpu_switch_context:
    /*
     * Save the general-purpose registers into the context
     * structure pointed to by $a0.
     */
    CONTEXT_SAVE_GENERAL_REGISTERS $a0

    /*
     * Save the CP0 Status register. This saves the
     * interrupt status of the current thread.
     */
    mfc0 $t0, $cp0_status
    sw $t0, CONTEXT_CP0_STATUS_OFFSET($a0)

    /*
     * Load the general-purpose registers from the target context.
     * The register $a1 is not being restored so we can use it as
     * a base register. Note that this switches to another stack!
     */
    CONTEXT_LOAD_GENERAL_REGISTERS $a1

    /*
     * Save the pointer to the current kernel stack top.
     */
    la $a0, kernel_stack_top
    sw $sp, 0($a0)

    /*
     * Restore the CP0 Status register from the target context and
     * return to the caller. Setting the Status register to the value
     * from the target context will restore the interrupt status. This
     * may enable or disable interrupts, depending on the value of the
     * Status register in the target context.
     *
     * Note that the Status register is actually loaded in the branch
     * delay slot of the jump instruction that returns control to the
     * target context.
     */
    lw $t0, CONTEXT_CP0_STATUS_OFFSET($a1)
    j $ra
    mtc0 $t0, $cp0_status

.end cpu_switch_context
