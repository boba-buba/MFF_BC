using System;
using System.Collections;
using System.Collections.Generic;

namespace My.System.Generic {

    /// <summary>
    /// Pprovides the reverse view on the existing Deque.
    /// </summary>
    public static class DequeTest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type of elements stored in the Deque</typeparam>
        /// <param name="d">The instance of Deque reverse view of which will be returned.</param>
        /// <returns>Reversed view of the Deque.</returns>
        public static IList<T> GetReverseView<T>(Deque<T> d)
        {
            Reverse<T> list = new Reverse<T>(d);
            return list;
        }
    }

    /// <summary>
    /// Class that represents reversed view. Works with reference to the Deque and implements reversed Enumerator.
    /// </summary>
    /// <typeparam name="TItem">Type of the items stored in the eque.</typeparam>
    public class Reverse<TItem> : IEnumerable<TItem> , IList<TItem>, ICollection<TItem> 
    {
        /// <summary>
        /// Deque.
        /// </summary>
        Deque<TItem> d;

        /// <summary>
        /// Initializes a new instance of the Reverse class that gets an instance of the Deque.
        /// </summary>
        /// <param name="d">Deque instance</param>
        public Reverse(Deque<TItem> d)
        {
            this.d = d;
        }

        internal bool Enumerating
        {
            get
            {
                return d._enumeratingNow;
            }
            set
            {
                d._enumeratingNow = value;
            }
        }

        #region ICollection implementation
        /// <summary>
        /// Read-only property, that returns number of elements in the Deque.
        /// </summary>
        public int Count { get { return d.Count; } }

        /// <summary>
        /// Gets a value indicating whether the Reverse is read-only. 
        /// </summary>
        public bool IsReadOnly => true;

        /// <summary>
        /// Adds an item to the front of the Deque.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Add(TItem item) { d.Insert(0, item); }

        /// <summary>
        /// Clears the contents of Deque.
        /// </summary>
        public void Clear() { d.Clear(); }

        /// <summary>
        /// Determines whether the Deque contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the Deque.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(TItem item) { return d.Contains(item); }

        /// <summary>
        /// Copies the elements of the Deque<typeparamref name="TItem"/> to an Array, starting at a particular Array index.
        /// Elements of the Deque from the end.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("index");

            int end = Math.Min(array.Length, d.Count);

            for (int i = 0; i < end; i++)
            {
                array[i + arrayIndex] = this[d.Count - 1 - i];
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the Deque.
        /// </summary>
        /// <param name="item">The object to remove from the Deque.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException">The Deque is read-only.</exception>
        public bool Remove(TItem item) { return d.Remove(item); }
        #endregion

        #region IList implementation
        /// <summary>
        /// Gets or sets the element at the (Deque.Count - 1 - index) in the Deque..
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public TItem this[int index]
        {
            get
            {
                return d[d.Count - 1 - index];
            }
            set
            {
                d[d.Count - 1 - index] = value;
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the Deque.
        /// </summary>
        /// <param name="item">The object to locate in the Deque.</param>
        /// <returns>The (Deque.Count - 1 - index) of item if found in the list; otherwise, -1.</returns>
        public int IndexOf(TItem item) 
        {
            for (int i = 0; i < Count; i++)
            {
                TItem? el = this[i];

                if (item == null && el == null)
                    return i;
                else if (item != null)
                {
                    if (item.Equals(el)) return i;
                }

            }
            return -1;
        }

        /// <summary>
        /// Inserts an item to the Deque at the Deque.Count - index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the Deque</param>
        public void Insert(int index, TItem item) {  d.Insert(d.Count - index, item); }

        /// <summary>
        /// Removes the Deque item at the index (Deque.Count - 1 - index).
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index) {  d.RemoveAt(d.Count - 1 - index); }
        #endregion
        
        #region IEnumerable implementation
        /// <summary>
        /// Returns a reversed enumerator that iterates through the Deque.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the Deque.</returns>
        public IEnumerator<TItem> GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region Reversed Enumerator class
        private class Enumerator : IEnumerator<TItem>
        {
            /// <summary>
            /// Reversed view to enumerate through.
            /// </summary>
            private readonly Reverse<TItem> _reverse;

            /// <summary>
            /// index if the current element.
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element.
            /// </summary>
            private TItem? _current;

            /// <summary>
            /// Initializes Enumerator to reversely iterate through the Deque
            /// </summary>
            /// <param name="deque">The Deque to iterate through.</param>
            internal Enumerator(Reverse<TItem> reverse)
            {
                _reverse = reverse;
                _index = 0;
                _current = default;
            }

            /// <summary>
            /// Gets the element in the Reverse at the current position of the enumerator.
            /// </summary>
            public TItem Current => _current!;

            object IEnumerator.Current
            {
                get
                {
                    if (_index >= _reverse.Count)
                    {
                        throw new InvalidOperationException();
                    }
                    return Current;
                }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the Reverse.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; 
            /// false if the enumerator has passed the end of the Reverse.</returns>
            public bool MoveNext()
            {
                Reverse<TItem> local = _reverse;
                _reverse.Enumerating = true;
                if (_index < local.Count)
                {
                    _current = local[_index];
                    _index++;
                    return true;
                }
                _reverse.Enumerating = false;
                return false;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the Reverse.
            /// </summary>
            public void Reset()
            {
                _index = 0;
                _current = default;
                _reverse.Enumerating = false;
            }

            #region IDisposable Members
            public void Dispose() { }
            #endregion
        }
        #endregion

    }

}