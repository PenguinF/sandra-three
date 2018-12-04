#region License
/*********************************************************************************
 * JsonUnknownSymbol.cs
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
using System.Collections.Generic;

namespace SysExtensions.Text.Json
{
    public class JsonUnknownSymbol : JsonSymbol
    {
        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for unexpected symbol characters.
        /// </summary>
        public static TextErrorInfo CreateError(string displayCharValue, int position)
            => new TextErrorInfo($"Unexpected symbol '{displayCharValue}'", position, 1);

        public TextErrorInfo Error { get; }

        public override IEnumerable<TextErrorInfo> Errors { get; }

        public override bool IsValueStartSymbol => true;

        public JsonUnknownSymbol(string displayCharValue, int position)
        {
            if (displayCharValue == null) throw new ArgumentNullException(nameof(displayCharValue));
            if (displayCharValue.Length == 0) throw new ArgumentException($"{nameof(displayCharValue)} should be non-empty", nameof(displayCharValue));

            Error = CreateError(displayCharValue, position);
            Errors = new[] { Error };
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitUnknownSymbol(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitUnknownSymbol(this);
    }
}
