#region License
/*********************************************************************************
 * RootJsonSyntax.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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
    /// Contains the syntax tree and list of parse errors which are the result of parsing source text in the JSON format.
    /// </summary>
    public sealed class RootJsonSyntax
    {
        /// <summary>
        /// Gets the root <see cref="JsonMultiValueSyntax"/> node.
        /// </summary>
        public JsonMultiValueSyntax Syntax { get; }

        /// <summary>
        /// The list containing all errors generated during a parse.
        /// </summary>
        public List<JsonErrorInfo> Errors { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="RootJsonSyntax"/>.
        /// </summary>
        /// <param name="syntax">
        /// The root <see cref="GreenJsonMultiValueSyntax"/> node from which to construct a <see cref="JsonMultiValueSyntax"/> abstract syntax tree.
        /// </param>
        /// <param name="errors">
        /// The enumeration containing all errors generated during a parse.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="syntax"/> and/or <paramref name="errors"/> are null.
        /// </exception>
        public RootJsonSyntax(GreenJsonMultiValueSyntax syntax, List<JsonErrorInfo> errors)
        {
            if (syntax == null) throw new ArgumentNullException(nameof(syntax));
            Syntax = new JsonMultiValueSyntax(syntax);
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}
