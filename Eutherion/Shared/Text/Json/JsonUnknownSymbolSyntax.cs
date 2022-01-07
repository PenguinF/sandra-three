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

using System;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a syntax node with an unknown symbol.
    /// </summary>
    public sealed class GreenJsonUnknownSymbolSyntax : GreenJsonValueSyntax, IGreenJsonSymbol
    {
        /// <summary>
        /// Gets a friendly representation of the unknown symbol.
        /// </summary>
        public string DisplayCharValue { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => JsonUnknownSymbolSyntax.UnknownSymbolLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public JsonSymbolType SymbolType => JsonSymbolType.UnknownSymbol;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonUnknownSymbolSyntax"/>.
        /// </summary>
        /// <param name="displayCharValue">
        /// A friendly representation of the unknown symbol.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="displayCharValue"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="displayCharValue"/> is empty.
        /// </exception>
        public GreenJsonUnknownSymbolSyntax(string displayCharValue)
        {
            if (displayCharValue == null) throw new ArgumentNullException(nameof(displayCharValue));
            if (displayCharValue.Length == 0) throw new ArgumentException($"{nameof(displayCharValue)} should be non-empty", nameof(displayCharValue));

            DisplayCharValue = displayCharValue;
        }

        public override void Accept(GreenJsonValueSyntaxVisitor visitor) => visitor.VisitUnknownSymbolSyntax(this);
        public override TResult Accept<TResult>(GreenJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitUnknownSymbolSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUnknownSymbolSyntax(this, arg);
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
        public override int Length => Green.Length;

        internal JsonUnknownSymbolSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonUnknownSymbolSyntax green) : base(parent) => Green = green;

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitUnknownSymbolSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitUnknownSymbolSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUnknownSymbolSyntax(this, arg);

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitUnknownSymbolSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitUnknownSymbolSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitUnknownSymbolSyntax(this, arg);
    }
}
