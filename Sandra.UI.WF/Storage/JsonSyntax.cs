#region License
/*********************************************************************************
 * JsonSyntax.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
#endregion

using System;

namespace Sandra.UI.WF.Storage
{
    public abstract class JsonTerminalSymbol
    {
        public string Json { get; }
        public int Start { get; }
        public int Length { get; }

        public JsonTerminalSymbol(string json, int start, int length)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (json.Length < start) throw new ArgumentOutOfRangeException(nameof(start));
            if (json.Length < start + length) throw new ArgumentOutOfRangeException(nameof(length));

            Json = json;
            Start = start;
            Length = length;
        }

        public abstract void Accept(JsonTerminalSymbolVisitor visitor);
        public abstract TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor);
    }

    public abstract class JsonTerminalSymbolVisitor
    {
        public virtual void DefaultVisit(JsonTerminalSymbol symbol) { }
        public virtual void Visit(JsonTerminalSymbol symbol) { if (symbol != null) symbol.Accept(this); }
        public virtual void VisitColon(JsonColon symbol) => DefaultVisit(symbol);
        public virtual void VisitComma(JsonComma symbol) => DefaultVisit(symbol);
        public virtual void VisitCurlyClose(JsonCurlyClose symbol) => DefaultVisit(symbol);
        public virtual void VisitCurlyOpen(JsonCurlyOpen symbol) => DefaultVisit(symbol);
        public virtual void VisitSquareBracketClose(JsonSquareBracketClose symbol) => DefaultVisit(symbol);
        public virtual void VisitSquareBracketOpen(JsonSquareBracketOpen symbol) => DefaultVisit(symbol);
        public virtual void VisitUnknownSymbol(JsonUnknownSymbol symbol) => DefaultVisit(symbol);
    }

    public abstract class JsonTerminalSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(JsonTerminalSymbol symbol) => default(TResult);
        public virtual TResult Visit(JsonTerminalSymbol symbol) => symbol == null ? default(TResult) : symbol.Accept(this);
        public virtual TResult VisitColon(JsonColon symbol) => DefaultVisit(symbol);
        public virtual TResult VisitComma(JsonComma symbol) => DefaultVisit(symbol);
        public virtual TResult VisitCurlyClose(JsonCurlyClose symbol) => DefaultVisit(symbol);
        public virtual TResult VisitCurlyOpen(JsonCurlyOpen symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSquareBracketClose(JsonSquareBracketClose symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSquareBracketOpen(JsonSquareBracketOpen symbol) => DefaultVisit(symbol);
        public virtual TResult VisitUnknownSymbol(JsonUnknownSymbol symbol) => DefaultVisit(symbol);
    }

    public class JsonCurlyOpen : JsonTerminalSymbol
    {
        public JsonCurlyOpen(string json, int start) : base(json, start, 1) { }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitCurlyOpen(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitCurlyOpen(this);
    }

    public class JsonCurlyClose : JsonTerminalSymbol
    {
        public JsonCurlyClose(string json, int start) : base(json, start, 1) { }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitCurlyClose(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitCurlyClose(this);
    }

    public class JsonSquareBracketOpen : JsonTerminalSymbol
    {
        public JsonSquareBracketOpen(string json, int start) : base(json, start, 1) { }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketOpen(this);
    }

    public class JsonSquareBracketClose : JsonTerminalSymbol
    {
        public JsonSquareBracketClose(string json, int start) : base(json, start, 1) { }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitSquareBracketClose(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketClose(this);
    }

    public class JsonColon : JsonTerminalSymbol
    {
        public JsonColon(string json, int start) : base(json, start, 1) { }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitColon(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitColon(this);
    }

    public class JsonComma : JsonTerminalSymbol
    {
        public JsonComma(string json, int start) : base(json, start, 1) { }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitComma(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitComma(this);
    }

    public class JsonUnknownSymbol : JsonTerminalSymbol
    {
        public JsonUnknownSymbol(string json, int start) : base(json, start, 1)
        {
        }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitUnknownSymbol(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitUnknownSymbol(this);
    }

    public class JsonErrorInfo
    {
        public string Message { get; }
        public int Start { get; }
        public int Length { get; }

        public JsonErrorInfo(string message, int start, int length)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            Message = message;
            Start = start;
            Length = length;
        }
    }
}
