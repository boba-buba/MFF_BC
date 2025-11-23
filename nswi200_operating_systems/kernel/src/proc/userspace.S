// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

.text
.set noreorder

#include <drivers/cp0.h>
#include <proc/context.h>

/*
 * void cpu_jump_to_userspace(
 *     uintptr_t a0_stack_top,
 *     uintptr_t a1_entry_func
 * )
 *
 * Switches processor to userspace code. The first argument is address
 * of the top of userland stack, the second argument is the address where
 * to continue the execution (i.e. userspace entry point).
 *
 * The jump is done via setting status register to emulate exception handling,
 * setting EPC and stack pointer and then executing the ERET that leaves the
 * exception level state and "returns" to userspace.
 */
.globl cpu_jump_to_userspace

.ent   cpu_jump_to_userspace
cpu_jump_to_userspace:
    /*
     * Switch the processor to Exception Level (EXL=1) which keeps the CPU
     * in kernel mode with interrupts disabled despite setting KSU=2 for
     * user mode execution with interrupts enabled (IE=1).
     *
     * This allows us to safely load the entry point into EPC and switch the
     * stack pointer to user stack. The final ERET clears EXL and jumps to the
     * address in EPC, but with EXL=0, the CPU is now executing in user mode
     * with interrupts enabled.
     */
    li $v0, (CP0_STATUS_KSU_USER | CP0_STATUS_EXL_BIT | CP0_STATUS_IE_BIT)
    mfc0 $v1, $REG_CP0_STATUS
    or $v1, $v1, $v0
    mtc0 $v1, $REG_CP0_STATUS

    mtc0 $a1, $REG_CP0_EPC
    move $sp, $a0
    eret
    nop
.end cpu_jump_to_userspace
