#region License
/*********************************************************************************
 * PType.List.cs
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

using Eutherion.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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
                GreenJsonListSyntax jsonListSyntax,
                int itemIndex,
                int listSyntaxStartPosition,
                ArrayBuilder<PTypeError> errors,
                out ItemT convertedTargetValue,
                out PValue value)
            {
                GreenJsonValueSyntax itemNode = jsonListSyntax.ListItemNodes[itemIndex].ValueNode.ContentNode;

                int itemNodeStart = listSyntaxStartPosition
                                  + jsonListSyntax.GetElementNodeStart(itemIndex)
                                  + jsonListSyntax.ListItemNodes[itemIndex].ValueNode.BackgroundBefore.Length;

                var itemValueOrError = itemType.TryCreateValue(
                    json,
                    itemNode,
                    out convertedTargetValue,
                    itemNodeStart,
                    errors);

                if (itemValueOrError.IsOption2(out value))
                {
                    return true;
                }

                // Report type error at this index.
                itemValueOrError.IsOption1(out ITypeErrorBuilder itemTypeError);
                errors.Add(ValueTypeErrorAtItemIndex.Create(itemTypeError, itemIndex, itemNode, json, itemNodeStart));
                return false;
            }

            internal sealed override Union<ITypeErrorBuilder, PValue> TryCreateValue(
                string json,
                GreenJsonValueSyntax valueNode,
                out T convertedValue,
                int valueNodeStartPosition,
                ArrayBuilder<PTypeError> errors)
            {
                if (valueNode is GreenJsonListSyntax jsonListSyntax)
                {
                    return TryCreateFromList(json, jsonListSyntax, out convertedValue, valueNodeStartPosition, errors).Match(
                        whenOption1: error => Union<ITypeErrorBuilder, PValue>.Option1(error),
                        whenOption2: list => list);
                }

                convertedValue = default;
                return ListTypeError;
            }

            internal abstract Union<ITypeErrorBuilder, PList> TryCreateFromList(
                string json,
                GreenJsonListSyntax jsonListSyntax,
                out T convertedValue,
                int listSyntaxStartPosition,
                ArrayBuilder<PTypeError> errors);

            public sealed override Maybe<T> TryConvert(PValue value)
                => value is PList list ? TryConvertFromList(list) : Maybe<T>.Nothing;

            public abstract Maybe<T> TryConvertFromList(PList list);

            public sealed override PValue GetPValue(T value)
                => GetBaseValue(value);

            public abstract PList GetBaseValue(T value);
        }

        /// <summary>
        /// A tuple with a fixed number of items greater than one, each item having a defined type.
        /// </summary>
        public abstract class TupleTypeBase<T> : ListBase<T>
        {
            internal static bool TryCreateTupleValue<ItemT>(
                PType<ItemT> itemType,
                string json,
                GreenJsonListSyntax jsonListSyntax,
                int itemIndex,
                int errorReportingOffset,
                ArrayBuilder<PTypeError> errors,
                out ItemT convertedTargetValue,
                out PValue value)
            {
                if (itemIndex < jsonListSyntax.FilteredListItemNodeCount)
                {
                    return TryCreateItemValue(
                        itemType,
                        json,
                        jsonListSyntax,
                        itemIndex,
                        errorReportingOffset,
                        errors,
                        out convertedTargetValue,
                        out value);
                }

                convertedTargetValue = default;
                value = default;
                return false;
            }
        }

        /// <summary>
        /// A list with an arbitrary number of items, all of the same subtype.
        /// </summary>
        public sealed class ValueList<T> : ListBase<IEnumerable<T>>
        {
            public PType<T> ItemType { get; }

            public ValueList(PType<T> itemType)
                => ItemType = itemType;

            internal override Union<ITypeErrorBuilder, PList> TryCreateFromList(
                string json,
                GreenJsonListSyntax jsonListSyntax,
                out IEnumerable<T> convertedValue,
                int listSyntaxStartPosition,
                ArrayBuilder<PTypeError> errors)
            {
                var validTargetValues = new List<T>();
                var validValues = new List<PValue>();
                int itemNodeCount = jsonListSyntax.FilteredListItemNodeCount;

                for (int itemIndex = 0; itemIndex < itemNodeCount; itemIndex++)
                {
                    // Error tolerance: ignore items of the wrong type.
                    if (TryCreateItemValue(
                        ItemType,
                        json,
                        jsonListSyntax,
                        itemIndex,
                        listSyntaxStartPosition,
                        errors,
                        out T convertedTargetValue,
                        out PValue value))
                    {
                        validTargetValues.Add(convertedTargetValue);
                        validValues.Add(value);
                    }
                }

                convertedValue = validTargetValues;
                return new PList(validValues);
            }

            public override Maybe<IEnumerable<T>> TryConvertFromList(PList list)
            {
                var validValues = new List<T>();

                for (int i = 0; i < list.Count; i++)
                {
                    // Error tolerance: ignore items of the wrong type.
                    if (ItemType.TryConvert(list[i]).IsJust(out T value))
                    {
                        validValues.Add(value);
                    }
                }

                return validValues;
            }

            public override PList GetBaseValue(IEnumerable<T> value)
                => new PList(value.Select(ItemType.GetPValue));
        }
    }
}
