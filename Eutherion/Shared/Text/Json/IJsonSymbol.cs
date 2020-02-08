#region License
/*********************************************************************************
 * IJsonSymbol.cs
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

using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a terminal json symbol.
    /// Instances of this type are returned by <see cref="JsonTokenizer"/>.
    /// </summary>
    public interface IGreenJsonSymbol : ISpan
    {
        /// <summary>
        /// Generates a sequence of errors associated with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the errors.
        /// </param>
        /// <returns>
        /// A sequence of errors associated with this symbol.
        /// </returns>
        IEnumerable<JsonErrorInfo> GetErrors(int startPosition);

        /// <summary>
        /// Converts this symbol into either a <see cref="GreenJsonBackgroundSyntax"/> or a <see cref="IJsonForegroundSymbol"/>.
        /// </summary>
        /// <returns>
        /// Either a <see cref="GreenJsonBackgroundSyntax"/> or a <see cref="IJsonForegroundSymbol"/>.
        /// </returns>
        Union<GreenJsonBackgroundSyntax, IJsonForegroundSymbol> AsBackgroundOrForeground();
    }

    /// <summary>
    /// Represents a terminal json symbol.
    /// These are all <see cref="JsonSyntax"/> nodes which have no child <see cref="JsonSyntax"/> nodes.
    /// Use <see cref="JsonSymbolVisitor"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public interface IJsonSymbol : ISpan
    {
        void Accept(JsonSymbolVisitor visitor);
        TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor);
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

            public override JsonSyntax VisitBackgroundListSyntax(JsonBackgroundListSyntax node) => node;
            public override JsonSyntax VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node) => node;
            public override JsonSyntax VisitColonSyntax(JsonColonSyntax node) => node;
            public override JsonSyntax VisitCommaSyntax(JsonCommaSyntax node) => node;
            public override JsonSyntax VisitCurlyCloseSyntax(JsonCurlyCloseSyntax node) => node;
            public override JsonSyntax VisitCurlyOpenSyntax(JsonCurlyOpenSyntax node) => node;
            public override JsonSyntax VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node) => node;
            public override JsonSyntax VisitMissingValueSyntax(JsonMissingValueSyntax node) => node;
            public override JsonSyntax VisitSquareBracketCloseSyntax(JsonSquareBracketCloseSyntax node) => node;
            public override JsonSyntax VisitSquareBracketOpenSyntax(JsonSquareBracketOpenSyntax node) => node;
            public override JsonSyntax VisitStringLiteralSyntax(JsonStringLiteralSyntax node) => node;
            public override JsonSyntax VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node) => node;
        }

        /// <summary>
        /// Converts this <see cref="IJsonSymbol"/> to a <see cref="JsonSyntax"/> node.
        /// </summary>
        /// <param name="jsonSymbol">
        /// The <see cref="IJsonSymbol"/> to convert.
        /// </param>
        /// <returns>
        /// The converted <see cref="JsonSyntax"/> node.
        /// </returns>
        public static JsonSyntax ToSyntax(this IJsonSymbol jsonSymbol) => jsonSymbol.Accept(ToJsonSyntaxConverter.Instance);
    }
}
