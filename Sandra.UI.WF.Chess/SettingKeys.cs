/*********************************************************************************
 * SettingKeys.cs
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
using System.Collections.Generic;

namespace Sandra.UI.WF
{
    internal static class SettingKeys
    {
        internal static readonly SettingKey Lang = new SettingKey(nameof(Lang).ToLowerInvariant());
        internal static readonly SettingKey Maximized = new SettingKey(nameof(Maximized).ToLowerInvariant());
        internal static readonly SettingKey Left = new SettingKey(nameof(Left).ToLowerInvariant());
        internal static readonly SettingKey Top = new SettingKey(nameof(Top).ToLowerInvariant());
        internal static readonly SettingKey Width = new SettingKey(nameof(Width).ToLowerInvariant());
        internal static readonly SettingKey Height = new SettingKey(nameof(Height).ToLowerInvariant());
        internal static readonly SettingKey Notation = new SettingKey(nameof(Notation).ToLowerInvariant());

        internal static readonly HashSet<SettingKey> All;

        static SettingKeys()
        {
            All = new HashSet<SettingKey>(new SettingKey[]
            {
                Lang,
                Maximized,
                Left,
                Top,
                Width,
                Height,
                Notation,
            });
        }
    }
}
