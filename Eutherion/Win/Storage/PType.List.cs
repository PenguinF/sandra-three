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
                JsonListSyntax jsonListSyntax,
                int itemIndex,
                ArrayBuilder<PTypeError> errors,
                out ItemT value,
                bool allowMissingValues)
            {
                JsonValueSyntax itemNode = jsonListSyntax.ListItemNodes[itemIndex].ValueNode;

                if (allowMissingValues && itemNode is JsonMissingValueSyntax)
                {
                    // Don't report anything. But no conversion either.
                    value = default;
                    return false;
                }

                var itemValueOrError = itemType.TryCreateValue(itemNode, errors);

                if (itemValueOrError.IsOption2(out value)) return true;

                // Report type error at this index.
                itemValueOrError.IsOption1(out ITypeErrorBuilder itemTypeError);
                errors.Add(new ValueTypeErrorAtItemIndex(itemTypeError, itemIndex, itemNode));
                return false;
            }

            internal sealed override Union<ITypeErrorBuilder, T> TryCreateValue(JsonValueSyntax valueNode, ArrayBuilder<PTypeError> errors)
            {
                if (valueNode is JsonListSyntax jsonListSyntax)
                {
                    return TryCreateFromList(jsonListSyntax, errors);
                }

                return ListTypeError;
            }

            internal abstract Union<ITypeErrorBuilder, T> TryCreateFromList(JsonListSyntax jsonListSyntax, ArrayBuilder<PTypeError> errors);

            public sealed override PValue ConvertToPValue(T value) => ConvertToPList(value);

            public abstract PList ConvertToPList(T value);
        }

        /// <summary>
        /// A tuple with a fixed number of items, each item having a defined type.
        /// </summary>
        public abstract class TupleTypeBase<T> : ListBase<T>
        {
            internal static bool TryCreateTupleValue<ItemT>(
                PType<ItemT> itemType,
                JsonListSyntax jsonListSyntax,
                int itemIndex,
                ArrayBuilder<PTypeError> errors,
                out ItemT value)
            {
                if (itemIndex < jsonListSyntax.ListItemNodes.Count)
                {
                    return TryCreateItemValue(
                        itemType,
                        jsonListSyntax,
                        itemIndex,
                        errors,
                        out value,
                        allowMissingValues: false);
                }

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

            public ValueList(PType<T> itemType) => ItemType = itemType;

            internal override Union<ITypeErrorBuilder, IEnumerable<T>> TryCreateFromList(JsonListSyntax jsonListSyntax, ArrayBuilder<PTypeError> errors)
            {
                var validTargetValues = new List<T>();
                int itemNodeCount = jsonListSyntax.ListItemNodes.Count;

                for (int itemIndex = 0; itemIndex < itemNodeCount; itemIndex++)
                {
                    // Error tolerance: ignore items of the wrong type.
                    if (TryCreateItemValue(
                        ItemType,
                        jsonListSyntax,
                        itemIndex,
                        errors,
                        out T value,
                        allowMissingValues: true))
                    {
                        validTargetValues.Add(value);
                    }
                }

                return validTargetValues;
            }

            public override PList ConvertToPList(IEnumerable<T> value) => new PList(value.Select(ItemType.ConvertToPValue));
        }
    }
}
