#region License
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
        public static readonly PTypeErrorBuilder MapTypeError = new PTypeErrorBuilder(JsonObject);

        public abstract class MapBase<T> : PType<T>
        {
            internal MapBase() { }

            internal sealed override Union<ITypeErrorBuilder, PValue> TryCreateValue(
                string json,
                JsonValueSyntax valueNode,
                out T convertedValue,
                int valueNodeStartPosition,
                List<JsonErrorInfo> errors)
            {
                if (valueNode is JsonMapSyntax jsonMapSyntax)
                {
                    return TryCreateFromMap(json, jsonMapSyntax, out convertedValue, valueNodeStartPosition, errors);
                }

                convertedValue = default;
                return MapTypeError;
            }

            internal abstract Union<ITypeErrorBuilder, PValue> TryCreateFromMap(
                string json,
                JsonMapSyntax jsonMapSyntax,
                out T convertedValue,
                int valueNodeStartPosition,
                List<JsonErrorInfo> errors);

            public sealed override Maybe<T> TryConvert(PValue value)
                => value is PMap map ? TryConvertFromMap(map) : Maybe<T>.Nothing;

            public abstract Maybe<T> TryConvertFromMap(PMap map);

            public sealed override PValue GetPValue(T value)
                => GetBaseValue(value);

            public abstract PMap GetBaseValue(T value);
        }

        /// <summary>
        /// A dictionary with an arbitrary number of string keys, and values are all of the same subtype.
        /// </summary>
        public class ValueMap<T> : MapBase<Dictionary<string, T>>
        {
            public PType<T> ItemType { get; }

            public ValueMap(PType<T> itemType)
                => ItemType = itemType;

            internal override Union<ITypeErrorBuilder, PValue> TryCreateFromMap(
                string json,
                JsonMapSyntax jsonMapSyntax,
                out Dictionary<string, T> convertedValue,
                int mapSyntaxStartPosition,
                List<JsonErrorInfo> errors)
            {
                var dictionary = new Dictionary<string, T>();
                var mapBuilder = new Dictionary<string, PValue>();

                foreach (var keyedNode in jsonMapSyntax.MapNodeKeyValuePairs)
                {
                    JsonStringLiteralSyntax keyNode = keyedNode.Key;
                    int keyNodeStart = keyNode.Start;
                    JsonValueSyntax valueNode = keyedNode.Value;
                    int valueNodeStart = valueNode.Start;

                    // Error tolerance: ignore items of the wrong type.
                    var itemValueOrError = ItemType.TryCreateValue(json, valueNode, out T value, valueNodeStart, errors);
                    if (itemValueOrError.IsOption2(out PValue itemValue))
                    {
                        dictionary.Add(keyNode.Value, value);
                        mapBuilder.Add(keyNode.Value, itemValue);
                    }
                    else
                    {
                        itemValueOrError.IsOption1(out ITypeErrorBuilder typeError);
                        errors.Add(ValueTypeErrorAtPropertyKey.Create(typeError, keyNode, valueNode, json, keyNodeStart, valueNodeStart));
                    }
                }

                convertedValue = dictionary;
                return new PMap(mapBuilder);
            }

            public override Maybe<Dictionary<string, T>> TryConvertFromMap(PMap map)
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

            public override PMap GetBaseValue(Dictionary<string, T> value)
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
