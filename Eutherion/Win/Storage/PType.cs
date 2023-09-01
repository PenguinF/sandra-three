#region License
/*********************************************************************************
 * PType.cs
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
    /// <summary>
    /// Represents the type of a value which can be deserialized from and serialized to JSON.
    /// </summary>
    /// <typeparam name="T">
    /// The .NET target <see cref="Type"/> to convert to and from.
    /// </typeparam>
    public abstract class PType<T>
    {
        internal abstract Union<ITypeErrorBuilder, T> TryCreateValue(JsonValueSyntax valueNode, ArrayBuilder<PTypeError> errors);

        /// <summary>
        /// Converts a value of the target .NET type <typeparamref name="T"/> to a <see cref="PValue"/>.
        /// </summary>
        /// <param name="value">
        /// The value to convert from.
        /// </param>
        /// <returns>
        /// The converted target value.
        /// </returns>
        public abstract PValue ConvertToPValue(T value);
    }
}
