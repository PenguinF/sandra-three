#region License
/*********************************************************************************
 * JsonValue.cs
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

using System;

namespace SysExtensions.Text.Json
{
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
}
