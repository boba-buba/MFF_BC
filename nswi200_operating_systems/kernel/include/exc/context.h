// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#ifndef _EXC_CONTEXT_H
#define _EXC_CONTEXT_H

/*
 * This file is shared between C and assembler sources, because we
 * want to keep information on low-level structures in the same file.
 * We use ifdefs to include the portions relevant to each context.
 */

/** Size of the exc_context_t structure below, needed in assembler code. */
#define EXC_CONTEXT_SIZE (26 * 4)

#ifndef __ASSEMBLER__

#include <types.h>

/**
 * CPU context for exception handling
 *
 * This context structure is used to store the state of a thread when its
 * execution is interrupted as a result of exception, external interrupt,
 * or a syscall. These events are different from voluntary context switch
 * where most of the context is already stored (implicitly) on the stack.
 * In this case, we need to store almost everything, including the special
 * multiply/divide registers.
 *
 * We do not need to store the $zero, $k0, and $k1 registers. Also, if we
 * can avoid using the callee-saved registers ($s0-$s7, $s8/$fp, and $gp for
 * non-pic code) before jumping into C code of the handler, they will be
 * preserved automatically by the compiler in any function that uses them.
 *
 * Finally, we need to store additional CP0 registers so that the handler
 * can identify the cause of the exception and (if possible) resume thread
 * execution after the exception has been handled.
 */
typedef struct {
    // 0
    // No need to store $zero.

    // 1
    unative_t at;

    // 2..3
    unative_t v0;
    unative_t v1;

    // 4..7
    unative_t a0;
    unative_t a1;
    unative_t a2;
    unative_t a3;

    // 8..15
    unative_t t0;
    unative_t t1;
    unative_t t2;
    unative_t t3;
    unative_t t4;
    unative_t t5;
    unative_t t6;
    unative_t t7;

    // 16..23
    // No need to store $s0-$s7.

    // 24..25
    unative_t t8;
    unative_t t9;

    // 26..27
    // No need to store $k0-$k1.

    // 28
    // No need to store $gp (non-pic code).

    // 29
    unative_t sp;

    // 30
    // No need to store $fp/$s8.

    // 31
    unative_t ra;

    // Multiply/divide registers.
    unative_t md_lo;
    unative_t md_hi;

    // CP0 registers.
    unative_t cp0_status;
    unative_t cp0_cause;
    unative_t cp0_epc;
    unative_t cp0_badvaddr;
    unative_t cp0_entryhi;
} exc_context_t;

#else /* __ASSEMBLER__ */
// clang-format off


/*
 * The offsets for general purpose and special registers
 * must match the exc_context_t structure.
 */
.set EXC_CONTEXT_AT_OFFSET, 0*4

.set EXC_CONTEXT_V0_OFFSET, 1*4
.set EXC_CONTEXT_V1_OFFSET, 2*4

.set EXC_CONTEXT_A0_OFFSET, 3*4
.set EXC_CONTEXT_A1_OFFSET, 4*4
.set EXC_CONTEXT_A2_OFFSET, 5*4
.set EXC_CONTEXT_A3_OFFSET, 6*4

.set EXC_CONTEXT_T0_OFFSET, 7*4
.set EXC_CONTEXT_T1_OFFSET, 8*4
.set EXC_CONTEXT_T2_OFFSET, 9*4
.set EXC_CONTEXT_T3_OFFSET, 10*4
.set EXC_CONTEXT_T4_OFFSET, 11*4
.set EXC_CONTEXT_T5_OFFSET, 12*4
.set EXC_CONTEXT_T6_OFFSET, 13*4
.set EXC_CONTEXT_T7_OFFSET, 14*4
.set EXC_CONTEXT_T8_OFFSET, 15*4
.set EXC_CONTEXT_T9_OFFSET, 16*4

.set EXC_CONTEXT_SP_OFFSET, 17*4
.set EXC_CONTEXT_RA_OFFSET, 18*4

.set EXC_CONTEXT_MD_LO_OFFSET, 19*4
.set EXC_CONTEXT_MD_HI_OFFSET, 20*4

.set EXC_CONTEXT_CP0_STATUS_OFFSET, 21*4
.set EXC_CONTEXT_CP0_CAUSE_OFFSET, 22*4
.set EXC_CONTEXT_CP0_EPC_OFFSET, 23*4
.set EXC_CONTEXT_CP0_BADVADDR_OFFSET, 24*4
.set EXC_CONTEXT_CP0_ENTRYHI_OFFSET, 25*4


/*
 * The EXC_CONTEXT_SAVE_GENERAL_REGISTERS macro stores general registers
 * into the ex_context_t structure. The macro can be used with any general
 * purpose register (no registers are destroyed in this code).
 *
 * The assembler directives in the macro turn off warnings when using the
 * $at register and also tell the assembler to avoid macro instructions
 * and instruction reordering.
 */
.macro EXC_CONTEXT_SAVE_GENERAL_REGISTERS base
.set push
.set noat
.set noreorder
.set nomacro

    sw $at, EXC_CONTEXT_AT_OFFSET(\base)

    sw $v0, EXC_CONTEXT_V0_OFFSET(\base)
    sw $v1, EXC_CONTEXT_V1_OFFSET(\base)

    sw $a0, EXC_CONTEXT_A0_OFFSET(\base)
    sw $a1, EXC_CONTEXT_A1_OFFSET(\base)
    sw $a2, EXC_CONTEXT_A2_OFFSET(\base)
    sw $a3, EXC_CONTEXT_A3_OFFSET(\base)

    sw $t0, EXC_CONTEXT_T0_OFFSET(\base)
    sw $t1, EXC_CONTEXT_T1_OFFSET(\base)
    sw $t2, EXC_CONTEXT_T2_OFFSET(\base)
    sw $t3, EXC_CONTEXT_T3_OFFSET(\base)
    sw $t4, EXC_CONTEXT_T4_OFFSET(\base)
    sw $t5, EXC_CONTEXT_T5_OFFSET(\base)
    sw $t6, EXC_CONTEXT_T6_OFFSET(\base)
    sw $t7, EXC_CONTEXT_T7_OFFSET(\base)
    sw $t8, EXC_CONTEXT_T8_OFFSET(\base)
    sw $t9, EXC_CONTEXT_T9_OFFSET(\base)

    sw $ra, EXC_CONTEXT_RA_OFFSET(\base)
    sw $sp, EXC_CONTEXT_SP_OFFSET(\base)

.set pop
.endm EXC_CONTEXT_SAVE_GENERAL_REGISTERS


/*
 * The EXC_CONTEXT_LOAD_GENERAL_REGISTERS macro loads general registers from
 * the exc_context_t structure. The macro \base parameter should be a register
 * that is different from the registers being restored. The $sp register is
 * an exception, because it is restored last (on purpose).
 *
 * When using the $k0 and $k1 registers, the interrupts MUST be disabled to
 * ensure that the code cannot be interrupted (and the contents of the $k0
 * and $k1 registers destroyed).
 *
 * Also keep in mind that the macro restores the stack pointer, which means
 * that it will switch to another stack.
 */
.macro EXC_CONTEXT_LOAD_GENERAL_REGISTERS base
.set push
.set noat
.set noreorder
.set nomacro

    lw $at, EXC_CONTEXT_AT_OFFSET(\base)

    lw $v0, EXC_CONTEXT_V0_OFFSET(\base)
    lw $v1, EXC_CONTEXT_V1_OFFSET(\base)

    lw $a0, EXC_CONTEXT_A0_OFFSET(\base)
    lw $a1, EXC_CONTEXT_A1_OFFSET(\base)
    lw $a2, EXC_CONTEXT_A2_OFFSET(\base)
    lw $a3, EXC_CONTEXT_A3_OFFSET(\base)

    lw $t0, EXC_CONTEXT_T0_OFFSET(\base)
    lw $t1, EXC_CONTEXT_T1_OFFSET(\base)
    lw $t2, EXC_CONTEXT_T2_OFFSET(\base)
    lw $t3, EXC_CONTEXT_T3_OFFSET(\base)
    lw $t4, EXC_CONTEXT_T4_OFFSET(\base)
    lw $t5, EXC_CONTEXT_T5_OFFSET(\base)
    lw $t6, EXC_CONTEXT_T6_OFFSET(\base)
    lw $t7, EXC_CONTEXT_T7_OFFSET(\base)
    lw $t8, EXC_CONTEXT_T8_OFFSET(\base)
    lw $t9, EXC_CONTEXT_T9_OFFSET(\base)

    lw $ra, EXC_CONTEXT_RA_OFFSET(\base)
    lw $sp, EXC_CONTEXT_SP_OFFSET(\base)

.set pop
.endm EXC_CONTEXT_LOAD_GENERAL_REGISTERS

// clang-format on
#endif

#endif
