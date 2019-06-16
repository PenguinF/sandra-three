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
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Chess.Constants.ForceInitialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Control.CheckForIllegalCrossThreadCalls = true;

            Application.Run(new SandraChessMainForm(args));
        }

        private class SandraChessMainForm : SingleInstanceMainForm
        {
            private readonly string[] commandLineArgs;
            private Session session;

            public SandraChessMainForm(string[] commandLineArgs)
            {
                this.commandLineArgs = commandLineArgs;

                // This hides the window at startup.
                ShowInTaskbar = false;
                WindowState = FormWindowState.Minimized;
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                // Use built-in localizer if none is provided.
                var builtInEnglishLocalizer = new BuiltInEnglishLocalizer(
                    LocalizedStringKeys.DefaultEnglishTranslations,
                    LocalizedConsoleKeys.DefaultEnglishTranslations,
                    SharedLocalizedStringKeys.DefaultEnglishTranslations(Session.ExecutableFileNameWithoutExtension),
                    JsonErrorInfoExtensions.DefaultEnglishJsonErrorTranslations);

                session = Session.Configure(new SettingsProvider(),
                                            builtInEnglishLocalizer,
                                            builtInEnglishLocalizer.Dictionary,
                                            Properties.Resources.Sandra);

                base.OnHandleCreated(e);
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                var mdiContainerForm = new MdiContainerForm();

                mdiContainerForm.Shown += (_, __) =>
                {
                    // Interpret each command line argument as a file to open.
                    commandLineArgs.ForEach(pgnFileName =>
                    {
                        // Catch exception for each open action individually.
                        try
                        {
                            mdiContainerForm.OpenOrActivatePgnFile(pgnFileName, isReadOnly: false);
                        }
                        catch (Exception exception)
                        {
                            // For now, show the exception to the user.
                            // Maybe user has no access to the path, or the given file name is not a valid.
                            // TODO: analyze what error conditions can occur and handle them appropriately.
                            MessageBox.Show(
                            $"Attempt to open code file '{pgnFileName}' failed with message: '{exception.Message}'",
                            pgnFileName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        }
                    });
                };

                mdiContainerForm.FormClosed += (_, __) => Close();

                Visible = false;

                mdiContainerForm.Show();
            }

            protected override void OnHandleDestroyed(EventArgs e)
            {
                base.OnHandleDestroyed(e);

                session?.Dispose();
            }
        }
    }
}
