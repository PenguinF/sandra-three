#region License
/*********************************************************************************
 * JsonCurlyClose.cs
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
    public class JsonCurlyClose : JsonSymbol
    {
        public const char CurlyCloseCharacter = '}';
        public const int CurlyCloseLength = 1;

        public static readonly JsonCurlyClose Value = new JsonCurlyClose();

        public override int Length => CurlyCloseLength;

        private JsonCurlyClose() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitCurlyClose(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitCurlyClose(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitCurlyClose(this, arg);
    }
}
