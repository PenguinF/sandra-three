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
using System.Linq;

namespace Eutherion.Text.Json
{
    public abstract class JsonSymbol : ISpan
    {
        public virtual bool IsBackground => false;
        public virtual bool IsValueStartSymbol => false;

        /// <summary>
        /// Gets if there are any errors associated with this symbol.
        /// </summary>
        public virtual bool HasErrors => false;

        /// <summary>
        /// If <see cref="HasErrors"/> is true, generates a non-empty sequence of errors associated
        /// with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the errors.
        /// </param>
        /// <returns>
        /// A sequence of errors associated with this symbol.
        /// </returns>
        public virtual IEnumerable<JsonErrorInfo> GetErrors(int startPosition) => Enumerable.Empty<JsonErrorInfo>();

        public abstract int Length { get; }

        public abstract void Accept(JsonSymbolVisitor visitor);
        public abstract TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg);
    }
}
