#region License
/*********************************************************************************
 * Union.Generate.cs
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

#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion
{
    public static class Union
    {
        public const int MaxOptionsToGenerate = 4;

        public static string GeneratedCode { get; private set; }

        private static readonly string ClassName = nameof(Union);

        private static IEnumerable<string> GenerateList(int count, Func<int, string> generator)
            => GenerateList(1, count, generator);

        private static IEnumerable<string> GenerateList(int start, int count, Func<int, string> generator)
            => Enumerable.Range(start, count).Select(generator);

        private static string ConcatList(int count, Func<int, string> generator)
            => string.Concat(GenerateList(count, generator));

        private static string CommaSeparatedList(int count, Func<int, string> generator)
            => string.Join(", ", GenerateList(count, generator));

        private static string Cardinal(int number)
            => number == 1 ? "one"
            : number == 2 ? "two"
            : number == 3 ? "three"
            : number == 4 ? "four"
            : Convert.ToString(number);

        private static string Ordinal(int number)
            => number == 1 ? "first"
            : number == 2 ? "second"
            : number == 3 ? "third"
            : number == 4 ? "fourth"
            : Convert.ToString(number);

        private static string TypeParameter(int option)
            => $"T{option}";

        private static string TypeParameters(int optionCount)
            => $"{CommaSeparatedList(optionCount, TypeParameter)}";

        private static string SubClassName(int option)
            => $"ValueOfType{option}";

        private static string OptionMethodName(int option)
            => $"Option{option}";

        private static string IsOptionMethodName(int option)
            => $"IsOption{option}";

        private static string ToOptionMethodName(int option)
            => $"ToOption{option}";

        private static string WhenOptionParamName(int option)
            => $"whenOption{option}";

        private static string WhenOptionParamRef(int option)
            => $@"<paramref name=""{WhenOptionParamName(option)}""/>";

        private static string MatchMethodActionOverloadParameter(int option)
            => $"Action<{TypeParameter(option)}> {WhenOptionParamName(option)} = null,";

        private static string MatchMethodFuncOverloadParameter(int option)
            => $"Func<{TypeParameter(option)}, TResult> {WhenOptionParamName(option)} = null,";

        private static string ClassSummary(int optionCount)
            => $@"
    /// <summary>
    /// Encapsulates a value which can have {Cardinal(optionCount)} different types.
    /// </summary>{ConcatList(optionCount, ClassSummaryTypeParam)}";

        private static string ClassSummaryTypeParam(int option)
            => $@"
    /// <typeparam name=""{TypeParameter(option)}"">
    /// The {Ordinal(option)} type of the value.
    /// </typeparam>";

        private static string ClassHeader(int optionCount)
            => $@"
    public abstract class {ClassName}<{TypeParameters(optionCount)}>
    {{";

        private static Func<int, string> SubClass(int optionCount)
            => option => $@"
        private sealed class {SubClassName(option)} : Union<{TypeParameters(optionCount)}>
        {{
            public readonly {TypeParameter(option)} Value;

            public {SubClassName(option)}({TypeParameter(option)} value) => Value = value;

            public override bool {IsOptionMethodName(option)}(out {TypeParameter(option)} value)
            {{
                value = Value;
                return true;
            }}

            public override {TypeParameter(option)} {ToOptionMethodName(option)}() => Value;

            public override void Match({ConcatList(optionCount, paramOption => $@"
                {MatchMethodActionOverloadParameter(paramOption)}")}
                Action otherwise = null)
            {{
                if ({WhenOptionParamName(option)} != null) {WhenOptionParamName(option)}(Value);
                else otherwise?.Invoke();
            }}

            public override TResult Match<TResult>({ConcatList(optionCount, paramOption => $@"
                {MatchMethodFuncOverloadParameter(paramOption)}")}
                Func<TResult> otherwise = null)
                => {WhenOptionParamName(option)} != null ? {WhenOptionParamName(option)}(Value)
                : otherwise != null ? otherwise()
                : default;
        }}
";

        private static string PrivateConstructor()
            => $@"
        private {ClassName}() {{ }}
";

        private static Func<int, string> PublicConstructor(int optionCount)
            => option => $@"
        /// <summary>
        /// Creates a new <see cref=""{ClassName}{{{TypeParameters(optionCount)}}}""/> with a value of the {Ordinal(option)} type.
        /// </summary>
        public static {ClassName}<{TypeParameters(optionCount)}> {OptionMethodName(option)}({TypeParameter(option)} value) => new {SubClassName(option)}(value);
";

        private static Func<int, string> ImplicitCastOperator(int optionCount)
            => option => $@"
        public static implicit operator {ClassName}<{TypeParameters(optionCount)}>({TypeParameter(option)} value) => new {SubClassName(option)}(value);
";

        private static Func<int, string> IsOptionMethod(int optionCount)
            => option => $@"
        /// <summary>
        /// Checks if this <see cref=""{ClassName}{{{TypeParameters(optionCount)}}}""/> contains a value of the {Ordinal(option)} type.
        /// </summary>
        /// <param name=""value"">
        /// The value of the {Ordinal(option)} type, if this function returns true; otherwise a default value.
        /// </param>
        /// <returns>
        /// True if this <see cref=""{ClassName}{{{TypeParameters(optionCount)}}}""/> contains a value of the {Ordinal(option)} type; otherwise false.
        /// </returns>
        public virtual bool {IsOptionMethodName(option)}(out {TypeParameter(option)} value)
        {{
            value = default;
            return false;
        }}
";

        private static Func<int, string> ToOptionMethod(int optionCount)
            => option => $@"
        /// <summary>
        /// Casts this <see cref=""{ClassName}{{{TypeParameters(optionCount)}}}""/> to a value of the {Ordinal(option)} type.
        /// </summary>
        /// <returns>
        /// The value of the {Ordinal(option)} type.
        /// </returns>
        /// <exception cref=""InvalidCastException"">
        /// Occurs when this <see cref=""{ClassName}{{{TypeParameters(optionCount)}}}""/> does not contain a value of the {Ordinal(option)} type.
        /// </exception>
        public virtual {TypeParameter(option)} {ToOptionMethodName(option)}() => throw new InvalidCastException();
";
        private static string MatchMethodActionOverloadSummary()
            => $@"
        /// <summary>
        /// Invokes an <see cref=""Action{{T}}""/> based on the type of the value.
        /// </summary>";

        private static string MatchMethodActionOverloadSummaryParameters(int option)
            => $@"
        /// <param name=""{WhenOptionParamName(option)}"">
        /// The <see cref=""Action{{{TypeParameter(option)}}}""/> to invoke when the value is of the {Ordinal(option)} type.
        /// </param>";

        private static string MatchMethodActionOverload(int optionCount)
            => $@"
        /// <param name=""otherwise"">
        /// The <see cref=""Action""/> to invoke if no action is specified for the type of the value.
        /// If {CommaSeparatedList(optionCount - 1, WhenOptionParamRef)} and {WhenOptionParamRef(optionCount)} are given, this parameter is not used.
        /// </param>
        public abstract void Match({ConcatList(optionCount, option => $@"
            {MatchMethodActionOverloadParameter(option)}")}
            Action otherwise = null);
";

        private static string MatchMethodFuncOverloadSummary()
            => $@"
        /// <summary>
        /// Invokes a <see cref=""Func{{T, TResult}}""/> based on the type of the value and returns its result.
        /// </summary>
        /// <typeparam name=""TResult"">
        /// Type of the value to return.
        /// </typeparam>";

        private static string MatchMethodFuncOverloadSummaryParameters(int option)
            => $@"
        /// <param name=""{WhenOptionParamName(option)}"">
        /// The <see cref=""Func{{{TypeParameter(option)}, TResult}}""/> to invoke when the value is of the {Ordinal(option)} type.
        /// </param>";

        private static string MatchMethodFuncOverload(int optionCount)
            => $@"
        /// <param name=""otherwise"">
        /// The <see cref=""Func{{TResult}}""/> to invoke if no action is specified for the type of the value.
        /// If {CommaSeparatedList(optionCount - 1, WhenOptionParamRef)} and {WhenOptionParamRef(optionCount)} are given, this parameter is not used.
        /// </param>
        /// <returns>
        /// The result of the invoked <see cref=""Func{{T, TResult}}""/>.
        /// </returns>
        public abstract TResult Match<TResult>({ConcatList(optionCount, option => $@"
            {MatchMethodFuncOverloadParameter(option)}")}
            Func<TResult> otherwise = null);
";

        private static string ClassBody(int optionCount)
            => string.Concat(
                ConcatList(optionCount, SubClass(optionCount)),
                PrivateConstructor(),
                ConcatList(optionCount, PublicConstructor(optionCount)),
                ConcatList(optionCount, ImplicitCastOperator(optionCount)),
                ConcatList(optionCount, IsOptionMethod(optionCount)),
                ConcatList(optionCount, ToOptionMethod(optionCount)),
                MatchMethodActionOverloadSummary(),
                ConcatList(optionCount, MatchMethodActionOverloadSummaryParameters),
                MatchMethodActionOverload(optionCount),
                MatchMethodFuncOverloadSummary(),
                ConcatList(optionCount, MatchMethodFuncOverloadSummaryParameters),
                MatchMethodFuncOverload(optionCount));

        private static string ClassFooter()
            => $@"    }}
";

        private static string UnionClass(int optionCount)
            => $"{ClassSummary(optionCount)}{ClassHeader(optionCount)}{ClassBody(optionCount)}{ClassFooter()}";

        private static string GenerateCode()
            => $@"
namespace Eutherion
{{{string.Concat(GenerateList(2, MaxOptionsToGenerate - 1, UnionClass))}}}
";

        /// <summary>
        /// Generates code for the Union<> classes.
        /// </summary>
        public static void Generate()
        {
            GeneratedCode = GenerateCode();
        }
    }
}
#endif
