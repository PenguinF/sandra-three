#region License
/*********************************************************************************
 * JsonBooleanLiteralSyntax.cs
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a boolean literal value syntax node.
    /// </summary>
    public abstract class GreenJsonBooleanLiteralSyntax : GreenJsonValueSyntax
    {
        public sealed class False : GreenJsonBooleanLiteralSyntax
        {
            public static False Instance = new False();

            private False() { }

            public override bool Value => false;

            public override JsonValue BooleanToken => JsonValue.FalseJsonValue;

            /// <summary>
            /// Gets the length of the text span corresponding with this node.
            /// </summary>
            public override int Length => JsonValue.FalseSymbolLength;

            public override TResult Match<TResult>(Func<TResult> whenFalse, Func<TResult> whenTrue) => whenFalse();
        }

        public sealed class True : GreenJsonBooleanLiteralSyntax
        {
            public static readonly True Instance = new True();

            private True() { }

            public override bool Value => true;

            public override JsonValue BooleanToken => JsonValue.TrueJsonValue;

            /// <summary>
            /// Gets the length of the text span corresponding with this node.
            /// </summary>
            public override int Length => JsonValue.TrueSymbolLength;

            public override TResult Match<TResult>(Func<TResult> whenFalse, Func<TResult> whenTrue) => whenTrue();
        }

        public abstract bool Value { get; }
        public abstract JsonValue BooleanToken { get; }

        private GreenJsonBooleanLiteralSyntax() { }

        public abstract TResult Match<TResult>(Func<TResult> whenFalse, Func<TResult> whenTrue);

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
            /// Gets the boolean value represented by this <see cref="JsonBooleanLiteralSyntax"/> node.
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
            /// Gets the boolean value represented by this <see cref="JsonBooleanLiteralSyntax"/> node.
            /// </summary>
            public override bool Value => true;

            internal True(JsonValueWithBackgroundSyntax parent) : base(parent) { }
        }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public abstract GreenJsonBooleanLiteralSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the boolean value represented by this <see cref="JsonBooleanLiteralSyntax"/> node.
        /// </summary>
        public abstract bool Value { get; }

        private JsonBooleanLiteralSyntax(JsonValueWithBackgroundSyntax parent) : base(parent) { }

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitBooleanLiteralSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitBooleanLiteralSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitBooleanLiteralSyntax(this, arg);

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitBooleanLiteralSyntax(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitBooleanLiteralSyntax(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitBooleanLiteralSyntax(this, arg);
    }
}
