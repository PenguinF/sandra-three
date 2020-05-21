#region License
/*********************************************************************************
 * MenuCaptionBarForm.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.Controls;
using Eutherion.Win.Native;
using Eutherion.Win.Utils;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// <see cref="UIActionForm"/> which displays a main menu inside a custom caption bar.
    /// It is advisable to give the main menu strip a back color so it does not display
    /// a gradient which clashes with the custom drawn caption bar area.
    /// </summary>
    public class MenuCaptionBarForm : UIActionForm, IWeakEventTarget
    {
        private struct Metrics
        {
            private const int buttonOuterRightMargin = 4;
            private const int closeButtonMargin = 4;
            private const int captionButtonWidth = 30;
            private const int captionButtonHeight = 24;

            public int TotalWidth;
            public int TotalHeight;

            public const int CaptionHeight = 30;

            public int MainMenuWidth;

            public int SystemButtonTop;
            public int SystemButtonWidth => captionButtonWidth;
            public int SystemButtonHeight;

            public int MinimizeButtonLeft;
            public int MaximizeButtonLeft;
            public int SaveButtonLeft;
            public int CloseButtonLeft;

            public void UpdateSystemButtonMetrics(bool saveButtonVisible)
            {
                // Calculate top edge position for all caption buttons: 1 pixel above center.
                SystemButtonTop = CaptionHeight - captionButtonHeight - 2;

                if (SystemButtonTop < 0)
                {
                    SystemButtonTop = 0;
                    SystemButtonHeight = CaptionHeight;
                }
                else
                {
                    SystemButtonTop = SystemButtonTop / 2;
                    SystemButtonHeight = captionButtonHeight;
                }

                // Calculate button positions can from right to left.
                CloseButtonLeft = TotalWidth - captionButtonWidth - buttonOuterRightMargin;
                SaveButtonLeft = CloseButtonLeft;
                if (saveButtonVisible) SaveButtonLeft -= captionButtonWidth;
                MaximizeButtonLeft = SaveButtonLeft - captionButtonWidth - closeButtonMargin;
                MinimizeButtonLeft = MaximizeButtonLeft - captionButtonWidth;
            }
        }

        private const int MainMenuHorizontalMargin = 8;

        private readonly NonSelectableButton minimizeButton;
        private readonly NonSelectableButton maximizeButton;
        private readonly NonSelectableButton saveButton;
        private readonly NonSelectableButton closeButton;

        private Button currentHoverButton;

        private bool isActive;
        private bool inDarkMode;
        private Color titleBarBackColor;
        private Color titleBarForeColor;
        private Color titleBarHoverColor;
        private Color titleBarHoverBorderColor;
        private Color? closeButtonHoverColorOverride;

        private Metrics currentMetrics;

        public MenuCaptionBarForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.UserMouse
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.FixedHeight
                | ControlStyles.FixedWidth
                | ControlStyles.ResizeRedraw
                | ControlStyles.Opaque, true);

            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;

            minimizeButton = CreateCaptionButton();
            minimizeButton.Click += (_, __) => WindowState = FormWindowState.Minimized;

            maximizeButton = CreateCaptionButton();
            maximizeButton.Click += (_, __) =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                UpdateMaximizeButtonIcon();
            };

            // Specialized save button which binds on the SaveToFile UIAction.
            saveButton = CreateCaptionButton();
            saveButton.Visible = false;
            saveButton.Click += (_, __) =>
            {
                try
                {
                    ActionHandler.TryPerformAction(SharedUIAction.SaveToFile.Action, true);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            };

            ActionHandler.UIActionsInvalidated += _ =>
            {
                // Update the save button each time the handler is invalidated.
                // Some kind of checked state doesn't seem to be supported, so ignore UIActionState.Checked.
                UIActionState currentActionState = ActionHandler.TryPerformAction(SharedUIAction.SaveToFile.Action, false);
                saveButton.Visible = currentActionState.Visible;
                saveButton.Enabled = currentActionState.Enabled;
                if (!saveButton.Enabled) saveButton.FlatAppearance.BorderColor = titleBarBackColor;
            };

            closeButton = CreateCaptionButton();
            closeButton.Click += (_, __) => Close();

            SuspendLayout();

            Controls.Add(minimizeButton);
            Controls.Add(maximizeButton);
            Controls.Add(saveButton);
            Controls.Add(closeButton);

            ResumeLayout();

            ThemeHelper.UserPreferencesChanged += ThemeHelper_UserPreferencesChanged;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Enables borders regardless of ControlBox and FormBorderStyle settings.
                const int WS_SIZEBOX = 0x40000;
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_SIZEBOX;
                return cp;
            }
        }

        private NonSelectableButton CreateCaptionButton()
        {
            var button = new NonSelectableButton
            {
                ImageAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0),
                Padding = new Padding(0),
                TabStop = false,
            };

            button.FlatAppearance.BorderSize = 1;
            button.MouseEnter += TitleBarButton_MouseEnter;
            button.MouseLeave += TitleBarButton_MouseLeave;
            button.MouseMove += TitleBarButton_MouseMove;

            return button;
        }

        /// <summary>
        /// Sets the currently used hover color of the close button.
        /// </summary>
        public void SetCloseButtonHoverColor(Color value)
        {
            closeButtonHoverColorOverride = value;
            closeButton.FlatAppearance.MouseOverBackColor = value;
        }

        /// <summary>
        /// Resets the currently used hover color of the close button.
        /// </summary>
        public void ResetCloseButtonHoverColor()
        {
            closeButtonHoverColorOverride = null;
            closeButton.FlatAppearance.MouseOverBackColor = titleBarHoverColor;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e.Control == MainMenuStrip)
            {
                foreach (var mainMenuItem in MainMenuStrip.Items.OfType<ToolStripDropDownItem>())
                {
                    mainMenuItem.DropDownOpening += MainMenuItem_DropDownOpening;
                    mainMenuItem.DropDownClosed += MainMenuItem_DropDownClosed;
                }

                UpdateCaptionAreaButtonsBackColor();
            }
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            if (e.Control == MainMenuStrip)
            {
                foreach (var mainMenuItem in MainMenuStrip.Items.OfType<ToolStripDropDownItem>())
                {
                    mainMenuItem.DropDownOpening -= MainMenuItem_DropDownOpening;
                    mainMenuItem.DropDownClosed -= MainMenuItem_DropDownClosed;
                }
            }

            base.OnControlRemoved(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            // For when another window is active when this form is first shown.
            UpdateCaptionAreaButtonsBackColor();
            base.OnLoad(e);
        }

        /// <summary>
        /// Returns the current color of the title bar.
        /// </summary>
        public Color TitleBarBackColor => titleBarBackColor;

        /// <summary>
        /// Occurs after the value of <see cref="TitleBarBackColor"/> was updated.
        /// </summary>
        public event EventHandler TitleBarBackColorChanged;

        /// <summary>
        /// Raises the <see cref="TitleBarBackColorChanged"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTitleBarBackColorChanged(EventArgs e) => TitleBarBackColorChanged?.Invoke(this, e);

        private void ThemeHelper_UserPreferencesChanged(_void sender, EventArgs e)
        {
            UpdateCaptionAreaButtonsBackColor();
        }

        private void UpdateCaptionAreaButtonsBackColor()
        {
            Color oldTitleBarBackColor = titleBarBackColor;
            titleBarBackColor = ThemeHelper.GetDwmAccentColor(isActive);
            inDarkMode = titleBarBackColor.GetBrightness() < 0.5f;
            titleBarForeColor = !isActive ? SystemColors.GrayText : inDarkMode ? Color.White : Color.Black;

            if (MainMenuStrip != null)
            {
                MainMenuStrip.BackColor = titleBarBackColor;
                MainMenuStrip.ForeColor = titleBarForeColor;

                foreach (var mainMenuItem in MainMenuStrip.Items.OfType<ToolStripDropDownItem>())
                {
                    mainMenuItem.ForeColor = titleBarForeColor;
                }

                if (MainMenuStrip.Renderer is ToolStripProfessionalRenderer professionalRenderer)
                {
                    titleBarHoverColor = professionalRenderer.ColorTable.ButtonSelectedHighlight;
                    titleBarHoverBorderColor = professionalRenderer.ColorTable.ButtonSelectedBorder;
                }
            }

            new[] { minimizeButton, maximizeButton, saveButton, closeButton }.ForEach(StyleButton);

            if (closeButtonHoverColorOverride != null)
            {
                // Reapply override after StyleButton reset the hover color.
                closeButton.FlatAppearance.MouseOverBackColor = closeButtonHoverColorOverride.Value;
            }

            if (inDarkMode)
            {
                closeButton.Image = SharedResources.close_white;
                minimizeButton.Image = SharedResources.minimize_white;
                saveButton.Image = SharedResources.save_white;
            }
            else
            {
                closeButton.Image = SharedResources.close;
                minimizeButton.Image = SharedResources.minimize;
                saveButton.Image = SharedResources.save;
            }

            UpdateMaximizeButtonIcon();

            Invalidate();

            if (oldTitleBarBackColor != titleBarBackColor)
            {
                // Raise event only after everything is updated.
                OnTitleBarBackColorChanged(EventArgs.Empty);
            }
        }

        private void StyleButton(Button titleBarButton)
        {
            titleBarButton.BackColor = titleBarBackColor;
            titleBarButton.ForeColor = titleBarForeColor;
            titleBarButton.FlatAppearance.MouseOverBackColor = titleBarHoverColor;
            StyleTitleBarButtonBorder(titleBarButton);
        }

        private void StyleTitleBarButtonBorder(Button titleBarButton)
        {
            if (titleBarButton != null)
            {
                titleBarButton.FlatAppearance.BorderColor
                    = titleBarButton == currentHoverButton && titleBarButton.Enabled
                    ? titleBarHoverBorderColor
                    : titleBarBackColor;
            }
        }

        private void TitleBarButton_MouseEnter(object sender, EventArgs e)
        {
            var oldHoverButton = currentHoverButton;
            currentHoverButton = (Button)sender;
            if (oldHoverButton != currentHoverButton)
            {
                StyleTitleBarButtonBorder(oldHoverButton);
                StyleTitleBarButtonBorder(currentHoverButton);
            }
        }

        private void TitleBarButton_MouseMove(object sender, MouseEventArgs e)
        {
            var oldHoverButton = currentHoverButton;
            currentHoverButton = (Button)sender;
            if (oldHoverButton != currentHoverButton)
            {
                StyleTitleBarButtonBorder(oldHoverButton);
                StyleTitleBarButtonBorder(currentHoverButton);
            }
        }

        private void TitleBarButton_MouseLeave(object sender, EventArgs e)
        {
            var oldHoverButton = currentHoverButton;
            currentHoverButton = null;
            StyleTitleBarButtonBorder(oldHoverButton);
        }

        private void MainMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // Use DefaultForeColor rather than titleBarForeColor when dropped down.
            ((ToolStripDropDownItem)sender).ForeColor = DefaultForeColor;
        }

        private void MainMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            ((ToolStripDropDownItem)sender).ForeColor = titleBarForeColor;
        }

        private void UpdateMaximizeButtonIcon()
        {
            maximizeButton.Image
                = WindowState == FormWindowState.Maximized
                ? (inDarkMode ? SharedResources.demaximize_white : SharedResources.demaximize)
                : (inDarkMode ? SharedResources.maximize_white : SharedResources.maximize);
        }

        protected override void OnActivated(EventArgs e)
        {
            isActive = true;
            UpdateCaptionAreaButtonsBackColor();
            base.OnActivated(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            isActive = false;
            UpdateCaptionAreaButtonsBackColor();
            base.OnDeactivate(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            var clientSize = ClientSize;

            currentMetrics.TotalWidth = clientSize.Width;
            currentMetrics.TotalHeight = clientSize.Height;

            if (MainMenuStrip != null && MainMenuStrip.Visible)
            {
                currentMetrics.MainMenuWidth = MainMenuHorizontalMargin +
                    MainMenuStrip.Items
                    .OfType<ToolStripItem>()
                    .Where(x => x.Visible)
                    .Select(x => x.Width)
                    .Sum();
            }
            else
            {
                currentMetrics.MainMenuWidth = 0;
            }

            if (currentMetrics.MainMenuWidth > 0)
            {
                MainMenuStrip.Width = currentMetrics.MainMenuWidth;
            }

            currentMetrics.UpdateSystemButtonMetrics(saveButton.Visible);

            closeButton.SetBounds(
                currentMetrics.CloseButtonLeft,
                currentMetrics.SystemButtonTop,
                currentMetrics.SystemButtonWidth,
                currentMetrics.SystemButtonHeight);

            if (saveButton.Visible)
            {
                saveButton.SetBounds(
                    currentMetrics.SaveButtonLeft,
                    currentMetrics.SystemButtonTop,
                    currentMetrics.SystemButtonWidth,
                    currentMetrics.SystemButtonHeight);
            }

            maximizeButton.SetBounds(
                currentMetrics.MaximizeButtonLeft,
                currentMetrics.SystemButtonTop,
                currentMetrics.SystemButtonWidth,
                currentMetrics.SystemButtonHeight);

            minimizeButton.SetBounds(
                currentMetrics.MinimizeButtonLeft,
                currentMetrics.SystemButtonTop,
                currentMetrics.SystemButtonWidth,
                currentMetrics.SystemButtonHeight);

            // Update maximize button because Aero snap changes the client size directly and updates
            // the window state, but does not seem to call WndProc with e.g. a WM_SYSCOMMAND.
            UpdateMaximizeButtonIcon();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (MainMenuStrip != null && MainMenuStrip.Items.Count > 0)
            {
                var g = e.Graphics;
                int width = Width;
                int mainMenuWidth = MainMenuStrip.Width;

                using (var captionAreaColorBrush = new SolidBrush(titleBarBackColor))
                {
                    g.FillRectangle(captionAreaColorBrush, new Rectangle(mainMenuWidth, 0, width - mainMenuWidth, MainMenuStrip.Height));
                }

                string text = Text;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    Font menuFont = MainMenuStrip.Font;

                    // Measure the text. If its left edge is about to disappear under the main menu,
                    // use a different Rectangle to center the text in.
                    // See also e.g. Visual Studio Code where it works the same way.
                    // Also use an extra MainMenuHorizontalMargin.
                    Size measuredTextSize = TextRenderer.MeasureText(text, menuFont);
                    int probableLeftTextEdge = (width - measuredTextSize.Width) / 2;

                    // (0, width) places the Text in the center of the whole caption area bar, which is the most natural.
                    int textAreaLeftEdge = 0;
                    int textAreaWidth = width;

                    int outerMenuRightEdge = mainMenuWidth + MainMenuHorizontalMargin;
                    if (probableLeftTextEdge < outerMenuRightEdge)
                    {
                        // Now place it in the middle between the outer menu right edge and the system buttons.
                        textAreaLeftEdge = outerMenuRightEdge;
                        textAreaWidth = minimizeButton.Left - textAreaLeftEdge;
                    }

                    // Take Y and Height from first menu item so text can be aligned with it.
                    var firstMenuItemBounds = MainMenuStrip.Items[0].Bounds;
                    var textAreaRectangle = new Rectangle(textAreaLeftEdge, firstMenuItemBounds.Y, textAreaWidth, firstMenuItemBounds.Height);

                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    TextRenderer.DrawText(
                        g,
                        text,
                        MainMenuStrip.Font,
                        textAreaRectangle,
                        titleBarForeColor,
                        titleBarBackColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM.NCHITTEST)
            {
                if (MainMenuStrip != null && MainMenuStrip.Items.Count > 0)
                {
                    Point position = PointToClient(new Point(m.LParam.ToInt32()));

                    if (position.Y >= 0
                        && position.Y < MainMenuStrip.Height
                        && position.X >= 0
                        && position.X < ClientSize.Width)
                    {
                        // This is the draggable 'caption' area.
                        m.Result = (IntPtr)HT.CAPTION;
                        return;
                    }
                }
            }
            else if (m.Msg == WM.DWMCOMPOSITIONCHANGED)
            {
                // Windows 7 and lower only. This message is not used in Windows 8 and higher.
                // Make sure to update the accent colors as well.
                UpdateCaptionAreaButtonsBackColor();
            }

            base.WndProc(ref m);

            // Make sure the maximize button icon is updated too when the FormWindowState is updated externally.
            if (m.Msg == WM.SYSCOMMAND)
            {
                int wParam = SC.MASK & m.WParam.ToInt32();
                if (wParam == SC.MAXIMIZE || wParam == SC.MINIMIZE || wParam == SC.RESTORE)
                {
                    UpdateMaximizeButtonIcon();
                }
            }
        }
    }
}
