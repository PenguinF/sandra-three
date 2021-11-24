#region License
/*********************************************************************************
 * ColorSquareIndexedArray.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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

namespace Sandra.Chess
{
    /// <summary>
    /// Contains an array which is indexed by a <see cref="Color"/> and a <see cref="Square"/>.
    /// Always initialize such an array with the <see cref="New"/> method, e.g.:
    /// <code>
    /// var array = ColorSquareIndexedArray&lt;ulong&gt;.New();
    /// </code>
    /// </summary>
    public struct ColorSquareIndexedArray<TValue>
    {
        private TValue[,] arr;

        private void Init()
        {
            if (arr == null) arr = new TValue[EnumHelper<Color>.EnumCount, EnumHelper<Square>.EnumCount];
        }

        /// <summary>
        /// Initializes an empty array with default values.
        /// </summary>
        public static ColorSquareIndexedArray<TValue> New()
        {
            ColorSquareIndexedArray<TValue> wrapped = default;
            wrapped.Init();
            return wrapped;
        }

        public TValue this[Color color, Square square]
        {
            get
            {
                try
                {
                    return arr[(int)color, (int)square];
                }
                catch (NullReferenceException)
                {
                    Init();
                    return arr[(int)color, (int)square];
                }
            }
            set
            {
                try
                {
                    arr[(int)color, (int)square] = value;
                }
                catch (NullReferenceException)
                {
                    Init();
                    arr[(int)color, (int)square] = value;
                }
            }
        }
    }
}
