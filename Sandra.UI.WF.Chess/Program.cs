/*********************************************************************************
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
            SettingCopy initialSettings = new SettingCopy();

            AutoSave = new AutoSave(AppName, initialSettings);
            Chess.Constants.ForceInitialize();
            Localizer.Current = Localizers.English;

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
