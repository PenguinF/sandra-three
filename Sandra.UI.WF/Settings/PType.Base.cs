/*********************************************************************************
 * PType.Base.cs
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
namespace Sandra.UI.WF
{
    public static partial class PType
    {
        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PBoolean"/> values.
        /// </summary>
        public static readonly PType<PBoolean> Boolean = new BaseType<PBoolean>();

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PInteger"/> values.
        /// </summary>
        public static readonly PType<PInteger> Integer = new BaseType<PInteger>();

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PString"/> values.
        /// </summary>
        public static readonly PType<PString> String = new BaseType<PString>();

        private sealed class BaseType<TValue> : PType<TValue>
            where TValue : PValue
        {
            public override bool TryGetValidValue(PValue value, out TValue targetValue)
            {
                if (value is TValue)
                {
                    targetValue = (TValue)value;
                    return true;
                }

                targetValue = default(TValue);
                return false;
            }

            public override PValue GetPValue(TValue value) => value;
        }
    }
}
