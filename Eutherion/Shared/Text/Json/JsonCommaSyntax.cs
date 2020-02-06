#region License
/*********************************************************************************
 * JsonCommaSyntax.cs
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
    public sealed class JsonComma : JsonForegroundSymbol
    {
        public const char CommaCharacter = ',';
        public const int CommaLength = 1;

        public static readonly JsonComma Value = new JsonComma();

        public override int Length => CommaLength;

        private JsonComma() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitComma(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitComma(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitComma(this, arg);
    }

    /// <summary>
    /// Represents a json comma syntax node.
    /// </summary>
    public sealed class JsonCommaSyntax : JsonSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public Union<JsonListSyntax, JsonMapSyntax> Parent { get; }

        /// <summary>
        /// Gets the index of this comma in the comma collection of its parent.
        /// </summary>
        public int CommaIndex { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public JsonComma Green => JsonComma.Value;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Match(
            whenOption1: listSyntax => JsonSquareBracketOpen.SquareBracketOpenLength + listSyntax.Green.ListItemNodes.GetSeparatorOffset(CommaIndex),
            whenOption2: mapSyntax => JsonCurlyOpen.CurlyOpenLength + mapSyntax.Green.KeyValueNodes.GetSeparatorOffset(CommaIndex));

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => JsonComma.CommaLength;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent.Match<JsonSyntax>(
            whenOption1: x => x,
            whenOption2: x => x);

        internal JsonCommaSyntax(JsonListSyntax parent, int commaIndex)
        {
            Parent = parent;
            CommaIndex = commaIndex;
        }

        internal JsonCommaSyntax(JsonMapSyntax parent, int commaIndex)
        {
            Parent = parent;
            CommaIndex = commaIndex;
        }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitComma(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitComma(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitComma(this, arg);
    }
}
