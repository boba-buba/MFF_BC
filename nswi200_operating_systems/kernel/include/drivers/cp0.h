// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#ifndef _DRIVERS_CP0_H
#define _DRIVERS_CP0_H

#define CP0_STATUS_IE_BIT 0x1
#define CP0_STATUS_EXL_BIT 0x2
#define CP0_STATUS_KSU_MASK 0x18
#define CP0_STATUS_KSU_USER 0x10

#define REG_CP0_INDEX 0
#define REG_CP0_ENTRYLO0 2
#define REG_CP0_ENTRYLO1 3
#define REG_CP0_PAGEMASK 5
#define REG_CP0_BADVADDR 8
#define REG_CP0_COUNT 9
#define REG_CP0_ENTRYHI 10
#define REG_CP0_COMPARE 11
#define REG_CP0_STATUS 12
#define REG_CP0_CAUSE 13
#define REG_CP0_EPC 14

#ifndef __ASSEMBLER__
#include <adt/bits.h>
#include <types.h>

#define cp0_quote_me_(x) #x
#define cp0_quote_me(x) cp0_quote_me_(x)

#define cp0_read(register_number) \
    ({ \
        unative_t __result; \
        __asm__ volatile( \
                ".set push \n" \
                ".set noreorder \n" \
                "nop \n" \
                "mfc0 %0, $" cp0_quote_me(register_number) " \n" \
                                                           ".set pop \n" \
                : "=r"(__result)); \
        __result; \
    })

#define cp0_write(register_number, value) \
    __asm__ volatile( \
            ".set push \n" \
            ".set noreorder \n" \
            "nop \n" \
            "mtc0 %0, $" cp0_quote_me(register_number) " \n" \
                                                       ".set pop \n" \
            : \
            : "r"(value))

#define cp0_read_count() cp0_read(REG_CP0_COUNT)
#define cp0_write_compare(value) cp0_write(REG_CP0_COMPARE, (value))

#define cp0_write_index(value) cp0_write(REG_CP0_INDEX, (value))

#define cp0_read_pagemask() cp0_read(REG_CP0_PAGEMASK)
#define cp0_write_pagemask_4k() cp0_write(REG_CP0_PAGEMASK, 0)

static inline unative_t cp0_entrylo_value(uint32_t pfn, bool dirty, bool valid, bool global) {
    return bitfield_put(pfn, 24, 6)
            | bitfield_put(0, 3, 3)
            | bitfield_put(dirty, 1, 2)
            | bitfield_put(valid, 1, 1)
            | bitfield_put(global, 1, 0);
}

#define cp0_read_entrylo0() cp0_read(REG_CP0_ENTRYLO0)
#define cp0_write_entrylo0(value) cp0_write(REG_CP0_ENTRYLO0, (value))
#define cp0_write_entrylo0_pfn_dvg(pfn, dirty, valid, global) \
    cp0_write_entrylo0(cp0_entrylo_value((pfn), (dirty), (valid), (global)))

#define cp0_read_entrylo1() cp0_read(REG_CP0_ENTRYLO1)
#define cp0_write_entrylo1(value) cp0_write(REG_CP0_ENTRYLO1, (value))
#define cp0_write_entrylo1_pfn_dvg(pfn, dirty, valid, global) \
    cp0_write_entrylo1(cp0_entrylo_value((pfn), (dirty), (valid), (global)))

static inline unative_t cp0_entryhi_value(uint32_t vpn2, uint8_t asid) {
    return bitfield_put(vpn2, 19, 13) | bitfield_put(asid, 8, 0);
}

#define cp0_read_entryhi() cp0_read(REG_CP0_ENTRYHI)
#define cp0_write_entryhi(value) cp0_write(REG_CP0_ENTRYHI, (value))
#define cp0_write_entryhi_vpn2_asid(vpn2, asid) \
    cp0_write_entryhi(cp0_entryhi_value((vpn2), (asid)))

/** Get exception code from cause register value.
 *
 * @param reg_value Value of the cause register.
 * @return Exception code.
 */
static inline unsigned int cp0_cause_get_exc_code(unative_t reg_value) {
    return (reg_value >> 2) & 0x1F;
}

/** Tells whether a specific interrupt is pending.
 *
 * @param reg_value Value of the cause register.
 * @param intr Interrupt number.
 * @return Whether given interrupt was triggered.
 */
static inline bool cp0_cause_is_interrupt_pending(unative_t reg_value, unsigned int intr) {
    return (reg_value >> 8) & (1 << intr);
}

/** Sets the currently active ASID.
 *
 * The currently active ASID is stored in the CP0 EntryHi
 * register. We only update the part of the register that
 * contains the ASID and leave the rest untouched.
 *
 * @param asid New ASID value.
 */
static inline void cp0_set_asid(unsigned int asid) {
    unative_t asid_mask = bitfield_mask(8, 0);
    cp0_write_entryhi((cp0_read_entryhi() & ~asid_mask) | (asid & asid_mask));
}

#endif

#endif
