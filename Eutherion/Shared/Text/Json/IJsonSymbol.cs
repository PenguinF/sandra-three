#region License
/*********************************************************************************
 * IJsonSymbol.cs
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

using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a terminal json symbol.
    /// Instances of this type are returned by <see cref="JsonTokenizer"/>.
    /// </summary>
    public interface IGreenJsonSymbol : ISpan
    {
        /// <summary>
        /// Generates a sequence of errors associated with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the errors.
        /// </param>
        /// <returns>
        /// A sequence of errors associated with this symbol.
        /// </returns>
        IEnumerable<JsonErrorInfo> GetErrors(int startPosition);

        /// <summary>
        /// Converts this symbol into either a <see cref="GreenJsonBackgroundSyntax"/> or a <see cref="JsonForegroundSymbol"/>.
        /// </summary>
        /// <returns>
        /// Either a <see cref="GreenJsonBackgroundSyntax"/> or a <see cref="JsonForegroundSymbol"/>.
        /// </returns>
        Union<GreenJsonBackgroundSyntax, JsonForegroundSymbol> AsBackgroundOrForeground();
    }

    /// <summary>
    /// Represents a terminal json symbol.
    /// These are all <see cref="JsonSyntax"/> nodes which have no child <see cref="JsonSyntax"/> nodes.
    /// Use <see cref="JsonSymbolVisitor"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public interface IJsonSymbol : ISpan
    {
        void Accept(JsonSymbolVisitor visitor);
        TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor);
        TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg);
    }
}
