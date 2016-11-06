/*********************************************************************************
 * Program.cs
 * 
 * Copyright (c) 2004-2016 Henk Nicolai
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
using System;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form mdiParent = new Form()
            {
                WindowState = FormWindowState.Maximized,
                IsMdiContainer = true,
            };

            Random rnd = new Random();
            for (int i = 3; i >= 0; --i)
            {
                PlayingBoardForm mdiChild = new PlayingBoardForm()
                {
                    MdiParent = mdiParent,
                    ClientSize = new System.Drawing.Size(400, 400),
                    Visible = true,
                };
                mdiChild.PlayingBoard.BoardSize = rnd.Next(5) + 4;
            }

            Application.Run(mdiParent);
        }
    }
}
