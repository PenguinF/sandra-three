/*********************************************************************************
 * ISyntaxRenderTarget.cs
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

namespace SysExtensions.SyntaxRenderer
{
    /// <summary>
    /// Defines methods for a target of a <see cref="SyntaxRenderer{TTerminal}"/>.
    /// </summary>
    public interface ISyntaxRenderTarget
    {
        /// <summary>
        /// Inserts rendered text into the render target.
        /// </summary>
        /// <param name="textPosition">
        /// Position where to insert the rendered text.
        /// </param>
        /// <param name="text">
        /// The rendered text to insert.
        /// </param>
        void InsertText(int textPosition, string text);

        /// <summary>
        /// Removes text from the render target.
        /// </summary>
        /// <param name="textStart">
        /// Start position of the text to remove.
        /// </param>
        /// <param name="textLength">
        /// Length of the text to remove.
        /// </param>
        void RemoveText(int textStart, int textLength);

        /// <summary>
        /// Called by text elements to request being brought into view.
        /// </summary>
        /// <param name="caretPosition">
        /// The position of the caret in the text to bring into view.
        /// </param>
        void BringIntoView(int caretPosition);

        /// <summary>
        /// Occurs when the position of the caret in the text has updated.
        /// </summary>
        event Action<int> CaretPositionChanged;
    }
}
