/*********************************************************************************
 * MdiContainerForm.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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
    public class MdiContainerForm : Form, IUIActionHandlerProvider
    {
        public EnumIndexedArray<ColoredPiece, Image> PieceImages { get; private set; }

        public MdiContainerForm()
        {
            IsMdiContainer = true;

            // Initialize UIActions before building the MainMenuStrip based on it.
            initializeUIActions();

            MainMenuStrip = new MenuStrip();
            UIMenuBuilder.BuildMenu(ActionHandler, MainMenuStrip.Items);
            MainMenuStrip.Visible = true;
            Controls.Add(MainMenuStrip);
        }

        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

        public const string MdiContainerFormUIActionPrefix = nameof(MdiContainerForm) + " ";

        public static readonly UIAction OpenNewPlayingBoardUIAction = new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenNewPlayingBoardUIAction));

        public UIActionState TryOpenNewPlayingBoard(bool perform)
        {
            if (perform) NewPlayingBoard();
            return UIActionVisibility.Enabled;
        }

        void initializeUIActions()
        {
            UIMenuNode.Container container = new UIMenuNode.Container("Game");
            ActionHandler.RootMenuNode.Nodes.Add(container);

            this.BindAction(OpenNewPlayingBoardUIAction, TryOpenNewPlayingBoard, new UIActionBinding()
            {
                ShowInMenu = true,
                MenuContainer = container,
                MenuCaption = "New playing board",
                MainShortcut = new ShortcutKeys(KeyModifiers.Control, ConsoleKey.B),
            });
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // This code makes shortcuts work for all UIActionHandlers.
            return KeyUtils.TryExecute(keyData) || base.ProcessCmdKey(ref msg, keyData);
        }

        public void NewPlayingBoard()
        {
            InteractiveGame game = new InteractiveGame(Position.GetInitialPosition());

            StandardChessBoardForm mdiChild = new StandardChessBoardForm()
            {
                MdiParent = this,
                ClientSize = new Size(400, 400),
            };
            mdiChild.Game = game;
            mdiChild.PieceImages = PieceImages;
            mdiChild.PlayingBoard.ForegroundImageRelativeSize = 0.9f;
            mdiChild.PerformAutoFit();

            mdiChild.PlayingBoard.BindAction(InteractiveGame.GotoPreviousMoveUIAction, game.TryGotoPreviousMove, InteractiveGame.DefaultGotoPreviousMoveBinding());
            mdiChild.PlayingBoard.BindAction(InteractiveGame.GotoNextMoveUIAction, game.TryGotoNextMove, InteractiveGame.DefaultGotoNextMoveBinding());
            UIMenu.AddTo(mdiChild.PlayingBoard);

            mdiChild.Load += (_, __) =>
            {
                var mdiChildBounds = mdiChild.Bounds;
                SnappingMdiChildForm movesForm = new SnappingMdiChildForm()
                {
                    MdiParent = this,
                    StartPosition = FormStartPosition.Manual,
                    Left = mdiChildBounds.Right,
                    Top = mdiChildBounds.Top,
                    Width = 200,
                    Height = mdiChildBounds.Height,
                    ShowIcon = false,
                    MaximizeBox = false,
                    FormBorderStyle = FormBorderStyle.SizableToolWindow,
                };

                EnumIndexedArray<Piece, string> englishPieceSymbols = EnumIndexedArray<Piece, string>.New();
                englishPieceSymbols[Piece.Knight] = "N";
                englishPieceSymbols[Piece.Bishop] = "B";
                englishPieceSymbols[Piece.Rook] = "R";
                englishPieceSymbols[Piece.Queen] = "Q";
                englishPieceSymbols[Piece.King] = "K";

                var movesTextBox = new MovesTextBox()
                {
                    Dock = DockStyle.Fill,
                    Game = game,
                    MoveFormatter = new ShortAlgebraicMoveFormatter(englishPieceSymbols),
                };

                movesTextBox.BindAction(InteractiveGame.GotoPreviousMoveUIAction, game.TryGotoPreviousMove, InteractiveGame.DefaultGotoPreviousMoveBinding());
                movesTextBox.BindAction(InteractiveGame.GotoNextMoveUIAction, game.TryGotoNextMove, InteractiveGame.DefaultGotoNextMoveBinding());
                UIMenu.AddTo(movesTextBox);

                movesForm.Controls.Add(movesTextBox);

                movesForm.Visible = true;
            };

            mdiChild.Visible = true;
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

        EnumIndexedArray<ColoredPiece, Image> loadChessPieceImages()
        {
            var array = EnumIndexedArray<ColoredPiece, Image>.New();
            array[ColoredPiece.BlackPawn] = loadImage("bp");
            array[ColoredPiece.BlackKnight] = loadImage("bn");
            array[ColoredPiece.BlackBishop] = loadImage("bb");
            array[ColoredPiece.BlackRook] = loadImage("br");
            array[ColoredPiece.BlackQueen] = loadImage("bq");
            array[ColoredPiece.BlackKing] = loadImage("bk");
            array[ColoredPiece.WhitePawn] = loadImage("wp");
            array[ColoredPiece.WhiteKnight] = loadImage("wn");
            array[ColoredPiece.WhiteBishop] = loadImage("wb");
            array[ColoredPiece.WhiteRook] = loadImage("wr");
            array[ColoredPiece.WhiteQueen] = loadImage("wq");
            array[ColoredPiece.WhiteKing] = loadImage("wk");
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
