#region License
/*********************************************************************************
 * SpecializedEnumerable.cs
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

namespace Eutherion.Utils
{
    /// <summary>
    /// Utility <see cref="IEnumerable{T}"/> that enumerates no elements of the given type.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the enumerated elements.
    /// </typeparam>
    public struct EmptyEnumerable<TResult> : IEnumerable<TResult>
    {
        /// <summary>
        /// Returns an <see cref="EmptyEnumerator{TResult}"/> which enumerates no elements of the given type.
        /// </summary>
        public IEnumerator<TResult> GetEnumerator() => EmptyEnumerator<TResult>.Instance;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Utility <see cref="IEnumerator{T}"/> that enumerates no elements of the given type.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the enumerated elements.
    /// </typeparam>
    public class EmptyEnumerator<TResult> : IEnumerator<TResult>
    {
        /// <summary>
        /// Gets the only <see cref="EmptyEnumerator{TResult}"/> instance.
        /// </summary>
        public static readonly EmptyEnumerator<TResult> Instance = new EmptyEnumerator<TResult>();

        private EmptyEnumerator() { }

        void IEnumerator.Reset() { }
        bool IEnumerator.MoveNext() => false;
        TResult IEnumerator<TResult>.Current => default;
        object IEnumerator.Current => default;
        void IDisposable.Dispose() { }
    }

    /// <summary>
    /// Utility <see cref="IEnumerable{T}"/> that enumerates a single element of the given type.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the enumerated elements.
    /// </typeparam>
    public struct SingleElementEnumerable<TResult> : IEnumerable<TResult>
    {
        /// <summary>
        /// Gets or sets the element to enumerate.
        /// </summary>
        public TResult Element;

        /// <summary>
        /// Initializes a new instance of <see cref="SingleElementEnumerable{TResult}"/>.
        /// </summary>
        /// <param name="element">
        /// The single element to enumerate.
        /// </param>
        public SingleElementEnumerable(TResult element) => Element = element;

        /// <summary>
        /// Returns a <see cref="SingleElementEnumerator{TResult}"/> which enumerates <see cref="Element"/>.
        /// </summary>
        public IEnumerator<TResult> GetEnumerator() => new SingleElementEnumerator<TResult>(Element);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Utility <see cref="IEnumerator{T}"/> that enumerates a single element of the given type.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the enumerated elements.
    /// </typeparam>
    public class SingleElementEnumerator<TResult> : IEnumerator<TResult>
    {
        private readonly TResult element;
        private bool yielded;

        /// <summary>
        /// Initializes a new instance of <see cref="SingleElementEnumerator{TResult}"/>.
        /// </summary>
        /// <param name="element">
        /// The single element to enumerate.
        /// </param>
        public SingleElementEnumerator(TResult element) => this.element = element;

        // When called from a foreach construct, it goes:
        // MoveNext() Current MoveNext() Dispose()
        void IEnumerator.Reset() => yielded = false;
        bool IEnumerator.MoveNext() { if (yielded) return false; yielded = true; return true; }
        TResult IEnumerator<TResult>.Current => element;
        object IEnumerator.Current => element;
        void IDisposable.Dispose() { }
    }
}
