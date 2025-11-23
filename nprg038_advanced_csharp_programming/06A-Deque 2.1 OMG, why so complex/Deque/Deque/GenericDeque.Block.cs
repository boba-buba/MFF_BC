using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace My.System.Generic {
    public partial class Deque<TItem> : ICollection<TItem>, IEnumerable<TItem>, IList<TItem>
    {
        #region Block 
        /// <summary>
        /// Class that represents Data Blocks Deque consists of.
        /// </summary>
        internal class Block
        {
            /// <summary>
            /// Total number of elements the internal data structure holds.
            /// </summary>
            private const int _capacity = 64;

            /// <summary>
            /// Gets the total number of elements the internal data structure holds.
            /// </summary>
            public static int Capacity => _capacity;
            /// <summary>
            /// Array taht holds actual elements.
            /// </summary>
            internal TItem[] _items = new TItem[_capacity];

            /// <summary>
            /// Gets or sets the element at the specified index.
            /// </summary>
            /// <param name="index"> The zero-based index of the element to get or set. </param>
            /// <returns></returns>
            public TItem this[int index]
            {
                get { return _items[index]; }
                set { _items[index] = value;}
            }
        }
        #endregion
    }
}