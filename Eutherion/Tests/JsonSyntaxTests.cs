#region License
/*********************************************************************************
 * JsonSyntaxTests.cs
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

using Eutherion.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;

namespace Eutherion.Shared.Tests
{
    public class JsonSyntaxTests
    {
        internal static GreenJsonMultiValueSyntax CreateMultiValue(GreenJsonValueSyntax valueContentNode)
        {
            return new GreenJsonMultiValueSyntax(
                new SingleElementEnumerable<GreenJsonValueWithBackgroundSyntax>(new GreenJsonValueWithBackgroundSyntax(
                    GreenJsonBackgroundListSyntax.Empty,
                    valueContentNode)),
                GreenJsonBackgroundListSyntax.Empty);
        }

        [Fact]
        public void ArgumentChecks()
        {
            Assert.Throws<ArgumentNullException>("source", () => GreenJsonBackgroundListSyntax.Create(null));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenJsonCommentSyntax.Create(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenJsonCommentSyntax.Create(0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonErrorStringSyntax(0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonIntegerLiteralSyntax(0, -1));

            Assert.Throws<ArgumentNullException>("validKey", () => new GreenJsonKeyValueSyntax(null, EmptyEnumerable<GreenJsonMultiValueSyntax>.Instance));
            Assert.Throws<ArgumentNullException>("source", () => new GreenJsonKeyValueSyntax(Maybe<GreenJsonStringLiteralSyntax>.Nothing, null));
            Assert.Throws<ArgumentException>("valueSectionNodes", () => new GreenJsonKeyValueSyntax(Maybe<GreenJsonStringLiteralSyntax>.Nothing, EmptyEnumerable<GreenJsonMultiValueSyntax>.Instance));

            Assert.Throws<ArgumentNullException>("source", () => new GreenJsonListSyntax(null, false));
            Assert.Throws<ArgumentException>("listItemNodes", () => new GreenJsonListSyntax(EmptyEnumerable<GreenJsonMultiValueSyntax>.Instance, false));

            Assert.Throws<ArgumentNullException>("source", () => new GreenJsonMapSyntax(null, false));
            Assert.Throws<ArgumentException>("keyValueNodes", () => new GreenJsonMapSyntax(EmptyEnumerable<GreenJsonKeyValueSyntax>.Instance, false));

            Assert.Throws<ArgumentNullException>("source", () => new GreenJsonMultiValueSyntax(null, GreenJsonBackgroundListSyntax.Empty));
            Assert.Throws<ArgumentException>("valueNodes", () => new GreenJsonMultiValueSyntax(EmptyEnumerable<GreenJsonValueWithBackgroundSyntax>.Instance, GreenJsonBackgroundListSyntax.Empty));
            Assert.Throws<ArgumentNullException>("backgroundAfter", () => new GreenJsonMultiValueSyntax(
                new SingleElementEnumerable<GreenJsonValueWithBackgroundSyntax>(new GreenJsonValueWithBackgroundSyntax(
                    GreenJsonBackgroundListSyntax.Empty, GreenJsonMissingValueSyntax.Value)), null));

            Assert.Throws<ArgumentNullException>("valueDelimiter", () => new GreenJsonRootLevelValueDelimiterSyntax((GreenJsonCurlyCloseSyntax)null));
            Assert.Throws<ArgumentNullException>("valueDelimiter", () => new GreenJsonRootLevelValueDelimiterSyntax((GreenJsonSquareBracketCloseSyntax)null));

            Assert.Throws<ArgumentNullException>("value", () => new GreenJsonStringLiteralSyntax(null, 2));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonStringLiteralSyntax(string.Empty, -1));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonUndefinedValueSyntax(0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonUnterminatedMultiLineCommentSyntax(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonUnterminatedMultiLineCommentSyntax(0));

            Assert.Throws<ArgumentNullException>("backgroundBefore", () => new GreenJsonValueWithBackgroundSyntax(null, GreenJsonBooleanLiteralSyntax.False.Instance));
            Assert.Throws<ArgumentNullException>("contentNode", () => new GreenJsonValueWithBackgroundSyntax(GreenJsonBackgroundListSyntax.Empty, null));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenJsonWhitespaceSyntax.Create(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenJsonWhitespaceSyntax.Create(0));

            Assert.Throws<ArgumentNullException>("value", () => JsonValue.TryCreate((string)null));
            Assert.Throws<ArgumentException>("value", () => JsonValue.TryCreate(string.Empty));

            Assert.Throws<ArgumentNullException>("syntax", () => new RootJsonSyntax(
                null,
                EmptyEnumerable<JsonErrorInfo>.Instance));
            Assert.Throws<ArgumentNullException>("errors", () => new RootJsonSyntax(
                CreateMultiValue(new GreenJsonIntegerLiteralSyntax(0, 1)),
                null));

            // Both GreenJsonStringLiteralSyntax instances should be the same, and in below test they are not.
            Assert.Throws<ArgumentException>("validKey", () => new GreenJsonKeyValueSyntax(
                new GreenJsonStringLiteralSyntax("x", 1),
                new SingleElementEnumerable<GreenJsonMultiValueSyntax>(CreateMultiValue(new GreenJsonStringLiteralSyntax("x", 1)))));
        }

        private const int SharedInstanceLengthMinusTwo = GreenJsonWhitespaceSyntax.SharedInstanceLength - 2;
        private const int SharedInstanceLengthMinusOne = GreenJsonWhitespaceSyntax.SharedInstanceLength - 1;
        private const int SharedInstanceLengthPlusOne = GreenJsonWhitespaceSyntax.SharedInstanceLength + 1;
        private const int SharedInstanceLengthPlusTwo = GreenJsonWhitespaceSyntax.SharedInstanceLength + 2;

        [Theory]
        [InlineData(1)]
        [InlineData(SharedInstanceLengthMinusTwo)]
        [InlineData(SharedInstanceLengthMinusOne)]
        [InlineData(GreenJsonWhitespaceSyntax.SharedInstanceLength)]
        [InlineData(SharedInstanceLengthPlusOne)]
        [InlineData(SharedInstanceLengthPlusTwo)]
        public void SharedInstanceHasCorrectLength(int length)
        {
            Assert.Equal(length, GreenJsonWhitespaceSyntax.Create(length).Length);
            Assert.Equal(length, GreenJsonCommentSyntax.Create(length).Length);
        }

        [Fact]
        public void JsonSymbolsWithLengthOne()
        {
            // As long as there's code around which depends on these symbols having length 1, this unit test is needed.
            Assert.Equal(1, GreenJsonColonSyntax.Value.Length);
            Assert.Equal(1, GreenJsonCommaSyntax.Value.Length);
            Assert.Equal(1, GreenJsonCurlyCloseSyntax.Value.Length);
            Assert.Equal(1, GreenJsonCurlyOpenSyntax.Value.Length);
            Assert.Equal(1, GreenJsonSquareBracketCloseSyntax.Value.Length);
            Assert.Equal(1, GreenJsonSquareBracketOpenSyntax.Value.Length);
            Assert.Equal(1, GreenJsonUnknownSymbolSyntax.Value.Length);
        }

        [Theory]
        [InlineData("\"", 10)]
        [InlineData("{}", 3)]
        // No newline conversions.
        [InlineData("\n", 1)]
        [InlineData("\r\n", 2)]
        public void UnchangedStringLiteralValueParameter(string value, int length)
        {
            // Length includes quotes for json strings.
            var jsonString = new GreenJsonStringLiteralSyntax(value, length + 2);
            Assert.Equal(value, jsonString.Value);
            Assert.Equal(length + 2, jsonString.Length);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(255)]
        [InlineData(2000)]
        public void UnchangedParametersInUnterminatedMultiLineComment(int commentTextLength)
        {
            var symbol = new GreenJsonUnterminatedMultiLineCommentSyntax(commentTextLength);
            Assert.Equal(commentTextLength, symbol.Length);
        }

        [Theory]
        [InlineData("false", false)]
        [InlineData("true", true)]
        public void BooleanJsonValues(string value, bool expectedBooleanValue)
        {
            var node = JsonValue.TryCreate(value);
            GreenJsonBooleanLiteralSyntax boolNode;
            if (expectedBooleanValue)
            {
                boolNode = Assert.IsType<GreenJsonBooleanLiteralSyntax.True>(node);
            }
            else
            {
                boolNode = Assert.IsType<GreenJsonBooleanLiteralSyntax.False>(node);
            }
            Assert.Equal(expectedBooleanValue, boolNode.Value);
            Assert.Equal(value, boolNode.LiteralJsonValue);
            Assert.Equal(value.Length, boolNode.Length);
        }

        public static IEnumerable<object[]> GetIntegerJsonValues()
        {
            yield return new object[] { "0", 0 };
            yield return new object[] { "+1", 1 };
            yield return new object[] { "-0", 0 };
            yield return new object[] { "-1", -1 };
            yield return new object[] { "-9", -9 };
            yield return new object[] { "20", 20 };
            yield return new object[] { "2147483647", 2147483647 };
            yield return new object[] { "+2147483647", 2147483647 };
            yield return new object[] { "000002147483647", 2147483647 };
            yield return new object[] { "-2147483648", -2147483648 };
            yield return new object[] { "-000002147483648", -2147483648 };
        }

        [Theory]
        [MemberData(nameof(GetIntegerJsonValues))]
        public void IntegerJsonValues(string value, int expectedIntegerValue)
        {
            var node = JsonValue.TryCreate(value);
            var intNode = Assert.IsType<GreenJsonIntegerLiteralSyntax>(node);
            Assert.Equal(expectedIntegerValue, (int)intNode.Value);
            Assert.Equal(value.Length, intNode.Length);
        }

        private static IEnumerable<(string, ulong)> UnsignedLongValues
        {
            get
            {
                yield return ("0000021474836470", 21474836470);
                yield return ("18446744073709551599", 18446744073709551599);
                yield return ("18446744073709551600", 18446744073709551600);
                yield return ("18446744073709551609", 18446744073709551609);
                yield return ("18446744073709551610", 18446744073709551610);
                yield return ("18446744073709551615", 18446744073709551615);
            }
        }

        public static IEnumerable<object[]> GetUnsignedLongJsonValues()
            => from x in UnsignedLongValues select new object[] { x.Item1, x.Item2 };

        [Theory]
        [MemberData(nameof(GetUnsignedLongJsonValues))]
        public void UnsignedLongJsonValues(string value, ulong expectedIntegerValue)
        {
            var node = JsonValue.TryCreate(value);
            var intNode = Assert.IsType<GreenJsonIntegerLiteralSyntax>(node);
            Assert.Equal(expectedIntegerValue, (ulong)intNode.Value);
            Assert.Equal(value.Length, intNode.Length);
        }

        [Fact]
        public void BigIntegerJsonValue()
        {
            // ulong.MaxValue + 1
            var node = JsonValue.TryCreate("18446744073709551616");
            var intNode = Assert.IsType<GreenJsonIntegerLiteralSyntax>(node);
            BigInteger bigInteger = intNode.Value - 1;
            Assert.Equal(ulong.MaxValue, (ulong)bigInteger);
        }

        [Theory]
        [InlineData("++0")]
        [InlineData("--0")]
        [InlineData("0.0")]
        [InlineData("1e+1")]
        [InlineData("{}")]
        [InlineData("\"\"")]
        public void UnknownJsonValues(string value)
        {
            // Assert that none of these create a legal json value.
            Assert.Null(JsonValue.TryCreate(value));
        }

        [Fact]
        public void KeyValueSyntaxPropertiesAreConsistent()
        {
            GreenJsonStringLiteralSyntax stringLiteral = new GreenJsonStringLiteralSyntax("x", 1);
            GreenJsonIntegerLiteralSyntax integerLiteral = new GreenJsonIntegerLiteralSyntax(1, 1);

            GreenJsonKeyValueSyntax keyValue = new GreenJsonKeyValueSyntax(
                stringLiteral,
                new GreenJsonMultiValueSyntax[]
                {
                    CreateMultiValue(stringLiteral),
                    CreateMultiValue(integerLiteral),
                });

            // The first GreenJsonMultiValueSyntax must be exactly equal to the valid key.
            Assert.True(keyValue.ValidKey.IsJust(out GreenJsonStringLiteralSyntax validKey));
            Assert.Same(stringLiteral, validKey);
            Assert.Same(stringLiteral, keyValue.KeyNode.ValueNode.ContentNode);

            // The second GreenJsonMultiValueSyntax must be exactly equal to the first value node.
            Assert.True(keyValue.FirstValueNode.IsJust(out GreenJsonMultiValueSyntax firstValueNode));
            Assert.Same(integerLiteral, firstValueNode.ValueNode.ContentNode);
        }

        [Fact]
        public void ListSyntaxPropertiesAreConsistent()
        {
            GreenJsonIntegerLiteralSyntax integerLiteral = new GreenJsonIntegerLiteralSyntax(1, 1);
            GreenJsonIntegerLiteralSyntax integerLiteral2 = new GreenJsonIntegerLiteralSyntax(1, 2);

            GreenJsonListSyntax list1 = new GreenJsonListSyntax(
                new GreenJsonMultiValueSyntax[]
                {
                    CreateMultiValue(integerLiteral),
                    CreateMultiValue(integerLiteral2),
                },
                false);

            Assert.Equal(2, list1.FilteredListItemNodeCount);

            GreenJsonListSyntax list2 = new GreenJsonListSyntax(
                new GreenJsonMultiValueSyntax[]
                {
                    CreateMultiValue(integerLiteral),
                    CreateMultiValue(GreenJsonMissingValueSyntax.Value),
                },
                false);

            Assert.Equal(1, list2.FilteredListItemNodeCount);
        }

        [Theory]
        [InlineData('*', 0)]
        [InlineData('€', 0)]
        [InlineData('≥', 0)]

        [InlineData('▓', 200)]
        public void UnexpectedSymbolError(char unexpectedCharacter, int position)
        {
            var error = JsonParseErrors.UnexpectedSymbol(unexpectedCharacter, position);
            Assert.NotNull(error);
            Assert.Equal(JsonErrorCode.UnexpectedSymbol, error.ErrorCode);
            JsonErrorTests.AssertErrorInfoParameters(error, unexpectedCharacter.ToString());
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2999, 3000)]
        [InlineData(0, int.MaxValue)]
        public void UnterminatedMultiLineCommentError(int start, int length)
        {
            var error = JsonParseErrors.UnterminatedMultiLineComment(start, length);
            Assert.NotNull(error);
            Assert.Equal(JsonErrorCode.UnterminatedMultiLineComment, error.ErrorCode);
            JsonErrorTests.AssertErrorInfoParameters(error);
            Assert.Equal(start, error.Start);
            Assert.Equal(length, error.Length);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2999, 3000)]
        [InlineData(0, int.MaxValue)]
        public void UnterminatedStringError(int start, int length)
        {
            var error = JsonParseErrors.UnterminatedString(start, length);
            Assert.NotNull(error);
            Assert.Equal(JsonErrorCode.UnterminatedString, error.ErrorCode);
            JsonErrorTests.AssertErrorInfoParameters(error);
            Assert.Equal(start, error.Start);
            Assert.Equal(length, error.Length);
        }

        [Theory]
        [InlineData('\u007f', "\\u007f", 1)]
        [InlineData('\n', "\\n", 70)]
        [InlineData('\0', "\\u0000", 1)]
        public void IllegalControlCharacterInStringError(char illegalControlCharacter, string expectedDisplayString, int position)
        {
            var error = JsonParseErrors.IllegalControlCharacterInString(illegalControlCharacter, position);
            Assert.Equal(JsonErrorCode.IllegalControlCharacterInString, error.ErrorCode);
            JsonErrorTests.AssertErrorInfoParameters(error, expectedDisplayString);
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        [Theory]
        [InlineData("\\ ", 2)]
        [InlineData("\\0", 1)]
        [InlineData("\\u", 0)]
        [InlineData("\\u00", 1)]
        [InlineData("\\uffff", 1)]
        public void UnrecognizedEscapeSequenceError(string escapeSequence, int position)
        {
            var error = JsonParseErrors.UnrecognizedEscapeSequence(escapeSequence, position, escapeSequence.Length);
            Assert.Equal(JsonErrorCode.UnrecognizedEscapeSequence, error.ErrorCode);
            JsonErrorTests.AssertErrorInfoParameters(error, escapeSequence);
            Assert.Equal(position, error.Start);
            Assert.Equal(escapeSequence.Length, error.Length);
        }
    }
}
