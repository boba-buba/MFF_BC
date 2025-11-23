// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <debug.h>
#include <drivers/machine.h>
#include <drivers/tlb.h>
#include <exc.h>
#include <proc/thread.h>

// static size_t vpn_offset = 0x1;

/** Handles TLB refill exception.
 *
 * The function receives a pointer to an exception context, which
 * represents a snapshot of CPU and (some) CP0 registers at the
 * time of the exception occurring.
 */
void handle_tlb_refill(exc_context_t* exc_context) {
    bool saved_state = interrupts_disable();
    uintptr_t bad_virtual_addr = exc_context->cp0_badvaddr;
    bool valid = true;
    thread_t* curr = thread_get_current();
    as_t* as_curr = thread_get_as(curr);
    uintptr_t phys0;
    uintptr_t phys1 = 0x0;

    uintptr_t vpn2 = (bad_virtual_addr >> 12) >> 1;

    errno_t err_code = as_get_mapping(as_curr, bad_virtual_addr, &phys0);
    if (err_code != EOK) {
        thread_kill(curr);
        interrupts_restore(saved_state);
        return;
    }

    size_t index = (bad_virtual_addr - (PAGE_NULL_COUNT * PAGE_SIZE)) >> 12;
    if (index % 2 == 1) {
        err_code = as_get_mapping(as_curr, bad_virtual_addr - PAGE_SIZE, &phys1);
    } else {
        err_code = as_get_mapping(as_curr, bad_virtual_addr + PAGE_SIZE, &phys1);
    }
    if (err_code != EOK) {
        valid = false;
    }

    uintptr_t entryl0 = phys0;
    uintptr_t entryl1 = phys1;
    bool valid0 = true;
    bool valid1 = valid;

    if (index % 2 == 1) {
        entryl0 = phys1;
        valid0 = valid;
        entryl1 = phys0;
        valid1 = true;
    }

    cp0_write_pagemask_4k();

    cp0_write_entrylo0_pfn_dvg(entryl0 >> 12, true, valid0, false);
    cp0_write_entrylo1_pfn_dvg(entryl1 >> 12, true, valid1, false);

    cp0_write_entryhi_vpn2_asid(vpn2, as_curr->asid);
    tlb_write_random();

    interrupts_restore(saved_state);
}