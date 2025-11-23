// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <errno.h>
#include <exc.h>
#include <mm/heap.h>
#include <proc/process.h>
#include <proc/userspace.h>

/** Entry function for every user-thread
 *
 * Function copies user app to te virt mem and jumps
 * to the userspace, where the function runs
 * @param ignored
 * @return retval Return value from the user app
 */
static void* user_thread_entry_func(void* ignored) {

    uint32_t* image_it = (uint32_t*)(PROCESS_IMAGE_START + PROCESS_ENTRY_POINT);
    uint32_t* user_it = (uint32_t*)PROCESS_ENTRY_POINT;
    uint32_t* data_end = (uint32_t*)(PROCESS_IMAGE_START + PROCESS_IMAGE_SIZE);

    while (image_it != data_end) {
        *user_it = *image_it;
        image_it++;
        user_it++;
    }

    cpu_jump_to_userspace((uintptr_t)0x3000, (uintptr_t)PROCESS_ENTRY_POINT);
    thread_t* curr = thread_get_current();
    dprintk("curr_thread retval: %d\n", curr->retval);

    return curr->retval;
}

/** Create new userspace process.
 *
 * @param process_out Where to place the initialized process_t structure.
 * @param image_location Virtual address (in kernel segment) where is the image of the raw application binary.
 * @param image_size Size of the application binary image.
 * @param process_memory_size Amount of virtual memory to give to the application (at least image_size).
 * @return Error code.
 * @retval EOK Process was created and its main thread started.
 * @retval ENOMEM Not enough memory to complete the operation.
 * @retval EINVAL Invalid call (unaligned size etc.).
 */
errno_t process_create(process_t** process_out, uintptr_t image_location, size_t image_size, size_t process_memory_size) {

    process_t* new_process = (process_t*)kmalloc(sizeof(process_t));
    if (new_process == NULL) {
        return ENOMEM;
    }

    errno_t err = thread_create_new_as(&new_process->user_thread, (thread_entry_func_t)user_thread_entry_func, NULL, THREAD_USPACE_FLAGS, "user-thread", process_memory_size);
    if (err != EOK) {
        kfree(new_process);
        return err;
    }
    new_process->image_start = image_location;
    new_process->image_size = image_size;

    *process_out = new_process;

    return EOK;
}

/** Wait for termination of another process.
 *
 * @param process Process to wait for.
 * @param exit_status Where to place the process exit status (return value from main).
 * @return Error code.
 * @retval EOK Joined successfully.
 * @retval EBUSY Some other thread is already joining this process.
 * @retval EKILLED Process was killed.
 * @retval EINVAL Invalid process.
 */
errno_t process_join(process_t* process, int* exit_status) { // add free()
    bool saved_state = interrupts_disable();
    void* retval;

    errno_t err_code = thread_join(process->user_thread, &retval);
    dprintk("Err code: (%d: %s), exit_status: %x\n", err_code, errno_as_str(err_code), retval);
    if (err_code != EOK) {
        *exit_status = (int)err_code;
        thread_destroy(process->user_thread);
        kfree(process);
        interrupts_restore(saved_state);
        return err_code;
    }

    *exit_status = (int)retval;
    kfree(process);
    interrupts_restore(saved_state);
    return EOK;
}