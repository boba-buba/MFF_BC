// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <np/list.h>
#include <np/proc.h>
#include <stdlib.h>

#define ALIGNMENT 4

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

static inline uintptr_t uintptr_align_up(uintptr_t addr, unative_t align) {
    return (addr + align - 1) & ~(align - 1);
}

/** Align given size up to the given power-of-two alignment. */
static inline size_t size_align_up(size_t size, unative_t align) {
    return (size + align - 1) & ~(align - 1);
}

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
    // Note: only called from linked_malloc, no need for disable/restore
    block_t* suitable_block = find_suitable_block(size);
    if (suitable_block == NULL) {
        // dprintk("Suitable block is null\n");
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
    // dprintk("suitabl block is processed\n");
    return suitable_block;
}

/** Implementation of the kmalloc for Linked List Allocator
 *
 * @param size Size we want to allocate
 * @return Pointer to place in memory we can use to write data
 */
static void* linked_malloc(size_t size) {
    block_t* new_block = create_block(size);
    if (new_block == NULL) {
        return NULL;
    }
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

static uintptr_t detect_end_of_available_memory(uintptr_t heap_start) {

    np_proc_info_t curr_process;
    bool ok = np_proc_info_get(&curr_process);
    if (!ok) {
        return 0;
    }

    uintptr_t heap_end = (uintptr_t)curr_process.virt_mem_size;
    return heap_end;
}

void heap_init(void) {
    uintptr_t heap_start = uintptr_align_up((uintptr_t)_app_end, 32);

    uintptr_t heap_end = detect_end_of_available_memory(heap_start);
    list_init(&linked_heap.list);

    block_t* first_block = (block_t*)heap_start;
    block_t* last_block = (block_t*)(heap_end - sizeof(block_t));
    size_t heap_size = (size_t)(heap_end - heap_start - 2 * sizeof(block_t));

    heap_init_blocks(first_block, last_block, heap_size);
}

/** Allocate size bytes on heap.
 *
 * @param size Amount of memory to allocate.
 * @return Pointer to allocated memory.
 * @retval NULL Out of memory.
 */
void* malloc(size_t size) {

    int aligned_size = (int)size_align_up(size, ALIGNMENT);

    block_t* ptr = linked_malloc(aligned_size);
    if (ptr == NULL) {
        return NULL;
    }
    return (void*)ptr;
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
void free(void* ptr) {
    if (ptr == NULL) {
        return;
    }

    block_t* block_to_free = (block_t*)((uintptr_t)ptr - sizeof(block_t));
    block_to_free->free = true;
    merge_neighbour_blocks(block_to_free);
}