#region License
/*********************************************************************************
 * JsonBooleanLiteralSyntax.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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
    /// <summary>
    /// Represents a boolean literal value syntax node.
    /// </summary>
    public abstract class GreenJsonBooleanLiteralSyntax : GreenJsonValueSyntax, IGreenJsonSymbol
    {
        /// <summary>
        /// Represents a 'false' literal value syntax node.
        /// </summary>
        public sealed class False : GreenJsonBooleanLiteralSyntax
        {
            /// <summary>
            /// Returns the singleton instance.
            /// </summary>
            public static readonly False Instance = new False();

            private False() { }

            /// <summary>
            /// Gets the boolean value represented by this literal syntax.
            /// </summary>
            public override bool Value => false;

            /// <summary>
            /// Gets the representation of this literal value in JSON source text.
            /// </summary>
            public override string LiteralJsonValue => JsonValue.False;

            /// <summary>
            /// Gets the length of the text span corresponding with this syntax node.
            /// </summary>
            public override int Length => JsonValue.FalseSymbolLength;

            /// <summary>
            /// Invokes a <see cref="Func{TResult}"/> based on whether this instance represents a false or true value, and returns its result.
            /// </summary>
            /// <typeparam name="TResult">
            /// Type of the value to return.
            /// </typeparam>
            /// <param name="whenFalse">
            /// The <see cref="Func{TResult}"/> to invoke if this instance represents a false value.
            /// </param>
            /// <param name="whenTrue">
            /// The <see cref="Func{TResult}"/> to invoke if this instance represents a true value.
            /// </param>
            /// <returns>
            /// The result of the invoked <see cref="Func{TResult}"/>, or a default value if the passed in function is null.
            /// </returns>
            public override TResult Match<TResult>(Func<TResult> whenFalse, Func<TResult> whenTrue) => whenFalse();
        }

        /// <summary>
        /// Represents a 'true' literal value syntax node.
        /// </summary>
        public sealed class True : GreenJsonBooleanLiteralSyntax
        {
            /// <summary>
            /// Returns the singleton instance.
            /// </summary>
            public static readonly True Instance = new True();

            private True() { }

            /// <summary>
            /// Gets the boolean value represented by this literal syntax.
            /// </summary>
            public override bool Value => true;

            /// <summary>
            /// Gets the representation of this literal value in JSON source text.
            /// </summary>
            public override string LiteralJsonValue => JsonValue.True;

            /// <summary>
            /// Gets the length of the text span corresponding with this syntax node.
            /// </summary>
            public override int Length => JsonValue.TrueSymbolLength;

            /// <summary>
            /// Invokes a <see cref="Func{TResult}"/> based on whether this instance represents a false or true value, and returns its result.
            /// </summary>
            /// <typeparam name="TResult">
            /// Type of the value to return.
            /// </typeparam>
            /// <param name="whenFalse">
            /// The <see cref="Func{TResult}"/> to invoke if this instance represents a false value.
            /// </param>
            /// <param name="whenTrue">
            /// The <see cref="Func{TResult}"/> to invoke if this instance represents a true value.
            /// </param>
            /// <returns>
            /// The result of the invoked <see cref="Func{TResult}"/>, or a default value if the passed in function is null.
            /// </returns>
            public override TResult Match<TResult>(Func<TResult> whenFalse, Func<TResult> whenTrue) => whenTrue();
        }

        /// <summary>
        /// Gets the boolean value represented by this literal syntax.
        /// </summary>
        public abstract bool Value { get; }

        /// <summary>
        /// Gets the representation of this literal value in JSON source text.
        /// </summary>
        public abstract string LiteralJsonValue { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public JsonSymbolType SymbolType => JsonSymbolType.BooleanLiteral;

        private GreenJsonBooleanLiteralSyntax() { }

        /// <summary>
        /// Invokes a <see cref="Func{TResult}"/> based on whether this instance represents a false or true value, and returns its result.
        /// </summary>
        /// <typeparam name="TResult">
        /// Type of the value to return.
        /// </typeparam>
        /// <param name="whenFalse">
        /// The <see cref="Func{TResult}"/> to invoke if this instance represents a false value.
        /// </param>
        /// <param name="whenTrue">
        /// The <see cref="Func{TResult}"/> to invoke if this instance represents a true value.
        /// </param>
        /// <returns>
        /// The result of the invoked <see cref="Func{TResult}"/>, or a default value if the passed in function is null.
        /// </returns>
        public abstract TResult Match<TResult>(Func<TResult> whenFalse, Func<TResult> whenTrue);

        IEnumerable<JsonErrorInfo> IGreenJsonSymbol.GetErrors(int startPosition) => EmptyEnumerable<JsonErrorInfo>.Instance;

        public override void Accept(GreenJsonValueSyntaxVisitor visitor) => visitor.VisitBooleanLiteralSyntax(this);
        public override TResult Accept<TResult>(GreenJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitBooleanLiteralSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitBooleanLiteralSyntax(this, arg);
    }

    /// <summary>
    /// Represents a boolean literal value syntax node.
    /// </summary>
    public abstract class JsonBooleanLiteralSyntax : JsonValueSyntax, IJsonSymbol
    {
        /// <summary>
        /// Represents a 'false' literal value syntax node.
        /// </summary>
        public sealed class False : JsonBooleanLiteralSyntax
        {
            /// <summary>
            /// Gets the bottom-up only 'green' representation of this syntax node.
            /// </summary>
            public override GreenJsonBooleanLiteralSyntax Green => GreenJsonBooleanLiteralSyntax.False.Instance;

            /// <summary>
            /// Gets the boolean value represented by this literal syntax.
            /// </summary>
            public override bool Value => false;

            internal False(JsonValueWithBackgroundSyntax parent) : base(parent) { }
        }

        /// <summary>
        /// Represents a 'true' literal value syntax node.
        /// </summary>
        public sealed class True : JsonBooleanLiteralSyntax
        {
            /// <summary>
            /// Gets the bottom-up only 'green' representation of this syntax node.
            /// </summary>
            public override GreenJsonBooleanLiteralSyntax Green => GreenJsonBooleanLiteralSyntax.True.Instance;

            /// <summary>
            /// Gets the boolean value represented by this literal syntax.
            /// </summary>
            public override bool Value => true;

            internal True(JsonValueWithBackgroundSyntax parent) : base(parent) { }
        }

        /// <summary>
        /// Returns either <see cref="GreenJsonBooleanLiteralSyntax.False.Instance"/> or <see cref="GreenJsonBooleanLiteralSyntax.True.Instance"/>,
        /// corresponding with the truth value of the parameter.
        /// </summary>
        /// <param name="boolValue">
        /// Specifies which literal syntax to return.
        /// </param>
        /// <returns>
        /// Either <see cref="GreenJsonBooleanLiteralSyntax.False.Instance"/> or <see cref="GreenJsonBooleanLiteralSyntax.True.Instance"/>.
        /// </returns>
        public static GreenJsonBooleanLiteralSyntax BoolJsonLiteral(bool boolValue)
            => boolValue
            ? GreenJsonBooleanLiteralSyntax.True.Instance
            : (GreenJsonBooleanLiteralSyntax)GreenJsonBooleanLiteralSyntax.False.Instance;

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public abstract GreenJsonBooleanLiteralSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the boolean value represented by this literal syntax.
        /// </summary>
        public abstract bool Value { get; }

        private JsonBooleanLiteralSyntax(JsonValueWithBackgroundSyntax parent) : base(parent) { }

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitBooleanLiteralSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitBooleanLiteralSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitBooleanLiteralSyntax(this, arg);

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitBooleanLiteralSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitBooleanLiteralSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitBooleanLiteralSyntax(this, arg);
    }
}
