#region License
/*********************************************************************************
 * JsonComma.cs
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

using Eutherion.Utils;

namespace Eutherion.Text.Json
{
    public sealed class JsonComma : JsonSymbol
    {
        public const char CommaCharacter = ',';
        public const int CommaLength = 1;

        public static readonly JsonComma Value = new JsonComma();

        public override int Length => CommaLength;

        private JsonComma() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitComma(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitComma(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitComma(this, arg);
    }

    public sealed class RedJsonComma : JsonSyntax
    {
        public Union<RedJsonListSyntax, RedJsonMapSyntax> Parent { get; }
        public int CommaIndex { get; }

        public JsonComma Green => JsonComma.Value;

        public override int Length => JsonComma.CommaLength;
        public override JsonSyntax ParentSyntax => Parent.Match<JsonSyntax>(
            whenOption1: x => x,
            whenOption2: x => x);

        internal RedJsonComma(RedJsonListSyntax parent, int commaIndex)
        {
            Parent = parent;
            CommaIndex = commaIndex;
        }

        internal RedJsonComma(RedJsonMapSyntax parent, int commaIndex)
        {
            Parent = parent;
            CommaIndex = commaIndex;
        }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitComma(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitComma(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitComma(this, arg);
    }
}
