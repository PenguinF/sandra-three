#region License
/*********************************************************************************
 * JsonRootLevelValueDelimiterSyntax.cs
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
using System.Diagnostics;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a syntax node which contains a value delimiter symbol at the root level.
    /// Example json "}" generates this syntax node.
    /// </summary>
    public sealed class GreenJsonRootLevelValueDelimiterSyntax : GreenJsonBackgroundSyntax
    {
        public IGreenJsonSymbol ValueDelimiter { get; }

        public override int Length => ValueDelimiter.Length;

        public GreenJsonRootLevelValueDelimiterSyntax(IGreenJsonSymbol valueDelimiter)
        {
            ValueDelimiter = valueDelimiter ?? throw new ArgumentNullException(nameof(valueDelimiter));
            Debug.Assert(ValueDelimiter.SymbolType >= JsonParser.ValueDelimiterThreshold);
        }

        public override void Accept(GreenJsonBackgroundSyntaxVisitor visitor) => visitor.VisitRootLevelValueDelimiterSyntax(this);
        public override TResult Accept<TResult>(GreenJsonBackgroundSyntaxVisitor<TResult> visitor) => visitor.VisitRootLevelValueDelimiterSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonBackgroundSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitRootLevelValueDelimiterSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node which contains a value delimiter symbol at the root level.
    /// Example json "}" generates this syntax node.
    /// </summary>
    public sealed class JsonRootLevelValueDelimiterSyntax : JsonBackgroundSyntax, IJsonSymbol
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonRootLevelValueDelimiterSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal JsonRootLevelValueDelimiterSyntax(JsonBackgroundListSyntax parent, int backgroundNodeIndex, GreenJsonRootLevelValueDelimiterSyntax green)
            : base(parent, backgroundNodeIndex)
            => Green = green;

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitRootLevelValueDelimiterSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitRootLevelValueDelimiterSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitRootLevelValueDelimiterSyntax(this, arg);
    }
}
