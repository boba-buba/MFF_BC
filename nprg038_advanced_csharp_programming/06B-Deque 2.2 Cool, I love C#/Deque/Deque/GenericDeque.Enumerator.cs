using System;
using System.Collections;
using System.Collections.Generic;

namespace My.System.Generic
{
    public partial class Deque<TItem> : ICollection<TItem>, IEnumerable<TItem>, IList<TItem>
    {
        #region Enumerator Class
        /// <summary>
        /// Supports a simple iteration over a non-generic collection.
        /// </summary>
        private class Enumerator : IEnumerator<TItem> 
        {
            /// <summary>
            /// The owner Deque of the enumerator
            /// </summary>
            private readonly Deque<TItem> _deque;

            /// <summary>
            /// Index of the current element
            /// </summary>
            private int _index;
            
            /// <summary>
            /// Current element
            /// </summary>
            private TItem? _current;

            /// <summary>
            /// Initializes Enumerator to iterate through the Deque
            /// </summary>
            /// <param name="deque">The Deque to iterate through.</param>
            internal Enumerator(Deque<TItem> deque)
            {
                _deque = deque;
                _index = 0;
                _current = default;
            }

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            public TItem Current => _current!;

            object IEnumerator.Current {
                get {
                    if (_index >= _deque.Count)
                    {
                        throw new InvalidOperationException();
                    }
                    return Current;
                }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; 
            /// false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                Deque<TItem> local = _deque;
                _deque._enumeratingNow = true;
                if (_index < local.Count)
                {
                    _current = local[_index];
                    _index++;
                    return true;
                }
                _deque._enumeratingNow = false;
                return false;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _index = 0;
                _current = default;
                _deque._enumeratingNow = false;
            }

            #region IDisposable Members
            public void Dispose() { }
            #endregion
        }
        #endregion
    }


}