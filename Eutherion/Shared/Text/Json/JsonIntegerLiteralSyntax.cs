#region License
/*********************************************************************************
 * JsonIntegerLiteralSyntax.cs
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

using System.Numerics;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents an integer literal value syntax node.
    /// </summary>
    public sealed class JsonIntegerLiteralSyntax : JsonValueSyntax
    {
        public JsonValue IntegerToken { get; }

        public BigInteger Value { get; }

        public override int Length => IntegerToken.Length;

        public JsonIntegerLiteralSyntax(JsonValue integerToken, BigInteger value)
        {
            IntegerToken = integerToken;
            Value = value;
        }

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitIntegerLiteralSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitIntegerLiteralSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitIntegerLiteralSyntax(this, arg);
    }
}
