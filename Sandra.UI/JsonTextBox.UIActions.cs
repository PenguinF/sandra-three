#region License
/*********************************************************************************
 * SettingsTextBox.UIActions.cs
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

using Eutherion.Win.UIActions;
using System;
using System.IO;

namespace Sandra.UI
{
    public partial class JsonTextBox
    {
        public const string JsonTextBoxUIActionPrefix = nameof(JsonTextBox) + ".";

        public static readonly DefaultUIActionBinding SaveToFile = new DefaultUIActionBinding(
            new UIAction(JsonTextBoxUIActionPrefix + nameof(SaveToFile)),
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.Save,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.S), },
                MenuIcon = Properties.Resources.save,
            });

        public UIActionState TrySaveToFile(bool perform)
        {
            if (ReadOnly) return UIActionVisibility.Hidden;

            if (perform)
            {
                File.WriteAllText(settingsFile.AbsoluteFilePath, Text);
                SetSavePoint();
            }

            return UIActionVisibility.Enabled;
        }
    }
}
