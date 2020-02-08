#region License
/*********************************************************************************
 * JsonValueStarterSymbolVisitor.cs
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
    /// <summary>
    /// Represents a visitor that visits a single <see cref="IJsonValueStarterSymbol"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonValueStarterSymbolVisitor
    {
        public virtual void DefaultVisit(IJsonValueStarterSymbol symbol) { }
        public virtual void Visit(IJsonValueStarterSymbol symbol) { if (symbol != null) symbol.Accept(this); }
        public virtual void VisitBooleanLiteralSyntax(GreenJsonBooleanLiteralSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitCurlyOpenSyntax(GreenJsonCurlyOpenSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitErrorStringSyntax(GreenJsonErrorStringSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitIntegerLiteralSyntax(GreenJsonIntegerLiteralSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitSquareBracketOpenSyntax(GreenJsonSquareBracketOpenSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitStringLiteralSyntax(GreenJsonStringLiteralSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax symbol) => DefaultVisit(symbol);
        public virtual void VisitValue(JsonValue symbol) => DefaultVisit(symbol);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="IJsonValueStarterSymbol"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonValueStarterSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(IJsonValueStarterSymbol symbol) => default;
        public virtual TResult Visit(IJsonValueStarterSymbol symbol) => symbol == null ? default : symbol.Accept(this);
        public virtual TResult VisitBooleanLiteralSyntax(GreenJsonBooleanLiteralSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitCurlyOpenSyntax(GreenJsonCurlyOpenSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitErrorStringSyntax(GreenJsonErrorStringSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitIntegerLiteralSyntax(GreenJsonIntegerLiteralSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSquareBracketOpenSyntax(GreenJsonSquareBracketOpenSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitStringLiteralSyntax(GreenJsonStringLiteralSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax symbol) => DefaultVisit(symbol);
        public virtual TResult VisitValue(JsonValue symbol) => DefaultVisit(symbol);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="IJsonValueStarterSymbol"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonValueStarterSymbolVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(IJsonValueStarterSymbol symbol, T arg) => default;
        public virtual TResult Visit(IJsonValueStarterSymbol symbol, T arg) => symbol == null ? default : symbol.Accept(this, arg);
        public virtual TResult VisitBooleanLiteralSyntax(GreenJsonBooleanLiteralSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitCurlyOpenSyntax(GreenJsonCurlyOpenSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitErrorStringSyntax(GreenJsonErrorStringSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitIntegerLiteralSyntax(GreenJsonIntegerLiteralSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitSquareBracketOpenSyntax(GreenJsonSquareBracketOpenSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitStringLiteralSyntax(GreenJsonStringLiteralSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitValue(JsonValue symbol, T arg) => DefaultVisit(symbol, arg);
    }
}
