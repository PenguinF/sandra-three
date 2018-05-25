﻿/*********************************************************************************
 * SettingHelper.cs
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
    /// <summary>
    /// Contains utility methods for handling settings.
    /// </summary>
    public static class SettingHelper
    {
        /// <summary>
        /// Returns if two <see cref="PValue"/>s are equal.
        /// </summary>
        /// <param name="x">
        /// The first <see cref="PValue"/>.
        /// </param>
        /// <param name="y">
        /// The second <see cref="PValue"/>.
        /// </param>
        /// <returns>
        /// Whether or not both <see cref="PValue"/>s are equal.
        /// </returns>
        public static bool AreEqual(PValue x, PValue y)
            => PValueEqualityComparer.Instance.AreEqual(x, y);
    }
}
