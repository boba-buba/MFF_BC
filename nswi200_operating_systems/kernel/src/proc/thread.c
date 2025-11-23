// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <abi.h>
#include <proc/context.h>
#include <proc/process.h>
#include <proc/scheduler.h>
#include <proc/thread.h>
#include <proc/userspace.h>

#include <drivers/tlb.h>

#include <adt/align.h>
#include <exc.h>
#include <mm/heap.h>

static list_t threads_all;
static thread_t* current_thread;

/** Copy string from source buffer to the destination buffer
 *
 * @param destination Place where the source string will be copied to
 * @param source String which will be copied
 */
static void strcpy(char* destination, const char* source) {

    while (*source != '\0') {
        *destination = *source;
        destination++;
        source++;
    }
    *destination = '\0';
}

/** Initialize support for threading.
 *
 * Called once at system boot.
 */
void threads_init(void) {
    list_init(&threads_all);
}

/** Print the number of all not finished threads.
 * For debugging purposes.
 */
void get_thread_count() {
    printk("%d threads are created\n", list_get_size(&threads_all));
}

/** The point where thread entry funcion runs
 */
static void entry_function_wrapper(void) {
    thread_t* curr = thread_get_current();
    thread_finish((curr->entry_func)(curr->data));
}

/** Create a new thread.
 *
 * The thread is automatically placed into the queue of ready threads.
 *
 * This function allocates space for both stack and the thread_t structure
 * (hence the double <code>**</code> in <code>thread_out</code>).
 *
 * This thread will use the same address space as the current one.
 *
 * @param thread_out Where to place the initialized thread_t structure.
 * @param entry Thread entry function.
 * @param data Data for the entry function.
 * @param flags Flags (unused).
 * @param name Thread name (for debugging purposes).
 * @return Error code.
 * @retval EOK Thread was created and started (added to ready queue).
 * @retval ENOMEM Not enough memory to complete the operation.
 * @retval EINVAL Invalid flags (unused).
 */
errno_t thread_create(thread_t** thread_out, thread_entry_func_t entry, void* data, unsigned int flags, const char* name) {
    bool saved_state = interrupts_disable();

    thread_t* new_thread = (thread_t*)kmalloc(sizeof(thread_t));
    if (new_thread == NULL) {
        interrupts_restore(saved_state);
        return ENOMEM;
    }

    new_thread->entry_func = entry;
    new_thread->data = data;
    new_thread->state = RUNNABLE;
    new_thread->waiting_for_me = false;
    new_thread->addr_space = NULL;
    new_thread->flags = flags;
    if (flags == THREAD_WITHOUT_AS_FLAGS && current_thread->addr_space) {
        new_thread->addr_space = current_thread->addr_space;
        new_thread->addr_space->thread_count++;
    }

    strcpy(new_thread->name, name);
    void* stack_bottom = kmalloc_page();
    if (stack_bottom == NULL) {
        kfree(new_thread);
        interrupts_restore(saved_state);
        return ENOMEM;
    }
    new_thread->stack_bottom = uintptr_align_up((uintptr_t)stack_bottom, ABI_STACK_ALIGNMENT);

    /** Alignment */
    (&new_thread->ctx)->sp = (unative_t)new_thread->stack_bottom + THREAD_STACK_SIZE - ABI_STACK_FRAME_SIZE;

    /** Prepare context */
    (&new_thread->ctx)->ra = (unative_t)entry_function_wrapper;

    (&new_thread->ctx)->cp0_status = 0x8001;

    list_append(&threads_all, &new_thread->link_all);

    *thread_out = (thread_t*)new_thread;
    scheduler_add_ready_thread(*thread_out);

    interrupts_restore(saved_state);
    return EOK;
}

/** Create a new thread with new address space.
 *
 * The thread is automatically placed into the queue of ready threads.
 *
 * This function allocates space for both stack and the thread_t structure
 * (hence the double <code>**</code> in <code>thread_out</code>).
 *
 * @param thread_out Where to place the initialized thread_t structure.
 * @param entry Thread entry function.
 * @param data Data for the entry function.
 * @param flags Flags (unused).
 * @param name Thread name (for debugging purposes).
 * @param as_size Address space size, aligned at page size (0 is correct though not very useful).
 * @return Error code.
 * @retval EOK Thread was created and started (added to ready queue).
 * @retval ENOMEM Not enough memory to complete the operation.
 * @retval EINVAL Invalid flags (unused).
 */
errno_t thread_create_new_as(thread_t** thread_out, thread_entry_func_t entry, void* data, unsigned int flags, const char* name, size_t as_size) {
    bool saved_state = interrupts_disable();
    errno_t err_code = thread_create(thread_out, entry, data, THREAD_WITH_AS_FLAGS, name);
    if (err_code != EOK) {
        interrupts_restore(saved_state);
        return err_code;
    }
    printk("thread is created\n");
    if (as_size != 0) {
        as_t* new_addr_space = as_create(as_size, flags);
        if (new_addr_space == NULL) {
            printk("new as was not created\n");
            scheduler_remove_thread(*thread_out);
            list_remove(&(*thread_out)->link_all);
            thread_destroy(*thread_out);
            interrupts_restore(saved_state);
            return ENOMEM;
        }
        (*thread_out)->flags = flags;
        (*thread_out)->addr_space = new_addr_space;
    }

    interrupts_restore(saved_state);
    return EOK;
}

/** Return information about currently executing thread.
 *
 * @retval NULL When no thread was started yet.
 */
thread_t* thread_get_current(void) {
    return current_thread;
}

/** Yield the processor. */
void thread_yield(void) {

    bool saved_state = interrupts_disable();
    scheduler_schedule_next();
    interrupts_restore(saved_state);
}

/** Current thread stops execution and is not scheduled until woken up. */
void thread_suspend(void) {
    bool saved_state = interrupts_disable();
    thread_t* curr = thread_get_current();
    curr->state = SLEEPING;

    scheduler_remove_thread(curr);
    thread_yield();
    interrupts_restore(saved_state);
}

/** Terminate currently running thread.
 *
 * Thread can (normally) terminate in two ways: by returning from the entry
 * function or by calling this function. The parameter to this function then
 * has the same meaning as the return value from the entry function.
 *
 * Note that this function never returns.
 *
 * @param retval Data to return in thread_join.
 */
void thread_finish(void* retval) {

    bool saved_state = interrupts_disable();
    thread_t* curr = thread_get_current();
    curr->state = FINISHED;
    if (retval != NULL) {
        curr->retval = retval;
    }

    scheduler_remove_thread(curr);
    thread_yield();
    interrupts_restore(saved_state);
    panic("thread outlived its termination %p", curr);
    while (1) {
    }
}

/** Tells if thread already called thread_finish() or returned from the entry
 * function.
 *
 * @param thread Thread in question.
 */
bool thread_has_finished(thread_t* thread) {
    return thread->state == FINISHED;
}

/** Wakes-up existing thread.
 *
 * Note that waking-up a running (or ready) thread has no effect (i.e. the
 * function shall not count wake-ups and suspends).
 *
 * Note that waking-up a thread does not mean that it will immediatelly start
 * executing.
 *
 * @param thread Thread to wake-up.
 * @return Error code.
 * @retval EOK Thread was woken-up (or was already ready/running).
 * @retval EEXITED Thread already finished its execution.
 */
errno_t thread_wakeup(thread_t* thread) {
    bool saved_state = interrupts_disable();

    if (thread->state == SLEEPING) {
        thread->state = RUNNABLE;
        scheduler_add_ready_thread(thread);
    }
    if (thread->state == FINISHED) {
        interrupts_restore(saved_state);
        return EEXITED;
    }

    interrupts_restore(saved_state);
    return EOK;
}

/** Joins another thread (waits for it to terminate).
 *
 * Note that <code>retval</code> could be <code>NULL</code> if the caller
 * is not interested in the returned value.
 *
 * @param thread Thread to wait for.
 * @param retval Where to place the value returned from thread_finish.
 * @return Error code.
 * @retval EOK Thread was joined.
 * @retval EBUSY Some other thread is already joining this one.
 * @retval EKILLED Thread was killed.
 */
errno_t thread_join(thread_t* thread, void** retval) {
    bool saved_state = interrupts_disable();

    if (thread == NULL) {
        panic("Thread to join is null\n");
    }

    if (thread->waiting_for_me) {
        interrupts_restore(saved_state);
        return EBUSY;
    }

    if (thread->state == SLEEPING) {
        thread_yield();
    }

    thread->waiting_for_me = true;

    while (!thread_has_finished(thread)) {

        thread_yield();
        if (thread->state == KILLED) {
            thread_destroy(thread);
            interrupts_restore(saved_state);
            return EKILLED;
        }
    }

    if (retval != NULL) {
        *retval = thread->retval;
    }
    thread_destroy(thread);

    interrupts_restore(saved_state);
    return EOK;
}

static void thread_clean_tlb_stealing(void) {
    tlb_probe();
    cp0_write_entryhi_vpn2_asid(0, 0);
    tlb_write_indexed();
}

/** Switch CPU context to a different thread.
 *
 * Note that this function must work even if there is no current thread
 * (i.e. for the very first context switch in the system).
 *
 * @param thread Thread to switch to.
 */
void thread_switch_to(thread_t* thread) {
    bool saved_state = interrupts_disable();
    thread_t* prev = current_thread;

    if (current_thread != NULL) {
        current_thread = thread;
        if (current_thread->addr_space != NULL) {
            thread_clean_tlb_stealing();
            cp0_set_asid(current_thread->addr_space->asid);
        }
        cpu_switch_context(&prev->ctx, &thread->ctx);
        interrupts_restore(saved_state);

    } else {
        context_t dummy_ctx;
        current_thread = thread;
        if (current_thread->addr_space != NULL) {
            thread_clean_tlb_stealing();
            cp0_set_asid(current_thread->addr_space->asid);
        }
        cpu_switch_context(&dummy_ctx, &thread->ctx);
        interrupts_restore(saved_state);
    }
}

/** Get address space of given thread. */
as_t* thread_get_as(thread_t* thread) {
    return (thread->addr_space);
}

/** Kills given thread.
 *
 * Note that this function shall work for any existing thread, including
 * currently running one.
 *
 * Joining a killed thread results in EKILLED return value from thread_join.
 *
 * @param thread Thread to kill.
 * @return Error code.
 * @retval EOK Thread was killed.
 * @retval EEXITED Thread already finished its execution.
 */
errno_t thread_kill(thread_t* thread) {

    bool saved_state = interrupts_disable();

    if (thread->state == FINISHED)
        return EEXITED;

    scheduler_remove_thread(thread);

    if (thread == thread_get_current()) {
        thread->state = KILLED;
        thread_yield();
    }

    thread->state = KILLED;
    interrupts_restore(saved_state);
    return EOK;
}

void thread_destroy(thread_t* thread) {
    if (thread) {
        kfree_page((void*)thread->stack_bottom);
        if (thread->addr_space) {
            thread->addr_space->thread_count--;
            if (thread->addr_space->thread_count == 0) {
                as_destroy(thread->addr_space);
            }
        }
        kfree(thread);
    }
}
