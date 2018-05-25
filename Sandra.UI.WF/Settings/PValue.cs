/*********************************************************************************
 * PValue.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
using System.Numerics;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Base interface for all types of values which are persistable.
    /// </summary>
    public interface PValue
    {
        void Accept(PValueVisitor visitor);
        TResult Accept<TResult>(PValueVisitor<TResult> visitor);
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
        public virtual void VisitString(PString value) => DefaultVisit(value);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="PValue"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PValueVisitor<TResult>
    {
        public virtual TResult DefaultVisit(PValue value) => default(TResult);
        public virtual TResult Visit(PValue value) => value == null ? default(TResult) : value.Accept(this);
        public virtual TResult VisitBoolean(PBoolean value) => DefaultVisit(value);
        public virtual TResult VisitInteger(PInteger value) => DefaultVisit(value);
        public virtual TResult VisitString(PString value) => DefaultVisit(value);
    }

    /// <summary>
    /// Represents a <see cref="bool"/> setting value.
    /// </summary>
    public struct PBoolean : PValue
    {
        public bool Value;

        public PBoolean(bool value) { Value = value; }

        public void Accept(PValueVisitor visitor) => visitor.VisitBoolean(this);
        public TResult Accept<TResult>(PValueVisitor<TResult> visitor) => visitor.VisitBoolean(this);
    }

    /// <summary>
    /// Represents a <see cref="BigInteger"/> setting value.
    /// </summary>
    public struct PInteger : PValue
    {
        public BigInteger Value;

        public PInteger(BigInteger value) { Value = value; }

        public void Accept(PValueVisitor visitor) => visitor.VisitInteger(this);
        public TResult Accept<TResult>(PValueVisitor<TResult> visitor) => visitor.VisitInteger(this);
    }

    /// <summary>
    /// Represents a <see cref="string"/> setting value.
    /// </summary>
    public struct PString : PValue
    {
        public string Value;

        public PString(string value) { Value = value; }

        public void Accept(PValueVisitor visitor) => visitor.VisitString(this);
        public TResult Accept<TResult>(PValueVisitor<TResult> visitor) => visitor.VisitString(this);
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

            // Only call init/visit after knowing that both types are exactly the same.
            return init(y).Visit(x);
        }

        private PValue compareValue;

        private PValueEqualityComparer init(PValue compareValue)
        {
            this.compareValue = compareValue;
            return this;
        }

        public override bool VisitBoolean(PBoolean value)
            => value.Value == ((PBoolean)compareValue).Value;

        public override bool VisitInteger(PInteger value)
            => value.Value == ((PInteger)compareValue).Value;

        public override bool VisitString(PString value)
            => value.Value == ((PString)compareValue).Value;
    }
}
