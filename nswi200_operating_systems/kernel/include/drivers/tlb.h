// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#ifndef _DRIVERS_TLB_H
#define _DRIVERS_TLB_H

#define TLB_ENTRY_COUNT 48

#define tlb_write_random() __asm__ volatile("tlbwr\n")
#define tlb_write_indexed() __asm__ volatile("tlbwi\n")
#define tlb_read() __asm__ volatile("tlbr\n")
#define tlb_probe() __asm__ volatile("tlbp\n")

#endif
