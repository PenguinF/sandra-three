﻿/*********************************************************************************
 * Program.cs
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
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    static class Program
    {
        internal const string AppName = "SandraChess";

        internal static AutoSave AutoSave { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AutoSave = new AutoSave(AppName, new SettingCopy());

            // Remove any stale/unknown settings.
            var update = AutoSave.CreateUpdate();
            AutoSave.CurrentSettings.Keys
                .Where(key => !SettingKeys.All.Contains(key))
                .ForEach(key => update.Remove(key));
            update.Persist();

            Chess.Constants.ForceInitialize();

            Localizer.Current = Localizers.English;
            ISettingValue settingValue;
            if (AutoSave.CurrentSettings.TryGetValue(SettingKeys.Lang, out settingValue))
            {
                if (SettingHelper.AreEqual(settingValue, Localizers.EnglishSettingValue))
                {
                    // Technically not necessary since it's the default value.
                    Localizer.Current = Localizers.English;
                }
                else if (SettingHelper.AreEqual(settingValue, Localizers.DutchSettingValue))
                {
                    Localizer.Current = Localizers.Dutch;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MdiContainerForm());

            // Wait until the auto-save background task has finished.
            AutoSave.Close();
        }

        internal static Image LoadImage(string imageFileKey)
        {
            try
            {
                string basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                return Image.FromFile(Path.Combine(basePath, "Images", imageFileKey + ".png"));
            }
            catch (Exception exc)
            {
                exc.Trace();
                return null;
            }
        }
    }
}
