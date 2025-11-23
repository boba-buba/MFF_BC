// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

/**
 * Implementation of the heap with Linked List Allocator.
 * In the heap.h there is a struct of Bump Allocator as well.
 */

#include <adt/align.h>
#include <debug.h>
#include <exc.h>
#include <mm/frame.h>
#include <mm/heap.h>

#define SIZE_STEP 0x1000
#define ALIGNMENT 4
#define HEAP_SIZE_IN_FRAMES 16
#define FRAME_COUNT_HEAP_EXTENSION 2

/** Implementation of Bump Allocator */
struct bump_alloc {
    uintptr_t heap_start;
    uintptr_t heap_end;
    uintptr_t next_free;
    int allocations;
};

/** Implementation of heap with Linked List Allocator */
struct ll_heap {
    list_t list;
};

/** Block which is used in Linked List Allocator */
typedef struct block {
    link_t link;
    bool free;
    size_t size;
} block_t;

static struct ll_heap linked_heap;

/** Find first block that can fit desired size
 *
 * @param size  Size that we want to allocate
 * @return      Pointer to block that has enough space
 */
static block_t* find_suitable_block(size_t size) {
    // Note: only called from create_block, no need for disable/restore
    list_foreach(linked_heap.list, block_t, link, block) {
        // dprintk("Block: %x, size: %x, free: %d, sizeof(block_t): %x\n", block, block->size, block->free, sizeof(block_t));
        if (block->free && (block->size >= (size + sizeof(block_t)))) {
            return block;
        }
    }
    return NULL;
}

/** Prepare block for allocation
 *
 * @param size  Size that we want to allocate
 * @return      Pointer to block that has enough space and has all flags set
 */
static block_t* create_block(size_t size) {
    // Note: only called from linked_kmalloc, no need for disable/restore
    block_t* suitable_block = find_suitable_block(size);
    if (suitable_block == NULL) {
        return NULL;
    }

    block_t* free_block = (block_t*)((uintptr_t)suitable_block + sizeof(block_t) + size);

    free_block->link.next = suitable_block->link.next;
    ((block_t*)(free_block->link.next))->link.prev = &(free_block->link);

    free_block->link.prev = &suitable_block->link;
    suitable_block->link.next = &free_block->link;

    free_block->free = true;
    free_block->size = suitable_block->size - size - sizeof(block_t);

    suitable_block->size = size;
    suitable_block->free = false;
    return suitable_block;
}

/** Implementation of the kmalloc for Linked List Allocator
 *
 * @param size Size we want to allocate
 * @return Pointer to place in memory we can use to write data
 */
static void* linked_kmalloc(size_t size) {
    bool saved_state = interrupts_disable(); // Note: Can there be race, because of linking blocks?
    block_t* new_block = create_block(size);
    if (new_block == NULL) {
        interrupts_restore(saved_state);
        return NULL;
    }
    interrupts_restore(saved_state);
    return (void*)new_block + sizeof(block_t);
}

/** Initialize first two blocks.
 *
 * @param heap_start    Block in the beginning of the heap
 * @param heap_end      Block in the end of the heap.
 * @param heap_size     Size of the first block.
 */
static void heap_init_blocks(block_t* heap_start, block_t* heap_end, size_t heap_size) {
    // Note: no need for disable/restore, as the fanction only called in heap_init
    list_init(&linked_heap.list);

    heap_start->size = heap_size;
    heap_start->free = true;
    heap_end->size = 0;
    heap_end->free = false;

    list_append(&(linked_heap.list), &(heap_start->link));
    list_append(&(linked_heap.list), &(heap_end->link));
}

/** Heap initialization.
 *
 * Called once during system boot.
 */
void heap_init(void) {
    /* Implementation with heap on top of the frame allocator. Heap will be smaller and would use frames from frame allocator*/
    uintptr_t heap_start_phys;
    size_t heap_size_frames = HEAP_SIZE_IN_FRAMES;
    errno_t error_code = frame_alloc(heap_size_frames, &heap_start_phys);
    while (error_code != EOK) {
        dprintk("heap_size_frames: %d\n", heap_size_frames);
        heap_size_frames /= 4;
        error_code = frame_alloc(heap_size_frames, &heap_start_phys);
    }

    uintptr_t heap_start = heap_start_phys + KERNEL_IMAGE_START;
    uintptr_t heap_end = heap_start + heap_size_frames * FRAME_SIZE;

    block_t* first_block = (block_t*)heap_start;
    block_t* last_block = (block_t*)(heap_end - sizeof(block_t));
    size_t heap_size = heap_end - heap_start - 2 * sizeof(block_t);

    dprintk("first block: %x, last block: %x\n", first_block, last_block);
    heap_init_blocks(first_block, last_block, heap_size);
}

/** Allocate size bytes on heap.
 *
 * @param size Amount of memory to allocate.
 * @return Pointer to allocated memory.
 * @retval NULL Out of memory.
 */
void* kmalloc(size_t size) {
    bool saved_state = interrupts_disable();
    int aligned_size = (int)size_align_up(size, ALIGNMENT);

    block_t* ptr = linked_kmalloc(aligned_size);
    if (ptr == NULL) {
        interrupts_restore(saved_state);
        return NULL;
    }

    interrupts_restore(saved_state);
    return (void*)ptr;
}

/** Allocate PAGE_SIZE (4096) bytes from frame allocator for heap optimization.
 *
 * Specifically for thread stack. Called in thread_create() when creating stack for thread
 *
 * @return Pointer to allocated memory.
 * @retval NULL Out of memory.
 */
void* kmalloc_page() {
    bool saved_state = interrupts_disable();
    uintptr_t page_phys;
    errno_t error_code = frame_alloc(1, &page_phys);
    if (error_code != EOK) {
        interrupts_restore(saved_state);
        return NULL;
    }

    uintptr_t page_virt = KERNEL_IMAGE_START + page_phys;
    interrupts_restore(saved_state);
    return (void*)page_virt;
}

/** Free PAGE_SIZE (4096) bytes from frame allocator for heap optimization.
 *
 * Specifically for thread stack.
 *
 * @param ptr Pointer (virtual address) to the page to be freed
 */
void kfree_page(void* ptr) {
    bool saved_state = interrupts_disable();
    uintptr_t page_phys = (uintptr_t)(ptr - KERNEL_IMAGE_START);
    frame_free(1, page_phys);
    interrupts_restore(saved_state);
}

/** Merge to blocks into one.
 *
 * As a result first block will absorb the second.
 *
 * @param first Block that will be expanded.
 * @param second Block which will be used to expand first.
 *
 */
static void merge_block_pair(block_t* first, block_t* second) {

    first->link.next = second->link.next;
    second->link.next->prev = &first->link;
    first->size += (second->size + sizeof(block_t));
}

/** Use after kfree to merge neighbour blocks if they are free.
 *
 * @param block_to_merged Newly freed block.
 */
static void merge_neighbour_blocks(block_t* block_to_merge) {
    block_t* right = (block_t*)block_to_merge->link.next;
    block_t* left = (block_t*)block_to_merge->link.prev;

    if (right->free) {
        merge_block_pair(block_to_merge, right);
    }
    if (left->free) {
        merge_block_pair(left, block_to_merge);
    }
}

/** Free memory previously allocated by kmalloc.
 *
 * @param ptr Pointer to memory to be freed.
 */
void kfree(void* ptr) {
    bool saved_state = interrupts_disable();
    if (ptr == NULL) {
        interrupts_restore(saved_state);
        return;
    }

    block_t* block_to_free = (block_t*)((uintptr_t)ptr - sizeof(block_t));
    block_to_free->free = true;
    merge_neighbour_blocks(block_to_free);
    interrupts_restore(saved_state);
}