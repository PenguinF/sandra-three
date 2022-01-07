#region License
/*********************************************************************************
 * IJsonSymbol.cs
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
    /// Represents a terminal JSON symbol.
    /// </summary>
    public interface IGreenJsonSymbol : ISpan
    {
        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        JsonSymbolType SymbolType { get; }
    }

    /// <summary>
    /// Represents a terminal JSON symbol.
    /// These are all <see cref="JsonSyntax"/> nodes which have no child <see cref="JsonSyntax"/> nodes.
    /// Use <see cref="JsonSymbolVisitor{T, TResult}"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public interface IJsonSymbol : ISpan
    {
        /// <summary>
        /// This method is for internal use only.
        /// </summary>
        TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg);
    }

    /// <summary>
    /// Contains extension methods for the <see cref="IJsonSymbol"/> interface.
    /// </summary>
    public static class JsonSymbolExtensions
    {
        private sealed class ToJsonSyntaxConverter : JsonSymbolVisitor<JsonSyntax>
        {
            public static readonly ToJsonSyntaxConverter Instance = new ToJsonSyntaxConverter();

            private ToJsonSyntaxConverter() { }

            public override JsonSyntax VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node, _void arg) => node;
            public override JsonSyntax VisitColonSyntax(JsonColonSyntax node, _void arg) => node;
            public override JsonSyntax VisitCommaSyntax(JsonCommaSyntax node, _void arg) => node;
            public override JsonSyntax VisitCommentSyntax(JsonCommentSyntax node, _void arg) => node;
            public override JsonSyntax VisitCurlyCloseSyntax(JsonCurlyCloseSyntax node, _void arg) => node;
            public override JsonSyntax VisitCurlyOpenSyntax(JsonCurlyOpenSyntax node, _void arg) => node;
            public override JsonSyntax VisitErrorStringSyntax(JsonErrorStringSyntax node, _void arg) => node;
            public override JsonSyntax VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node, _void arg) => node;
            public override JsonSyntax VisitRootLevelValueDelimiterSyntax(JsonRootLevelValueDelimiterSyntax node, _void arg) => node;
            public override JsonSyntax VisitSquareBracketCloseSyntax(JsonSquareBracketCloseSyntax node, _void arg) => node;
            public override JsonSyntax VisitSquareBracketOpenSyntax(JsonSquareBracketOpenSyntax node, _void arg) => node;
            public override JsonSyntax VisitStringLiteralSyntax(JsonStringLiteralSyntax node, _void arg) => node;
            public override JsonSyntax VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node, _void arg) => node;
            public override JsonSyntax VisitUnknownSymbolSyntax(JsonUnknownSymbolSyntax node, _void arg) => node;
            public override JsonSyntax VisitUnterminatedMultiLineCommentSyntax(JsonUnterminatedMultiLineCommentSyntax node, _void arg) => node;
            public override JsonSyntax VisitWhitespaceSyntax(JsonWhitespaceSyntax node, _void arg) => node;
        }

        /// <summary>
        /// Converts this <see cref="IJsonSymbol"/> to a <see cref="JsonSyntax"/> node.
        /// </summary>
        /// <param name="symbol">
        /// The <see cref="IJsonSymbol"/> to convert.
        /// </param>
        /// <returns>
        /// The converted <see cref="JsonSyntax"/> node.
        /// </returns>
        public static JsonSyntax ToSyntax(this IJsonSymbol symbol) => ToJsonSyntaxConverter.Instance.Visit(symbol);
    }
}
