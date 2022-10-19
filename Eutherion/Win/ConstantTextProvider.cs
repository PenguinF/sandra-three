#region License
/*********************************************************************************
 * ConstantTextProvider.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

namespace Eutherion.Win
{
    /// <summary>
    /// <see cref="ITextProvider"/> which always provides the same text to a UI element.
    /// </summary>
    public class ConstantTextProvider : IFunc<string>
    {
        /// <summary>
        /// Gets the <see cref="ConstantTextProvider"/> which returns an empty string.
        /// </summary>
        public static readonly ConstantTextProvider Empty = new ConstantTextProvider();

        /// <summary>
        /// Gets the text from this text provider.
        /// </summary>
        public string Text { get; }

        private ConstantTextProvider() { Text = string.Empty; }

        /// <summary>
        /// Initializes a new instance of <see cref="ConstantTextProvider"/> with a specified text.
        /// </summary>
        /// <param name="text">
        /// The text for the <see cref="ConstantTextProvider"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="text"/> is empty.
        /// </exception>
        public ConstantTextProvider(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException($"{nameof(text)} is empty", nameof(text));
        }

        string IFunc<string>.Eval() => Text;
    }
}
