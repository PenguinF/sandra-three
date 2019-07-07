#region License
/*********************************************************************************
 * JsonValueWithBackgroundSyntax.cs
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

using System;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a <see cref="JsonValueSyntax"/> node together with the background symbols
    /// which directly precede it in an abstract json syntax tree.
    /// </summary>
    public class JsonValueWithBackgroundSyntax
    {
        /// <summary>
        /// Gets the background symbols which directly precede the content value node.
        /// </summary>
        public JsonBackgroundSyntax BackgroundBefore { get; }

        /// <summary>
        /// Gets the content node containing the actual json value.
        /// </summary>
        public JsonValueSyntax ContentNode { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonValueWithBackgroundSyntax"/>.
        /// </summary>
        /// <param name="backgroundBefore">
        /// The background symbols which directly precede the content value node.
        /// </param>
        /// <param name="contentNode">
        /// The content node containing the actual json value.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="backgroundBefore"/> and/or <paramref name="contentNode"/> are null.
        /// </exception>
        public JsonValueWithBackgroundSyntax(JsonBackgroundSyntax backgroundBefore, JsonValueSyntax contentNode)
        {
            BackgroundBefore = backgroundBefore ?? throw new ArgumentNullException(nameof(backgroundBefore));
            ContentNode = contentNode ?? throw new ArgumentNullException(nameof(contentNode));
            Length = BackgroundBefore.Length + ContentNode.Length;
        }
    }
}
