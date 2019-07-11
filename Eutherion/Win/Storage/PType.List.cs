#region License
/*********************************************************************************
 * PType.List.cs
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

using Eutherion.Text.Json;
using Eutherion.Utils;
using System.Collections.Generic;

namespace Eutherion.Win.Storage
{
    public static partial class PType
    {
        public static readonly PTypeErrorBuilder ListTypeError = new PTypeErrorBuilder(JsonArray);

        private static readonly PTypeErrorBuilder TupleItemTypeMismatchError
            = new PTypeErrorBuilder(PTypeErrorBuilder.TupleItemTypeMismatchError);

        public abstract class ListBase<T> : PType<T>
        {
            internal static bool TryCreateItemValue<ItemT>(
                PType<ItemT> itemType,
                string json,
                JsonListSyntax jsonListSyntax,
                int itemIndex,
                int listSyntaxStartPosition,
                List<JsonErrorInfo> errors,
                out ItemT convertedTargetValue,
                out PValue value)
            {
                JsonValueSyntax itemNode = jsonListSyntax.ListItemNodes[itemIndex].ValueNode.ContentNode;

                int itemNodeStart = listSyntaxStartPosition
                                  + jsonListSyntax.GetElementNodeStart(itemIndex)
                                  + jsonListSyntax.ListItemNodes[itemIndex].ValueNode.BackgroundBefore.Length;

                return itemType.TryCreateValue(
                    json,
                    itemNode,
                    out convertedTargetValue,
                    itemNodeStart,
                    errors).IsOption2(out value);
            }

            internal sealed override Union<ITypeErrorBuilder, PValue> TryCreateValue(
                string json,
                JsonValueSyntax valueNode,
                out T convertedValue,
                int valueNodeStartPosition,
                List<JsonErrorInfo> errors)
            {
                if (valueNode is JsonListSyntax jsonListSyntax)
                {
                    return TryCreateFromList(json, jsonListSyntax, out convertedValue, valueNodeStartPosition, errors);
                }

                convertedValue = default;
                return ListTypeError;
            }

            internal abstract Union<ITypeErrorBuilder, PValue> TryCreateFromList(
                string json,
                JsonListSyntax jsonListSyntax,
                out T convertedValue,
                int listSyntaxStartPosition,
                List<JsonErrorInfo> errors);

            public sealed override Maybe<T> TryConvert(PValue value)
                => value is PList list ? TryConvertFromList(list) : Maybe<T>.Nothing;

            public abstract Maybe<T> TryConvertFromList(PList list);

            public sealed override PValue GetPValue(T value)
                => GetBaseValue(value);

            public abstract PList GetBaseValue(T value);
        }

        public sealed class TupleType<T1, T2> : ListBase<(T1, T2)>
        {
            public const int ExpectedItemCount = 2;

            public (PType<T1>, PType<T2>) ItemTypes { get; }

            public TupleType((PType<T1>, PType<T2>) itemTypes)
                => ItemTypes = itemTypes;

            internal override Union<ITypeErrorBuilder, PValue> TryCreateFromList(
                string json,
                JsonListSyntax jsonListSyntax,
                out (T1, T2) convertedValue,
                int listSyntaxStartPosition,
                List<JsonErrorInfo> errors)
            {
                if (jsonListSyntax.FilteredListItemNodeCount == ExpectedItemCount
                    && TryCreateItemValue(ItemTypes.Item1, json, jsonListSyntax, 0, listSyntaxStartPosition, errors, out T1 value1, out PValue itemValue1)
                    && TryCreateItemValue(ItemTypes.Item2, json, jsonListSyntax, 1, listSyntaxStartPosition, errors, out T2 value2, out PValue itemValue2))
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

        public sealed class TupleType<T1, T2, T3, T4, T5> : ListBase<(T1, T2, T3, T4, T5)>
        {
            public const int ExpectedItemCount = 5;

            public (PType<T1>, PType<T2>, PType<T3>, PType<T4>, PType<T5>) ItemTypes { get; }

            public TupleType((PType<T1>, PType<T2>, PType<T3>, PType<T4>, PType<T5>) itemTypes)
                => ItemTypes = itemTypes;

            internal override Union<ITypeErrorBuilder, PValue> TryCreateFromList(
                string json,
                JsonListSyntax jsonListSyntax,
                out (T1, T2, T3, T4, T5) convertedValue,
                int listSyntaxStartPosition,
                List<JsonErrorInfo> errors)
            {
                if (jsonListSyntax.FilteredListItemNodeCount == ExpectedItemCount
                    && TryCreateItemValue(ItemTypes.Item1, json, jsonListSyntax, 0, listSyntaxStartPosition, errors, out T1 value1, out PValue itemValue1)
                    && TryCreateItemValue(ItemTypes.Item2, json, jsonListSyntax, 1, listSyntaxStartPosition, errors, out T2 value2, out PValue itemValue2)
                    && TryCreateItemValue(ItemTypes.Item3, json, jsonListSyntax, 2, listSyntaxStartPosition, errors, out T3 value3, out PValue itemValue3)
                    && TryCreateItemValue(ItemTypes.Item4, json, jsonListSyntax, 3, listSyntaxStartPosition, errors, out T4 value4, out PValue itemValue4)
                    && TryCreateItemValue(ItemTypes.Item5, json, jsonListSyntax, 4, listSyntaxStartPosition, errors, out T5 value5, out PValue itemValue5))
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
