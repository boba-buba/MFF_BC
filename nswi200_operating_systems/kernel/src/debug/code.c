// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <debug/code.h>
#include <lib/print.h>

/** Dump function code at given address.
 *
 * Generally, the output should look like disassembly without
 * mnemonics part.
 *
 * @param name Function name to print in header.
 * @param address Function address.
 * @instruction_count How many instructions to print.
 */
void debug_dump_function(const char* name, uintptr_t address, size_t instruction_count) {
    printk("%x <%s>:\n", address, name);
    uint8_t* it = (uint8_t*)address;

    uint8_t byte0, byte1, byte2, byte3;

    for (size_t j = 0; j < instruction_count; j++) {
        byte0 = *it;
        it++;
        byte1 = *it;
        it++;
        byte2 = *it;
        it++;
        byte3 = *it;
        it++;
        const char buff[] = { byte3, byte2, byte1, byte0 };
        printk("%x:        ", address + j * 4);
        unsigned char* c = (unsigned char*)buff;
        for (size_t i = 0; i < 4; i++) {
            if (c[i] < 10) {
                printk("%c", '0');
            }
            printk("%x", c[i]);
        }
        if (j != instruction_count - 1) {
            printk("%c", '\n');
        }
    }
}
