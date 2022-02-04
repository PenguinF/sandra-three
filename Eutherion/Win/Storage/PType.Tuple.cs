#region License
/*********************************************************************************
 * PType.Tuple.cs
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

namespace Eutherion.Win.Storage
{
    public static partial class PType
    {
        public sealed class TupleType<T1, T2> : TupleTypeBase<(T1, T2)>
        {
            public const int ExpectedItemCount = 2;

            public (PType<T1>, PType<T2>) ItemTypes { get; }

            public TupleType((PType<T1>, PType<T2>) itemTypes)
                => ItemTypes = itemTypes;

            internal override Union<ITypeErrorBuilder, PValue> TryCreateFromList(
                string json,
                GreenJsonListSyntax jsonListSyntax,
                out (T1, T2) convertedValue,
                int listSyntaxStartPosition,
                List<JsonErrorInfo> errors)
            {
                int actualItemCount = jsonListSyntax.FilteredListItemNodeCount;

                if (TryCreateItemValue(ItemTypes.Item1, json, jsonListSyntax, 0, listSyntaxStartPosition, errors, out T1 value1, out PValue itemValue1)
                    && TryCreateItemValue(ItemTypes.Item2, json, jsonListSyntax, 1, listSyntaxStartPosition, errors, out T2 value2, out PValue itemValue2)
                    && actualItemCount == ExpectedItemCount)
                {
                    convertedValue = (value1, value2);
                    return new PList(new[] { itemValue1, itemValue2 });
                }

                convertedValue = default;
                return TupleItemTypeMismatchError;
            }

            public override Maybe<(T1, T2)> TryConvertFromList(PList list)
            {
                if (list.Count == ExpectedItemCount
                    && ItemTypes.Item1.TryConvert(list[0]).IsJust(out T1 value1)
                    && ItemTypes.Item2.TryConvert(list[1]).IsJust(out T2 value2))
                {
                    return (value1, value2);
                }

                return Maybe<(T1, T2)>.Nothing;
            }

            public override PList GetBaseValue((T1, T2) value)
            {
                var (value1, value2) = value;
                return new PList(new[]
                {
                    ItemTypes.Item1.GetPValue(value1),
                    ItemTypes.Item2.GetPValue(value2),
                });
            }
        }

        public sealed class TupleType<T1, T2, T3> : TupleTypeBase<(T1, T2, T3)>
        {
            public const int ExpectedItemCount = 3;

            public (PType<T1>, PType<T2>, PType<T3>) ItemTypes { get; }

            public TupleType((PType<T1>, PType<T2>, PType<T3>) itemTypes)
                => ItemTypes = itemTypes;

            internal override Union<ITypeErrorBuilder, PValue> TryCreateFromList(
                string json,
                GreenJsonListSyntax jsonListSyntax,
                out (T1, T2, T3) convertedValue,
                int listSyntaxStartPosition,
                List<JsonErrorInfo> errors)
            {
                int actualItemCount = jsonListSyntax.FilteredListItemNodeCount;

                if (TryCreateItemValue(ItemTypes.Item1, json, jsonListSyntax, 0, listSyntaxStartPosition, errors, out T1 value1, out PValue itemValue1)
                    && TryCreateItemValue(ItemTypes.Item2, json, jsonListSyntax, 1, listSyntaxStartPosition, errors, out T2 value2, out PValue itemValue2)
                    && TryCreateItemValue(ItemTypes.Item3, json, jsonListSyntax, 2, listSyntaxStartPosition, errors, out T3 value3, out PValue itemValue3)
                    && actualItemCount == ExpectedItemCount)
                {
                    convertedValue = (value1, value2, value3);
                    return new PList(new[] { itemValue1, itemValue2, itemValue3 });
                }

                convertedValue = default;
                return TupleItemTypeMismatchError;
            }

            public override Maybe<(T1, T2, T3)> TryConvertFromList(PList list)
            {
                if (list.Count == ExpectedItemCount
                    && ItemTypes.Item1.TryConvert(list[0]).IsJust(out T1 value1)
                    && ItemTypes.Item2.TryConvert(list[1]).IsJust(out T2 value2)
                    && ItemTypes.Item3.TryConvert(list[2]).IsJust(out T3 value3))
                {
                    return (value1, value2, value3);
                }

                return Maybe<(T1, T2, T3)>.Nothing;
            }

            public override PList GetBaseValue((T1, T2, T3) value)
            {
                var (value1, value2, value3) = value;
                return new PList(new[]
                {
                    ItemTypes.Item1.GetPValue(value1),
                    ItemTypes.Item2.GetPValue(value2),
                    ItemTypes.Item3.GetPValue(value3),
                });
            }
        }

        public sealed class TupleType<T1, T2, T3, T4> : TupleTypeBase<(T1, T2, T3, T4)>
        {
            public const int ExpectedItemCount = 4;

            public (PType<T1>, PType<T2>, PType<T3>, PType<T4>) ItemTypes { get; }

            public TupleType((PType<T1>, PType<T2>, PType<T3>, PType<T4>) itemTypes)
                => ItemTypes = itemTypes;

            internal override Union<ITypeErrorBuilder, PValue> TryCreateFromList(
                string json,
                GreenJsonListSyntax jsonListSyntax,
                out (T1, T2, T3, T4) convertedValue,
                int listSyntaxStartPosition,
                List<JsonErrorInfo> errors)
            {
                int actualItemCount = jsonListSyntax.FilteredListItemNodeCount;

                if (TryCreateItemValue(ItemTypes.Item1, json, jsonListSyntax, 0, listSyntaxStartPosition, errors, out T1 value1, out PValue itemValue1)
                    && TryCreateItemValue(ItemTypes.Item2, json, jsonListSyntax, 1, listSyntaxStartPosition, errors, out T2 value2, out PValue itemValue2)
                    && TryCreateItemValue(ItemTypes.Item3, json, jsonListSyntax, 2, listSyntaxStartPosition, errors, out T3 value3, out PValue itemValue3)
                    && TryCreateItemValue(ItemTypes.Item4, json, jsonListSyntax, 3, listSyntaxStartPosition, errors, out T4 value4, out PValue itemValue4)
                    && actualItemCount == ExpectedItemCount)
                {
                    convertedValue = (value1, value2, value3, value4);
                    return new PList(new[] { itemValue1, itemValue2, itemValue3, itemValue4 });
                }

                convertedValue = default;
                return TupleItemTypeMismatchError;
            }

            public override Maybe<(T1, T2, T3, T4)> TryConvertFromList(PList list)
            {
                if (list.Count == ExpectedItemCount
                    && ItemTypes.Item1.TryConvert(list[0]).IsJust(out T1 value1)
                    && ItemTypes.Item2.TryConvert(list[1]).IsJust(out T2 value2)
                    && ItemTypes.Item3.TryConvert(list[2]).IsJust(out T3 value3)
                    && ItemTypes.Item4.TryConvert(list[3]).IsJust(out T4 value4))
                {
                    return (value1, value2, value3, value4);
                }

                return Maybe<(T1, T2, T3, T4)>.Nothing;
            }

            public override PList GetBaseValue((T1, T2, T3, T4) value)
            {
                var (value1, value2, value3, value4) = value;
                return new PList(new[]
                {
                    ItemTypes.Item1.GetPValue(value1),
                    ItemTypes.Item2.GetPValue(value2),
                    ItemTypes.Item3.GetPValue(value3),
                    ItemTypes.Item4.GetPValue(value4),
                });
            }
        }

        public sealed class TupleType<T1, T2, T3, T4, T5> : TupleTypeBase<(T1, T2, T3, T4, T5)>
        {
            public const int ExpectedItemCount = 5;

            public (PType<T1>, PType<T2>, PType<T3>, PType<T4>, PType<T5>) ItemTypes { get; }

            public TupleType((PType<T1>, PType<T2>, PType<T3>, PType<T4>, PType<T5>) itemTypes)
                => ItemTypes = itemTypes;

            internal override Union<ITypeErrorBuilder, PValue> TryCreateFromList(
                string json,
                GreenJsonListSyntax jsonListSyntax,
                out (T1, T2, T3, T4, T5) convertedValue,
                int listSyntaxStartPosition,
                List<JsonErrorInfo> errors)
            {
                int actualItemCount = jsonListSyntax.FilteredListItemNodeCount;

                if (TryCreateItemValue(ItemTypes.Item1, json, jsonListSyntax, 0, listSyntaxStartPosition, errors, out T1 value1, out PValue itemValue1)
                    && TryCreateItemValue(ItemTypes.Item2, json, jsonListSyntax, 1, listSyntaxStartPosition, errors, out T2 value2, out PValue itemValue2)
                    && TryCreateItemValue(ItemTypes.Item3, json, jsonListSyntax, 2, listSyntaxStartPosition, errors, out T3 value3, out PValue itemValue3)
                    && TryCreateItemValue(ItemTypes.Item4, json, jsonListSyntax, 3, listSyntaxStartPosition, errors, out T4 value4, out PValue itemValue4)
                    && TryCreateItemValue(ItemTypes.Item5, json, jsonListSyntax, 4, listSyntaxStartPosition, errors, out T5 value5, out PValue itemValue5)
                    && actualItemCount == ExpectedItemCount)
                {
                    convertedValue = (value1, value2, value3, value4, value5);
                    return new PList(new[] { itemValue1, itemValue2, itemValue3, itemValue4, itemValue5 });
                }

                convertedValue = default;
                return TupleItemTypeMismatchError;
            }

            public override Maybe<(T1, T2, T3, T4, T5)> TryConvertFromList(PList list)
            {
                if (list.Count == ExpectedItemCount
                    && ItemTypes.Item1.TryConvert(list[0]).IsJust(out T1 value1)
                    && ItemTypes.Item2.TryConvert(list[1]).IsJust(out T2 value2)
                    && ItemTypes.Item3.TryConvert(list[2]).IsJust(out T3 value3)
                    && ItemTypes.Item4.TryConvert(list[3]).IsJust(out T4 value4)
                    && ItemTypes.Item5.TryConvert(list[4]).IsJust(out T5 value5))
                {
                    return (value1, value2, value3, value4, value5);
                }

                return Maybe<(T1, T2, T3, T4, T5)>.Nothing;
            }

            public override PList GetBaseValue((T1, T2, T3, T4, T5) value)
            {
                var (value1, value2, value3, value4, value5) = value;
                return new PList(new[]
                {
                    ItemTypes.Item1.GetPValue(value1),
                    ItemTypes.Item2.GetPValue(value2),
                    ItemTypes.Item3.GetPValue(value3),
                    ItemTypes.Item4.GetPValue(value4),
                    ItemTypes.Item5.GetPValue(value5),
                });
            }
        }
    }
}
