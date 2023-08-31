﻿#region License
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
                JsonListSyntax jsonListSyntax,
                int itemIndex,
                ArrayBuilder<PTypeError> errors,
                out ItemT convertedTargetValue,
                out PValue value)
            {
                JsonValueSyntax itemNode = jsonListSyntax.ListItemNodes[itemIndex].ValueNode;
                var itemValueOrError = itemType.TryCreateValue(itemNode, out convertedTargetValue, errors);

                if (itemValueOrError.IsOption2(out value))
                {
                    return true;
                }

                // Report type error at this index.
                itemValueOrError.IsOption1(out ITypeErrorBuilder itemTypeError);
                errors.Add(new ValueTypeErrorAtItemIndex(itemTypeError, itemIndex, itemNode));
                return false;
            }

            internal sealed override Union<ITypeErrorBuilder, PValue> TryCreateValue(
                JsonValueSyntax valueNode,
                out T convertedValue,
                ArrayBuilder<PTypeError> errors)
            {
                if (valueNode is JsonListSyntax jsonListSyntax)
                {
                    return TryCreateFromList(jsonListSyntax, out convertedValue, errors).Match(
                        whenOption1: error => Union<ITypeErrorBuilder, PValue>.Option1(error),
                        whenOption2: list => list);
                }

                convertedValue = default;
                return ListTypeError;
            }

            internal abstract Union<ITypeErrorBuilder, PList> TryCreateFromList(
                JsonListSyntax jsonListSyntax,
                out T convertedValue,
                ArrayBuilder<PTypeError> errors);

            public sealed override Maybe<T> TryConvert(PValue value)
                => value is PList list ? TryConvertFromList(list) : Maybe<T>.Nothing;

            public abstract Maybe<T> TryConvertFromList(PList list);

            public sealed override PValue ConvertToPValue(T value) => ConvertToPList(value);

            public abstract PList ConvertToPList(T value);
        }

        /// <summary>
        /// A tuple with a fixed number of items greater than one, each item having a defined type.
        /// </summary>
        public abstract class TupleTypeBase<T> : ListBase<T>
        {
            internal static bool TryCreateTupleValue<ItemT>(
                PType<ItemT> itemType,
                JsonListSyntax jsonListSyntax,
                int itemIndex,
                ArrayBuilder<PTypeError> errors,
                out ItemT convertedTargetValue,
                out PValue value)
            {
                if (itemIndex < jsonListSyntax.ListItemNodes.Count)
                {
                    return TryCreateItemValue(
                        itemType,
                        jsonListSyntax,
                        itemIndex,
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
                JsonListSyntax jsonListSyntax,
                out IEnumerable<T> convertedValue,
                ArrayBuilder<PTypeError> errors)
            {
                var validTargetValues = new List<T>();
                var validValues = new List<PValue>();
                int itemNodeCount = jsonListSyntax.ListItemNodes.Count;

                for (int itemIndex = 0; itemIndex < itemNodeCount; itemIndex++)
                {
                    // Error tolerance: ignore items of the wrong type.
                    if (TryCreateItemValue(
                        ItemType,
                        jsonListSyntax,
                        itemIndex,
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

            public override PList ConvertToPList(IEnumerable<T> value) => new PList(value.Select(ItemType.ConvertToPValue));
        }
    }
}
