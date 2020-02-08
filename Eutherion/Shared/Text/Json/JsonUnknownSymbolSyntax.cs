#region License
/*********************************************************************************
 * JsonUnknownSymbolSyntax.cs
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
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a json syntax node with an unknown symbol.
    /// </summary>
    public sealed class GreenJsonUnknownSymbolSyntax : IJsonValueStarterSymbol
    {
        /// <summary>
        /// Gets a friendly representation of the unknown symbol.
        /// </summary>
        public string DisplayCharValue { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => JsonUnknownSymbolSyntax.UnknownSymbolLength;

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

        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for this syntax node.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to create the error.
        /// </param>
        /// <returns>
        /// The new <see cref="JsonErrorInfo"/>.
        /// </returns>
        public JsonErrorInfo GetError(int startPosition) => JsonUnknownSymbolSyntax.CreateError(DisplayCharValue, startPosition);

        IEnumerable<JsonErrorInfo> IGreenJsonSymbol.GetErrors(int startPosition) => new SingleElementEnumerable<JsonErrorInfo>(GetError(startPosition));
        Union<GreenJsonBackgroundSyntax, IJsonForegroundSymbol> IGreenJsonSymbol.AsBackgroundOrForeground() => this;
        Union<IJsonValueDelimiterSymbol, IJsonValueStarterSymbol> IJsonForegroundSymbol.AsValueDelimiterOrStarter() => this;

        void IJsonValueStarterSymbol.Accept(JsonValueStarterSymbolVisitor visitor) => visitor.VisitUnknownSymbolSyntax(this);
        TResult IJsonValueStarterSymbol.Accept<TResult>(JsonValueStarterSymbolVisitor<TResult> visitor) => visitor.VisitUnknownSymbolSyntax(this);
        TResult IJsonValueStarterSymbol.Accept<T, TResult>(JsonValueStarterSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitUnknownSymbolSyntax(this, arg);
    }

    public static class JsonUnknownSymbolSyntax
    {
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
    }
}
