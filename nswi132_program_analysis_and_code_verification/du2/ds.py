
from nagini_contracts.contracts import *
from nagini_contracts.contracts import Predicate
from nagini_contracts.obligations import MustTerminate
from typing import List, Optional

class Element:
    def __init__(self, value: int) -> None:
        Ensures(Acc(self._value))
        Ensures(Acc(self._next))
        self._value: int = value
        self._next: Optional[Element] = None
        return

# @Predicate
# def list_segment(node: Optional[Element], length: int) -> bool:
#     (node is None and length == 0) or \
#     (node is not None and \
#     length > 0 and Acc(node._value) and Acc(node._next) and \
#     list_segment(node._next, length - 1))

class DS:

    def __init__(self, value: int) -> None:
        Ensures(Acc(self._head))
        Ensures(Acc(self._tail))
        Ensures(Acc(self._count))
        first_element : Element = Element(value)
        self._head : Element = first_element  # Start of list
        self._tail : Element = first_element  # End of list (where we add new nodes)
        self._count : int = 1

    def Add(self, val: int) -> None:
        Requires(Acc(self._tail))
        Requires(Acc(self._head))
        Requires(Acc(self._tail._next))
        Requires(Acc(self._count))
        Ensures(Acc(self._tail))
        Ensures(Acc(self._head))
        Ensures(Acc(self._tail._next))
        Ensures(Acc(self._count))
        Ensures(Old(self._count) == self._count - 1)
        new_node = Element(val)
        if self._count == 0:
            self._head = new_node
        else:
            self._tail._next = new_node
            self._tail = new_node
        self._count += 1

    def Get(self, index: int) -> Optional[int]:
        Requires(index >= 0)
        Requires(Acc(self._count) and index < self._count)
        Requires(Acc(self._head))
        Requires(Acc(self._head._next))
        #Requires(Implies(Acc(Old(current._next)) == True, Acc(current._next) == True))        
        current: Optional[Element] = self._head
        for _ in range(index):
            current = current._next

        return current._value if current is not None else None

    def GetHigher(self, val: int) -> Optional[int]:
        Requires(Acc(self._head))
        #Requires(list_segment(self._head, self._count))
        current: Optional[Element] = self._head
        higher: Optional[int] = None
        while current is not None:
            Invariant(current is None or type(current) == Element)
            #Invariant(list_segment(current, self._count))  # partial ownership if needed
            Invariant(Acc(current._value))
            Invariant(Acc(current._next))
            if current._value > val:
                if higher is None or current._value < higher:
                    higher = current._value
            current = current._next
        return higher

    def Remove(self, index: int) -> None:
        Requires(Acc(self._count))
        Requires(Acc(self._head))
        Requires(Acc(self._head._next))
        Requires(Acc(self._tail))
        Ensures(Acc(self._count))
        Ensures(Old(self._count) >= self._count)
        if index < 0 or index >= self._count:
            return

        if index == 0:
            self._head = self._head._next
            self._count -= 1
            if self._count == 0:
                self._tail = None
            return

        prev = self._head
        for _ in range(index - 1):
            prev = prev._next
        to_delete = prev._next
        prev._next = to_delete._next
        if to_delete == self._tail:
            self._tail = prev
        self._count -= 1

    def RemoveAll(self, val: int) -> None:
        Requires(Acc(self._head))
        Requires(Acc(self._head._value))
        Requires(Acc(self._head._next))
        Requires(Acc(self._count))
        Ensures(Acc(self._count))
        Ensures(Old(self._count) >= self._count and self._count >= 0)
        dummy = Element(0)
        dummy._next = self._head
        prev = dummy
        current = self._head

        while current:
            Invariant(Acc(current._value))
            Invariant(Acc(current._next))
            if current._value == val:
                prev._next = current._next
                self._count -= 1
            else:
                prev = current
            current = current._next

        self._head = dummy._next

        # Update tail
        current = self._head
        if not current:
            self._tail = None
        else:
            while current._next:
                current = current._next
            self._tail = current

    def Sort(self) -> None:
        Requires(Acc(self._head))
        Requires(Acc(self._head._value))
        # Convert to list
        elements : List[int] = []
        current = self._head
        while current:
            Invariant(Acc(current._value))
            elements.append(current._value)
            current = current._next

        # Sort values
        elements = sorted(elements)

        # Rebuild list
        current = self._head
        for val in elements:
            current._value = val
            current = current._next

    def FindMin(self) -> Optional[int]:
        Requires(Acc(self._count))
        Requires(Acc(self._head))
        if self._count == 0:
            return None
        current = self._head
        min_val = current._value
        while current:
            if current._value < min_val:
                min_val = current._value
            current = current._next
        return min_val

    def Contains(self, val: int) -> bool:
        Requires(Acc(self._head))
        Requires(Acc(self._head._value))
        #Invariant(Implies(Acc(current), Acc(current._next)))
        current : Element = self._head
        while current:
            if current._value == val:
                return True
            current = current._next
        return False

    def Clear(self) -> None:
        Requires(Acc(self._head))
        Requires(Acc(self._tail))
        Requires(Acc(self._count))

        self._count = 0
        self._head = None
        self._tail = None

    @Pure
    def Size(self) -> int:
        Requires(Rd(self._count))
        Ensures(Result() == self._count) # len(self._elements) doesnt work
        return self._count
