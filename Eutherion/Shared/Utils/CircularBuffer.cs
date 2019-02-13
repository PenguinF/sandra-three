#region License
/*********************************************************************************
 * CircularBuffer.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
**********************************************************************************/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eutherion.Utils
{
    /// <summary>
    /// Represents a buffer of items with a maximum capacity.
    /// </summary>
    /// <typeparam name="TItem">
    /// Specifies the type of items in the buffer.
    /// </typeparam>
    [DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(CircularBufferDebugView<>))]
    public class CircularBuffer<TItem> : IReadOnlyList<TItem>
    {
        private readonly List<TItem> list;

        private int lastAddedItemIndex;

        private int maximumCapacity;

        /// <summary>
        /// Gets or sets the maximum capacity of this <see cref="CircularBuffer{T}"/>.
        /// The value of this property cannot be zero or less.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The new value is zero or less.
        /// </exception>
        public int MaximumCapacity
        {
            get => maximumCapacity;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(MaximumCapacity),
                        value,
                        $"The value of {nameof(MaximumCapacity)} cannot be zero or less.");
                }

                // Discard oldest items if maximumCapacity decreased.

                // Example 1: MaxCapacity goes from 7 to 3. list.Count == 5 and lastAddedItemIndex == 4.
                //    A B C D E . .
                // =>     C D E
                // numberOfItemsToDiscard == 2, tailItemsToDiscard == 0, headItemsToDiscard == 2, lastAddedItemIndex′ == 2

                // Example 2: MaxCapacity goes from 7 to 3. list.Count == 7 and lastAddedItemIndex == 5.
                //    B C D E F G A
                // =>       E F G
                // numberOfItemsToDiscard == 4, tailItemsToDiscard == 1, headItemsToDiscard == 3, lastAddedItemIndex′ == 2

                // Example 3: MaxCapacity goes from 7 to 3. list.Count == 7 and lastAddedItemIndex == 1.
                //    F G A B C D E
                // => F G E
                // numberOfItemsToDiscard == 4, tailItemsToDiscard == 4, headItemsToDiscard == 0, lastAddedItemIndex′ == 1

                int count = Count;
                int numberOfItemsToDiscard = count - value;
                if (numberOfItemsToDiscard > 0)
                {
                    // Calculate how many items at the head and the tail of the list must be discarded.
                    int tailItemsToDiscard = count - lastAddedItemIndex - 1;
                    if (tailItemsToDiscard > numberOfItemsToDiscard) tailItemsToDiscard = numberOfItemsToDiscard;
                    int headItemsToDiscard = numberOfItemsToDiscard - tailItemsToDiscard;

                    if (tailItemsToDiscard > 0) list.RemoveRange(lastAddedItemIndex + 1, tailItemsToDiscard);
                    if (headItemsToDiscard > 0) list.RemoveRange(0, headItemsToDiscard);

                    lastAddedItemIndex -= headItemsToDiscard;
                }

                // Update list.Capacity as well to reduce memory allocations.
                list.Capacity = value;
                maximumCapacity = value;
            }
        }

        /// <summary>
        /// Gets the number of items in this <see cref="CircularBuffer{T}"/>.
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// Gets the element at the specified index in the <see cref="CircularBuffer{T}"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get.
        /// </param>
        /// <returns>
        /// The element at the specified index in the read-only list.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/> is outside the bounds of this <see cref="CircularBuffer{T}"/>.
        /// </exception>
        public TItem this[int index] => list[MapIndex(index)];

        /// <summary>
        /// Initializes a new instance of <see cref="CircularBuffer{T}"/> with a maximum capacity.
        /// </summary>
        /// <param name="maximumCapacity">
        /// The initial maximum capacity. The value of this parameter cannot be zero or less.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maximumCapacity"/> is zero or less.
        /// </exception>
        public CircularBuffer(int maximumCapacity)
        {
            lastAddedItemIndex = -1;
            list = new List<TItem>(maximumCapacity);
            MaximumCapacity = maximumCapacity;
        }

        // Maps a semantic index onto an actual index in the list.
        private int MapIndex(int index)
        {
            int invertedIndex = lastAddedItemIndex - index;
            return invertedIndex < 0 ? invertedIndex + Count : invertedIndex;
        }

        /// <summary>
        /// Enumerates through the <see cref="CircularBuffer{T}"/> from newest to oldest items.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> for the <see cref="CircularBuffer{T}"/>.
        /// </returns>
        public IEnumerator<TItem> GetEnumerator()
        {
            int count = Count;

            if (count == 0) yield break;

            int invertedIndex = lastAddedItemIndex;
            for (; ; )
            {
                yield return list[invertedIndex];
                invertedIndex--;
                if (invertedIndex < 0) invertedIndex = count - 1;
                if (invertedIndex == lastAddedItemIndex) yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Adds an item to the start of the <see cref="CircularBuffer{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The item to add.
        /// </param>
        public void Add(TItem item)
        {
            lastAddedItemIndex++;

            int count = Count;
            if (MaximumCapacity == count)
            {
                // Replace oldest item with newest if the maximum capacity is reached.
                if (lastAddedItemIndex == count) lastAddedItemIndex = 0;
                list[lastAddedItemIndex] = item;
            }
            else
            {
                // Add because this avoids Array.Copy().
                list.Add(item);
            }
        }
    }

    /// <summary>
    /// Debug viewer for <see cref="CircularBuffer{T}"/>.
    /// </summary>
    internal sealed class CircularBufferDebugView<T>
    {
        private readonly CircularBuffer<T> buffer;

        public CircularBufferDebugView(CircularBuffer<T> buffer)
            => this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Values
        {
            get
            {
                T[] array = new T[buffer.Count];
                int index = 0;
                foreach (T value in buffer) array[index++] = value;
                return array;
            }
        }
    }
}
