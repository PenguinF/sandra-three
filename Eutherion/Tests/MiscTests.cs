#region License
/*********************************************************************************
 * MiscTests.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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

using Eutherion.Testing;
using Eutherion.Text;
using Eutherion.Text.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Eutherion.Win.Tests
{
    public class MiscTests
    {
        [Fact]
        public void ArgumentChecks()
        {
            Assert.Throws<ArgumentNullException>("parameter", () => JsonErrorInfoParameterDisplayHelper.GetFormattedDisplayValue(null, TextFormatter.Default));
            Assert.Throws<ArgumentNullException>("localizer", () => JsonErrorInfoParameterDisplayHelper.GetFormattedDisplayValue(new JsonErrorInfoParameter<char>('a'), null));

            Assert.Throws<ArgumentNullException>(() => FormatUtilities.SoftFormat(null));
        }

        private sealed class TestFormatter : TextFormatter
        {
            public static readonly string TestNullString = "NULL";
            public static readonly string TestUntypedObjectString = "UNTYPED({0})";

            public override string Format(StringKey<ForFormattedText> key, string[] parameters)
            {
                if (key == JsonErrorInfoParameterDisplayHelper.NullString)
                    return TestNullString;

                if (key == JsonErrorInfoParameterDisplayHelper.UntypedObjectString)
                    return FormatUtilities.SoftFormat(TestUntypedObjectString, parameters);

                // Throw an exception here, no other keys should be used than above 2.
                throw new InvalidOperationException();
            }
        }

        private static IEnumerable<(JsonErrorInfoParameter parameter, string expectedDisplayValue)> ErrorParameterDisplayValuesTestData()
        {
            yield return (new JsonErrorInfoParameter<char>(' '), "' '");
            yield return (new JsonErrorInfoParameter<char>('\n'), "'\\n'");
            yield return (new JsonErrorInfoParameter<char>('\u0000'), "'\\u0000'");
            yield return (new JsonErrorInfoParameter<char>('√'), "'√'");

            yield return (new JsonErrorInfoParameter<string>(null), TestFormatter.TestNullString);
            yield return (new JsonErrorInfoParameter<string>(""), "\"\"");
            yield return (new JsonErrorInfoParameter<string>("x"), "\"x\"");
            yield return (new JsonErrorInfoParameter<string>("      "), "\"      \"");

            yield return (new JsonErrorInfoParameter<bool?>(null), TestFormatter.TestNullString);
            yield return (new JsonErrorInfoParameter<bool?>(false), string.Format(TestFormatter.TestUntypedObjectString, bool.FalseString));

            yield return (new JsonErrorInfoParameter<int?>(null), TestFormatter.TestNullString);
            yield return (new JsonErrorInfoParameter<int?>(0), string.Format(TestFormatter.TestUntypedObjectString, 0));
        }

        public static IEnumerable<object[]> WrappedErrorParameterDisplayValuesTestData() => TestUtilities.Wrap(ErrorParameterDisplayValuesTestData());

        [Theory]
        [MemberData(nameof(WrappedErrorParameterDisplayValuesTestData))]
        public void ErrorParameterDisplayValues(JsonErrorInfoParameter parameter, string expectedDisplayValue)
        {
            Assert.Equal(expectedDisplayValue, JsonErrorInfoParameterDisplayHelper.GetFormattedDisplayValue(parameter, new TestFormatter()));
        }

        private static IEnumerable<(string format, string[] parameters, string expectedResult)> SoftFormatParameterCases()
        {
            // Invalid format strings should revert to displaying the parameter list.
            yield return ("{a}", null, "{a}");
            yield return ("{a}", new string[] { "x" }, "{a}(x)");
            yield return ("{a}", new string[] { "x", "2" }, "{a}(x, 2)");
            yield return ("{0:}}", null, "{0:}}");
            yield return ("{0:}}", new string[] { "x" }, "{0:}}(x)");
            yield return ("{0:}}", new string[] { "x", "2" }, "{0:}}(x, 2)");
            yield return ("{-1}", null, "{-1}");

            // Edge cases when format string is empty.
            yield return ("", null, "");
            yield return ("", new string[] { "x" }, "");
            yield return ("", new string[] { "x", "2" }, "");

            // Behave like string.Format for sufficient number of parameters.
            yield return ("z{0,10}z", new string[] { "abcdef" }, "z    abcdefz");
            yield return ("z{0,-10}z", new string[] { "abcdef" }, "zabcdef    z");
            yield return ("z{0,10:x2}z", new string[] { "abcdef" }, "z    abcdefz");
            yield return ("z{0,-10:x2}z", new string[] { "abcdef" }, "zabcdef    z");

            // Substitution points should be removed if null or insufficient parameters are provided.
            yield return ("z{0}z", null, "zz");
            yield return ("z{10}z", null, "zz");
            yield return ("z{0}z", Array.Empty<string>(), "zz");
            yield return ("z{10}z", Array.Empty<string>(), "zz");
            yield return ("z{10}z", new string[] { "a", "b", "c" }, "zz");
            yield return ("z{1}z{3}z", new string[] { "a", "b", "c" }, "zbzz");
        }

        public static IEnumerable<object[]> WrappedSoftFormatParameterCases() => TestUtilities.Wrap(SoftFormatParameterCases());

        [Theory]
        [MemberData(nameof(WrappedSoftFormatParameterCases))]
        public void SoftFormats(string format, string[] parameters, string expectedResult)
        {
            Assert.Equal(expectedResult, FormatUtilities.SoftFormat(format, parameters));
        }
    }
}
