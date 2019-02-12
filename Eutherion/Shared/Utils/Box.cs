﻿#region License
/*********************************************************************************
 * Box.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

namespace Eutherion.Utils
{
    /// <summary>
    /// References a value, adding an indirection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value.
    /// </typeparam>
    public class Box<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Box{T}"/> class
        /// with a default value for its <see cref="Value"/> property.
        /// </summary>
        public Box() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Box{T}"/> class
        /// with an initial value for its <see cref="Value"/> property.
        /// </summary>
        /// <param name="value">
        /// The initial value to reference.
        /// </param>
        public Box(T value) => Value = value;

        /// <summary>
        /// Gets or sets the referenced value.
        /// </summary>
        public T Value { get; set; }
    }
}