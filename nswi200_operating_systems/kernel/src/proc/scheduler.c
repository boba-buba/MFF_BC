// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <debug.h>
#include <drivers/timer.h>
#include <exc.h>
#include <proc/scheduler.h>
#include <proc/thread.h>

static list_t threads_runnable;

#ifndef KERNEL_SCHEDULER_QUANTUM
/** Scheduling quantum default. */
#define KERNEL_SCHEDULER_QUANTUM 2000
#endif

/** Initialize support for scheduling.
 *
 * Called once at system boot.
 */
void scheduler_init(void) {
    list_init(&threads_runnable);
}

/** Marks given thread as ready to be executed.
 *
 * It is expected that this thread would be added at the end
 * of the queue to run in round-robin fashion.
 *
 * @param thread Thread to make runnable.
 */
void scheduler_add_ready_thread(thread_t* thread) {
    bool saved_state = interrupts_disable();
    list_append(&threads_runnable, &thread->link_runnable);
    interrupts_restore(saved_state);
}

/** Removes given thread from scheduling.
 *
 * Expected to be called when thread is suspended, terminates execution
 * etc.
 *
 * @param thread Thread to remove from the queue.
 */
void scheduler_remove_thread(thread_t* thread) {
    bool saved_state = interrupts_disable();
    list_remove(&thread->link_runnable);
    interrupts_restore(saved_state);
}

/** Switch to next thread in the queue. */
void scheduler_schedule_next(void) {
    bool saved_state = interrupts_disable();
    // dprintk("Count runnable: %d\n", list_get_size(&threads_runnable));

    if (list_is_empty(&threads_runnable)) {
        panic("No threads to schedule\n");
    }

    link_t* first = list_rotate(&threads_runnable);
    thread_t* first_thread = list_item(first, thread_t, link_runnable);

    timer_interrupt_after(KERNEL_SCHEDULER_QUANTUM);
    thread_switch_to(first_thread);
    interrupts_restore(saved_state);
}