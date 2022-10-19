#region License
/*********************************************************************************
 * MiscTests.cs
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
            Assert.Throws<ArgumentNullException>("parameter", () => JsonErrorInfoParameterDisplayHelper.GetLocalizedDisplayValue(null, TextFormatter.Default));
            Assert.Throws<ArgumentNullException>("localizer", () => JsonErrorInfoParameterDisplayHelper.GetLocalizedDisplayValue(new JsonErrorInfoParameter<char>('a'), null));

            Assert.Throws<ArgumentNullException>(() => FormatUtilities.SoftFormat(null));
        }

        private sealed class TestLocalizer : TextFormatter
        {
            public static readonly string TestNullString = "NULL";
            public static readonly string TestUntypedObjectString = "UNTYPED({0})";

            public override string Format(StringKey<ForFormattedText> localizedStringKey, string[] parameters)
            {
                if (localizedStringKey == JsonErrorInfoParameterDisplayHelper.NullString)
                    return TestNullString;

                if (localizedStringKey == JsonErrorInfoParameterDisplayHelper.UntypedObjectString)
                    return FormatUtilities.SoftFormat(TestUntypedObjectString, parameters);

                // Throw an exception here, no other keys should be used than above 2.
                throw new InvalidOperationException();
            }
        }

        public static IEnumerable<object[]> ErrorParameterDisplayValuesTestData()
        {
            yield return new object[] { new JsonErrorInfoParameter<char>(' '), "' '" };
            yield return new object[] { new JsonErrorInfoParameter<char>('\n'), "'\\n'" };
            yield return new object[] { new JsonErrorInfoParameter<char>('\u0000'), "'\\u0000'" };
            yield return new object[] { new JsonErrorInfoParameter<char>('√'), "'√'" };

            yield return new object[] { new JsonErrorInfoParameter<string>(null), TestLocalizer.TestNullString };
            yield return new object[] { new JsonErrorInfoParameter<string>(""), "\"\"" };
            yield return new object[] { new JsonErrorInfoParameter<string>("x"), "\"x\"" };
            yield return new object[] { new JsonErrorInfoParameter<string>("      "), "\"      \"" };

            yield return new object[] { new JsonErrorInfoParameter<bool?>(null), TestLocalizer.TestNullString };
            yield return new object[] { new JsonErrorInfoParameter<bool?>(false), string.Format(TestLocalizer.TestUntypedObjectString, bool.FalseString) };

            yield return new object[] { new JsonErrorInfoParameter<int?>(null), TestLocalizer.TestNullString };
            yield return new object[] { new JsonErrorInfoParameter<int?>(0), string.Format(TestLocalizer.TestUntypedObjectString, 0) };
        }

        [Theory]
        [MemberData(nameof(ErrorParameterDisplayValuesTestData))]
        public void ErrorParameterDisplayValues(JsonErrorInfoParameter parameter, string expectedDisplayValue)
        {
            Assert.Equal(expectedDisplayValue, JsonErrorInfoParameterDisplayHelper.GetLocalizedDisplayValue(parameter, new TestLocalizer()));
        }

        public static object[][] SoftFormatParameterCases() => new object[][]
        {
            // Invalid format strings should revert to displaying the parameter list.
            new object[] { "{a}", null, "{a}" },
            new object[] { "{a}", new string[] { "x" }, "{a}(x)" },
            new object[] { "{a}", new string[] { "x", "2" }, "{a}(x, 2)" },
            new object[] { "{0:}}", null, "{0:}}" },
            new object[] { "{0:}}", new string[] { "x" }, "{0:}}(x)" },
            new object[] { "{0:}}", new string[] { "x", "2" }, "{0:}}(x, 2)" },
            new object[] { "{-1}", null, "{-1}" },

            // Edge cases when format string is empty.
            new object[] { "", null, "" },
            new object[] { "", new string[] { "x" }, "" },
            new object[] { "", new string[] { "x", "2" }, "" },

            // Behave like string.Format for sufficient number of parameters.
            new object[] { "z{0,10}z", new string[] { "abcdef" }, "z    abcdefz" },
            new object[] { "z{0,-10}z", new string[] { "abcdef" }, "zabcdef    z" },
            new object[] { "z{0,10:x2}z", new string[] { "abcdef" }, "z    abcdefz" },
            new object[] { "z{0,-10:x2}z", new string[] { "abcdef" }, "zabcdef    z" },

            // Substitution points should be removed if null or insufficient parameters are provided.
            new object[] { "z{0}z", null, "zz" },
            new object[] { "z{10}z", null, "zz" },
            new object[] { "z{0}z", Array.Empty<string>(), "zz" },
            new object[] { "z{10}z", Array.Empty<string>(), "zz" },
            new object[] { "z{10}z", new string[] { "a", "b", "c" }, "zz" },
            new object[] { "z{1}z{3}z", new string[] { "a", "b", "c" }, "zbzz" },
        };

        [Theory]
        [MemberData(nameof(SoftFormatParameterCases))]
        public void SoftFormats(string format, string[] parameters, string expectedResult)
        {
            Assert.Equal(expectedResult, FormatUtilities.SoftFormat(format, parameters));
        }
    }
}
