// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <drivers/cp0.h>
#include <drivers/machine.h>
#include <exc.h>
#include <lib/print.h>
#include <proc/scheduler.h>

#define EXCEPTION_INTERRUPT 0
#define EXCEPTION_MOD 1
#define EXCEPTION_TLBL 2
#define EXCEPTION_TLBS 3
#define EXCEPTION_ADEL 4
#define EXCEPTION_SYSCALL 8
#define EXCEPTION_CPU 11

/** Handles general exception.
 *
 * The function receives a pointer to an exception context, which
 * represents a snapshot of CPU and (some) CP0 registers at the
 * time of the exception occurring.
 */
void handle_exception_general(exc_context_t* exc_context) {

    unsigned int exception_code = cp0_cause_get_exc_code(exc_context->cp0_cause);
    if (exception_code == EXCEPTION_SYSCALL) {
        handle_syscall(exc_context);
        scheduler_schedule_next();
        return;
    } else if (exception_code == EXCEPTION_CPU) {
        thread_kill(thread_get_current());
        return;
    } else if (exception_code == EXCEPTION_ADEL) {
        thread_kill(thread_get_current());
        return;
    }
    if (exception_code != EXCEPTION_INTERRUPT) {

        printk("exception: cause %x, status %x, epc %x, exception_code %x, v1 %x\n", exc_context->cp0_cause, exc_context->cp0_status, exc_context->cp0_epc, exception_code, exc_context->v1);
        machine_enter_debugger();
        machine_halt();
    }

    if (!cp0_cause_is_interrupt_pending(exc_context->cp0_cause, 7)) {
        printk("exception: cause %x, status %x, epc %x\n", exc_context->cp0_cause, exc_context->cp0_status, exc_context->cp0_epc);
        machine_enter_debugger();
        machine_halt();
    }

    scheduler_schedule_next();
}