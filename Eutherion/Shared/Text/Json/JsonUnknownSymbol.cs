#region License
/*********************************************************************************
 * JsonUnknownSymbol.cs
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

using System;
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    public sealed class JsonUnknownSymbol : JsonForegroundSymbol
    {
        public const int UnknownSymbolLength = 1;

        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for unexpected symbol characters.
        /// </summary>
        public static JsonErrorInfo CreateError(string displayCharValue, int position)
            => new JsonErrorInfo(JsonErrorCode.UnexpectedSymbol, position, 1, new[] { displayCharValue });

        public string DisplayCharValue { get; }

        public override bool IsValueStartSymbol => true;

        public override int Length => UnknownSymbolLength;

        public override bool HasErrors => true;

        public override IEnumerable<JsonErrorInfo> GetErrors(int position)
            => new SingleElementEnumerable<JsonErrorInfo>(CreateError(DisplayCharValue, position));

        public JsonUnknownSymbol(string displayCharValue)
        {
            if (displayCharValue == null) throw new ArgumentNullException(nameof(displayCharValue));
            if (displayCharValue.Length == 0) throw new ArgumentException($"{nameof(displayCharValue)} should be non-empty", nameof(displayCharValue));

            DisplayCharValue = displayCharValue;
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitUnknownSymbol(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitUnknownSymbol(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitUnknownSymbol(this, arg);
    }
}
