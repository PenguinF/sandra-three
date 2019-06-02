#region License
/*********************************************************************************
 * Program.cs
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

using Eutherion.Win.AppTemplate;
using System;
using System.Windows.Forms;

namespace Sandra.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Use built-in localizer if none is provided.
            var builtInEnglishLocalizer = new BuiltInEnglishLocalizer(
                LocalizedStringKeys.DefaultEnglishTranslations,
                LocalizedConsoleKeys.DefaultEnglishTranslations,
                SharedLocalizedStringKeys.DefaultEnglishTranslations(Session.ExecutableFileNameWithoutExtension),
                JsonErrorInfoExtensions.DefaultEnglishJsonErrorTranslations);

            using (var session = Session.Configure(new SettingsProvider(),
                                                   builtInEnglishLocalizer,
                                                   builtInEnglishLocalizer.Dictionary,
                                                   Properties.Resources.Sandra))
            {
                Chess.Constants.ForceInitialize();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var mdiContainerForm = new MdiContainerForm();

                mdiContainerForm.Load += (_, __) =>
                {
                    // Inform session of the current synchronization context once a message loop exists.
                    session.CaptureSynchronizationContext();
                };

                Application.Run(mdiContainerForm);
            }
        }
    }
}
