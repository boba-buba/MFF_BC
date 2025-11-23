# DÚ6A: Deque 2.1 OMG, why so complex?

**It is completely forbidden to use the concept of C# iterator methods in your solution = usage of `yield return` is illegal!**

Your task is to implement a data structure commonly known as Deque (double-ended
queue). Base principle of this data structure is the same as for the regular
FIFO queue, but Deque also supports addition and removal of elements from both
ends. The importnant feature is that all 4 of these operations (addition/removal
of elements from the beginning/end) must be done in amortized constant time.
Your task is also to design new interface that would define a contract for all
possible implementations of the Deque data structure (and your implementation
should implement this new interface). This interface should be an addition to
other interfaces of .NET for collections.

As an internal data structure in your solution, you are **required** to use an array
of arrays, which is the way of implementing a class `deque` from C++ STL library.

![Example of deque structure](https://recodex.mff.cuni.cz:4000/v1/uploaded-files/e52aed76-aba7-11e7-a937-00505601122b/download)

[_Original Source: <http://kremer.cpsc.ucalgary.ca/STL/1024x768/deque.html> (M.
Nelson. C++ Programmer’s Guide to the Standard Template Library. IDG Books
Worldwide, Inc. Foster City, CA, 1995)_]  
where the size of primary array `Map` is increased according to the load, but the size of each `Data Block` is constant.

Your solution should contain a generic type `Deque` with one type parameter
defining the type of elements stored in the deque. Your implementation should
also implement a `IList<T>` interface and provide an access to any element of
the deque in constant time (i.e. O(1)).

You should also assume, that the typical usage of the data structure would be an
iteration in both directions and also to change to way of understanding where is
the beginning and the end of the deque -- i.e. define also a second data
structure, that would provide inverse view on the deque. I.e. adding to the
beginning in reverse view is the same as adding to the end in normal view. Also
using the `foreach` cycle on the reverse view iterates over the elements in
reverse order as in regular view.

The goal of this assignment is to try the design of a library and the design of
the implementation of a data structure, such that it complies with the rules
and conventions of standard .NET collections. This is also the reason why your
submission is not the final product. You won't be reading any input and you
won't provide any output. Instead, your implementation will be tested by several
unit tests. In order to get your implementation to work with the unit tests in
the ReCodEx system, your implementation is required to comply with the following:

  * it must not contain `Main` method in any class
  * it must contain public generic class named `Deque` with one type parameter
without any restriction on the type parameter.
  * it must contain public static class named `DequeTest`, which will provide
the reverse view on the existing Deque for the ReCodEx system (in any case, you
should design your Deque so that it is possible to get the reverse view even
without the `DequeTest` class)

`DequeTest` class must have to following prototype:

    public static class DequeTest {
    	public static IList<T> GetReverseView<T>(Deque<T> d) {
    		...
    	}
    }

In order to provide you some information about what each test in the ReCodEx
use, here is the list of test cases and the list methods exercised within these
tests:

    1: foreach, Add
    2: [], Count, Add
    3: Add, Insert(0, …), foreach, [], Count
    4: Remove, Add, Insert(0, …), foreach, [], Count
    5: Insert, Remove, Add, foreach, [], Count
    6: IndexOf, RemoveAt, Insert, Add, foreach, [], Count
    7: IndexOf, Remove, Insert, Add, foreach, [], Count
    8: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    9: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    10: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    11: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    12: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    13: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup, chybné parametry, indexy mimo meze, neexistující prvky)
    14: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup, chybné parametry, indexy mimo meze, neexistující prvky)
    15: foreach, [], Count na ReverseView, Add, Insert(0, …), foreach, [], Count
    16: foreach, [], Count na ReverseView, IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup, chybné parametry, indexy mimo meze, neexistující prvky)
    17: foreach, [], Count na ReverseView, IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup, chybné parametry, indexy mimo meze, neexistující prvky)
    18: IndexOf, Insert, Add, foreach, [], Count
    19: IndexOf, foreach, [], Count na ReverseView, Clear, IndexOf, Insert, Add, foreach, [], Count
    20: Clear, Insert, Remove, Add, foreach, [], Count (velký vstup)
    21: RemoveAt, Add, foreach, [], Count (velký vstup)
    22: RemoveAt(0), RemoveAt, Add, foreach, [], Count (velký vstup)
    23: Insert(0, …), RemoveAt(0), RemoveAt, Add, foreach, [], Count (velký vstup)
    24: CopyTo, Insert, RemoveAt, Add, foreach, [], Count (velký vstup)
    25: Insert, RemoveAt, Add, foreach, [], Count na Deque i na její ReverseView (velký vstup)
    26: Insert(0, …), RemoveAt(0), Add, [], Count (velmi velký vstup)
    27: Add, RemoveAt, [], Count (velmi velký vstup)
    28: modification of the Deque during its enumeration → expected InvalidOperationException
    29: modification of the Deque during its enumeration → expected InvalidOperationException
    30: modification of the Deque during its enumeration → expected InvalidOperationException
    31: modification of the Deque during its enumeration → expected InvalidOperationException
    32: modification of the Deque during its enumeration → expected InvalidOperationException
    33: modification of the Deque during its enumeration → expected InvalidOperationException