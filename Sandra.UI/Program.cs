#region License
/*********************************************************************************
 * Program.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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

using Eutherion;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Sandra.UI
{
    static class Program
    {
        internal static SandraChessMainForm MainForm { get; private set; }

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

#if DEBUG
            // Write traced exceptions to Debug.
            ExceptionSink.Instance.TracingException += exception =>
            {
                Debug.WriteLine($"{exception.GetType().FullName}: {exception.Message}");
            };
#endif

            MainForm = new SandraChessMainForm(args);
            Application.Run(MainForm);
        }
    }
}
