import threading
from time import sleep
from random import randint, choice
from ds import DS

# Assuming DataStructure is in the same file or imported correctly
def test_basic_usage():
    print(" Basic usage test")
    ds = DS(0)

    ds.Add(10)
    ds.Add(5)
    ds.Add(20)

    print("Size:", ds.Size())
    print("Get index 1:", ds.Get(1))
    print("Contains 5:", ds.Contains(5))
    print("Minimum:", ds.FindMin())
    ds.Sort()

    higher = ds.GetHigher(6)
    print("Least higher than 6:", higher)

    ds.Remove(0)

    ds.RemoveAll(20)

    ds.Clear()
    print("After Clear size:", ds.Size())


def test_out_of_bounds():
    print("\n Out-of-bounds test")
    ds = DS(0)
    ds.Add(100)

    try:
        ds.Get(5)
    except Exception as e:
        print("Caught expected exception:", e)

    try:
        ds.Remove(-1)
    except Exception as e:
        print("Caught expected exception:", e)


def thread_add(ds: DS, tid: int):
    for _ in range(5):
        val = randint(1, 100)
        ds.Add(val)
        print(f"[Adder-{tid}] Added {val}")
        sleep(randint(0, 2) * 0.1)

def thread_remove(ds: DS):
    for _ in range(3):
        if ds.Size() > 0:
            try:
                ds.Remove(0)
                print(f"[{threading.current_thread().name}] Removed index 0")
            except Exception as e:
                print(f"[{threading.current_thread().name}] Remove failed:", e)
        sleep(randint(0, 1))


def test_concurrent_access():
    print("\n Simulated concurrent access (no locking)")
    ds = DS(0)
    threads = []

    # Adders
    for i in range(3):
        t = threading.Thread(target=thread_add, args=(ds, i * 10), name=f"Adder-{i}")
        threads.append(t)

    # Removers
    for i in range(2):
        t = threading.Thread(target=thread_remove, args=(ds,), name=f"Remover-{i}")
        threads.append(t)

    for t in threads:
        t.start()
    for t in threads:
        t.join()

    print("Final count:", ds.Size())


def thread_remove_random(ds: DS, tid: int):
    for _ in range(5):
        if ds.Size() > 0:
            try:
                index = randint(0, ds.Size() - 1)
                ds.Remove(index)
                print(f"[Remover-{tid}] Removed index {index}")
            except Exception as e:
                print(f"[Remover-{tid}] Exception: {e}")
        sleep(randint(0, 2) * 0.1)


def thread_reader(ds: DS, tid: int):
    for _ in range(5):
        if ds.Size() > 0:
            index = randint(0, ds.Size() - 1)
            try:
                val = ds.Get(index)
                print(f"[Reader-{tid}] Got value {val} at index {index}")
            except Exception as e:
                print(f"[Reader-{tid}] Exception: {e}")
        sleep(randint(0, 2) * 0.1)


def thread_mix(ds: DS, tid: int):
    for _ in range(10):
        action = choice(['add', 'remove', 'get', 'contains'])
        try:
            if action == 'add':
                val = randint(1, 50)
                ds.Add(val)
                print(f"[Mixed-{tid}] Added {val}")
            elif action == 'remove' and ds.Size() > 0:
                ds.Remove(0)
                print(f"[Mixed-{tid}] Removed index 0")
            elif action == 'get' and ds.Size() > 0:
                print(f"[Mixed-{tid}] Got {ds.Get(0)} from index 0")
            elif action == 'contains':
                val = randint(1, 50)
                result = ds.Contains(val)
                print(f"[Mixed-{tid}] Contains {val}: {result}")
        except Exception as e:
            print(f"[Mixed-{tid}] Exception: {e}")
        sleep(randint(0, 2) * 0.1)


def run_concurrent_tests():
    ds = DS(0)

    threads = []

    # Adders
    for i in range(3):
        t = threading.Thread(target=thread_add, args=(ds, i))
        threads.append(t)

    # Removers
    for i in range(2):
        t = threading.Thread(target=thread_remove_random, args=(ds, i))
        threads.append(t)

    # Readers
    for i in range(2):
        t = threading.Thread(target=thread_reader, args=(ds, i))
        threads.append(t)

    # Mixers
    for i in range(2):
        t = threading.Thread(target=thread_mix, args=(ds, i))
        threads.append(t)

    print("Starting concurrent threads...")
    for t in threads:
        t.start()
    for t in threads:
        t.join()

    print(" Final size reported:", ds.Size())

if __name__ == "__main__":
    test_basic_usage()
    test_out_of_bounds()
    test_concurrent_access()
    run_concurrent_tests()
