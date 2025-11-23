// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <adt/list.h>
#include <debug.h>
#include <drivers/printer.h>
#include <exc.h>
#include <lib/print.h>
#include <lib/stdarg.h>
#include <proc/thread.h>
#include <types.h>

static void print_uint(uint32_t num);

static void print_str(const char* str) {
    if (str == NULL) {
        const char* str_null = "(null)";
        print_str(str_null);
        return;
    }
    while (*str != '\0') {
        printer_putchar(*str);
        str++;
    }
}

static void print_int(int num) {
    long int num_long = (long int)num;
    if (num_long < 0) {
        printer_putchar('-');
        num_long = -num_long;
    }
    print_uint(num_long);
}

static void print_hex(unsigned int num, bool capital) {
    const char* alphabet = (capital) ? "0123456789ABCDEF" : "0123456789abcdef";
    size_t size = 8;

    char buff[size];
    for (size_t i = 0; i < size; i++) {
        buff[size - 1 - i] = alphabet[num % 16];
        num /= 16;
    }

    size_t j = 0;
    while (j < size && buff[j] == '0') {
        j++;
    }

    if (j == size) {
        printer_putchar('0');
        return;
    }

    for (size_t i = j; i < size; i++) {
        printer_putchar(buff[i]);
    }
}

static void print_uint(uint32_t num) {
    size_t size = 16;
    char buff[size];
    for (size_t i = 0; i < size; i++) {
        buff[size - 1 - i] = num % 10;
        num /= 10;
    }
    size_t j = 0;
    while (j < size && buff[j] == 0) {
        j++;
    }

    if (j == size) {
        printer_putchar('0');
        return;
    }
    for (size_t i = j; i < size; i++) {
        printer_putchar(buff[i] + '0');
    }
}

static void print_ptr(void* ptr) {
    if (ptr == NULL) {
        print_str("(nil)");
        return;
    }
    print_str("0x");
    unsigned int num = (unsigned int)ptr;
    print_hex(num, false);
}

static void print_list(void* list) {
    list_t* list_to_print = (list_t*)list;
    size_t size = list_get_size(list_to_print);
    if (size == 0) {
        print_ptr(list_to_print);
        print_str("[empty]");
    } else {
        print_ptr(list_to_print);
        printer_putchar('[');
        print_int(size);
        print_str(": ");
        link_t* it = list_to_print->head.next;
        for (size_t i = 0; i < size; ++i) {
            if (i != 0)
                printer_putchar('-');
            print_ptr(it);
            it = it->next;
        }
        printer_putchar(']');
    }
}

static void print_function(void* func) {
    print_ptr(func);
    printer_putchar('[');
    uint8_t* it = (uint8_t*)func;

    uint8_t byte0, byte1, byte2, byte3;

    for (size_t j = 0; j < 4; j++) {
        byte0 = *it;
        it++;
        byte1 = *it;
        it++;
        byte2 = *it;
        it++;
        byte3 = *it;
        it++;
        const char buff[] = { byte3, byte2, byte1, byte0 };

        unsigned char* c = (unsigned char*)buff;
        for (size_t i = 0; i < 4; i++) {
            if (c[i] < 10) {
                printer_putchar('0');
            }
            print_hex(c[i], false);
        }
        if (j != 3) {
            printer_putchar(' ');
        }
    }
    printer_putchar(']');
}

static void print_thread(void* data) {
    thread_t* thread = (thread_t*)data;

    printk("thread address: %x, name: %s, ra: %x, sp: %x\n", thread, thread->name, thread->ctx.ra, thread->ctx.sp); // thread->addr_space->asid);
}

/** Prints given formatted string to console.
 *
 * @param format printf-style formatting string.
 */
void printk(const char* format, ...) {
    bool saved_state = interrupts_disable();

    const char* it = format;

    va_list args;
    va_start(args, format);

    while (*it != '\0') {
        if (*it == '%') {
            it++;
            char type = *it;
            switch (type) {
            case 'c':
                printer_putchar(va_arg(args, int));
                break;
            case 'd':
                print_int(va_arg(args, int));
                break;
            case 'X':
                print_hex(va_arg(args, unsigned int), true);
                break;
            case 'x':
                print_hex(va_arg(args, unsigned int), false);
                break;
            case 's':
                print_str(va_arg(args, char*));
                break;
            case 'u':
                print_uint(va_arg(args, uint32_t));
                break;
            case 'p':
                it++;
                if (*it == '\0') {
                    it--;
                    break;
                }
                if (*it == 'L') {
                    void* list_to_print = va_arg(args, void*);
                    print_list(list_to_print);
                } else if (*it == 'F') {
                    print_function(va_arg(args, void*));
                } else if (*it == 'T') {
                    print_thread(va_arg(args, void*));
                } else {
                    it--;
                    print_ptr(va_arg(args, void*));
                }
                break;
            default:
                printer_putchar('%');
                break;
            }
        } else {
            printer_putchar(*it);
        }
        it++;
    }

    va_end(args);
    interrupts_restore(saved_state);
}

/** Prints given string to console, terminating it with newline.
 *
 * @param s String to print.
 */
void puts(const char* s) {
    while (*s != '\0') {
        printer_putchar(*s);
        s++;
    }
    printer_putchar('\n');
}