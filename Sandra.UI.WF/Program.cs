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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
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

            List<Image> pieceImages = new List<Image>
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

            Random rnd = new Random();
            for (int i = 29; i >= 0; --i)
            {
                PlayingBoardForm mdiChild = new PlayingBoardForm()
                {
                    MdiParent = mdiParent,
                    ClientSize = new Size(400, 400),
                    Visible = true,
                };

                if (rnd.Next(4) == 0)
                {
                    mdiChild.PlayingBoard.SizeToFit = false;
                    int sign = rnd.Next(2) - 1; // allowing zero is desirable in this case
                    if (sign == 0)
                    {
                        mdiChild.PlayingBoard.SquareSize = 24;
                    }
                    else
                    {
                        int delta = Math.Min(24, (int)Math.Floor(Math.Sqrt(rnd.NextDouble()) * 25));
                        mdiChild.PlayingBoard.SquareSize = 24 + sign * delta;
                    }
                }

                mdiChild.PlayingBoard.ForegroundImageRelativeSize = Math.Sqrt(rnd.NextDouble()); // sqrt gets it closer to 1.
                mdiChild.PlayingBoard.BorderWidth = rnd.Next(4);
                mdiChild.PlayingBoard.InnerSpacing = rnd.Next(3);

                int boardSize = rnd.Next(5) + 4;
                mdiChild.PlayingBoard.BoardSize = boardSize;
                for (int x = 0; x < boardSize; ++x)
                {
                    for (int y = 0; y < boardSize; ++y)
                    {
                        if (rnd.Next(2) == 0)
                        {
                            mdiChild.PlayingBoard.SetForegroundImage(x, y, pieceImages[rnd.Next(pieceImages.Count)]);
                        }
                    }
                }

                mdiChild.PlayingBoard.MouseEnterSquare += playingBoard_MouseEnterSquare;
                mdiChild.PlayingBoard.MouseLeaveSquare += playingBoard_MouseLeaveSquare;

                mdiChild.PerformAutoFit();
            }

            Application.Run(mdiParent);
        }

        private static void playingBoard_MouseEnterSquare(object sender, SquareEventArgs e)
        {
            PlayingBoard playingBoard = (PlayingBoard)sender;
            if (!playingBoard.IsDraggingImage)
            {
                playingBoard.SetIsImageHighLighted(e.X, e.Y, true);
            }
        }

        private static void playingBoard_MouseLeaveSquare(object sender, SquareEventArgs e)
        {
            PlayingBoard playingBoard = (PlayingBoard)sender;
            if (!playingBoard.IsDraggingImage)
            {
                playingBoard.SetIsImageHighLighted(e.X, e.Y, false);
            }
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
