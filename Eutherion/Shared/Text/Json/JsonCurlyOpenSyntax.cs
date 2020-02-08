#region License
/*********************************************************************************
 * JsonCurlyOpenSyntax.cs
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
    /// Represents a json curly open syntax node.
    /// </summary>
    public sealed class GreenJsonCurlyOpenSyntax : JsonForegroundSymbol
    {
        public static readonly GreenJsonCurlyOpenSyntax Value = new GreenJsonCurlyOpenSyntax();

        public override bool IsValueStartSymbol => true;
        public override int Length => JsonCurlyOpenSyntax.CurlyOpenLength;

        private GreenJsonCurlyOpenSyntax() { }

        public override void Accept(JsonForegroundSymbolVisitor visitor) => visitor.VisitCurlyOpenSyntax(this);
        public override TResult Accept<TResult>(JsonForegroundSymbolVisitor<TResult> visitor) => visitor.VisitCurlyOpenSyntax(this);
        public override TResult Accept<T, TResult>(JsonForegroundSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitCurlyOpenSyntax(this, arg);
    }

    /// <summary>
    /// Represents a json curly open syntax node.
    /// </summary>
    public sealed class JsonCurlyOpenSyntax : JsonSyntax
    {
        public const char CurlyOpenCharacter = '{';
        public const int CurlyOpenLength = 1;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public JsonMapSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonCurlyOpenSyntax Green => GreenJsonCurlyOpenSyntax.Value;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => 0;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => CurlyOpenLength;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent;

        internal JsonCurlyOpenSyntax(JsonMapSyntax parent) => Parent = parent;

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitCurlyOpenSyntax(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitCurlyOpenSyntax(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitCurlyOpenSyntax(this, arg);
    }
}
