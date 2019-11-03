#region License
/*********************************************************************************
 * JsonValue.cs
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
    public sealed class JsonValue : JsonSymbol
    {
        public const int FalseSymbolLength = 5;
        public const int TrueSymbolLength = 4;

        public static readonly string False = "false";
        public static readonly string True = "true";

        public static readonly JsonValue FalseJsonValue = new JsonValue(False);
        public static readonly JsonValue TrueJsonValue = new JsonValue(True);

        public static JsonValue BoolJsonValue(bool boolValue) => boolValue ? TrueJsonValue : FalseJsonValue;

        public static JsonValue Create(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length <= 0) throw new ArgumentException(nameof(value));

            return value == False ? FalseJsonValue
                : value == True ? TrueJsonValue
                : new JsonValue(value);
        }

        public string Value { get; }

        public override bool IsValueStartSymbol => true;
        public override int Length => Value.Length;

        private JsonValue(string value) => Value = value;

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitValue(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitValue(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitValue(this, arg);
    }
}
