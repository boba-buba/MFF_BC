class Element:
    def __init__(self, value: int) -> None:
        self._value: int = value
        self._next: Element = None
        return

class DS:
    def __init__(self, value: int) -> None:
        first_element = Element(value)
        self._head : Element = first_element  # Start of list
        self._tail : Element = first_element  # End of list (where we add new nodes)
        self._count : int = 1

    def Add(self, val: int) -> None:
        new_node = Element(val)
        self._tail._next = new_node
        self._tail = new_node
        self._count += 1

    def Get(self, index: int) -> int:
        if index < 0 or index >= self._count:
            raise IndexError("Index out of bounds")
        current = self._head
        for _ in range(index):
            current = current._next
        return current._value


data = DS(1)

data.Add(2)
data.Add(3)

print(data.Get(2))
