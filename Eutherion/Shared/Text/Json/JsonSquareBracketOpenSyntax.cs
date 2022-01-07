﻿#region License
/*********************************************************************************
 * JsonSquareBracketOpenSyntax.cs
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

using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a square bracket open syntax node.
    /// </summary>
    public sealed class GreenJsonSquareBracketOpenSyntax : IGreenJsonSymbol
    {
        /// <summary>
        /// Returns the singleton instance.
        /// </summary>
        public static readonly GreenJsonSquareBracketOpenSyntax Value = new GreenJsonSquareBracketOpenSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public int Length => JsonSpecialCharacter.SpecialCharacterLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public JsonSymbolType SymbolType => JsonSymbolType.BracketOpen;

        private GreenJsonSquareBracketOpenSyntax() { }

        IEnumerable<JsonErrorInfo> IGreenJsonSymbol.GetErrors(int startPosition) => EmptyEnumerable<JsonErrorInfo>.Instance;
    }

    /// <summary>
    /// Represents a square bracket open syntax node.
    /// </summary>
    public sealed class JsonSquareBracketOpenSyntax : JsonSyntax, IJsonSymbol
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public JsonListSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonSquareBracketOpenSyntax Green => GreenJsonSquareBracketOpenSyntax.Value;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => 0;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => JsonSpecialCharacter.SpecialCharacterLength;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent;

        internal JsonSquareBracketOpenSyntax(JsonListSyntax parent) => Parent = parent;

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitSquareBracketOpenSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketOpenSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitSquareBracketOpenSyntax(this, arg);
    }
}
