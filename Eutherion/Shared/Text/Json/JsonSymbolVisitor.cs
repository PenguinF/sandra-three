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
    public abstract class JsonSymbolVisitor
    {
        public virtual void DefaultVisit(JsonSymbol symbol) { }
        public virtual void Visit(JsonSymbol symbol) { if (symbol != null) symbol.Accept(this); }
        public virtual void VisitColon(JsonColon symbol) => DefaultVisit(symbol);
        public virtual void VisitComma(JsonComma symbol) => DefaultVisit(symbol);
        public virtual void VisitComment(JsonComment symbol) => DefaultVisit(symbol);
        public virtual void VisitCurlyClose(JsonCurlyClose symbol) => DefaultVisit(symbol);
        public virtual void VisitCurlyOpen(JsonCurlyOpen symbol) => DefaultVisit(symbol);
        public virtual void VisitErrorString(JsonErrorString symbol) => DefaultVisit(symbol);
        public virtual void VisitSquareBracketClose(JsonSquareBracketClose symbol) => DefaultVisit(symbol);
        public virtual void VisitSquareBracketOpen(JsonSquareBracketOpen symbol) => DefaultVisit(symbol);
        public virtual void VisitString(JsonString symbol) => DefaultVisit(symbol);
        public virtual void VisitUnknownSymbol(JsonUnknownSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol) => DefaultVisit(symbol);
        public virtual void VisitValue(JsonValue symbol) => DefaultVisit(symbol);
        public virtual void VisitWhitespace(JsonWhitespace symbol) => DefaultVisit(symbol);
    }

    public abstract class JsonSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(JsonSymbol symbol) => default;
        public virtual TResult Visit(JsonSymbol symbol) => symbol == null ? default : symbol.Accept(this);
        public virtual TResult VisitColon(JsonColon symbol) => DefaultVisit(symbol);
        public virtual TResult VisitComma(JsonComma symbol) => DefaultVisit(symbol);
        public virtual TResult VisitComment(JsonComment symbol) => DefaultVisit(symbol);
        public virtual TResult VisitCurlyClose(JsonCurlyClose symbol) => DefaultVisit(symbol);
        public virtual TResult VisitCurlyOpen(JsonCurlyOpen symbol) => DefaultVisit(symbol);
        public virtual TResult VisitErrorString(JsonErrorString symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSquareBracketClose(JsonSquareBracketClose symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSquareBracketOpen(JsonSquareBracketOpen symbol) => DefaultVisit(symbol);
        public virtual TResult VisitString(JsonString symbol) => DefaultVisit(symbol);
        public virtual TResult VisitUnknownSymbol(JsonUnknownSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol) => DefaultVisit(symbol);
        public virtual TResult VisitValue(JsonValue symbol) => DefaultVisit(symbol);
        public virtual TResult VisitWhitespace(JsonWhitespace symbol) => DefaultVisit(symbol);
    }

    public abstract class JsonSymbolVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(JsonSymbol symbol, T arg) => default;
        public virtual TResult Visit(JsonSymbol symbol, T arg) => symbol == null ? default : symbol.Accept(this, arg);
        public virtual TResult VisitColon(JsonColon symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitComma(JsonComma symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitComment(JsonComment symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitCurlyClose(JsonCurlyClose symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitCurlyOpen(JsonCurlyOpen symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitErrorString(JsonErrorString symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitSquareBracketClose(JsonSquareBracketClose symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitSquareBracketOpen(JsonSquareBracketOpen symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitString(JsonString symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitUnknownSymbol(JsonUnknownSymbol symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitValue(JsonValue symbol, T arg) => DefaultVisit(symbol, arg);
        public virtual TResult VisitWhitespace(JsonWhitespace symbol, T arg) => DefaultVisit(symbol, arg);
    }
}
