using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Reflection;

namespace My.System.Generic
{

    /// <summary>
    /// Data structure commonly known as Deque (double-ended queue). 
    /// Base principle of this data structure is the same as for the regular FIFO queue, 
    /// but Deque also supports addition and removal of elements from both
    /// ends. The importnant feature is that all 4 of these operations (addition/removal
    /// of elements from the beginning/end) must be done in amortized constant time.
    /// </summary>
    /// <typeparam name="TItem">Specifies the element type of the linked list.</typeparam>
    public partial class Deque<TItem> : ICollection<TItem>, IEnumerable<TItem>, IList<TItem>
    {

        /// <summary>
        /// Default number of Data Blocks in Deque without resizing
        /// </summary>
        private const int DefaultCount = 4;

        /// <summary>
        /// Coefficient which is used for resizing Deque
        /// </summary>
        private const int ResizeCoefficient = 2;
        
        /// <summary>
        /// List that holds Data Blocks
        /// </summary>
        internal List<Block> _blocks;

        /// <summary>
        /// Actual Blocks number (default value equals <typeparamref name="DefaultCount"/>)
        /// </summary>
        internal int _blocksCount;

        /// <summary>
        /// Flag that specifies, that Deque is being enumerated, therefore can not be modified
        /// </summary>
        internal bool _enumeratingNow = false;

        /// <summary>
        /// Internal index of the front end of Deque.
        /// </summary>
        private int _beginning;

        /// <summary>
        /// Internal index of the back end of Deque. 
        /// </summary>
        private int _end;
        
        /// <summary>
        /// Actual number of elements of type TItem stored.
        /// </summary>
        private int _count;

        /// <summary>
        /// Initializes a new instance of the Deque class that
        /// is empty and has the default initial capacity.
        /// </summary>
        public Deque()
        {
            _end = Block.Capacity * 2;
            _beginning = _end - 1;
            _count = 0;
            _blocks = new List<Block>() { new Block(), new Block(), new Block(), new Block()};
            _blocksCount = DefaultCount;
        }

        #region IEnumerable implementation
        /// <summary>
        /// Returns an enumerator that iterates through the Deque.
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

        #region ICollection implementation
        /// <summary>
        /// Read-only property describing how many elements are in the Deque.
        /// </summary>
        public int Count 
        { 
            get
            {
                return _count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Deque is read-only. 
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an item to the Deque.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="InvalidOperationException"></exception>        
        public void Add(TItem item)
        {
            if (_enumeratingNow) throw new InvalidOperationException();

            AddLast(item);
            _count++;
        }

        /// <summary>
        /// Clears the contents of Deque.
        /// </summary>
        public void Clear()
        {
            _blocks.Clear();
            _blocks = new();
            for (int i = 0; i < DefaultCount; i++) { _blocks.Add(new Block()); }
            _count = 0;
        }

        /// <summary>
        /// Determines whether the Deque contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the Deque.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(TItem item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (item.Equals(this[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Copies the elements of the Deque<typeparamref name="TItem"/> to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException("array");
            if (arrayIndex <  0) throw new ArgumentOutOfRangeException("index"); 

            int end = Math.Min(array.Length, _count);
            for (int i  = 0; i < end; i++)
            {
                array[i+arrayIndex] = this[i];
            }
        }


        /// <summary>
        /// Removes the first occurrence of a specific object from the Deque.
        /// </summary>
        /// <param name="item">The object to remove from the Deque.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException">The Deque is read-only.</exception>
        public bool Remove(TItem item)
        {
            if (_enumeratingNow) throw new InvalidOperationException();
            if (IsReadOnly) throw new NotSupportedException();
            if (!Contains(item)) return false;
            RemoveAt(IndexOf(item));

            return true;
        }

        #endregion

        #region IList implementation
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">The Deque is read only.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index is not a valid index in the Deque.</exception>
        public TItem this[int index] //user idx
        {
            get
            {
                if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException();
                int blockNumber = (_beginning + index + 1) / Block.Capacity;
                int itemIdxInBlock = (_beginning + index + 1) % Block.Capacity;
                return _blocks[blockNumber][itemIdxInBlock];
            }
            set
            {
                if (IsReadOnly) throw new NotSupportedException();
                Block block = _blocks[(_beginning + index + 1) / Block.Capacity];
                int itemIdxInBlock = (_beginning + index + 1) % Block.Capacity;
                block._items[itemIdxInBlock] = value;

            }
        }

        /// <summary>
        /// Determines the index of a specific item in the Deque.
        /// </summary>
        /// <param name="item">The object to locate in the Deque.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(TItem item) //returns user idx
        {
            for (int i = 0; i < _count; i++)
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
        /// Inserts an item to the Deque at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="item">The object to insert into the Deque.</param>
        /// <exception cref="NotSupportedException">The Deque is read-only.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index is not a valid index in the Deque.</exception>
        public void Insert(int index, TItem item) //user idx
        {
            if (IsReadOnly) { throw new NotSupportedException(); }
            if (index < 0 || index >= (_end - _beginning)) { throw new ArgumentOutOfRangeException("index"); } // ???? souvisly ?

            if (index == _count)
            {
                this.AddLast(item);
            }
            else if (index == 0)
            {
                this.AddFront(item);
            }
            else this.AddWithShift(index, item);
            _count++;
        }

        /// <summary>
        /// Removes the Deque item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="InvalidOperationException">The Deque is being enumerated now.</exception>
        /// <exception cref="NotSupportedException">The Deque is read-only.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index is not a valid index in the Deque.</exception>
        public void RemoveAt(int index) //user idx
        {
            if (_enumeratingNow) throw new InvalidOperationException();
            if (IsReadOnly) { throw new NotSupportedException(); }
            if (index < 0 || index > (_end - _beginning)) { throw new ArgumentOutOfRangeException("index"); }
            if (index == _count - 1)
            {
                this.RemoveLast();
            }
            else if (index == 0)
            {
                this.RemoveFirst();
            }
            else RemoveWithShift(index);
            _count--;
        }

        #endregion
        
        /// <summary>
        /// Adds to the front _blocksCount/2 blocks and to the end _blocksCount/2 blocks
        /// </summary>
        private void Resize()
        {
            int addBlocks = _blocksCount / ResizeCoefficient;
            List<Block> newBlocks = new();
            for (int i = 0; i < addBlocks; i++)
            {
                newBlocks.Add(new Block());
            }
            for (int i = 0; i < _blocksCount; i++)
            {
                newBlocks.Add(_blocks[i]);
            }
            for (int i = 0; i < addBlocks; i++)
            {
                newBlocks.Add(new Block());
            }
            _blocks = newBlocks;
            _blocksCount *= ResizeCoefficient;
            _beginning +=  Block.Capacity * addBlocks;
            _end += Block.Capacity * addBlocks;
        }

        /// <summary>
        /// Adds an item to the end of the Deque. Amortized O(1)
        /// </summary>
        /// <param name="item"></param>
        private void AddLast(TItem item)
        {
            if (_end == Block.Capacity * _blocksCount) 
            { 
                Resize(); 
            }
            this[_count] = item;
            _end++;
        }

        /// <summary>
        /// Adds an item to the front of the Deque. Amortized O(1)
        /// </summary>
        /// <param name="item"></param>
        private void AddFront(TItem item)
        {
            if (_beginning < 0) { Resize(); }
            Block block = _blocks[_beginning / Block.Capacity];
            int index = _beginning % Block.Capacity;
            block[index] = item;
            _beginning--;
        }

        /// <summary>
        /// Adds an item to the middle of the Deque. O(n).
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the IList.</param>
        private void AddWithShift(int index, TItem item) // user idx
        {
            TItem nextElement;
            TItem currentElement = item;
            int next = index + 1;
            for (int i = index; i < _count; i++)
            {
                nextElement = this[i];
                this[i] = currentElement;
                currentElement = nextElement;
            }
            //for last element
            AddLast(currentElement);
        }

        /// <summary>
        /// Removes item from the end of tge Deque. 0(1).
        /// </summary>
        private void RemoveLast()
        {
            this[_count - 1] = default;
            _end--;
        }

        /// <summary>
        /// Removes item from the front of tge Deque. 0(1).
        /// </summary>
        private void RemoveFirst()
        {
            this[0] = default;
            _beginning++;
        }

        /// <summary>
        /// Removes an item at the index from the middle of the Deque. O(n).
        /// </summary>
        /// <param name="index"></param>
        private void RemoveWithShift(int index) //user idx
        {
            for (int i = index; i < _count - 1; i++)
            {
                int next = i + 1;
                TItem newVal = this[next];
                this[i] = newVal;
            }
            RemoveLast();
        }
    }
}