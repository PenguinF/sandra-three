#region License
/*********************************************************************************
 * Maybe.cs
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

namespace Eutherion.Utils
{
    /// <summary>
    /// Represents an optional value.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the optional value.
    /// </typeparam>
    public abstract class Maybe<T>
    {
        private sealed class NothingValue : Maybe<T>
        {
            public override bool IsNothing => true;

            public override bool IsJust(out T value)
            {
                value = default;
                return false;
            }
        }

        private sealed class JustValue : Maybe<T>
        {
            public readonly T Value;

            public JustValue(T value) => Value = value;

            public override bool IsNothing => false;

            public override bool IsJust(out T value)
            {
                value = Value;
                return true;
            }
        }

        /// <summary>
        /// Represents the <see cref="Maybe{T}"/> without a value.
        /// </summary>
        public static readonly Maybe<T> Nothing = new NothingValue();

        /// <summary>
        /// Creates a <see cref="Maybe{T}"/> instance which contains a value.
        /// </summary>
        /// <param name="value">
        /// The value to wrap.
        /// </param>
        /// <returns>
        /// The <see cref="Maybe{T}"/> which contains the value.
        /// </returns>
        public static Maybe<T> Just(T value) => new JustValue(value);

        public static implicit operator Maybe<T>(T value) => new JustValue(value);

        private Maybe() { }

        /// <summary>
        /// Returns if this <see cref="Maybe{T}"/> is empty, i.e. does not contain a value.
        /// </summary>
        public abstract bool IsNothing { get; }

        /// <summary>
        /// Returns if this <see cref="Maybe{T}"/> contains a value, and if it does, returns it.
        /// </summary>
        /// <param name="value">
        /// The value contained in this <see cref="Maybe{T}"/>, or a default value if empty.
        /// </param>
        /// <returns>
        /// Whether or not this <see cref="Maybe{T}"/> contains a value.
        /// </returns>
        public abstract bool IsJust(out T value);
    }
}
