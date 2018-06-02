﻿/*********************************************************************************
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
using SysExtensions;

namespace Sandra.UI.WF
{
    internal static class SettingKeys
    {
        internal static readonly SettingProperty<bool> Maximized = new SettingProperty<bool>(
            new SettingKey(nameof(Maximized).ToLowerInvariant()),
            PType.Boolean.Instance);

        internal static readonly SettingProperty<FormState> Window = new SettingProperty<FormState>(
            new SettingKey(nameof(Window).ToLowerInvariant()),
            FormState.Type);

        internal static readonly SettingProperty<MovesTextBox.MFOSettingValue> Notation = new SettingProperty<MovesTextBox.MFOSettingValue>(
            new SettingKey(nameof(Notation).ToLowerInvariant()),
            new PType.Enumeration<MovesTextBox.MFOSettingValue>(EnumHelper<MovesTextBox.MFOSettingValue>.AllValues));

        internal static readonly SettingProperty<int> Zoom = new SettingProperty<int>(
            new SettingKey(nameof(Zoom).ToLowerInvariant()),
            PType.RichTextZoomFactor.Instance);
    }
}
