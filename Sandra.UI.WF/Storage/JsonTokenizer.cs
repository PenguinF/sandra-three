﻿#region License
/*********************************************************************************
 * JsonTokenizer.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
#endregion

using System;
using System.Collections.Generic;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Based on https://www.json.org/.
    /// </summary>
    public sealed class JsonTokenizer
    {
        /// <summary>
        /// Gets the JSON which is tokenized.
        /// </summary>
        public string Json { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonTokenizer"/>.
        /// </summary>
        /// <param name="json">
        /// The JSON to tokenize.
        /// </param>
        public JsonTokenizer(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            Json = json;
        }

        /// <summary>
        /// Tokenizes the source <see cref="Json"/> from start to end.
        /// </summary>
        /// <returns>
        /// An enumeration of <see cref="JsonTerminalSymbol"/> instances.
        /// </returns>
        public IEnumerable<JsonTerminalSymbol> TokenizeAll()
        {
            if (Json.Length > 0)
            {
                yield return new JsonUnknownSymbol(Json, 0, Json.Length);
            }
        }
    }
}
