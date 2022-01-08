#region License
/*********************************************************************************
 * JsonMissingValueSyntax.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a missing value. It has a length of 0.
    /// </summary>
    public sealed class GreenJsonMissingValueSyntax : GreenJsonValueSyntax
    {
        /// <summary>
        /// Returns the singleton instance.
        /// </summary>
        public static readonly GreenJsonMissingValueSyntax Value = new GreenJsonMissingValueSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => 0;

        private GreenJsonMissingValueSyntax() { }

        internal override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMissingValueSyntax(this, arg);
    }

    /// <summary>
    /// Represents a missing value. It has a length of 0.
    /// </summary>
    public sealed class JsonMissingValueSyntax : JsonValueSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonMissingValueSyntax Green => GreenJsonMissingValueSyntax.Value;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => 0;

        internal JsonMissingValueSyntax(JsonValueWithBackgroundSyntax parent) : base(parent) { }

        internal override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMissingValueSyntax(this, arg);
    }
}
