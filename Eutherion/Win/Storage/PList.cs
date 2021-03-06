﻿#region License
/*********************************************************************************
 * PList.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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
using System.Linq;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Represents a read-only list of <see cref="PValue"/>s.
    /// </summary>
    public class PList : IReadOnlyList<PValue>, PValue
    {
        private readonly PValue[] array;

        /// <summary>
        /// Initializes a new instance of <see cref="PList"/>.
        /// </summary>
        /// <param name="list">
        /// The list which contains the values to construct this <see cref="PList"/> with.
        /// </param>
        public PList(IEnumerable<PValue> list) => array = list.ToArrayEx();

        /// <summary>
        /// Gets the <see cref="PValue"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index where to locate the <see cref="PValue"/>.
        /// </param>
        /// <returns>
        /// The <see cref="PValue"/> at the specified index.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/> is lower than 0, or greater than or equal to <see cref="Count"/>.
        /// </exception>
        public PValue this[int index] => array[index];

        /// <summary>
        /// Gets the number of elements in the <see cref="PList"/>.
        /// </summary>
        public int Count => array.Length;

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="PList"/>.
        /// </summary>
        /// <returns>
        /// An enumerator that iterates through the <see cref="PList"/>.
        /// </returns>
        public IEnumerator<PValue> GetEnumerator() => ((ICollection<PValue>)array).GetEnumerator();

        /// <summary>
        /// Compares this <see cref="PList"/> with another and returns if they are equal.
        /// </summary>
        /// <param name="other">
        /// The <see cref="PList"/> to compare with.
        /// </param>
        /// <returns>
        /// true if both <see cref="PList"/> instances are equal; otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="other"/> is null.
        /// </exception>
        public bool EqualTo(PList other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            if (array.Length != other.array.Length) return false;

            PValueEqualityComparer eq = PValueEqualityComparer.Instance;
            for (int i = 0; i < array.Length; ++i)
            {
                if (!eq.AreEqual(array[i], other.array[i])) return false;
            }
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void PValue.Accept(PValueVisitor visitor) => visitor.VisitList(this);
        TResult PValue.Accept<TResult>(PValueVisitor<TResult> visitor) => visitor.VisitList(this);
        TResult PValue.Accept<T, TResult>(PValueVisitor<T, TResult> visitor, T arg) => visitor.VisitList(this, arg);
    }
}
