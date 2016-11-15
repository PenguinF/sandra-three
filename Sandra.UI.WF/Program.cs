﻿/*********************************************************************************
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
using System.Drawing;
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

            MdiContainerForm mdiParent = new MdiContainerForm();

            StandardChessBoardForm mdiChild = new StandardChessBoardForm()
            {
                MdiParent = mdiParent,
                ClientSize = new Size(400, 400),
                Visible = true,
            };
            mdiChild.PlayingBoard.ForegroundImageRelativeSize = 0.9f;
            mdiChild.PerformAutoFit();

            Application.Run(mdiParent);
        }
    }
}
