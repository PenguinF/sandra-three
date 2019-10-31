#region License
/*********************************************************************************
 * JsonSquareBracketOpen.cs
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
    public sealed class JsonSquareBracketOpen : JsonSymbol
    {
        public const char SquareBracketOpenCharacter = '[';
        public const int SquareBracketOpenLength = 1;

        public static readonly JsonSquareBracketOpen Value = new JsonSquareBracketOpen();

        public override bool IsValueStartSymbol => true;
        public override int Length => SquareBracketOpenLength;

        private JsonSquareBracketOpen() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitSquareBracketOpen(this, arg);
    }

    public sealed class RedJsonSquareBracketOpen : JsonSyntax
    {
        public RedJsonListSyntax Parent { get; }

        public JsonSquareBracketOpen Green => JsonSquareBracketOpen.Value;

        public override int Start => 0;
        public override int Length => JsonSquareBracketOpen.SquareBracketOpenLength;
        public override JsonSyntax ParentSyntax => Parent;

        internal RedJsonSquareBracketOpen(RedJsonListSyntax parent) => Parent = parent;

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitSquareBracketOpen(this, arg);
    }
}
