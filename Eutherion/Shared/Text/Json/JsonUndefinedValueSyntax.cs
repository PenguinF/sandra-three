#region License
/*********************************************************************************
 * JsonUndefinedValueSyntax.cs
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
    /// Represents a syntax node with an undefined or unsupported value.
    /// </summary>
    public sealed class GreenJsonUndefinedValueSyntax : GreenJsonValueSyntax, IGreenJsonSymbol
    {
        public string UndefinedValue { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => UndefinedValue.Length;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public JsonSymbolType SymbolType => JsonSymbolType.UndefinedValue;

        public GreenJsonUndefinedValueSyntax(string undefinedValue) => UndefinedValue = undefinedValue ?? throw new ArgumentNullException(nameof(undefinedValue));

        internal override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUndefinedValueSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node with an undefined or unsupported value.
    /// </summary>
    public sealed class JsonUndefinedValueSyntax : JsonValueSyntax, IJsonSymbol
    {
        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for an undefined value.
        /// </summary>
        public static JsonErrorInfo CreateError(string undefinedValue, int position, int length)
            => new JsonErrorInfo(JsonErrorCode.UnrecognizedValue, position, length, new[] { undefinedValue });

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonUndefinedValueSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal JsonUndefinedValueSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonUndefinedValueSyntax green) : base(parent) => Green = green;

        internal override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUndefinedValueSyntax(this, arg);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitUndefinedValueSyntax(this, arg);
    }
}
