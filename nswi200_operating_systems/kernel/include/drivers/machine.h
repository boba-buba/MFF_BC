// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#ifndef _DRIVERS_MACHINE_H
#define _DRIVERS_MACHINE_H

/** MSIM-specific instruction for dumping general registers. */
#define machine_dump_rd_asm .word 0x37

/** MSIM-specific instruction for dumping CP0 registers. */
#define machine_dump_cp0_asm .word 0x0e

/** MSIM-specific instruction for halting the simulation (power-off). */
#define machine_halt_asm .word 0x28

/** MSIM-specific instruction for entering interactive mode. */
#define machine_enter_debugger_asm .word 0x29

#ifndef __ASSEMBLER__

/** Halts the (virtual) machine. */
static inline void machine_halt(void) {
    __asm__ volatile(".word 0x28\n");
}

/** Enter MSIM interactive mode. */
static inline void machine_enter_debugger(void) {
    __asm__ volatile(".word 0x29\n");
}

#endif

#endif
