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

using SysExtensions.TextIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sandra.UI.WF.Storage
{
    public class JsonTextElement : TextElement<JsonTerminalSymbol>
    {
        public string Json { get; }

        public JsonTextElement(JsonTerminalSymbol symbol, string json, int start)
            : this(symbol, json, start, 1)
        {
        }

        public JsonTextElement(JsonTerminalSymbol symbol, string json, int start, int length) : base(symbol)
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

        public bool IsBackground => TerminalSymbol.IsBackground;
        public bool IsValueStartSymbol => TerminalSymbol.IsValueStartSymbol;
        public IEnumerable<TextErrorInfo> Errors => TerminalSymbol.Errors;

        public void Accept(JsonTerminalSymbolVisitor visitor) => TerminalSymbol.Accept(visitor);
        public TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => TerminalSymbol.Accept(visitor);
    }

    public abstract class JsonTerminalSymbol
    {
        public virtual bool IsBackground => false;
        public virtual bool IsValueStartSymbol => false;
        public virtual IEnumerable<TextErrorInfo> Errors => Enumerable.Empty<TextErrorInfo>();

        public abstract void Accept(JsonTerminalSymbolVisitor visitor);
        public abstract TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor);
    }

    public abstract class JsonTerminalSymbolVisitor
    {
        public virtual void DefaultVisit(JsonTerminalSymbol symbol) { }
        public virtual void Visit(JsonTerminalSymbol symbol) { if (symbol != null) symbol.Accept(this); }
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
    }

    public abstract class JsonTerminalSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(JsonTerminalSymbol symbol) => default(TResult);
        public virtual TResult Visit(JsonTerminalSymbol symbol) => symbol == null ? default(TResult) : symbol.Accept(this);
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
    }

    public class JsonComment : JsonTerminalSymbol
    {
        public const char CommentStartFirstCharacter = '/';
        public const char SingleLineCommentStartSecondCharacter = '/';
        public const char MultiLineCommentStartSecondCharacter = '*';

        public static readonly string SingleLineCommentStart
            = new string(new[] { CommentStartFirstCharacter, SingleLineCommentStartSecondCharacter });

        public override bool IsBackground => true;

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitComment(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitComment(this);
    }

    public class JsonUnterminatedMultiLineComment : JsonTerminalSymbol
    {
        public TextErrorInfo Error { get; }

        public override bool IsBackground => true;
        public override IEnumerable<TextErrorInfo> Errors { get; }

        public JsonUnterminatedMultiLineComment(TextErrorInfo error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            Error = error;
            Errors = new[] { error };
        }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitUnterminatedMultiLineComment(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitUnterminatedMultiLineComment(this);
    }

    public class JsonCurlyOpen : JsonTerminalSymbol
    {
        public const char CurlyOpenCharacter = '{';

        public override bool IsValueStartSymbol => true;

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitCurlyOpen(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitCurlyOpen(this);
    }

    public class JsonCurlyClose : JsonTerminalSymbol
    {
        public const char CurlyCloseCharacter = '}';

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitCurlyClose(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitCurlyClose(this);
    }

    public class JsonSquareBracketOpen : JsonTerminalSymbol
    {
        public const char SquareBracketOpenCharacter = '[';

        public override bool IsValueStartSymbol => true;

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketOpen(this);
    }

    public class JsonSquareBracketClose : JsonTerminalSymbol
    {
        public const char SquareBracketCloseCharacter = ']';

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitSquareBracketClose(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketClose(this);
    }

    public class JsonColon : JsonTerminalSymbol
    {
        public const char ColonCharacter = ':';

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitColon(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitColon(this);
    }

    public class JsonComma : JsonTerminalSymbol
    {
        public const char CommaCharacter = ',';

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitComma(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitComma(this);
    }

    public class JsonUnknownSymbol : JsonTerminalSymbol
    {
        public TextErrorInfo Error { get; }

        public override IEnumerable<TextErrorInfo> Errors { get; }

        public override bool IsValueStartSymbol => true;

        public JsonUnknownSymbol(TextErrorInfo error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            Error = error;
            Errors = new[] { error };
        }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitUnknownSymbol(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitUnknownSymbol(this);
    }

    public class JsonValue : JsonTerminalSymbol
    {
        public static readonly string True = "true";
        public static readonly string False = "false";

        public string Value { get; }

        public override bool IsValueStartSymbol => true;

        public JsonValue(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Value = value;
        }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitValue(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitValue(this);
    }

    public class JsonString : JsonTerminalSymbol
    {
        public const char QuoteCharacter = '"';
        public const char EscapeCharacter = '\\';

        /// <summary>
        /// Generates the escape sequence string for a character.
        /// </summary>
        public static string EscapedCharacterString(char c)
        {
            switch (c)
            {
                case '\0': return "\\0";
                case '\b': return "\\b";
                case '\f': return "\\f";
                case '\n': return "\\n";
                case '\r': return "\\r";
                case '\t': return "\\t";
                case '\v': return "\\v";
                case QuoteCharacter: return "\\\"";
                case EscapeCharacter: return "\\\\";
                default: return $"\\u{((int)c).ToString("x4")}";
            }
        }

        private const char HighestControlCharacter = '\u009f';
        private const int ControlCharacterIndexLength = HighestControlCharacter + 1;

        // An index in memory is as fast as it gets for determining whether or not a character should be escaped.
        public static readonly bool[] CharacterMustBeEscapedIndex;

        static JsonString()
        {
            // Will be initialized with all false values.
            CharacterMustBeEscapedIndex = new bool[ControlCharacterIndexLength];

            //https://www.compart.com/en/unicode/category/Cc
            for (int i = 0; i < ' '; i++) CharacterMustBeEscapedIndex[i] = true;
            for (int i = '\u007f'; i <= HighestControlCharacter; i++) CharacterMustBeEscapedIndex[i] = true;

            // Individual characters.
            CharacterMustBeEscapedIndex[QuoteCharacter] = true;
            CharacterMustBeEscapedIndex[EscapeCharacter] = true;
        }

        /// <summary>
        /// Returns whether or not a character must be escaped when in a JSON string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CharacterMustBeEscaped(char c)
        {
            if (c < ControlCharacterIndexLength) return CharacterMustBeEscapedIndex[c];

            // Express this as two inequality conditions so second condition may not have to be evaluated.
            //https://www.compart.com/en/unicode/category/Zl - line separator
            //https://www.compart.com/en/unicode/category/Zp - paragraph separator
            return c >= '\u2028' && c <= '\u2029';
        }

        public string Value { get; }

        public override bool IsValueStartSymbol => true;

        public JsonString(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Value = value;
        }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitString(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitString(this);
    }

    public class JsonErrorString : JsonTerminalSymbol
    {
        public override IEnumerable<TextErrorInfo> Errors { get; }
        public override bool IsValueStartSymbol => true;

        public JsonErrorString(params TextErrorInfo[] errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            Errors = errors;
        }

        public JsonErrorString(IEnumerable<TextErrorInfo> errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            Errors = errors.ToArray();
        }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitErrorString(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitErrorString(this);
    }
}
