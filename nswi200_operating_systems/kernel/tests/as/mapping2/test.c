// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

/*
 * Tests that the 'as_get_mapping' function returns non-overlapping
 * frame addresses for multiple live address spaces.
 */

#include <adt/list.h>
#include <exc.h>
#include <ktest.h>
#include <ktest/thread.h>
#include <mm/as.h>
#include <mm/heap.h>
#include <proc/thread.h>

#define AS_SIZE_PAGES_MAX 64
#define AS_SIZE_PAGES_MIN 4

/*
 * Run workers (which read the page mapping) in batches of limited size.
 * This is needed for implementations with on-demand paging where allocating
 * all address spaces at once would exhaust memory (for the as_t structures)
 * without leaving any memory for the actual backing frames.
 */
#define WORKER_BATCH_SIZE 16

typedef struct {
    thread_t* thread;
    size_t frame_count;
    uintptr_t* frames;
    link_t link;
} worker_t;

static list_t workers;
static size_t workers_ready_count;

static volatile bool workers_can_run;
static volatile bool workers_can_finish;
static volatile bool workers_out_of_memory;

static inline size_t atomic_get(size_t* value_ptr) {
    bool ipl = interrupts_disable();
    size_t result = *value_ptr;
    interrupts_restore(ipl);
    return result;
}

static inline void atomic_increment(size_t* value_ptr) {
    bool ipl = interrupts_disable();
    (*value_ptr)++;
    interrupts_restore(ipl);
}

/*
 * Get a worker structure corresponding to the given thread.
 * Needs to be implemented this way because we cannot rely on
 * thread parameters to be supported by all implementations.
 */
static worker_t* unsafe_get_worker(thread_t* thread) {
    list_foreach(workers, worker_t, link, worker) {
        if (worker->thread == thread) {
            return worker;
        }
    }

    return NULL;
}

static worker_t* get_worker(thread_t* thread) {
    bool ipl = interrupts_disable();
    worker_t* result = unsafe_get_worker(thread);
    interrupts_restore(ipl);

    ktest_assert(result != NULL, "internal error: unable to find worker for %pT", thread);
    return result;
}

static void* as_worker(void* ignored) {
    /*
     * Keep spinning until the worker is allowed to actually run.
     *
     * This also means that we don't touch the list of workers
     * before the main thread adds this worker to it.
     */
    while (!workers_can_run) {
        thread_yield();
    }

    /*
     * Collect physical frame addresses for virtual page numbers, with
     * the exception of PAGE_NULL_COUNT pages at the start of the address
     * space. If 'as_get_mapping' fails with ENOMEM, signal out of memory
     * condition.
     */
    worker_t* self = get_worker(thread_get_current());
    dprintk("Worker %p started...\n", self);

    as_t* self_as = thread_get_as(self->thread);
    for (size_t vpn = PAGE_NULL_COUNT; vpn < self->frame_count; vpn++) {
        uintptr_t va = vpn * PAGE_SIZE;

        errno_t err = as_get_mapping(self_as, va, &self->frames[vpn]);
        if (err == ENOMEM) {
            self->frames[vpn] = 0;
            workers_out_of_memory = true;
            continue;
        }

        ktest_assert_errno(err, "as_get_mapping");
    }

    /*
     * Indicate 'readiness to finish' but keep spinning
     * until the worker is actually allowed to finish.
     */
    atomic_increment(&workers_ready_count);

    while (!workers_can_finish) {
        thread_yield();
    }

    return NULL;
}

static worker_t* create_worker(size_t as_size_pages) {
    /*
     * Allocate worker structure, frame array, and a worker thread.
     * If everything succeeds, return the worker structure, otherwise
     * release the allocated memory (preferably in LIFO fashion) and
     * report failure.
     */
    worker_t* worker = kmalloc(sizeof(worker_t));
    if (worker != NULL) {
        /*
         * Include PAGE_NULL_COUNT (unused) elements at the beginning of the
         * array so that we can index it directly using the virtual page number.
         * This simplifies address calculations and address printing.
         */
        worker->frame_count = as_size_pages + PAGE_NULL_COUNT;

        worker->frames = kmalloc(sizeof(uintptr_t) * worker->frame_count);
        if (worker->frames != NULL) {
            size_t as_size = as_size_pages * PAGE_SIZE;
            errno_t err = thread_create_new_as(
                    &worker->thread, as_worker, NULL, 0,
                    "test-as-mapping2", as_size);

            if (err == EOK) {
                // Success: return the worker structure.
                return worker;
            }

            // Failure: release memory and report failure.
            kfree(worker->frames);
        }

        kfree(worker);
    }

    return NULL;
}

static void unlatch_workers_and_await_ready(size_t expected_ready_count) {
    workers_can_run = true;

    while (atomic_get(&workers_ready_count) != expected_ready_count) {
        thread_yield();
        printk(".");
    }

    workers_can_run = false;
    printk("\n");
}

void kernel_test(void) {
    ktest_start("as/mapping2");

    // No need to synchronize here (there are no workers yet).
    list_init(&workers);
    workers_ready_count = 0;

    workers_can_run = false;
    workers_can_finish = false;
    workers_out_of_memory = false;

    /*
     * Allocate a batch of workers for address spaces of given size.
     * If it fails, retry with a smaller address space. When there are
     * enough workers, unlatch them to let them read the page mappings.
     *
     * Finish when the address space size drops below the given limit
     * or when any of the workers signals an out of memory condition.
     */
    size_t workers_count = 0;
    size_t as_size_pages = AS_SIZE_PAGES_MAX;

    while (!workers_out_of_memory && as_size_pages >= AS_SIZE_PAGES_MIN) {
        worker_t* worker = create_worker(as_size_pages);
        if (worker == NULL) {
            // Try again (or stop trying).
            as_size_pages /= 2;
            continue;
        }

        printk("Created worker #%u (%p %pT): %u pages\n",
                workers_count, worker, worker->thread, as_size_pages);

        bool ipl = interrupts_disable();
        list_append(&workers, &worker->link);
        interrupts_restore(ipl);

        // If there are enough new workers, let them run to completion.
        workers_count++;
        if ((workers_count % WORKER_BATCH_SIZE) == 0) {
            printk("Unlatching workers (up to #%u)...", workers_count - 1);
            unlatch_workers_and_await_ready(workers_count);
        }
    }

    // Let any leftover workers run to completion.
    if (atomic_get(&workers_ready_count) < workers_count) {
        printk("Unlatching leftover workers (up to #%u)...", workers_count - 1);
        unlatch_workers_and_await_ready(workers_count);
    }

    // Report on how did we exit the loop.
    if (workers_out_of_memory) {
        printk("Workers signaled out of memory condition.\n");
    } else {
        printk("Address space size (%u pages) is below %u.\n", as_size_pages, AS_SIZE_PAGES_MIN);
    }

    printk("Created %u workers in total.\n", workers_count);

    // Unlatch the finish phase so that all threads can be joined.
    printk("Joining all workers...");

    workers_can_finish = true;
    list_foreach(workers, worker_t, link, worker) {
        ktest_thread_join_checked(worker->thread, NULL);
    }

    printk(" done.\n");

    printk("Checking for overlaps...\n");

    size_t checked_mappings_count = 0;
    list_foreach(workers, worker_t, link, outer_worker) {
        for (size_t outer_vpn = PAGE_NULL_COUNT; outer_vpn < outer_worker->frame_count; outer_vpn++) {
            uintptr_t outer_pa = outer_worker->frames[outer_vpn];
            if (outer_pa == 0) {
                // Out of memory (probably due to paging on demand).
                continue;
            }

            // Check alignment.
            uintptr_t outer_va = outer_vpn * PAGE_SIZE;
            ktest_assert((outer_pa % PAGE_SIZE) == 0,
                    "Worker %p[%u]: frame address for page 0x%x[%u] not aligned (0x%x)",
                    outer_worker, outer_worker->frame_count,
                    outer_va, outer_vpn, outer_pa);

            // Check overlap.
            list_foreach(workers, worker_t, link, inner_worker) {
                for (size_t inner_vpn = PAGE_NULL_COUNT; inner_vpn < inner_worker->frame_count; inner_vpn++) {
                    if ((inner_worker == outer_worker) && (inner_vpn == outer_vpn)) {
                        // The mapping for the same VPN in the same worker will be equal.
                        continue;
                    }

                    uintptr_t inner_pa = inner_worker->frames[inner_vpn];
                    if (inner_pa == 0) {
                        // Out of memory (probably due to paging on demand).
                        continue;
                    }

                    uintptr_t inner_va = inner_vpn * PAGE_SIZE;
                    ktest_assert(
                            outer_pa != inner_pa,
                            "Worker %p[%u]: frame 0x%x backs page 0x%x[%u] but also 0x%x[%u] in %p[%u]",
                            outer_worker, outer_worker->frame_count,
                            outer_pa, outer_va, outer_vpn, inner_va, inner_vpn,
                            inner_worker, inner_worker->frame_count);

                    checked_mappings_count++;
                }
            }
        }
    }

    ktest_assert(checked_mappings_count > 0,
            "No pairs checked, probably no memory ever allocated for backing frames.");

    ktest_passed();
}
