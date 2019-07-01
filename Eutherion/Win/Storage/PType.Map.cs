﻿#region License
/*********************************************************************************
 * PType.Map.cs
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
        /// <summary>
        /// A dictionary with an arbitrary number of string keys, and values are all of the same subtype.
        /// </summary>
        public class ValueMap<T> : PType<Dictionary<string, T>>
        {
            public PType<T> ItemType { get; }

            public ValueMap(PType<T> itemType)
                => ItemType = itemType;

            internal override Union<ITypeErrorBuilder, PValue> TryCreateValue(
                string json,
                JsonSyntaxNode valueNode,
                out Dictionary<string, T> convertedValue,
                List<JsonErrorInfo> errors)
            {
                if (valueNode is JsonMapSyntax jsonMapSyntax)
                {
                    var mapBuilder = new Dictionary<string, PValue>();
                    var dictionary = new Dictionary<string, T>();

                    foreach (var keyedNode in jsonMapSyntax.MapNodeKeyValuePairs)
                    {
                        // Error tolerance: ignore items of the wrong type.
                        var itemValueOrError = ItemType.TryCreateValue(json, keyedNode.Value, out T value, errors);
                        if (itemValueOrError.IsOption2(out PValue itemValue))
                        {
                            mapBuilder.Add(keyedNode.Key.Value, itemValue);
                            dictionary.Add(keyedNode.Key.Value, value);
                        }
                        else
                        {
                            itemValueOrError.IsOption1(out ITypeErrorBuilder typeError);
                            errors.Add(ValueTypeErrorAtPropertyKey.Create(typeError, keyedNode.Key, keyedNode.Value, json));
                        }
                    }

                    convertedValue = dictionary;
                    return new PMap(mapBuilder);
                }

                convertedValue = default;
                return MapTypeError;
            }

            public override Maybe<Dictionary<string, T>> TryConvert(PValue value)
            {
                if (value is PMap map)
                {
                    var dictionary = new Dictionary<string, T>();

                    foreach (var kv in map)
                    {
                        // Error tolerance: ignore items of the wrong type.
                        if (ItemType.TryConvert(kv.Value).IsJust(out T itemValue))
                        {
                            dictionary.Add(kv.Key, itemValue);
                        }
                    }

                    return dictionary;
                }

                return Maybe<Dictionary<string, T>>.Nothing;
            }

            public override PValue GetPValue(Dictionary<string, T> value)
            {
                var dictionary = new Dictionary<string, PValue>();

                foreach (var kv in value)
                {
                    dictionary.Add(kv.Key, ItemType.GetPValue(kv.Value));
                }

                return new PMap(dictionary);
            }
        }
    }
}