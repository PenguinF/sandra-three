/*********************************************************************************
 * MdiContainerForm.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public class MdiContainerForm : Form
    {
        public List<Image> PieceImages { get; private set; }

        public MdiContainerForm()
        {
            IsMdiContainer = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Show in the center of the monitor where the mouse currently is.
            var activeScreen = Screen.FromPoint(MousePosition);
            Rectangle workingArea = activeScreen.WorkingArea;

            // Two thirds the size of the active monitor's working area.
            workingArea.Inflate(-workingArea.Width / 6, -workingArea.Height / 6);

            // Update the bounds of the form.
            SetBounds(workingArea.X, workingArea.Y, workingArea.Width, workingArea.Height, BoundsSpecified.All);

            // Load chess piece images from a fixed path.
            PieceImages = loadChessPieceImages();
        }

        List<Image> loadChessPieceImages()
        {
            return new List<Image>
            {
                loadImage("bp"),
                loadImage("bn"),
                loadImage("bb"),
                loadImage("br"),
                loadImage("bq"),
                loadImage("bk"),
                loadImage("wp"),
                loadImage("wn"),
                loadImage("wb"),
                loadImage("wr"),
                loadImage("wq"),
                loadImage("wk"),
            };
        }

        static Image loadImage(string imageFileKey)
        {
            try
            {
                string basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                return Image.FromFile(Path.Combine(basePath, "Images", imageFileKey + ".png"));
            }
            catch (Exception exc)
            {
                Debug.Write(exc.Message);
                return null;
            }
        }
    }
}
