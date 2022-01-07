#region License
/*********************************************************************************
 * JsonUnknownSymbolSyntax.cs
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
    /// Represents a syntax node with an unknown symbol.
    /// </summary>
    public sealed class GreenJsonUnknownSymbolSyntax : GreenJsonValueSyntax, IGreenJsonSymbol
    {
        /// <summary>
        /// Returns the singleton instance.
        /// </summary>
        public static readonly GreenJsonUnknownSymbolSyntax Value = new GreenJsonUnknownSymbolSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => JsonUnknownSymbolSyntax.UnknownSymbolLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public JsonSymbolType SymbolType => JsonSymbolType.UnknownSymbol;

        private GreenJsonUnknownSymbolSyntax() { }

        internal override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUnknownSymbolSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node with an unknown symbol.
    /// </summary>
    public sealed class JsonUnknownSymbolSyntax : JsonValueSyntax, IJsonSymbol
    {
        /// <summary>
        /// Returns 1, which is always the length of an unknown symbol.
        /// </summary>
        public const int UnknownSymbolLength = 1;

        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for unexpected symbol characters.
        /// </summary>
        /// <param name="displayCharValue">
        /// A friendly representation of the unknown symbol.
        /// </param>
        /// <param name="startPosition">
        /// The start position for which to create the error.
        /// </param>
        /// <returns>
        /// The new <see cref="JsonErrorInfo"/>.
        /// </returns>
        public static JsonErrorInfo CreateError(string displayCharValue, int startPosition)
            => new JsonErrorInfo(JsonErrorCode.UnexpectedSymbol, startPosition, UnknownSymbolLength, new[] { displayCharValue });

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonUnknownSymbolSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => UnknownSymbolLength;

        internal JsonUnknownSymbolSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonUnknownSymbolSyntax green) : base(parent) => Green = green;

        internal override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUnknownSymbolSyntax(this, arg);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitUnknownSymbolSyntax(this, arg);
    }
}
