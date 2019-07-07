#region License
/*********************************************************************************
 * JsonWhitespace.cs
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

using System;

namespace Eutherion.Text.Json
{
    public class JsonWhitespace : JsonSymbol
    {
        /// <summary>
        /// Maximum length before new <see cref="JsonWhitespace"/> instances are always newly allocated.
        /// </summary>
        public const int SharedWhitespaceInstanceLength = 255;

        private static readonly JsonWhitespace[] SharedInstances;

        static JsonWhitespace()
        {
            SharedInstances = new JsonWhitespace[SharedWhitespaceInstanceLength - 1];

            for (int i = SharedWhitespaceInstanceLength - 2; i >= 0; i--)
            {
                // Do not allocate a zero length whitespace.
                SharedInstances[i] = new JsonWhitespace(i + 1);
            }
        }

        public override bool IsBackground => true;
        public override int Length { get; }

        public static JsonWhitespace Create(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (length < SharedWhitespaceInstanceLength) return SharedInstances[length - 1];
            return new JsonWhitespace(length);
        }

        private JsonWhitespace(int length) => Length = length;

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitWhitespace(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitWhitespace(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitWhitespace(this, arg);
    }
}
