/*********************************************************************************
 * SettingComment.cs
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
using System;
using System.Collections.Generic;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a comment in a setting file.
    /// </summary>
    public class SettingComment
    {
        public IEnumerable<string> Paragraphs { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingComment"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> is null.
        /// </exception>
        public SettingComment(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            Paragraphs = new string[] { text };
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingComment"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="paragraphs"/> is null.
        /// </exception>
        public SettingComment(params string[] paragraphs)
        {
            if (paragraphs == null) throw new ArgumentNullException(nameof(paragraphs));
            Paragraphs = paragraphs;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingComment"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="paragraphs"/> is null.
        /// </exception>
        public SettingComment(IEnumerable<string> paragraphs)
        {
            if (paragraphs == null) throw new ArgumentNullException(nameof(paragraphs));
            Paragraphs = paragraphs;
        }
    }
}
