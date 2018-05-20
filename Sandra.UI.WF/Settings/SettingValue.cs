/*********************************************************************************
 * SettingValue.cs
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
namespace Sandra.UI.WF
{
    /// <summary>
    /// Base interface for all types of values which are allowed in settings.
    /// </summary>
    /// <remarks>
    /// Not using <see cref="object"/> because this impacts change tracking.
    /// </remarks>
    public interface ISettingValue
    {
        void Accept(SettingValueVisitor visitor);
        TResult Accept<TResult>(SettingValueVisitor<TResult> visitor);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="ISettingValue"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class SettingValueVisitor
    {
        public virtual void DefaultVisit(ISettingValue value) { }
        public virtual void Visit(ISettingValue value) { if (value != null) value.Accept(this); }
        public virtual void VisitBoolean(BooleanSettingValue value) => DefaultVisit(value);
        public virtual void VisitInt32(Int32SettingValue value) => DefaultVisit(value);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="ISettingValue"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class SettingValueVisitor<TResult>
    {
        public virtual TResult DefaultVisit(ISettingValue value) => default(TResult);
        public virtual TResult Visit(ISettingValue value) => value == null ? default(TResult) : value.Accept(this);
        public virtual TResult VisitBoolean(BooleanSettingValue value) => DefaultVisit(value);
        public virtual TResult VisitInt32(Int32SettingValue value) => DefaultVisit(value);
    }

    /// <summary>
    /// Represents a <see cref="bool"/> setting value.
    /// </summary>
    public struct BooleanSettingValue : ISettingValue
    {
        public bool Value;

        public void Accept(SettingValueVisitor visitor) => visitor.VisitBoolean(this);
        public TResult Accept<TResult>(SettingValueVisitor<TResult> visitor) => visitor.VisitBoolean(this);
    }

    /// <summary>
    /// Represents a <see cref="int"/> setting value.
    /// </summary>
    public struct Int32SettingValue : ISettingValue
    {
        public int Value;

        public void Accept(SettingValueVisitor visitor) => visitor.VisitInt32(this);
        public TResult Accept<TResult>(SettingValueVisitor<TResult> visitor) => visitor.VisitInt32(this);
    }

    /// <summary>
    /// Determines equality of two <see cref="ISettingValue"/>s.
    /// </summary>
    public sealed class SettingValueEqualityComparer : SettingValueVisitor<bool>
    {
        public static readonly SettingValueEqualityComparer Instance = new SettingValueEqualityComparer();

        private SettingValueEqualityComparer() { }

        public static bool AreEqual(ISettingValue x, ISettingValue y)
        {
            // Equality of null values.
            if (x == null) return y == null;

            // If not null, types must match exactly.
            if (y == null || x.GetType() != y.GetType()) return false;

            // Only call init/visit after knowing that both types are exactly the same.
            return Instance.init(y).Visit(x);
        }

        private ISettingValue compareValue;

        private SettingValueEqualityComparer init(ISettingValue compareValue)
        {
            this.compareValue = compareValue;
            return this;
        }

        public override bool VisitBoolean(BooleanSettingValue value)
            => value.Value == ((BooleanSettingValue)compareValue).Value;

        public override bool VisitInt32(Int32SettingValue value)
            => value.Value == ((Int32SettingValue)compareValue).Value;
    }
}
