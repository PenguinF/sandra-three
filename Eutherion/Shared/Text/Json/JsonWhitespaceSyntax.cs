#region License
/*********************************************************************************
 * JsonWhitespaceSyntax.cs
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
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a json syntax node which contains whitespace.
    /// </summary>
    public sealed class GreenJsonWhitespaceSyntax : GreenJsonBackgroundSyntax, IGreenJsonSymbol
    {
        /// <summary>
        /// Maximum length before new <see cref="GreenJsonWhitespaceSyntax"/> instances are always newly allocated.
        /// </summary>
        public const int SharedWhitespaceInstanceLength = 255;

        private static readonly GreenJsonWhitespaceSyntax[] SharedInstances;

        static GreenJsonWhitespaceSyntax()
        {
            SharedInstances = new GreenJsonWhitespaceSyntax[SharedWhitespaceInstanceLength - 1];

            for (int i = SharedWhitespaceInstanceLength - 2; i >= 0; i--)
            {
                // Do not allocate a zero length whitespace.
                SharedInstances[i] = new GreenJsonWhitespaceSyntax(i + 1);
            }
        }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonWhitespaceSyntax"/> with a specified length.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <returns>
        /// The new <see cref="GreenJsonWhitespaceSyntax"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public static GreenJsonWhitespaceSyntax Create(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (length < SharedWhitespaceInstanceLength) return SharedInstances[length - 1];
            return new GreenJsonWhitespaceSyntax(length);
        }

        private GreenJsonWhitespaceSyntax(int length) => Length = length;

        IEnumerable<JsonErrorInfo> IGreenJsonSymbol.GetErrors(int startPosition) => EmptyEnumerable<JsonErrorInfo>.Instance;

        Union<GreenJsonBackgroundSyntax, JsonForegroundSymbol> IGreenJsonSymbol.AsBackgroundOrForeground() => this;
    }
}
