#region License
/*********************************************************************************
 * JsonSymbolVisitor.cs
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
    /// Represents a visitor that visits a single <see cref="IJsonSymbol"/>, which is a <see cref="JsonSyntax"/> node without any child nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonSymbolVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(IJsonSymbol node, T arg) => default;
        public virtual TResult Visit(IJsonSymbol node, T arg) => node == null ? default : node.Accept(this, arg);

        public virtual TResult VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitColonSyntax(JsonColonSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitCommaSyntax(JsonCommaSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitCommentSyntax(JsonCommentSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitCurlyCloseSyntax(JsonCurlyCloseSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitCurlyOpenSyntax(JsonCurlyOpenSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitErrorStringSyntax(JsonErrorStringSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitRootLevelValueDelimiterSyntax(JsonRootLevelValueDelimiterSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitSquareBracketCloseSyntax(JsonSquareBracketCloseSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitSquareBracketOpenSyntax(JsonSquareBracketOpenSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitStringLiteralSyntax(JsonStringLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUnknownSymbolSyntax(JsonUnknownSymbolSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUnterminatedMultiLineCommentSyntax(JsonUnterminatedMultiLineCommentSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitWhitespaceSyntax(JsonWhitespaceSyntax node, T arg) => DefaultVisit(node, arg);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="IJsonSymbol"/>, which is a <see cref="JsonSyntax"/> node without any child nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonSymbolVisitor<TResult> : JsonSymbolVisitor<_void, TResult>
    {
        public virtual TResult Visit(IJsonSymbol node) => Visit(node, _void._);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="IJsonSymbol"/>, which is a <see cref="JsonSyntax"/> node without any child nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonSymbolVisitor : JsonSymbolVisitor<_void, _void>
    {
        public virtual void Visit(IJsonSymbol node) => Visit(node, _void._);
    }
}
