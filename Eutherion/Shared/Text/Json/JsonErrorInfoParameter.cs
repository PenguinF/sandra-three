#region License
/*********************************************************************************
 * JsonErrorInfoParameter.cs
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Holds a parameter of a <see cref="JsonErrorInfo"/>.
    /// </summary>
    public abstract class JsonErrorInfoParameter
    {
        internal JsonErrorInfoParameter() { }

        /// <summary>
        /// Gets the untyped parameter value.
        /// </summary>
        public abstract object UntypedValue { get; }
    }

    /// <summary>
    /// Holds a parameter of a <see cref="JsonErrorInfo"/>.
    /// </summary>
    /// <typeparam name="ParameterType">
    /// The type of parameter.
    /// </typeparam>
    public sealed class JsonErrorInfoParameter<ParameterType> : JsonErrorInfoParameter
    {
        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        public ParameterType Value { get; }

        /// <summary>
        /// Gets the untyped parameter value.
        /// </summary>
        public override object UntypedValue => Value;

        /// <summary>
        /// Initializes a new instance of <see cref="JsonErrorInfoParameter{ParameterType}"/>.
        /// </summary>
        /// <param name="value">
        /// The parameter value.
        /// </param>
        public JsonErrorInfoParameter(ParameterType value) => Value = value;
    }
}
