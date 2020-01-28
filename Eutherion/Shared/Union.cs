#region License
/*********************************************************************************
 * Union.cs
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

namespace Eutherion
{
    /// <summary>
    /// Encapsulates a value which can have two different types.
    /// </summary>
    /// <typeparam name="T1">
    /// The first type of the value.
    /// </typeparam>
    /// <typeparam name="T2">
    /// The second type of the value.
    /// </typeparam>
    public abstract class Union<T1, T2>
    {
        private sealed class ValueOfType1 : Union<T1, T2>
        {
            public readonly T1 Value;

            public ValueOfType1(T1 value) => Value = value;

            public override bool IsOption1() => true;

            public override bool IsOption1(out T1 value)
            {
                value = Value;
                return true;
            }

            public override T1 ToOption1() => Value;

            public override void Match(
                Action<T1> whenOption1 = null,
                Action<T2> whenOption2 = null,
                Action otherwise = null)
            {
                if (whenOption1 != null) whenOption1(Value);
                else otherwise?.Invoke();
            }

            public override TResult Match<TResult>(
                Func<T1, TResult> whenOption1 = null,
                Func<T2, TResult> whenOption2 = null,
                Func<TResult> otherwise = null)
                => whenOption1 != null ? whenOption1(Value)
                : otherwise != null ? otherwise()
                : default;
        }

        private sealed class ValueOfType2 : Union<T1, T2>
        {
            public readonly T2 Value;

            public ValueOfType2(T2 value) => Value = value;

            public override bool IsOption2() => true;

            public override bool IsOption2(out T2 value)
            {
                value = Value;
                return true;
            }

            public override T2 ToOption2() => Value;

            public override void Match(
                Action<T1> whenOption1 = null,
                Action<T2> whenOption2 = null,
                Action otherwise = null)
            {
                if (whenOption2 != null) whenOption2(Value);
                else otherwise?.Invoke();
            }

            public override TResult Match<TResult>(
                Func<T1, TResult> whenOption1 = null,
                Func<T2, TResult> whenOption2 = null,
                Func<TResult> otherwise = null)
                => whenOption2 != null ? whenOption2(Value)
                : otherwise != null ? otherwise()
                : default;
        }

        private Union() { }

        /// <summary>
        /// Creates a new <see cref="Union{T1, T2}"/> with a value of the first type.
        /// </summary>
        public static Union<T1, T2> Option1(T1 value) => new ValueOfType1(value);

        /// <summary>
        /// Creates a new <see cref="Union{T1, T2}"/> with a value of the second type.
        /// </summary>
        public static Union<T1, T2> Option2(T2 value) => new ValueOfType2(value);

        public static implicit operator Union<T1, T2>(T1 value) => new ValueOfType1(value);

        public static implicit operator Union<T1, T2>(T2 value) => new ValueOfType2(value);

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2}"/> contains a value of the first type.
        /// </summary>
        /// <returns>
        /// True if this <see cref="Union{T1, T2}"/> contains a value of the first type; otherwise false.
        /// </returns>
        public virtual bool IsOption1() => false;

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2}"/> contains a value of the second type.
        /// </summary>
        /// <returns>
        /// True if this <see cref="Union{T1, T2}"/> contains a value of the second type; otherwise false.
        /// </returns>
        public virtual bool IsOption2() => false;

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2}"/> contains a value of the first type.
        /// </summary>
        /// <param name="value">
        /// The value of the first type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref="Union{T1, T2}"/> contains a value of the first type; otherwise false.
        /// </returns>
        public virtual bool IsOption1(out T1 value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2}"/> contains a value of the second type.
        /// </summary>
        /// <param name="value">
        /// The value of the second type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref="Union{T1, T2}"/> contains a value of the second type; otherwise false.
        /// </returns>
        public virtual bool IsOption2(out T2 value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Casts this <see cref="Union{T1, T2}"/> to a value of the first type.
        /// </summary>
        /// <returns>
        /// The value of the first type.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Occurs when this <see cref="Union{T1, T2}"/> does not contain a value of the first type.
        /// </exception>
        public virtual T1 ToOption1() => throw new InvalidCastException();

        /// <summary>
        /// Casts this <see cref="Union{T1, T2}"/> to a value of the second type.
        /// </summary>
        /// <returns>
        /// The value of the second type.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Occurs when this <see cref="Union{T1, T2}"/> does not contain a value of the second type.
        /// </exception>
        public virtual T2 ToOption2() => throw new InvalidCastException();

        /// <summary>
        /// Invokes an <see cref="Action{T}"/> based on the type of the value.
        /// </summary>
        /// <param name="whenOption1">
        /// The <see cref="Action{T1}"/> to invoke when the value is of the first type.
        /// </param>
        /// <param name="whenOption2">
        /// The <see cref="Action{T2}"/> to invoke when the value is of the second type.
        /// </param>
        /// <param name="otherwise">
        /// The <see cref="Action"/> to invoke if no action is specified for the type of the value.
        /// If <paramref name="whenOption1"/> and <paramref name="whenOption2"/> are given, this parameter is not used.
        /// </param>
        public abstract void Match(
            Action<T1> whenOption1 = null,
            Action<T2> whenOption2 = null,
            Action otherwise = null);

        /// <summary>
        /// Invokes a <see cref="Func{T, TResult}"/> based on the type of the value and returns its result.
        /// </summary>
        /// <typeparam name="TResult">
        /// Type of the value to return.
        /// </typeparam>
        /// <param name="whenOption1">
        /// The <see cref="Func{T1, TResult}"/> to invoke when the value is of the first type.
        /// </param>
        /// <param name="whenOption2">
        /// The <see cref="Func{T2, TResult}"/> to invoke when the value is of the second type.
        /// </param>
        /// <param name="otherwise">
        /// The <see cref="Func{TResult}"/> to invoke if no action is specified for the type of the value.
        /// If <paramref name="whenOption1"/> and <paramref name="whenOption2"/> are given, this parameter is not used.
        /// </param>
        /// <returns>
        /// The result of the invoked <see cref="Func{T, TResult}"/>.
        /// </returns>
        public abstract TResult Match<TResult>(
            Func<T1, TResult> whenOption1 = null,
            Func<T2, TResult> whenOption2 = null,
            Func<TResult> otherwise = null);
    }

    /// <summary>
    /// Encapsulates a value which can have three different types.
    /// </summary>
    /// <typeparam name="T1">
    /// The first type of the value.
    /// </typeparam>
    /// <typeparam name="T2">
    /// The second type of the value.
    /// </typeparam>
    /// <typeparam name="T3">
    /// The third type of the value.
    /// </typeparam>
    public abstract class Union<T1, T2, T3>
    {
        private sealed class ValueOfType1 : Union<T1, T2, T3>
        {
            public readonly T1 Value;

            public ValueOfType1(T1 value) => Value = value;

            public override bool IsOption1() => true;

            public override bool IsOption1(out T1 value)
            {
                value = Value;
                return true;
            }

            public override T1 ToOption1() => Value;

            public override void Match(
                Action<T1> whenOption1 = null,
                Action<T2> whenOption2 = null,
                Action<T3> whenOption3 = null,
                Action otherwise = null)
            {
                if (whenOption1 != null) whenOption1(Value);
                else otherwise?.Invoke();
            }

            public override TResult Match<TResult>(
                Func<T1, TResult> whenOption1 = null,
                Func<T2, TResult> whenOption2 = null,
                Func<T3, TResult> whenOption3 = null,
                Func<TResult> otherwise = null)
                => whenOption1 != null ? whenOption1(Value)
                : otherwise != null ? otherwise()
                : default;
        }

        private sealed class ValueOfType2 : Union<T1, T2, T3>
        {
            public readonly T2 Value;

            public ValueOfType2(T2 value) => Value = value;

            public override bool IsOption2() => true;

            public override bool IsOption2(out T2 value)
            {
                value = Value;
                return true;
            }

            public override T2 ToOption2() => Value;

            public override void Match(
                Action<T1> whenOption1 = null,
                Action<T2> whenOption2 = null,
                Action<T3> whenOption3 = null,
                Action otherwise = null)
            {
                if (whenOption2 != null) whenOption2(Value);
                else otherwise?.Invoke();
            }

            public override TResult Match<TResult>(
                Func<T1, TResult> whenOption1 = null,
                Func<T2, TResult> whenOption2 = null,
                Func<T3, TResult> whenOption3 = null,
                Func<TResult> otherwise = null)
                => whenOption2 != null ? whenOption2(Value)
                : otherwise != null ? otherwise()
                : default;
        }

        private sealed class ValueOfType3 : Union<T1, T2, T3>
        {
            public readonly T3 Value;

            public ValueOfType3(T3 value) => Value = value;

            public override bool IsOption3() => true;

            public override bool IsOption3(out T3 value)
            {
                value = Value;
                return true;
            }

            public override T3 ToOption3() => Value;

            public override void Match(
                Action<T1> whenOption1 = null,
                Action<T2> whenOption2 = null,
                Action<T3> whenOption3 = null,
                Action otherwise = null)
            {
                if (whenOption3 != null) whenOption3(Value);
                else otherwise?.Invoke();
            }

            public override TResult Match<TResult>(
                Func<T1, TResult> whenOption1 = null,
                Func<T2, TResult> whenOption2 = null,
                Func<T3, TResult> whenOption3 = null,
                Func<TResult> otherwise = null)
                => whenOption3 != null ? whenOption3(Value)
                : otherwise != null ? otherwise()
                : default;
        }

        private Union() { }

        /// <summary>
        /// Creates a new <see cref="Union{T1, T2, T3}"/> with a value of the first type.
        /// </summary>
        public static Union<T1, T2, T3> Option1(T1 value) => new ValueOfType1(value);

        /// <summary>
        /// Creates a new <see cref="Union{T1, T2, T3}"/> with a value of the second type.
        /// </summary>
        public static Union<T1, T2, T3> Option2(T2 value) => new ValueOfType2(value);

        /// <summary>
        /// Creates a new <see cref="Union{T1, T2, T3}"/> with a value of the third type.
        /// </summary>
        public static Union<T1, T2, T3> Option3(T3 value) => new ValueOfType3(value);

        public static implicit operator Union<T1, T2, T3>(T1 value) => new ValueOfType1(value);

        public static implicit operator Union<T1, T2, T3>(T2 value) => new ValueOfType2(value);

        public static implicit operator Union<T1, T2, T3>(T3 value) => new ValueOfType3(value);

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3}"/> contains a value of the first type.
        /// </summary>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3}"/> contains a value of the first type; otherwise false.
        /// </returns>
        public virtual bool IsOption1() => false;

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3}"/> contains a value of the second type.
        /// </summary>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3}"/> contains a value of the second type; otherwise false.
        /// </returns>
        public virtual bool IsOption2() => false;

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3}"/> contains a value of the third type.
        /// </summary>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3}"/> contains a value of the third type; otherwise false.
        /// </returns>
        public virtual bool IsOption3() => false;

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3}"/> contains a value of the first type.
        /// </summary>
        /// <param name="value">
        /// The value of the first type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3}"/> contains a value of the first type; otherwise false.
        /// </returns>
        public virtual bool IsOption1(out T1 value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3}"/> contains a value of the second type.
        /// </summary>
        /// <param name="value">
        /// The value of the second type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3}"/> contains a value of the second type; otherwise false.
        /// </returns>
        public virtual bool IsOption2(out T2 value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3}"/> contains a value of the third type.
        /// </summary>
        /// <param name="value">
        /// The value of the third type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3}"/> contains a value of the third type; otherwise false.
        /// </returns>
        public virtual bool IsOption3(out T3 value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Casts this <see cref="Union{T1, T2, T3}"/> to a value of the first type.
        /// </summary>
        /// <returns>
        /// The value of the first type.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Occurs when this <see cref="Union{T1, T2, T3}"/> does not contain a value of the first type.
        /// </exception>
        public virtual T1 ToOption1() => throw new InvalidCastException();

        /// <summary>
        /// Casts this <see cref="Union{T1, T2, T3}"/> to a value of the second type.
        /// </summary>
        /// <returns>
        /// The value of the second type.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Occurs when this <see cref="Union{T1, T2, T3}"/> does not contain a value of the second type.
        /// </exception>
        public virtual T2 ToOption2() => throw new InvalidCastException();

        /// <summary>
        /// Casts this <see cref="Union{T1, T2, T3}"/> to a value of the third type.
        /// </summary>
        /// <returns>
        /// The value of the third type.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Occurs when this <see cref="Union{T1, T2, T3}"/> does not contain a value of the third type.
        /// </exception>
        public virtual T3 ToOption3() => throw new InvalidCastException();

        /// <summary>
        /// Invokes an <see cref="Action{T}"/> based on the type of the value.
        /// </summary>
        /// <param name="whenOption1">
        /// The <see cref="Action{T1}"/> to invoke when the value is of the first type.
        /// </param>
        /// <param name="whenOption2">
        /// The <see cref="Action{T2}"/> to invoke when the value is of the second type.
        /// </param>
        /// <param name="whenOption3">
        /// The <see cref="Action{T3}"/> to invoke when the value is of the third type.
        /// </param>
        /// <param name="otherwise">
        /// The <see cref="Action"/> to invoke if no action is specified for the type of the value.
        /// If <paramref name="whenOption1"/>, <paramref name="whenOption2"/> and <paramref name="whenOption3"/> are given, this parameter is not used.
        /// </param>
        public abstract void Match(
            Action<T1> whenOption1 = null,
            Action<T2> whenOption2 = null,
            Action<T3> whenOption3 = null,
            Action otherwise = null);

        /// <summary>
        /// Invokes a <see cref="Func{T, TResult}"/> based on the type of the value and returns its result.
        /// </summary>
        /// <typeparam name="TResult">
        /// Type of the value to return.
        /// </typeparam>
        /// <param name="whenOption1">
        /// The <see cref="Func{T1, TResult}"/> to invoke when the value is of the first type.
        /// </param>
        /// <param name="whenOption2">
        /// The <see cref="Func{T2, TResult}"/> to invoke when the value is of the second type.
        /// </param>
        /// <param name="whenOption3">
        /// The <see cref="Func{T3, TResult}"/> to invoke when the value is of the third type.
        /// </param>
        /// <param name="otherwise">
        /// The <see cref="Func{TResult}"/> to invoke if no action is specified for the type of the value.
        /// If <paramref name="whenOption1"/>, <paramref name="whenOption2"/> and <paramref name="whenOption3"/> are given, this parameter is not used.
        /// </param>
        /// <returns>
        /// The result of the invoked <see cref="Func{T, TResult}"/>.
        /// </returns>
        public abstract TResult Match<TResult>(
            Func<T1, TResult> whenOption1 = null,
            Func<T2, TResult> whenOption2 = null,
            Func<T3, TResult> whenOption3 = null,
            Func<TResult> otherwise = null);
    }

    /// <summary>
    /// Encapsulates a value which can have four different types.
    /// </summary>
    /// <typeparam name="T1">
    /// The first type of the value.
    /// </typeparam>
    /// <typeparam name="T2">
    /// The second type of the value.
    /// </typeparam>
    /// <typeparam name="T3">
    /// The third type of the value.
    /// </typeparam>
    /// <typeparam name="T4">
    /// The fourth type of the value.
    /// </typeparam>
    public abstract class Union<T1, T2, T3, T4>
    {
        private sealed class ValueOfType1 : Union<T1, T2, T3, T4>
        {
            public readonly T1 Value;

            public ValueOfType1(T1 value) => Value = value;

            public override bool IsOption1() => true;

            public override bool IsOption1(out T1 value)
            {
                value = Value;
                return true;
            }

            public override T1 ToOption1() => Value;

            public override void Match(
                Action<T1> whenOption1 = null,
                Action<T2> whenOption2 = null,
                Action<T3> whenOption3 = null,
                Action<T4> whenOption4 = null,
                Action otherwise = null)
            {
                if (whenOption1 != null) whenOption1(Value);
                else otherwise?.Invoke();
            }

            public override TResult Match<TResult>(
                Func<T1, TResult> whenOption1 = null,
                Func<T2, TResult> whenOption2 = null,
                Func<T3, TResult> whenOption3 = null,
                Func<T4, TResult> whenOption4 = null,
                Func<TResult> otherwise = null)
                => whenOption1 != null ? whenOption1(Value)
                : otherwise != null ? otherwise()
                : default;
        }

        private sealed class ValueOfType2 : Union<T1, T2, T3, T4>
        {
            public readonly T2 Value;

            public ValueOfType2(T2 value) => Value = value;

            public override bool IsOption2() => true;

            public override bool IsOption2(out T2 value)
            {
                value = Value;
                return true;
            }

            public override T2 ToOption2() => Value;

            public override void Match(
                Action<T1> whenOption1 = null,
                Action<T2> whenOption2 = null,
                Action<T3> whenOption3 = null,
                Action<T4> whenOption4 = null,
                Action otherwise = null)
            {
                if (whenOption2 != null) whenOption2(Value);
                else otherwise?.Invoke();
            }

            public override TResult Match<TResult>(
                Func<T1, TResult> whenOption1 = null,
                Func<T2, TResult> whenOption2 = null,
                Func<T3, TResult> whenOption3 = null,
                Func<T4, TResult> whenOption4 = null,
                Func<TResult> otherwise = null)
                => whenOption2 != null ? whenOption2(Value)
                : otherwise != null ? otherwise()
                : default;
        }

        private sealed class ValueOfType3 : Union<T1, T2, T3, T4>
        {
            public readonly T3 Value;

            public ValueOfType3(T3 value) => Value = value;

            public override bool IsOption3() => true;

            public override bool IsOption3(out T3 value)
            {
                value = Value;
                return true;
            }

            public override T3 ToOption3() => Value;

            public override void Match(
                Action<T1> whenOption1 = null,
                Action<T2> whenOption2 = null,
                Action<T3> whenOption3 = null,
                Action<T4> whenOption4 = null,
                Action otherwise = null)
            {
                if (whenOption3 != null) whenOption3(Value);
                else otherwise?.Invoke();
            }

            public override TResult Match<TResult>(
                Func<T1, TResult> whenOption1 = null,
                Func<T2, TResult> whenOption2 = null,
                Func<T3, TResult> whenOption3 = null,
                Func<T4, TResult> whenOption4 = null,
                Func<TResult> otherwise = null)
                => whenOption3 != null ? whenOption3(Value)
                : otherwise != null ? otherwise()
                : default;
        }

        private sealed class ValueOfType4 : Union<T1, T2, T3, T4>
        {
            public readonly T4 Value;

            public ValueOfType4(T4 value) => Value = value;

            public override bool IsOption4() => true;

            public override bool IsOption4(out T4 value)
            {
                value = Value;
                return true;
            }

            public override T4 ToOption4() => Value;

            public override void Match(
                Action<T1> whenOption1 = null,
                Action<T2> whenOption2 = null,
                Action<T3> whenOption3 = null,
                Action<T4> whenOption4 = null,
                Action otherwise = null)
            {
                if (whenOption4 != null) whenOption4(Value);
                else otherwise?.Invoke();
            }

            public override TResult Match<TResult>(
                Func<T1, TResult> whenOption1 = null,
                Func<T2, TResult> whenOption2 = null,
                Func<T3, TResult> whenOption3 = null,
                Func<T4, TResult> whenOption4 = null,
                Func<TResult> otherwise = null)
                => whenOption4 != null ? whenOption4(Value)
                : otherwise != null ? otherwise()
                : default;
        }

        private Union() { }

        /// <summary>
        /// Creates a new <see cref="Union{T1, T2, T3, T4}"/> with a value of the first type.
        /// </summary>
        public static Union<T1, T2, T3, T4> Option1(T1 value) => new ValueOfType1(value);

        /// <summary>
        /// Creates a new <see cref="Union{T1, T2, T3, T4}"/> with a value of the second type.
        /// </summary>
        public static Union<T1, T2, T3, T4> Option2(T2 value) => new ValueOfType2(value);

        /// <summary>
        /// Creates a new <see cref="Union{T1, T2, T3, T4}"/> with a value of the third type.
        /// </summary>
        public static Union<T1, T2, T3, T4> Option3(T3 value) => new ValueOfType3(value);

        /// <summary>
        /// Creates a new <see cref="Union{T1, T2, T3, T4}"/> with a value of the fourth type.
        /// </summary>
        public static Union<T1, T2, T3, T4> Option4(T4 value) => new ValueOfType4(value);

        public static implicit operator Union<T1, T2, T3, T4>(T1 value) => new ValueOfType1(value);

        public static implicit operator Union<T1, T2, T3, T4>(T2 value) => new ValueOfType2(value);

        public static implicit operator Union<T1, T2, T3, T4>(T3 value) => new ValueOfType3(value);

        public static implicit operator Union<T1, T2, T3, T4>(T4 value) => new ValueOfType4(value);

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the first type.
        /// </summary>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the first type; otherwise false.
        /// </returns>
        public virtual bool IsOption1() => false;

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the second type.
        /// </summary>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the second type; otherwise false.
        /// </returns>
        public virtual bool IsOption2() => false;

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the third type.
        /// </summary>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the third type; otherwise false.
        /// </returns>
        public virtual bool IsOption3() => false;

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the fourth type.
        /// </summary>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the fourth type; otherwise false.
        /// </returns>
        public virtual bool IsOption4() => false;

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the first type.
        /// </summary>
        /// <param name="value">
        /// The value of the first type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the first type; otherwise false.
        /// </returns>
        public virtual bool IsOption1(out T1 value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the second type.
        /// </summary>
        /// <param name="value">
        /// The value of the second type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the second type; otherwise false.
        /// </returns>
        public virtual bool IsOption2(out T2 value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the third type.
        /// </summary>
        /// <param name="value">
        /// The value of the third type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the third type; otherwise false.
        /// </returns>
        public virtual bool IsOption3(out T3 value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Checks if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the fourth type.
        /// </summary>
        /// <param name="value">
        /// The value of the fourth type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref="Union{T1, T2, T3, T4}"/> contains a value of the fourth type; otherwise false.
        /// </returns>
        public virtual bool IsOption4(out T4 value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Casts this <see cref="Union{T1, T2, T3, T4}"/> to a value of the first type.
        /// </summary>
        /// <returns>
        /// The value of the first type.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Occurs when this <see cref="Union{T1, T2, T3, T4}"/> does not contain a value of the first type.
        /// </exception>
        public virtual T1 ToOption1() => throw new InvalidCastException();

        /// <summary>
        /// Casts this <see cref="Union{T1, T2, T3, T4}"/> to a value of the second type.
        /// </summary>
        /// <returns>
        /// The value of the second type.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Occurs when this <see cref="Union{T1, T2, T3, T4}"/> does not contain a value of the second type.
        /// </exception>
        public virtual T2 ToOption2() => throw new InvalidCastException();

        /// <summary>
        /// Casts this <see cref="Union{T1, T2, T3, T4}"/> to a value of the third type.
        /// </summary>
        /// <returns>
        /// The value of the third type.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Occurs when this <see cref="Union{T1, T2, T3, T4}"/> does not contain a value of the third type.
        /// </exception>
        public virtual T3 ToOption3() => throw new InvalidCastException();

        /// <summary>
        /// Casts this <see cref="Union{T1, T2, T3, T4}"/> to a value of the fourth type.
        /// </summary>
        /// <returns>
        /// The value of the fourth type.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Occurs when this <see cref="Union{T1, T2, T3, T4}"/> does not contain a value of the fourth type.
        /// </exception>
        public virtual T4 ToOption4() => throw new InvalidCastException();

        /// <summary>
        /// Invokes an <see cref="Action{T}"/> based on the type of the value.
        /// </summary>
        /// <param name="whenOption1">
        /// The <see cref="Action{T1}"/> to invoke when the value is of the first type.
        /// </param>
        /// <param name="whenOption2">
        /// The <see cref="Action{T2}"/> to invoke when the value is of the second type.
        /// </param>
        /// <param name="whenOption3">
        /// The <see cref="Action{T3}"/> to invoke when the value is of the third type.
        /// </param>
        /// <param name="whenOption4">
        /// The <see cref="Action{T4}"/> to invoke when the value is of the fourth type.
        /// </param>
        /// <param name="otherwise">
        /// The <see cref="Action"/> to invoke if no action is specified for the type of the value.
        /// If <paramref name="whenOption1"/>, <paramref name="whenOption2"/>, <paramref name="whenOption3"/> and <paramref name="whenOption4"/> are given, this parameter is not used.
        /// </param>
        public abstract void Match(
            Action<T1> whenOption1 = null,
            Action<T2> whenOption2 = null,
            Action<T3> whenOption3 = null,
            Action<T4> whenOption4 = null,
            Action otherwise = null);

        /// <summary>
        /// Invokes a <see cref="Func{T, TResult}"/> based on the type of the value and returns its result.
        /// </summary>
        /// <typeparam name="TResult">
        /// Type of the value to return.
        /// </typeparam>
        /// <param name="whenOption1">
        /// The <see cref="Func{T1, TResult}"/> to invoke when the value is of the first type.
        /// </param>
        /// <param name="whenOption2">
        /// The <see cref="Func{T2, TResult}"/> to invoke when the value is of the second type.
        /// </param>
        /// <param name="whenOption3">
        /// The <see cref="Func{T3, TResult}"/> to invoke when the value is of the third type.
        /// </param>
        /// <param name="whenOption4">
        /// The <see cref="Func{T4, TResult}"/> to invoke when the value is of the fourth type.
        /// </param>
        /// <param name="otherwise">
        /// The <see cref="Func{TResult}"/> to invoke if no action is specified for the type of the value.
        /// If <paramref name="whenOption1"/>, <paramref name="whenOption2"/>, <paramref name="whenOption3"/> and <paramref name="whenOption4"/> are given, this parameter is not used.
        /// </param>
        /// <returns>
        /// The result of the invoked <see cref="Func{T, TResult}"/>.
        /// </returns>
        public abstract TResult Match<TResult>(
            Func<T1, TResult> whenOption1 = null,
            Func<T2, TResult> whenOption2 = null,
            Func<T3, TResult> whenOption3 = null,
            Func<T4, TResult> whenOption4 = null,
            Func<TResult> otherwise = null);
    }
}
