#region License
/*********************************************************************************
 * PType.Tuple.Generate.cs
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

#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion.Win.Storage
{
    public static partial class PType
    {
        public const int MaxTupleTypesToGenerate = 5;

        public static string GeneratedTupleTypeCode { get; private set; }

        private static IEnumerable<string> GenerateList(int count, Func<int, string> generator)
            => GenerateList(1, count, generator);

        private static IEnumerable<string> GenerateList(int start, int count, Func<int, string> generator)
            => Enumerable.Range(start, count).Select(generator);

        private static string SeparatedList(string separator, int count, Func<int, string> generator)
            => string.Join(separator, GenerateList(count, generator));

        private static string ConcatList(int count, Func<int, string> generator)
            => string.Concat(GenerateList(count, generator));

        private static string CommaSeparatedList(int count, Func<int, string> generator)
            => SeparatedList(", ", count, generator);

        private static string TypeParameter(int index) => $"T{index}";

        private static string ValuePType(int index) => $"PType<T{index}>";

        private static string TupleTypeClass(int size) => $@"
        public sealed class TupleType<{CommaSeparatedList(size, TypeParameter)}> : TupleTypeBase<({CommaSeparatedList(size, TypeParameter)})>
        {{
            public const int ExpectedItemCount = {size};

            public ({CommaSeparatedList(size, ValuePType)}) ItemTypes {{ get; }}

            public TupleType(({CommaSeparatedList(size, ValuePType)}) itemTypes)
                => ItemTypes = itemTypes;

            internal override Union<ITypeErrorBuilder, PList> TryCreateFromList(
                JsonListSyntax jsonListSyntax,
                out ({CommaSeparatedList(size, TypeParameter)}) convertedValue,
                ArrayBuilder<PTypeError> errors)
            {{
                int actualItemCount = jsonListSyntax.ListItemNodes.Count;

                if ({SeparatedList("                    && ", size, i => $@"TryCreateTupleValue(ItemTypes.Item{i}, jsonListSyntax, {i - 1}, errors, out {TypeParameter(i)} value{i}, out PValue itemValue{i})
")}                    && actualItemCount == ExpectedItemCount)
                {{
                    convertedValue = ({CommaSeparatedList(size, i => $"value{i}")});
                    return new PList(new[] {{ {CommaSeparatedList(size, i => $"itemValue{i}")} }});
                }}

                convertedValue = default;
                return TupleItemTypeMismatchError;
            }}

            public override Maybe<({CommaSeparatedList(size, TypeParameter)})> TryConvertFromList(PList list)
            {{
                if (list.Count == ExpectedItemCount{ConcatList(size, i => $@"
                    && ItemTypes.Item{i}.TryConvert(list[{i - 1}]).IsJust(out {TypeParameter(i)} value{i})")})
                {{
                    return ({CommaSeparatedList(size, i => $"value{i}")});
                }}

                return Maybe<({CommaSeparatedList(size, TypeParameter)})>.Nothing;
            }}

            public override PList ConvertToPList(({CommaSeparatedList(size, TypeParameter)}) value)
            {{
                var ({CommaSeparatedList(size, i => $"value{i}")}) = value;
                return new PList(new[]
                {{{ConcatList(size, i => $@"
                    ItemTypes.Item{i}.GetPValue(value{i}),")}
                }});
            }}
        }}
";

        private static string GenerateTupleTypeCode()
            => $@"namespace Eutherion.Win.Storage
{{
    public static partial class PType
    {{{string.Concat(GenerateList(2, MaxTupleTypesToGenerate - 1, TupleTypeClass))}    }}
}}
";

        /// <summary>
        /// Generates code for the TupleType<> classes.
        /// </summary>
        public static void TupleTypeCode()
        {
            GeneratedTupleTypeCode = GenerateTupleTypeCode();
        }
    }
}
#endif
