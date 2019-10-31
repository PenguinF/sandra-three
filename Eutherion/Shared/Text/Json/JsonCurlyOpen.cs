#region License
/*********************************************************************************
 * JsonCurlyOpen.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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
    public sealed class JsonCurlyOpen : JsonSymbol
    {
        public const char CurlyOpenCharacter = '{';
        public const int CurlyOpenLength = 1;

        public static readonly JsonCurlyOpen Value = new JsonCurlyOpen();

        public override bool IsValueStartSymbol => true;
        public override int Length => CurlyOpenLength;

        private JsonCurlyOpen() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitCurlyOpen(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitCurlyOpen(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitCurlyOpen(this, arg);
    }

    public sealed class RedJsonCurlyOpen : JsonSyntax
    {
        public RedJsonMapSyntax Parent { get; }

        public JsonCurlyOpen Green => JsonCurlyOpen.Value;

        public override int Length => JsonCurlyOpen.CurlyOpenLength;
        public override JsonSyntax ParentSyntax => Parent;

        internal RedJsonCurlyOpen(RedJsonMapSyntax parent) => Parent = parent;

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitCurlyOpen(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitCurlyOpen(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitCurlyOpen(this, arg);
    }
}
