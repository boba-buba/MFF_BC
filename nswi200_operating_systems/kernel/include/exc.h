// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#ifndef _EXC_H
#define _EXC_H

#include <drivers/cp0.h>
#include <exc/context.h>
#include <types.h>

void handle_exception_general(exc_context_t* exc_context);
void handle_tlb_refill(exc_context_t* exc_context);
void handle_syscall(exc_context_t* context);

/** Disables all interrupts (on the CPU) and returns the previous state.
 *
 * Saves and returns the value of bit 0 (IE) of the CP0 Status
 * register prior to clearing it (and thus disabling interrupts).
 */
static inline bool interrupts_disable(void) {
    unative_t status = cp0_read(REG_CP0_STATUS);
    cp0_write(REG_CP0_STATUS, status & ~CP0_STATUS_IE_BIT);
    return (status & CP0_STATUS_IE_BIT) != 0;
}

/** Restores the interrupt-enable state (on the CPU).
 *
 * Sets bit 0 (IE) of the CP0 Status register to the given value.
 * Typically used with the return value of interrupts_disable()
 * to perform a complementary operation.
 */
static inline void interrupts_restore(bool enable) {
    unative_t status = cp0_read(REG_CP0_STATUS) & ~CP0_STATUS_IE_BIT;
    status |= enable ? CP0_STATUS_IE_BIT : 0;
    cp0_write(REG_CP0_STATUS, status);
}

#endif
