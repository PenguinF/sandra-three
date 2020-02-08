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
    /// Denotes any terminal json symbol that is not treated as background such as comments or whitespace.
    /// </summary>
    public abstract class JsonForegroundSymbol : IGreenJsonSymbol
    {
        public virtual bool IsValueStartSymbol => false;

        /// <summary>
        /// Gets if there are any errors associated with this symbol.
        /// </summary>
        public virtual bool HasErrors => false;

        /// <summary>
        /// Generates a sequence of errors associated with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the errors.
        /// </param>
        /// <returns>
        /// A sequence of errors associated with this symbol.
        /// </returns>
        public virtual IEnumerable<JsonErrorInfo> GetErrors(int startPosition) => EmptyEnumerable<JsonErrorInfo>.Instance;

        public abstract int Length { get; }

        public abstract void Accept(JsonForegroundSymbolVisitor visitor);
        public abstract TResult Accept<TResult>(JsonForegroundSymbolVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(JsonForegroundSymbolVisitor<T, TResult> visitor, T arg);

        public Union<GreenJsonBackgroundSyntax, JsonForegroundSymbol> AsBackgroundOrForeground() => this;
    }
}
