#region License
/*********************************************************************************
 * JsonColon.cs
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
    public sealed class JsonColon : JsonForegroundSymbol
    {
        public const char ColonCharacter = ':';
        public const int ColonLength = 1;

        public static readonly JsonColon Value = new JsonColon();

        public override int Length => ColonLength;

        private JsonColon() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitColon(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitColon(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitColon(this, arg);
    }
}
