// SPDX-License-Identifier: Apache-2.0
// Copyright 2019 Charles University

#ifndef _MM_HEAP_H
#define _MM_HEAP_H

#include "self.h"
#include <adt/list.h>
#include <lib/print.h>
#include <types.h>

void heap_init(void);
void* kmalloc(size_t size);
void kfree(void* ptr);
void* kmalloc_page(void);
void kfree_page(void* ptr);

#endif
