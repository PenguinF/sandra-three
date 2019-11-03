#region License
/*********************************************************************************
 * JsonColonSyntax.cs
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
    public sealed class JsonColonSyntax : JsonSyntax
    {
        public RedJsonKeyValueSyntax Parent { get; }
        public int ColonIndex { get; }

        public JsonColon Green => JsonColon.Value;

        public override int Start => Parent.Green.ValueSectionNodes.GetSeparatorOffset(ColonIndex);
        public override int Length => JsonColon.ColonLength;
        public override JsonSyntax ParentSyntax => Parent;

        internal JsonColonSyntax(RedJsonKeyValueSyntax parent, int colonIndex)
        {
            Parent = parent;
            ColonIndex = colonIndex;
        }

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitColon(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitColon(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitColon(this, arg);
    }
}
