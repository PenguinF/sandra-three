﻿#region License
/*********************************************************************************
 * JsonUnterminatedMultiLineComment.cs
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
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    public sealed class JsonUnterminatedMultiLineComment : JsonSymbol
    {
        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for unterminated multiline comments.
        /// </summary>
        /// <param name="start">
        /// The start position of the unterminated comment.
        /// </param>
        /// <param name="length">
        /// The length of the unterminated comment.
        /// </param>
        public static JsonErrorInfo CreateError(int start, int length)
            => new JsonErrorInfo(JsonErrorCode.UnterminatedMultiLineComment, start, length);

        public override int Length { get; }

        public override bool IsBackground => true;

        public override bool HasErrors => true;

        public override IEnumerable<JsonErrorInfo> GetErrors(int start)
        {
            yield return CreateError(start, Length);
        }

        public JsonUnterminatedMultiLineComment(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitUnterminatedMultiLineComment(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitUnterminatedMultiLineComment(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitUnterminatedMultiLineComment(this, arg);
    }
}
