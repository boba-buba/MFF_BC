# Milestone 05: Virtual Memory Management

After a short break in the form of thread support, preemptive scheduling, and
synchronization primitives, this milestone returns to memory management. The
main goal of this milestone is to extend your kernel with basic support for
virtual memory, which will be needed for user-space threads and processes.

Implementing support for virtual memory requires managing the address
translation hardware (usually referred to as _memory management unit_, MMU) and
servicing exceptions related to address translation that are bound to occur
when threads access virtual memory. This in turn requires the kernel to support
the notion of a virtual address space backed by physical memory.

Translation of virtual to physical addresses is generally performed with the
granularity of fixed-size blocks (instead of individual bytes) so that the
hardware can implement it **efficiently**. In the virtual address space, the
blocks are called (virtual) _pages_, while in the physical address space, the
blocks are called (physical) _frames_. In some contexts, they may be referred
to as _virtual pages_ and _physical pages_.

Regardless of the name, both pages and frames have equal size, and are
**naturally aligned** (i.e., they start at addresses divisible by their size).
This is necessary to allow the address translation hardware to map the part of
the virtual address representing a virtual page number to a corresponding
physical frame number and then just append the bits representing the offset in
the virtual page to the physical frame number, producing a physical address
(more on [address translation](#address-translation) later).

Consequently, the memory management subsystem in your kernel must manage both
(page-sized and page-aligned) frames as well as arbitrary blocks of heap
memory. This is best achieved by providing different allocators for different
purposes, but because both allocators ultimately manage the same resource
(physical memory), they need to cooperate to avoid clashes. Your kernel already
provides a heap allocator, but lacks the frame allocator, which you will need
to implement. Because the frame allocator operates with tighter restrictions,
the best approach is to stack the allocators so that heap allocator uses the
frame allocator to acquire blocks of memory (more on
[frame allocator](#frame-allocator) later).


---

## Address spaces

In general-purpose operating systems, an address space is a collection of
virtual memory areas (VMA) which represent ranges of valid virtual addresses
that a program can use. The virtual memory areas typically correspond to areas
known from process memory layout, i.e., code, static data, heap, and thread
stack(s), but there can be more (think of dynamically linked libraries). The
operating system is responsible for establishing the mapping of (valid) virtual
memory addresses to physical memory addresses (with the granularity of pages).

Your task in this milestone will be simpler, because we require your
implementation to only support a **single** VMA. The size of the VMA will be
fixed at creation time, and it will always start at virtual address
`2 * PAGE_SIZE`. This means that the first two pages (addresses from `0` to
`2 * PAGE_SIZE - 1`) **WILL NOT** be mapped, which ensures that dereferencing
a `NULL` pointer causes an exception.

Threads will be associated with address spaces. When creating a new
thread, the creator can choose whether to share its own address space
with the thread being created, or whether to create a thread with a new
address space of given size. To this end, the thread API in
the [`proc/thread.h`](kernel/include/proc/thread.h) header includes two new
functions that need to be implemented in
[`proc/thread.c`](kernel/src/proc/thread.c).

The kernel should destroy an address space when the **last** thread using it
terminates. This implies that the kernel needs to keep track of the number of
threads using a particular address space.

The functions in the [`mm/as.h`](kernel/include/mm/as.h) header file represent
the interface you must to implement (in [`mm/as.c`](kernel/src/mm/as.c)) to
enable address space management. You are expected to extend the `as_t`
structure as necessary to suit the needs of your implementation.


---

## Frame allocator

As mentioned earlier, the address translation hardware requires that virtual
memory pages be mapped to page-sized physical memory frames (both aligned to
page size). Your kernel therefore needs to be able to allocate the physical
frames that will be used to back the virtual pages, which is what a frame
allocator does.

In your kernel, physical memory is currently managed (as a single
contiguous block) by the kernel heap allocator. It allows allocating
blocks with a relatively fine granularity, both in terms of size and
alignment. However, the heap allocator is unsuitable for allocating
physical memory frames for address translation, because it stores
service information inside the allocated blocks and the `kmalloc()`
interface is not rich enough to specify alignment (both for size and
starting addresses).

For this reason, memory management is typically layered. The frame
allocator is used to manage physical memory with the granularity of
pages. To be able to provide whole frames, the allocator stores usage
information elsewhere (i.e., not in or near the returned frames). The
kernel heap allocator is then built on top of the frame allocator,
requesting new frames when running out of free space. Such a heap
allocator therefore needs to support operating with non-contiguous
ranges of physical memory.

While this would be the preferred solution, an acceptable (baseline) solution
is to split the available physical memory between the frame allocator and the
kernel heap allocator at boot time (without dynamic resizing) and keep them
separate.

Regardless of the implementation, the functions in the [`mm/frame.h`](kernel/include/mm/frame.h)
header file represent the frame allocator interface you are required to
implement (in [`mm/frame.c`](kernel/src/mm/frame.c)). The functions are
expected to work in a manner similar to `kmalloc()` and `kfree()`, even though
the API is slightly different.

Your implementation can assume that all physical memory will fit into the
512 MiB segment (`KSEG0`) directly accessible from the kernel. Neither
your frame allocator nor your memory-probing code needs to consider physical
memory beyond this.

For optimal memory use, the kernel can allocate the underlying bitmap
(or any other data structure used to track physical memory allocation)
dynamically (during initialization) from free memory. In other words,
the frame allocator would first reserve a few frames as storage for the
service data. If you do not wish to optimize your kernel in such a way,
you can allocate the bitmap statically: because we assume only 512 MiB
memory will be used, the actual memory consumption will be relatively
low (see the definition of the `FRAME_COUNT_MAX` symbol in the
[`mm/frame.h`](kernel/include/mm/frame.h) header file). We expect you to use
this macro to determine the bitmap size when pre-allocating the bitmap at
compile-time.

You can safely assume that the amount of memory available will never change
once the machine started (i.e. no need to prepare for live memory adding
or removal). Values detected at OS boot are final until machine halt.

---

## Address translation

The actual address translation on MIPS is done via software-managed TLB,
but only for addresses from certain ranges. So far your kernel code has
been executing (and accessing memory) at addresses in the `KSEG0` range
(2-2.5 GiB, or `0x80000000`-`0xA0000000`), which is not mapped to
physical addresses via TLB, but via a hard-wired mechanism which simply
subtracts `0x80000000` (2 GiB) from virtual the address. This is a
common processor feature, often referred to as _identity mapping_, which
simplifies kernel bootstrap, and which is why you did not need to worry
about setting up and managing the TLB so far.

However, for user-space virtual memory support, your kernel needs to manage
mapping for addresses in the `KUSEG` range (0-2 GiB) which are mapped
using the TLB. Whenever a CPU accesses an address in this range for which
there is no translation entry in the TLB, it triggers a *TLB exception* which
the operating system kernel must handle.

Specifically, the kernel needs to use the information about the
exception (especially the contents of the CP0 `BadVAddr` register) to
determine whether the accessed address is valid or invalid for the
currently running thread. If the access is valid, the kernel must insert
a corresponding address translation entry into the TLB and restart the
offending instruction. If the access is invalid, the offending thread
must be terminated, which is what the `thread_kill()` function is for.

The processor distinguishes three types of TLB exceptions. In most
cases, your kernel will be handling *TLB refill* exceptions, which occur
if there is no TLB entry that matches the virtual address. To this end,
you need to implement the `handle_tlb_refill()` function in
[`mm/tlb.c`](kernel/src/mm/tlb.c). Other types of TLB exceptions can occur,
but these need to be handled in the general exception handler (the
`handle_exception_general()` function in [`mm/exc.c`](kernel/src/mm/exc.c)).
Please consult the section on TLB exceptions (starting on page 138/158) in the
[MIPS R4000 Microprocessor User's Manual](https://d3s.mff.cuni.cz/files/teaching/nswi200/202324/doc/mips-r4000-manual.pdf)
for details.



### Hardware support for address spaces

To make the address translation more efficient when running multiple processes
with different virtual memory maps, the MIPS MMU allows tagging the TLB entries
with an 8-bit _address space identifier_ (ASID) to disambiguate TLB entries
with identical virtual addresses.

The preferred implementation of address translation is to use the ASID
to allow translation entries for different address spaces to coexist in
the TLB. This requires allocating/assigning an unused ASID to an address
space on creation, and reclaiming the ASID on address space destruction.

At the other end of the spectrum, the baseline solution allows implementing
address translation without ASIDs, which just requires flushing the entire
TLB on each address space switch.

There are also other alternatives --- see the website and individual test
suites for typical extensions. Due to the broad spectrum of possible solutions,
we will review this part manually and assign extra points based on the actual
implementation.

Note that because the number of ASIDs supported by the hardware is rather high,
intensive tests tend to run for quite a while. To reduce test execution time,
we have introduced the `HARDWARE_ASIDS_MAX` constant (set at compile-time)
which limits the number of ASIDs available in your code. In practice, this will
not limit the number of ASIDs supported by the hardware (MSIM), but your code
(and the tests) will assume that a lower number of ASIDs is available and
should start recycling the ASIDs earlier, thus reducing execution time.


---

## Support code

This milestone again updates the kernel base code to provide the necessary
context and some convenience functions. We recommend to review the update to
get an idea what has changed and where.


### Exception handling

Compared to the previous milestone, there is another change to exception
handling. This time we have added the assembly language portion of the
*TLB Refill* exception handler. The code is mostly identical to the general
exception handling code, but it calls a different C function
(`handle_tlb_refill()` in [`exc/tlb.c`](kernel/src/exc/tlb.c)) to handle the
exception. This is where you need to put your implementation of the
*TLB Refill* handler.

### TLB management

MIPS uses a software-managed TLB and provides special instructions (`tlbwr`,
`tlbwi`, and `tlbr`) for reading and writing the contents of the TLB. We have
made those instructions available to C code through the `tlb_*` macros defined
in the [`drivers/tlb.h`](kernel/include/drivers/tlb.h) header file.

As an example of how those macros are used, the following code adds a
mapping from virtual addresses `0x2000` and `0x3000` to physical frames
`0x17000` and `0x1B000` for ASID `0x15`.

```c
cp0_write_pagemask_4k();
cp0_write_entrylo0_pfn_dvg(0x17, true, true, false);
cp0_write_entrylo1_pfn_dvg(0x1B, true, true, false);
cp0_write_entryhi_vpn2_asid(0x1, 0x15);
tlb_write_random();
```

The `_dvg` suffix stands for `dirty`, `valid`, and `global` boolean
arguments passed to the function, and is meant to make it a little
easier for the reader to distinguish the boolean values at the call
site.

Note that the CP0 `EntryLo0` and `EntryLo1` registers hold a **pair** of
physical addresses that correspond to **two consecutive** virtual pages
starting with page number `vpn2 << 1`.

Please consult the
[MIPS R4000 Microprocessor User's Manual](https://d3s.mff.cuni.cz/files/teaching/nswi004/resources/mips-processor.pdf)
for more details on the software interface to memory management unit
(starting on page 84/114).

Note that you can use the `cpu0 tlbd` command of the MSIM simulator to
display the contents of the TLB when in interactive mode.


### Bitmaps

For convenience, we provide a simple bitmap implementation. The interface
can be found in the [`adt/bitmap.h`](kernel/include/adt/bitmap.h) header file.
You may want to use this in your frame allocator code, but it is not mandated.
If you use your own data structure, make sure that the low-level implementation
details of the data structure are not intermixed with the frame allocator code.

### Tests

The milestone provides new tests to help you drive the implementation.
The tests can be split into several groups: tests for the frame
allocator and tests for the virtual memory mapping.

The frame allocator tests are similar to the heap allocator tests. Do
not forget to run those too to ensure that your frame allocator and heap
allocator can happily coexist.

The virtual memory mapping tests check that your threads can access
virtual addresses in the first few kilobytes of virtual memory and that
separate address spaces do not influence each other.

As usual, the tests should help you check that you have not forgotten
any major part of the implementation. It is possible to pass some tests
even with a solution that does not really work.

---

## Additional hints/suggestions

* Allocate thread stacks from frame allocator directly. You will need
  a much smaller heap then.
* What is PFN and VPN2?
* Why the `TLBWR` and `TLBWI` instructions do not have any operands?
* What happens when (timer) interrupt occurs in the middle of TLB
  exception handling?
* What happens when `handle_tlb_refill()` returns?
* What happens when entry in TLB is marked as invalid?
* Development hints and suggestions
    * Frame allocator can use the mock frames, similarly to M02
    * Frame can be temporarily allocated from heap by requesting an 8 KiB
      block and returning a properly aligned address from inside the block.
    * Ensure that your address space functions work first.
    * Ensure that you can look-up the mapping (see the
      [`as/mapping1`](kernel/tests/as/mapping1/test.c), and
      [`as/mapping2`](kernel/tests/as/mapping2/test.c) tests).
    * Focus first on cases in which the kernel will see only a few address
      spaces (and simply use a counter for an ASID).
    * Even though we do not prescribe an API for ASID management, we suggest to
      create one and use it to avoid interleaving the implementation details of
      ASID management with other code.

---

## Submission

The **baseline solution** for this milestone must pass the baseline tests for
`M01`, `M02`, `M03`, `M04` and `M05`. The commit representing the solution
must be tagged with `m05`. Submitting a **working baseline solution** within
this milestone's deadline allows you to gain bonus points towards a better
grade.

Extended features are usually covered by separate test suites (see comments on
the first line of the respective suite for details). Some features cannot be
reliably tested and will be evaluated during final review.

Please, see also the notes in the main [README.md](README.md).
