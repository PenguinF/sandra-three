#region License
/*********************************************************************************
 * JsonSquareBracketClose.cs
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
    public sealed class JsonSquareBracketClose : JsonSymbol
    {
        public const char SquareBracketCloseCharacter = ']';
        public const int SquareBracketCloseLength = 1;

        public static readonly JsonSquareBracketClose Value = new JsonSquareBracketClose();

        public override int Length => SquareBracketCloseLength;

        private JsonSquareBracketClose() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitSquareBracketClose(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketClose(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitSquareBracketClose(this, arg);
    }

    public sealed class RedJsonSquareBracketClose : JsonSyntax
    {
        public RedJsonListSyntax Parent { get; }

        public JsonSquareBracketClose Green => JsonSquareBracketClose.Value;

        public override int Start => Parent.Length - JsonSquareBracketClose.SquareBracketCloseLength;
        public override int Length => JsonSquareBracketClose.SquareBracketCloseLength;
        public override JsonSyntax ParentSyntax => Parent;

        internal RedJsonSquareBracketClose(RedJsonListSyntax parent) => Parent = parent;

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitSquareBracketClose(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketClose(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitSquareBracketClose(this, arg);
    }
}
