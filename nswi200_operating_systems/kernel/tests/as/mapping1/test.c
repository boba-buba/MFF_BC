// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

/*
 * Tests that as_get_mapping returns non-overlapping frame addresses.
 */

#include <ktest.h>
#include <ktest/thread.h>
#include <mm/as.h>
#include <proc/thread.h>

/**
 * Number of pages of the new created AS.
 */
#define AS_SIZE_PAGES 128

static void* as_worker(void* ignored) {
    as_t* my_space = thread_get_as(thread_get_current());

    //
    // Check that first few pages are not actually mapped.
    //
    for (unsigned int index = 0; index < PAGE_NULL_COUNT; index++) {
        uintptr_t page_addr = index * PAGE_SIZE;
        uintptr_t phys;
        errno_t err = as_get_mapping(my_space, page_addr, &phys);
        ktest_assert(err == ENOENT, "page 0x%x should not be mapped but got %s (%s)", page_addr, err, errno_as_str(err));
    }

    //
    // Skip the unmapped pages at the beginning when checking the actual
    // mapping is correct.
    //
    uintptr_t frames[AS_SIZE_PAGES];
    for (unsigned int index = 0; index < AS_SIZE_PAGES; index++) {
        uintptr_t page_addr = (index + PAGE_NULL_COUNT) * PAGE_SIZE;
        errno_t err = as_get_mapping(my_space, page_addr, &frames[index]);
        dprintk("Page #%u => 0x%x\n", index, frames[index]);
        ktest_assert_errno(err, "as_get_mapping");
        ktest_assert(frames[index] % PAGE_SIZE == 0, "frame address for page 0x%x not aligned (0x%x)", page_addr, frames[index]);
    }

    for (unsigned int i = 0; i < AS_SIZE_PAGES; i++) {
        for (unsigned int j = i + 1; j < AS_SIZE_PAGES; j++) {
            ktest_assert(frames[i] != frames[j], "two pages (0x%x and 0x%x) backed by same frame 0x%x", (i + PAGE_NULL_COUNT) * PAGE_SIZE, (j + PAGE_NULL_COUNT) * PAGE_SIZE, frames[i]);
        }
    }

    return NULL;
}

void kernel_test(void) {
    ktest_start("as/mapping1");

    thread_t* worker;
    ktest_thread_create_new_as_checked(&worker, as_worker, NULL, 0, "test-as-mapping1", AS_SIZE_PAGES * PAGE_SIZE);

    ktest_thread_join_checked(worker, NULL);

    ktest_passed();
}
