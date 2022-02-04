#region License
/*********************************************************************************
 * PType.List.cs
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
                List<JsonErrorInfo> errors,
                out ItemT convertedTargetValue,
                out PValue value)
            {
                GreenJsonValueSyntax itemNode = jsonListSyntax.ListItemNodes[itemIndex].ValueNode.ContentNode;

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
                GreenJsonValueSyntax valueNode,
                out T convertedValue,
                int valueNodeStartPosition,
                List<JsonErrorInfo> errors)
            {
                if (valueNode is GreenJsonListSyntax jsonListSyntax)
                {
                    return TryCreateFromList(json, jsonListSyntax, out convertedValue, valueNodeStartPosition, errors);
                }

                convertedValue = default;
                return ListTypeError;
            }

            internal abstract Union<ITypeErrorBuilder, PValue> TryCreateFromList(
                string json,
                GreenJsonListSyntax jsonListSyntax,
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
                List<JsonErrorInfo> errors,
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
    }
}
