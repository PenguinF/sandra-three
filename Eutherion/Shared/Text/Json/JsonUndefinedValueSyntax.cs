﻿#region License
/*********************************************************************************
 * JsonUndefinedValueSyntax.cs
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
    /// <summary>
    /// Represents a literal syntax node with an undefined or unsupported value.
    /// </summary>
    public sealed class JsonUndefinedValueSyntax : JsonValueSyntax
    {
        public JsonSymbol UndefinedToken { get; }

        public override int Length => UndefinedToken.Length;

        public JsonUndefinedValueSyntax(JsonSymbol undefinedToken) => UndefinedToken = undefinedToken;

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitUndefinedValueSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitUndefinedValueSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUndefinedValueSyntax(this, arg);
    }

    public sealed class RedJsonUndefinedValueSyntax : RedJsonValueSyntax
    {
        public JsonUndefinedValueSyntax Green { get; }

        public override int Length => Green.Length;

        internal RedJsonUndefinedValueSyntax(RedJsonValueWithBackgroundSyntax parent, JsonUndefinedValueSyntax green) : base(parent) => Green = green;

        public override void Accept(RedJsonValueSyntaxVisitor visitor) => visitor.VisitUndefinedValueSyntax(this);
        public override TResult Accept<TResult>(RedJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitUndefinedValueSyntax(this);
        public override TResult Accept<T, TResult>(RedJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUndefinedValueSyntax(this, arg);
    }
}
