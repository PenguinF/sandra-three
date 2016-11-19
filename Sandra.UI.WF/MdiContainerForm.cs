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
using Sandra.Chess;
using System;
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
        public EnumIndexedArray<NonEmptyColoredPiece, Image> PieceImages { get; private set; }

        public MdiContainerForm()
        {
            IsMdiContainer = true;
            MainMenuStrip = new MenuStrip();
            MainMenuStrip.Items.Add("New playing board (Ctrl+B)", null, (_, __) => { NewPlayingBoard(); });
            MainMenuStrip.Visible = true;
            Controls.Add(MainMenuStrip);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.B))
            {
                NewPlayingBoard();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void NewPlayingBoard()
        {
            StandardChessBoardForm mdiChild = new StandardChessBoardForm()
            {
                MdiParent = this,
                ClientSize = new Size(400, 400),
                Visible = true,
            };
            mdiChild.PieceImages = PieceImages;
            mdiChild.PlayingBoard.ForegroundImageRelativeSize = 0.9f;
            mdiChild.PerformAutoFit();
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

            NewPlayingBoard();
        }

        EnumIndexedArray<NonEmptyColoredPiece, Image> loadChessPieceImages()
        {
            var array = EnumIndexedArray<NonEmptyColoredPiece, Image>.New();
            array[NonEmptyColoredPiece.BlackPawn] = loadImage("bp");
            array[NonEmptyColoredPiece.BlackKnight] = loadImage("bn");
            array[NonEmptyColoredPiece.BlackBishop] = loadImage("bb");
            array[NonEmptyColoredPiece.BlackRook] = loadImage("br");
            array[NonEmptyColoredPiece.BlackQueen] = loadImage("bq");
            array[NonEmptyColoredPiece.BlackKing] = loadImage("bk");
            array[NonEmptyColoredPiece.WhitePawn] = loadImage("wp");
            array[NonEmptyColoredPiece.WhiteKnight] = loadImage("wn");
            array[NonEmptyColoredPiece.WhiteBishop] = loadImage("wb");
            array[NonEmptyColoredPiece.WhiteRook] = loadImage("wr");
            array[NonEmptyColoredPiece.WhiteQueen] = loadImage("wq");
            array[NonEmptyColoredPiece.WhiteKing] = loadImage("wk");
            return array;
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
