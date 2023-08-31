#region License
/*********************************************************************************
 * PValue.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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

using System.Numerics;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Base interface for all types of values which are persistable.
    /// </summary>
#pragma warning disable IDE1006 // Interface not starting with 'I' is intentional.
    public interface PValue
#pragma warning restore IDE1006
    {
        void Accept(PValueVisitor visitor);
        TResult Accept<TResult>(PValueVisitor<TResult> visitor);
        TResult Accept<T, TResult>(PValueVisitor<T, TResult> visitor, T arg);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="PValue"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PValueVisitor
    {
        public virtual void DefaultVisit(PValue value) { }
        public virtual void Visit(PValue value) { if (value != null) value.Accept(this); }
        public virtual void VisitBoolean(PBoolean value) => DefaultVisit(value);
        public virtual void VisitInteger(PInteger value) => DefaultVisit(value);
        public virtual void VisitList(PList value) => DefaultVisit(value);
        public virtual void VisitMap(PMap value) => DefaultVisit(value);
        public virtual void VisitString(PString value) => DefaultVisit(value);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="PValue"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PValueVisitor<TResult>
    {
        public virtual TResult DefaultVisit(PValue value) => default;
        public virtual TResult Visit(PValue value) => value == null ? default : value.Accept(this);
        public virtual TResult VisitBoolean(PBoolean value) => DefaultVisit(value);
        public virtual TResult VisitInteger(PInteger value) => DefaultVisit(value);
        public virtual TResult VisitList(PList value) => DefaultVisit(value);
        public virtual TResult VisitMap(PMap value) => DefaultVisit(value);
        public virtual TResult VisitString(PString value) => DefaultVisit(value);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="PValue"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PValueVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(PValue value, T arg) => default;
        public virtual TResult Visit(PValue value, T arg) => value == null ? default : value.Accept(this, arg);
        public virtual TResult VisitBoolean(PBoolean value, T arg) => DefaultVisit(value, arg);
        public virtual TResult VisitInteger(PInteger value, T arg) => DefaultVisit(value, arg);
        public virtual TResult VisitList(PList value, T arg) => DefaultVisit(value, arg);
        public virtual TResult VisitMap(PMap value, T arg) => DefaultVisit(value, arg);
        public virtual TResult VisitString(PString value, T arg) => DefaultVisit(value, arg);
    }

    /// <summary>
    /// Represents a <see cref="bool"/> persistent value.
    /// </summary>
    public struct PBoolean : PValue
    {
        public bool Value;

        public PBoolean(bool value) => Value = value;

        void PValue.Accept(PValueVisitor visitor) => visitor.VisitBoolean(this);
        TResult PValue.Accept<TResult>(PValueVisitor<TResult> visitor) => visitor.VisitBoolean(this);
        TResult PValue.Accept<T, TResult>(PValueVisitor<T, TResult> visitor, T arg) => visitor.VisitBoolean(this, arg);
    }

    /// <summary>
    /// Represents a <see cref="BigInteger"/> persistent value.
    /// </summary>
    public struct PInteger : PValue
    {
        public BigInteger Value;

        public PInteger(BigInteger value) => Value = value;

        void PValue.Accept(PValueVisitor visitor) => visitor.VisitInteger(this);
        TResult PValue.Accept<TResult>(PValueVisitor<TResult> visitor) => visitor.VisitInteger(this);
        TResult PValue.Accept<T, TResult>(PValueVisitor<T, TResult> visitor, T arg) => visitor.VisitInteger(this, arg);
    }

    /// <summary>
    /// Represents a <see cref="string"/> persistent value.
    /// </summary>
    public struct PString : PValue
    {
        public string Value;

        public PString(string value) => Value = value;

        void PValue.Accept(PValueVisitor visitor) => visitor.VisitString(this);
        TResult PValue.Accept<TResult>(PValueVisitor<TResult> visitor) => visitor.VisitString(this);
        TResult PValue.Accept<T, TResult>(PValueVisitor<T, TResult> visitor, T arg) => visitor.VisitString(this, arg);
    }

    /// <summary>
    /// Contains constant a number of useful <see cref="PValue"/>s.
    /// </summary>
    public static class PConstantValue
    {
        /// <summary>
        /// Gets the false <see cref="PValue"/>.
        /// </summary>
        public static readonly PBoolean False = new PBoolean(false);

        /// <summary>
        /// Gets the true <see cref="PValue"/>.
        /// </summary>
        public static readonly PBoolean True = new PBoolean(true);
    }

    /// <summary>
    /// Determines equality of two <see cref="PValue"/>s.
    /// </summary>
    public sealed class PValueEqualityComparer : PValueVisitor<bool>
    {
        public static readonly PValueEqualityComparer Instance = new PValueEqualityComparer();

        private PValueEqualityComparer() { }

        /// <summary>
        /// Returns if two <see cref="PValue"/>s are equal.
        /// </summary>
        /// <param name="x">
        /// The first <see cref="PValue"/>.
        /// </param>
        /// <param name="y">
        /// The second <see cref="PValue"/>.
        /// </param>
        /// <returns>
        /// Whether or not both <see cref="PValue"/>s are equal.
        /// </returns>
        public bool AreEqual(PValue x, PValue y)
        {
            // Equality of null values.
            if (x == null) return y == null;

            // If not null, types must match exactly.
            if (y == null || x.GetType() != y.GetType()) return false;

            // Only call visit after knowing that both types are exactly the same.
            compareValue = y;
            return Visit(x);
        }

        private PValue compareValue;

        public override bool VisitBoolean(PBoolean value)
            => value.Value == ((PBoolean)compareValue).Value;

        public override bool VisitInteger(PInteger value)
            => value.Value == ((PInteger)compareValue).Value;

        public override bool VisitList(PList value)
            => value.EqualTo((PList)compareValue);

        public override bool VisitMap(PMap value)
            => value.EqualTo((PMap)compareValue);

        public override bool VisitString(PString value)
            => value.Value == ((PString)compareValue).Value;
    }
}
