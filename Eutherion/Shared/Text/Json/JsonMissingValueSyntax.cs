#region License
/*********************************************************************************
 * JsonMissingValueSyntax.cs
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a missing value in a list. It has a length of 0.
    /// </summary>
    public sealed class GreenJsonMissingValueSyntax : GreenJsonValueSyntax
    {
        public static readonly GreenJsonMissingValueSyntax Value = new GreenJsonMissingValueSyntax();

        public override int Length => 0;

        private GreenJsonMissingValueSyntax() { }

        public override void Accept(GreenJsonValueSyntaxVisitor visitor) => visitor.VisitMissingValueSyntax(this);
        public override TResult Accept<TResult>(GreenJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitMissingValueSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMissingValueSyntax(this, arg);
    }

    /// <summary>
    /// Represents a json missing value syntax node. It has a length of 0.
    /// </summary>
    public sealed class JsonMissingValueSyntax : JsonValueSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonMissingValueSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => 0;

        internal JsonMissingValueSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonMissingValueSyntax green) : base(parent) => Green = green;

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitMissingValueSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitMissingValueSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMissingValueSyntax(this, arg);

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitMissingValueSyntax(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitMissingValueSyntax(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitMissingValueSyntax(this, arg);
    }
}
