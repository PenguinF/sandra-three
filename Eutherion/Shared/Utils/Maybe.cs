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

using System;

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

            public override void Match(
                Action whenNothing,
                Action<T> whenJust)
                => whenNothing?.Invoke();

            public override TResult Match<TResult>(
                Func<TResult> whenNothing,
                Func<T, TResult> whenJust)
                => whenNothing != null ? whenNothing() : default;

            public override Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> whenJust)
                => Maybe<TResult>.Nothing;
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

            public override void Match(
                Action whenNothing,
                Action<T> whenJust)
                => whenJust?.Invoke(Value);

            public override TResult Match<TResult>(
                Func<TResult> whenNothing,
                Func<T, TResult> whenJust)
                => whenJust != null ? whenJust(Value) : default;

            public override Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> whenJust)
                => whenJust(Value);
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

        public abstract void Match(
            Action whenNothing,
            Action<T> whenJust);

        public abstract TResult Match<TResult>(
            Func<TResult> whenNothing,
            Func<T, TResult> whenJust);

        /// <summary>
        /// If this <see cref="Maybe{T}"/> contains a value, applies a function to it
        /// to return a <see cref="Maybe{T}"/> of another target type.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result.
        /// </typeparam>
        /// <param name="whenJust">
        /// The function to apply if this <see cref="Maybe{T}"/> contains a value.
        /// </param>
        /// <returns>
        /// The <see cref="Maybe{T}"/> of the result type.
        /// </returns>
        /// <exception cref="NullReferenceException">
        /// <paramref name="whenJust"/> is null.
        /// </exception>
        public abstract Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> whenJust);

        /// <summary>
        /// If this <see cref="Maybe{T}"/> contains a value, applies a function to it
        /// to return a <see cref="Maybe{T}"/> of another target type.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result.
        /// </typeparam>
        /// <param name="whenJust">
        /// The function to apply if this <see cref="Maybe{T}"/> contains a value.
        /// </param>
        /// <returns>
        /// The <see cref="Maybe{T}"/> of the result type.
        /// </returns>
        /// <exception cref="NullReferenceException">
        /// <paramref name="whenJust"/> is null.
        /// </exception>
        public Maybe<TResult> Select<TResult>(Func<T, TResult> whenJust)
            => Bind(sourceValue => Maybe<TResult>.Just(whenJust(sourceValue)));

        /// <summary>
        /// If this <see cref="Maybe{T}"/> contains a value, applies two functions to it
        /// to return a flattened <see cref="Maybe{T}"/> of another target type, i.e.
        /// a <see cref="Maybe{T}"/> which does not wrap another <see cref="Maybe{T}"/>.
        /// </summary>
        /// <typeparam name="TIntermediate">
        /// The type of the wrapped intermediate value returned by <paramref name="whenJust"/>.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The type of the result.
        /// </typeparam>
        /// <param name="whenJust">
        /// The function to apply if this <see cref="Maybe{T}"/> contains a value.
        /// </param>
        /// <param name="resultSelector">
        /// The function to apply to flatten the result.
        /// </param>
        /// <returns>
        /// The flattened <see cref="Maybe{T}"/> of the result type.
        /// </returns>
        /// <exception cref="NullReferenceException">
        /// <paramref name="whenJust"/> and/or <paramref name="resultSelector"/> are null.
        /// </exception>
        public Maybe<TResult> SelectMany<TIntermediate, TResult>(
            Func<T, Maybe<TIntermediate>> whenJust,
            Func<T, TIntermediate, TResult> resultSelector)
            => Bind(sourceValue => whenJust(sourceValue).Bind(intermediateValue => Maybe<TResult>.Just(resultSelector(sourceValue, intermediateValue))));
    }
}
