// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <drivers/machine.h>
#include <drivers/printer.h>
#include <drivers/timer.h>
#include <exc.h>
#include <lib/print.h>
#include <proc/thread.h>

/** Handles exit syscall with different statuses
 * @param status Status with which the userspace application terminated
 */
static void handle_exit_syscall(unative_t status) {
    thread_t* curr = thread_get_current();
    switch (status) {
    case 0:
        thread_finish(0);
        break;
    case 1:
        thread_kill(curr);
        break;
    default:
        thread_finish((void*)status);
        break;
    }
}

static void handle_proc_info(exc_context_t* exc_context) {
    thread_t* curr = thread_get_current();

    assert(curr->addr_space != NULL);
    // dprintk("SIZE: %x\n", curr->addr_space->size);
    (exc_context->a0) = curr->addr_space->size;
}

/** Handles a syscall.
 *
 * The function receives a pointer to an exception context, which
 * represents the snapshot of CPU and (some) CP0 registers at the
 * time of the syscall.
 */
void handle_syscall(exc_context_t* exc_context) {

    switch (exc_context->v0) {
    case 0:
        handle_exit_syscall(exc_context->a0);
        break;
    case 1:
        printer_putchar((char)(exc_context->a0));
        break;
    case 2:
        handle_proc_info(exc_context);
        break;
    default:
        panic("Unknown syscall code\n");
    }

    // On success, shift EPC by 4 to resume execution of the interrupted
    // thread on the next instruction (we don't want to restart it).
    exc_context->cp0_epc += 4;
}