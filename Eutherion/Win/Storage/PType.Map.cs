#region License
/*********************************************************************************
 * PType.Map.cs
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

namespace Eutherion.Win.Storage
{
    public static partial class PType
    {
        public static readonly PTypeErrorBuilder MapTypeError = new PTypeErrorBuilder(JsonObject);

        public abstract class MapBase<T> : PType<T>
        {
            internal MapBase() { }

            internal sealed override Union<ITypeErrorBuilder, T> TryCreateValue(JsonValueSyntax valueNode, ArrayBuilder<PTypeError> errors)
            {
                if (valueNode is JsonMapSyntax jsonMapSyntax)
                {
                    return TryCreateFromMap(jsonMapSyntax, errors);
                }

                return MapTypeError;
            }

            internal abstract Union<ITypeErrorBuilder, T> TryCreateFromMap(JsonMapSyntax jsonMapSyntax, ArrayBuilder<PTypeError> errors);

            public sealed override PValue ConvertToPValue(T value) => ConvertToPMap(value);

            public abstract PMap ConvertToPMap(T value);
        }

        /// <summary>
        /// A dictionary with an arbitrary number of string keys, and values are all of the same subtype.
        /// </summary>
        public sealed class ValueMap<T> : MapBase<Dictionary<string, T>>
        {
            public PType<T> ItemType { get; }

            public ValueMap(PType<T> itemType)
                => ItemType = itemType;

            internal override Union<ITypeErrorBuilder, Dictionary<string, T>> TryCreateFromMap(JsonMapSyntax jsonMapSyntax, ArrayBuilder<PTypeError> errors)
            {
                var dictionary = new Dictionary<string, T>();

                foreach (var (keyNode, valueNode) in jsonMapSyntax.DefinedKeyValuePairs())
                {
                    // Error tolerance: ignore items of the wrong type.
                    var itemValueOrError = ItemType.TryCreateValue(valueNode, errors);

                    if (itemValueOrError.IsOption2(out T value))
                    {
                        dictionary.Add(keyNode.Value, value);
                    }
                    else
                    {
                        ITypeErrorBuilder typeError = itemValueOrError.ToOption1();
                        errors.Add(new ValueTypeErrorAtPropertyKey(typeError, keyNode, valueNode));
                    }
                }

                return dictionary;
            }

            public override PMap ConvertToPMap(Dictionary<string, T> value)
            {
                var dictionary = new Dictionary<string, PValue>();

                foreach (var kv in value)
                {
                    dictionary.Add(kv.Key, ItemType.ConvertToPValue(kv.Value));
                }

                return new PMap(dictionary);
            }
        }
    }
}
