#region License
/*********************************************************************************
 * JsonSymbolVisitor.cs
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
    public abstract class JsonForegroundSymbolVisitor
    {
        public virtual void DefaultVisit(IJsonForegroundSymbol symbol) { }
        public virtual void Visit(IJsonForegroundSymbol symbol) { if (symbol != null) symbol.Accept(this); }
        public virtual void VisitColonSyntax(GreenJsonColonSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitCommaSyntax(GreenJsonCommaSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitCurlyCloseSyntax(GreenJsonCurlyCloseSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitCurlyOpenSyntax(GreenJsonCurlyOpenSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitErrorStringSyntax(GreenJsonErrorStringSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitSquareBracketCloseSyntax(GreenJsonSquareBracketCloseSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitSquareBracketOpenSyntax(GreenJsonSquareBracketOpenSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitStringLiteralSyntax(JsonString symbol) => DefaultVisit(symbol);
        public virtual void VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitValue(JsonValue symbol) => DefaultVisit(symbol);
    }

    public abstract class JsonForegroundSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(IJsonForegroundSymbol symbol) => default;
        public virtual TResult Visit(IJsonForegroundSymbol symbol) => symbol == null ? default : symbol.Accept(this);
        public virtual TResult VisitColonSyntax(GreenJsonColonSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitCommaSyntax(GreenJsonCommaSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitCurlyCloseSyntax(GreenJsonCurlyCloseSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitCurlyOpenSyntax(GreenJsonCurlyOpenSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitErrorStringSyntax(GreenJsonErrorStringSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSquareBracketCloseSyntax(GreenJsonSquareBracketCloseSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSquareBracketOpenSyntax(GreenJsonSquareBracketOpenSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitStringLiteralSyntax(JsonString symbol) => DefaultVisit(symbol);
        public virtual TResult VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitValue(JsonValue symbol) => DefaultVisit(symbol);
    }

    public abstract class JsonForegroundSymbolVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(IJsonForegroundSymbol symbol, T arg) => default;
        public virtual TResult Visit(IJsonForegroundSymbol symbol, T arg) => symbol == null ? default : symbol.Accept(this, arg);
        public virtual TResult VisitColonSyntax(GreenJsonColonSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitCommaSyntax(GreenJsonCommaSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitCurlyCloseSyntax(GreenJsonCurlyCloseSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitCurlyOpenSyntax(GreenJsonCurlyOpenSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitErrorStringSyntax(GreenJsonErrorStringSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitSquareBracketCloseSyntax(GreenJsonSquareBracketCloseSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitSquareBracketOpenSyntax(GreenJsonSquareBracketOpenSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitStringLiteralSyntax(JsonString symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitValue(JsonValue symbol, T arg) => DefaultVisit(symbol, arg);
    }

    /// <summary>
    /// Represents a visitor that visits a <see cref="JsonSyntax"/> (<see cref="IJsonSymbol"/>) which has no child <see cref="JsonSyntax"/> nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonSymbolVisitor
    {
        public virtual void DefaultVisit(IJsonSymbol node) { }
        public virtual void Visit(IJsonSymbol node) { if (node != null) node.Accept(this); }
        public virtual void VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitColonSyntax(JsonColonSyntax node) => DefaultVisit(node);
        public virtual void VisitCommaSyntax(JsonCommaSyntax node) => DefaultVisit(node);
        public virtual void VisitCommentSyntax(JsonCommentSyntax node) => DefaultVisit(node);
        public virtual void VisitCurlyCloseSyntax(JsonCurlyCloseSyntax node) => DefaultVisit(node);
        public virtual void VisitCurlyOpenSyntax(JsonCurlyOpenSyntax node) => DefaultVisit(node);
        public virtual void VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitMissingValueSyntax(JsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual void VisitRootLevelValueDelimiterSyntax(JsonRootLevelValueDelimiterSyntax node) => DefaultVisit(node);
        public virtual void VisitSquareBracketCloseSyntax(JsonSquareBracketCloseSyntax node) => DefaultVisit(node);
        public virtual void VisitSquareBracketOpenSyntax(JsonSquareBracketOpenSyntax node) => DefaultVisit(node);
        public virtual void VisitStringLiteralSyntax(JsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node) => DefaultVisit(node);
        public virtual void VisitUnterminatedMultiLineCommentSyntax(JsonUnterminatedMultiLineCommentSyntax node) => DefaultVisit(node);
        public virtual void VisitWhitespaceSyntax(JsonWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a <see cref="JsonSyntax"/> (<see cref="IJsonSymbol"/>) which has no child <see cref="JsonSyntax"/> nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(IJsonSymbol node) => default;
        public virtual TResult Visit(IJsonSymbol node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitColonSyntax(JsonColonSyntax node) => DefaultVisit(node);
        public virtual TResult VisitCommaSyntax(JsonCommaSyntax node) => DefaultVisit(node);
        public virtual TResult VisitCommentSyntax(JsonCommentSyntax node) => DefaultVisit(node);
        public virtual TResult VisitCurlyCloseSyntax(JsonCurlyCloseSyntax node) => DefaultVisit(node);
        public virtual TResult VisitCurlyOpenSyntax(JsonCurlyOpenSyntax node) => DefaultVisit(node);
        public virtual TResult VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMissingValueSyntax(JsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitRootLevelValueDelimiterSyntax(JsonRootLevelValueDelimiterSyntax node) => DefaultVisit(node);
        public virtual TResult VisitSquareBracketCloseSyntax(JsonSquareBracketCloseSyntax node) => DefaultVisit(node);
        public virtual TResult VisitSquareBracketOpenSyntax(JsonSquareBracketOpenSyntax node) => DefaultVisit(node);
        public virtual TResult VisitStringLiteralSyntax(JsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUnterminatedMultiLineCommentSyntax(JsonUnterminatedMultiLineCommentSyntax node) => DefaultVisit(node);
        public virtual TResult VisitWhitespaceSyntax(JsonWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a <see cref="JsonSyntax"/> (<see cref="IJsonSymbol"/>) which has no child <see cref="JsonSyntax"/> nodes.
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
        public virtual TResult VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMissingValueSyntax(JsonMissingValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitRootLevelValueDelimiterSyntax(JsonRootLevelValueDelimiterSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitSquareBracketCloseSyntax(JsonSquareBracketCloseSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitSquareBracketOpenSyntax(JsonSquareBracketOpenSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitStringLiteralSyntax(JsonStringLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUnterminatedMultiLineCommentSyntax(JsonUnterminatedMultiLineCommentSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitWhitespaceSyntax(JsonWhitespaceSyntax node, T arg) => DefaultVisit(node, arg);
    }
}
