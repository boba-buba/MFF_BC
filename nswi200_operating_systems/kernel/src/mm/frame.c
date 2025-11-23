// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

/** Structure we have:
 *     2G
 * ....|kernel| free space| space we do not have ....
 *
 * Bitmap will be this free space, where we can allocate frames
 *            |00000000000|
 *      kernel|free space | space we do not have
 * Dynamic allocation of the storage for frame_bitmap
 *            |..|00000000|
 *              ^       ^
 *              |       |
 *     bitmap storage   frames
 *
 * Heap will take frames from frame allocator
 */

#include "mm/self.h"
#include <adt/bitmap.h>
#include <adt/bits.h>
#include <debug.h>
#include <exc.h>
#include <mm/frame.h>
#include <types.h>

#define SIZE_STEP 0x1000
#define HEAP_SIZE_START 0x10000

static bitmap_t frame_bitmap;
static uint8_t* frame_bitmap_storage;
static uintptr_t frame_alloc_start; /** initialized in frame_init */

/** Get the end of the space we can use for frames
 *
 * @param start End of the kernel image - start of the free space
 * @return           Pointer to the last free 4 Kib
 */
static uintptr_t detect_end_of_available_memory(uintptr_t start) {
    volatile uintptr_t ptr = start;
    uintptr_t space_end = start;
    const uint32_t pattern = 0xdeadbeef;
    while (true) {
        *(uint32_t*)ptr = pattern;

        /* outside of the scope, where we can write */
        if (*(uint32_t*)ptr != pattern) {
            space_end -= SIZE_STEP;
            break;
        }
        space_end += SIZE_STEP;
        ptr += SIZE_STEP;
    }
    return space_end;
}

/**
 * Initializes frame allocator.
 *
 * Called once at system boot.
 *
 */
void frame_init(void) {

    uintptr_t free_space_start = kernel_get_image_end(); /** 2G + kernel image */
    uintptr_t free_space_start_aligned = uintptr_align_up(free_space_start, FRAME_SIZE); /** aligned to frame size */

    uintptr_t free_space_end = detect_end_of_available_memory(free_space_start_aligned);

    size_t free_frames_count = (free_space_end - free_space_start_aligned) / FRAME_SIZE + 1; /** + 1 because detect_end_of_available_memory returns last free frame */
    frame_bitmap_storage = (uint8_t*)free_space_start_aligned; /** Storage will be at the start of the free space */
    frame_alloc_start = free_space_start_aligned - KERNEL_IMAGE_START + FRAME_SIZE; /** One frame is enough to store the the storage. It stores no more than 300 byte */
    dprintk("frame count: %d, frame_alloc_start: %x, frame_bitmap_storage: %x\n", free_frames_count, frame_alloc_start, frame_bitmap_storage);

    bitmap_init(&frame_bitmap, free_frames_count - 1, frame_bitmap_storage); /** - 1 because one frame is used to store the storage for bitmap */
}

/**
 * Allocate continuous sequence of physical frames.
 *
 * The allocated frames can be returned by frame_free.
 *
 * Note that frame_free must be given the number of frames being freed.
 * This means that the frame allocator does not need to record the number
 * of frames allocated (only the their allocation state).
 *
 * The requirement for 'count' to be a power of 2 is intended to allow
 * implementing a class of frame allocators which require it. This is
 * not an obstacle for other frame allocators.
 *
 * @param count How many frames to allocate, must be power of 2.
 * @param phys Output-value pointer to the physical address of the
 * first frame in sequence.
 * @return Error code.
 * @retval EOK Frames allocated.
 * @retval ENOMEM Not enough memory.
 */
errno_t frame_alloc(size_t count, uintptr_t* phys) {
    bool saved_state = interrupts_disable();
    assert(count > 0);
    assert(is_power_of_two(count));

    size_t offset;
    errno_t error_code = bitmap_find_range(&frame_bitmap, count, 0, &offset);
    if (error_code == ENOENT) {
        interrupts_restore(saved_state);
        return ENOMEM;
    }
    dprintk("offset: %d, count: %d, bitmap_size: %d\n", offset, count, bitmap_get_size(&frame_bitmap));
    bitmap_fill_range(&frame_bitmap, offset, count);
    *phys = (uintptr_t)(frame_alloc_start + (offset * FRAME_SIZE)); /** return physical address: returns kernel size + offset * frame_size*/
    interrupts_restore(saved_state);
    return EOK;
}

/**
 * Free continuous sequence of physical frames.
 *
 * The returned frames were previously allocated by frame_alloc.
 *
 * @param count How many frames to free (power of 2).
 * @param phys Physical address of the first frame in sequence.
 * @return Error code.
 * @retval EOK Frames freed.
 * @retval ENOENT Invalid frame address.
 * @retval EBUSY Some frames were not allocated (double free).
 */
errno_t frame_free(size_t count, uintptr_t phys) {
    bool saved_state = interrupts_disable();
    assert(count > 0);
    assert(is_power_of_two(count));

    uintptr_t address_without_kernel = phys - frame_alloc_start;
    size_t offset = address_without_kernel / FRAME_SIZE;

    size_t bitmap_length = bitmap_get_size(&frame_bitmap);

    if (offset >= bitmap_length || (offset + count) > bitmap_length) {
        interrupts_restore(saved_state);
        return ENOENT;
    }
    if (!bitmap_check_range_is(&frame_bitmap, offset, count, 1)) {
        interrupts_restore(saved_state);
        return EBUSY;
    }
    bitmap_clear_range(&frame_bitmap, offset, count);
    interrupts_restore(saved_state);
    return EOK;
}
