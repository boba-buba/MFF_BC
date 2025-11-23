// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <adt/bitmap.h>
#include <drivers/cp0.h>
#include <drivers/tlb.h>
#include <exc.h>
#include <mm/as.h>
#include <mm/frame.h>
#include <mm/heap.h>

static bitmap_t asids_bitmap;
static uint8_t storage[BITMAP_GET_STORAGE_SIZE(HARDWARE_ASIDS_MAX)];

/** Initialize support for asids
 *
 * Called once at system boot during as_init
 */
static void asids_init(void) {
    bitmap_init(&asids_bitmap, HARDWARE_ASIDS_MAX, storage);
    bitmap_fill_range(&asids_bitmap, 0, 1);
}

/** Find asid that can be used by new address space
 *
 * @param asid will be filled with asid
 * @return EOK there is free asid
 * @return ENOENT all asids are taken
 */
static errno_t asids_find_free(size_t* asid) {
    bool saved_state = interrupts_disable();
    size_t offset;
    errno_t error_code = bitmap_find_range(&asids_bitmap, 1, 0, &offset);
    if (error_code != EOK) {
        interrupts_restore(saved_state);
        return error_code;
    }
    bitmap_fill_range(&asids_bitmap, offset, 1);

    *asid = offset;
    interrupts_restore(saved_state);
    return error_code;
}

static void asids_free(size_t asid) {
    bitmap_clear_range(&asids_bitmap, asid, 1);
}

static void asids_steal_init(void) {
    // printk("clearing\n");
    bitmap_clear_range(&asids_bitmap, 0, HARDWARE_ASIDS_MAX);
    bitmap_fill_range(&asids_bitmap, 0, 1);
}

/** Initializes support for address spaces.
 *
 * Called once at system boot.
 */
void as_init(void) {
    asids_init();
}

/** Create new address space of given size.
 *
 * The address space must start at address (PAGE_SIZE * PAGE_NULL_COUNT).
 *
 * @param size Address space size (in bytes), aligned to PAGE_SIZE.
 * @param flags Flags (unused).
 * @return New address space.
 * @retval NULL Out of memory.
 */
as_t* as_create(size_t size, unsigned int flags) {
    bool saved_state = interrupts_disable();

    as_t* addr_space = (as_t*)kmalloc(sizeof(as_t));
    if (addr_space == NULL) {
        interrupts_restore(saved_state);
        return NULL;
    }

    size_t as_size_in_frames = size / FRAME_SIZE;

    addr_space->frames = (uintptr_t*)kmalloc(sizeof(uintptr_t) * as_size_in_frames);
    if (addr_space->frames == NULL) {
        // printk("no mem on heap\n");
        kfree(addr_space);
        interrupts_restore(saved_state);
        return NULL;
    }

    addr_space->size = size;
    addr_space->thread_count = 1;

    uintptr_t phys_addr;
    errno_t err;
    uintptr_t* it = addr_space->frames;
    for (size_t i = 0; i < as_size_in_frames; i++) {
        err = frame_alloc(1, &phys_addr);
        if (err == ENOMEM) {
            // printk("no mem for frames\n");
            kfree(addr_space->frames);
            kfree(addr_space);
            interrupts_restore(saved_state);
            return NULL;
        }
        *it = phys_addr;
        it++;
    }
    size_t new_asid;
    err = asids_find_free(&new_asid);
    if (err == EOK) {
        addr_space->asid = new_asid;
        interrupts_restore(saved_state);
        return addr_space;
    }
    asids_steal_init();
    err = asids_find_free(&new_asid);
    if (err == EOK) {
        printk("after steal init\n");
        addr_space->asid = new_asid;
        interrupts_restore(saved_state);
        return addr_space;
    }
    // printk("wrong\n");
    kfree(addr_space->frames);
    kfree(addr_space);
    interrupts_restore(saved_state);
    return NULL;
}

/** Get the size (in bytes) of a given address space. */
size_t as_get_size(as_t* as) {
    return as->size;
}

/** Destroy a given address space, freeing all the memory. */
void as_destroy(as_t* as) {

    bool saved_state = interrupts_disable();
    asids_free(as->asid);
    size_t as_size_in_frames = as->size / FRAME_SIZE;
    uintptr_t* it = as->frames;
    for (size_t i = 0; i < as_size_in_frames; i++) {
        errno_t err_code = frame_free(1, *it);
        if (err_code != EOK) {
            break;
        }
        it++;
    }
    kfree(as->frames);
    kfree(as);
    interrupts_restore(saved_state);
}

/** Get mapping between virtual and physical addresses.
 *
 * Both input (virtual) and output (physical) addresses MUST be aligned
 * to PAGE_SIZE. Consequently, the function basically only translates
 * between virtual page numbers and physical frame numbers, but consumes
 * and produces addresses.
 *
 * This function also allows implementing lazy mapping by allocating the
 * backing physical frame when a virtual page is first referenced and then
 * return the appropriate mapping. Because memory allocation can fail, it
 * is possible to return ENOMEM if the lazy allocation fails (though the
 * tests do not explicitly check for this situation).
 *
 * @param as Address space to use.
 * @param virt Virtual address, aligned to PAGE_SIZE.
 * @param phys Output-value pointer to the physical address (aligned to
 * PAGE_SIZE) corresponding to the given virtual address.
 * @return Error code.
 * @retval EOK Mapping found.
 * @retval ENOENT Mapping does not exist.
 */
errno_t as_get_mapping(as_t* as, uintptr_t virt, uintptr_t* phys) {

    bool saved_state = interrupts_disable();
    assert((as != NULL));

    if (virt < (PAGE_NULL_COUNT * PAGE_SIZE)) {
        interrupts_restore(saved_state);
        return ENOENT;
    }

    size_t index = (virt - (PAGE_NULL_COUNT * PAGE_SIZE)) >> 12;
    size_t as_size_in_frames = as->size / FRAME_SIZE;
    if (index < as_size_in_frames) {
        *phys = as->frames[index];
        interrupts_restore(saved_state);
        return EOK;
    }

    interrupts_restore(saved_state);
    return ENOENT;
}