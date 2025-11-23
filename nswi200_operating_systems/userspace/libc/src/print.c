// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#include <np/syscall.h>
#include <stdarg.h>
#include <stdbool.h>
#include <stdio.h>

/** Print single character to console.
 *
 * @param c Character to print.
 * @return Character written as an unsigned char cast to an int.
 */
int putchar(int c) {
    __SYSCALL1(SYSCALL_PUTC, (unative_t)c);
    // TODO: actually print the character: either via syscall or
    // via mapping printer to some user-accessible address
    return (unsigned char)c;
}

/** Prints the given string to console, terminating it with a newline.
 *
 * @param str String to print.
 * @return Number of printed characters.
 */
int puts(const char* str) {
    int count = 1; // For new-line
    while (*str != 0) {
        putchar(*str);
        str++;
        count++;
    }
    putchar('\n');
    return count;
}

static void print_uint(uint32_t num);

static void print_str(const char* str) {
    if (str == NULL) {
        const char* str_null = "(null)";
        print_str(str_null);
        return;
    }
    while (*str != '\0') {
        putchar(*str);
        str++;
    }
}

static void print_int(int num) {
    long int num_long = (long int)num;
    if (num_long < 0) {
        putchar('-');
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
        putchar('0');
        return;
    }

    for (size_t i = j; i < size; i++) {
        putchar(buff[i]);
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
        putchar('0');
        return;
    }
    for (size_t i = j; i < size; i++) {
        putchar(buff[i] + '0');
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

static void print_function(void* func) {
    print_ptr(func);
    putchar('[');
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
                putchar('0');
            }
            print_hex(c[i], false);
        }
        if (j != 3) {
            putchar(' ');
        }
    }
    putchar(']');
}

/** Prints the given formatted string to console.
 *
 * @param format printf-style formatting string.
 * @return Number of printed characters.
 */
int printf(const char* format, ...) {

    const char* it = format;

    va_list args;
    va_start(args, format);

    while (*it != '\0') {
        if (*it == '%') {
            it++;
            char type = *it;
            switch (type) {
            case 'c':
                putchar(va_arg(args, int));
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
                if (*it == 'F') {
                    print_function(va_arg(args, void*));
                } else {
                    it--;
                    print_ptr(va_arg(args, void*));
                }
                break;
            default:
                putchar('%');
                break;
            }
        } else {
            putchar(*it);
        }
        it++;
    }

    va_end(args);

    return 0;
}
