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

using SysExtensions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sandra.UI.WF.Storage
{
    public class JsonTextElement : TextElement<JsonSymbol>
    {
        public JsonTextElement(JsonSymbol symbol, int start, int length) : base(symbol, start, length)
        {
        }
    }

    public abstract class JsonSymbol
    {
        public virtual bool IsBackground => false;
        public virtual bool IsValueStartSymbol => false;
        public virtual IEnumerable<TextErrorInfo> Errors => Enumerable.Empty<TextErrorInfo>();

        public abstract void Accept(JsonSymbolVisitor visitor);
        public abstract TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor);
    }

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
    }

    public abstract class JsonSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(JsonSymbol symbol) => default(TResult);
        public virtual TResult Visit(JsonSymbol symbol) => symbol == null ? default(TResult) : symbol.Accept(this);
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

    public class JsonComment : JsonSymbol
    {
        public const char CommentStartFirstCharacter = '/';
        public const char SingleLineCommentStartSecondCharacter = '/';
        public const char MultiLineCommentStartSecondCharacter = '*';

        public static readonly string SingleLineCommentStart
            = new string(new[] { CommentStartFirstCharacter, SingleLineCommentStartSecondCharacter });

        public static readonly JsonComment Value = new JsonComment();

        private JsonComment() { }

        public override bool IsBackground => true;

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitComment(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitComment(this);
    }

    public class JsonUnterminatedMultiLineComment : JsonSymbol
    {
        public TextErrorInfo Error { get; }

        public override bool IsBackground => true;
        public override IEnumerable<TextErrorInfo> Errors { get; }

        public JsonUnterminatedMultiLineComment(TextErrorInfo error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
            Errors = new[] { error };
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitUnterminatedMultiLineComment(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitUnterminatedMultiLineComment(this);
    }

    public class JsonCurlyOpen : JsonSymbol
    {
        public const char CurlyOpenCharacter = '{';

        public static readonly JsonCurlyOpen Value = new JsonCurlyOpen();

        private JsonCurlyOpen() { }

        public override bool IsValueStartSymbol => true;

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitCurlyOpen(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitCurlyOpen(this);
    }

    public class JsonCurlyClose : JsonSymbol
    {
        public const char CurlyCloseCharacter = '}';

        public static readonly JsonCurlyClose Value = new JsonCurlyClose();

        private JsonCurlyClose() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitCurlyClose(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitCurlyClose(this);
    }

    public class JsonSquareBracketOpen : JsonSymbol
    {
        public const char SquareBracketOpenCharacter = '[';

        public static readonly JsonSquareBracketOpen Value = new JsonSquareBracketOpen();

        private JsonSquareBracketOpen() { }

        public override bool IsValueStartSymbol => true;

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketOpen(this);
    }

    public class JsonSquareBracketClose : JsonSymbol
    {
        public const char SquareBracketCloseCharacter = ']';

        public static readonly JsonSquareBracketClose Value = new JsonSquareBracketClose();

        private JsonSquareBracketClose() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitSquareBracketClose(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketClose(this);
    }

    public class JsonColon : JsonSymbol
    {
        public const char ColonCharacter = ':';

        public static readonly JsonColon Value = new JsonColon();

        private JsonColon() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitColon(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitColon(this);
    }

    public class JsonComma : JsonSymbol
    {
        public const char CommaCharacter = ',';

        public static readonly JsonComma Value = new JsonComma();

        private JsonComma() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitComma(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitComma(this);
    }

    public class JsonUnknownSymbol : JsonSymbol
    {
        public TextErrorInfo Error { get; }

        public override IEnumerable<TextErrorInfo> Errors { get; }

        public override bool IsValueStartSymbol => true;

        public JsonUnknownSymbol(TextErrorInfo error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
            Errors = new[] { error };
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitUnknownSymbol(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitUnknownSymbol(this);
    }

    public class JsonValue : JsonSymbol
    {
        public static readonly string True = "true";
        public static readonly string False = "false";

        public string Value { get; }

        public override bool IsValueStartSymbol => true;

        public JsonValue(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitValue(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitValue(this);
    }

    public class JsonString : JsonSymbol
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
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitString(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitString(this);
    }

    public class JsonErrorString : JsonSymbol
    {
        public override IEnumerable<TextErrorInfo> Errors { get; }
        public override bool IsValueStartSymbol => true;

        public JsonErrorString(params TextErrorInfo[] errors)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public JsonErrorString(IEnumerable<TextErrorInfo> errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            Errors = errors.ToArray();
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitErrorString(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitErrorString(this);
    }
}
