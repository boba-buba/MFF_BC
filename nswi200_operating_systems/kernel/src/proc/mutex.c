// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <exc.h>
#include <proc/mutex.h>

/** Initializes given mutex.
 *
 * @param mutex Mutex to initialize.
 * @return Error code.
 * @retval EOK Mutex was successfully initialized.
 */
errno_t mutex_init(mutex_t* mutex) {
    bool saved_state = interrupts_disable();
    mutex->locked = false;
    list_init(&mutex->waiting_threads);
    interrupts_restore(saved_state);
    return EOK;
}

/** Destroys given mutex.
 *
 * The function must panic if the mutex is still locked when being destroyed.
 *
 * @param mutex Mutex to destroy.
 */
void mutex_destroy(mutex_t* mutex) {
    bool saved_state = interrupts_disable();
    if (mutex->locked) {
        panic("Try to destroy mutex when locked\n");
    }
    interrupts_restore(saved_state);
}

/** Locks the mutex.
 *
 * Note that when this function returns, the mutex must be locked.
 *
 * @param mutex Mutex to be locked.
 */
void mutex_lock(mutex_t* mutex) {
    bool saved_state = interrupts_disable();
    if (mutex->locked) {
        thread_t* curr = thread_get_current();
        list_append(&mutex->waiting_threads, &curr->sync_link);
        thread_suspend();
        interrupts_restore(saved_state);
        return;
    }
    mutex->locked = true;
    mutex->locked_me = thread_get_current();
    interrupts_restore(saved_state);
}

/** Unlocks the mutex.
 *
 * Note that when this function returns, the mutex might be already locked
 * by a different thread.
 *
 * This function shall panic if the mutex is unlocked by a different thread
 * than the one that locked it.
 *
 * @param mutex Mutex to be unlocked.
 */
void mutex_unlock(mutex_t* mutex) {
    bool saved_state = interrupts_disable();
    if (mutex->locked_me != thread_get_current()) {
        panic("Try to unlock thread which I did not lock\n");
    }
    if (list_is_empty(&mutex->waiting_threads)) {
        mutex->locked = false;
    } else {
        link_t* first = list_pop(&mutex->waiting_threads);
        thread_t* thread_to_wake = list_item(first, thread_t, sync_link);
        mutex->locked_me = thread_to_wake;
        thread_wakeup(thread_to_wake);
    }
    interrupts_restore(saved_state);
}

/** Try to lock the mutex without waiting.
 *
 * If the mutex is already locked, do nothing and return EBUSY.
 *
 * @param mutex Mutex to be locked.
 * @return Error code.
 * @retval EOK Mutex was successfully locked.
 * @retval EBUSY Mutex is currently locked by a different thread.
 */
errno_t mutex_trylock(mutex_t* mutex) {
    bool saved_state = interrupts_disable();
    if (mutex->locked) {
        return EBUSY;
    }
    mutex->locked = true;
    mutex->locked_me = thread_get_current();
    interrupts_restore(saved_state);
    return EOK;
}