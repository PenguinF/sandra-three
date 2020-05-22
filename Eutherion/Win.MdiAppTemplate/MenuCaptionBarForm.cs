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
using Eutherion.Win.UIActions;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    public class OldMenuCaptionBarForm : UIActionForm, IWeakEventTarget
    {
        private struct Metrics
        {
            private const int buttonOuterRightMargin = 4;
            private const int closeButtonMargin = 4;
            private const int captionButtonWidth = 30;
            private const int captionButtonHeight = 24;

            public int TotalWidth;
            public int TotalHeight;

            public bool IsMaximized;
            public int HorizontalResizeBorderThickness;
            public int VerticalResizeBorderThickness;

            public int CaptionHeight;

            public int MainMenuTop => VerticalResizeBorderThickness;
            public int MainMenuLeft => HorizontalResizeBorderThickness;
            public int MainMenuWidth;
            public int MainMenuHeight => CaptionHeight;

            public int ClientAreaLeft => HorizontalResizeBorderThickness;
            public int ClientAreaTop => VerticalResizeBorderThickness + CaptionHeight;
            public int ClientAreaWidth => TotalWidth - HorizontalResizeBorderThickness * 2;
            public int ClientAreaHeight => TotalHeight - CaptionHeight - VerticalResizeBorderThickness * 2;

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
                    SystemButtonTop /= 2;
                    SystemButtonHeight = captionButtonHeight;
                }

                SystemButtonTop += VerticalResizeBorderThickness;

                // Calculate button positions can from right to left.
                CloseButtonLeft = TotalWidth - HorizontalResizeBorderThickness - captionButtonWidth - buttonOuterRightMargin;
                SaveButtonLeft = CloseButtonLeft;
                if (saveButtonVisible) SaveButtonLeft -= captionButtonWidth;
                MaximizeButtonLeft = SaveButtonLeft - captionButtonWidth - closeButtonMargin;
                MinimizeButtonLeft = MaximizeButtonLeft - captionButtonWidth;
            }
        }

        private const int MainMenuHorizontalMargin = 8;

        public const int DefaultCaptionHeight = 30;
        public int CaptionHeight { get; set; } = DefaultCaptionHeight;

        private readonly NonSelectableButton minimizeButton;
        private readonly NonSelectableButton maximizeButton;
        private readonly NonSelectableButton saveButton;
        private readonly NonSelectableButton closeButton;

        private Button currentHoverButton;
        private Color? closeButtonHoverColorOverride;

        private Metrics currentMetrics;

        public OldMenuCaptionBarForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.UserMouse
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.FixedHeight
                | ControlStyles.FixedWidth
                | ControlStyles.ResizeRedraw
                | ControlStyles.Opaque, true);

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
                if (!saveButton.Enabled) saveButton.FlatAppearance.BorderColor = ObservableStyle.BackColor;
            };

            closeButton = CreateCaptionButton();
            closeButton.Click += (_, __) => Close();

            SuspendLayout();

            Controls.Add(minimizeButton);
            Controls.Add(maximizeButton);
            Controls.Add(saveButton);
            Controls.Add(closeButton);

            ResumeLayout();

            ObservableStyle.NotifyChange += ObservableStyle_NotifyChange;

            AllowTransparency = true;
            TransparencyKey = ObservableStyle.SuggestedTransparencyKey;
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
            closeButton.FlatAppearance.MouseOverBackColor = ObservableStyle.HoverColor;
        }

        /// <summary>
        /// Gets the regular UIActions for this Form.
        /// </summary>
        public UIActionBindings StandardUIActionBindings => new UIActionBindings
        {
            { SharedUIAction.Close, TryClose },
        };

        /// <summary>
        /// Binds the regular UIActions to this Form.
        /// </summary>
        public void BindStandardUIActions() => this.BindActions(StandardUIActionBindings);

        public UIActionState TryClose(bool perform)
        {
            if (perform) Close();
            return UIActionVisibility.Enabled;
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

                if (MainMenuStrip.Renderer is ToolStripProfessionalRenderer professionalRenderer)
                {
                    ObservableStyle.Update(() =>
                    {
                        ObservableStyle.HoverColor = professionalRenderer.ColorTable.ButtonSelectedHighlight;
                        ObservableStyle.HoverBorderColor = professionalRenderer.ColorTable.ButtonSelectedBorder;
                        ObservableStyle.Font = MainMenuStrip.Font;
                    });
                }
                else
                {
                    ObservableStyle.Font = MainMenuStrip.Font;
                }
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
        /// Returns the style of the title bar which can be observed for changes.
        /// </summary>
        public MenuCaptionBarFormStyle ObservableStyle { get; } = new MenuCaptionBarFormStyle();

        private void ObservableStyle_NotifyChange(object sender, EventArgs e)
        {
            if (TransparencyKey != ObservableStyle.SuggestedTransparencyKey)
            {
                TransparencyKey = ObservableStyle.SuggestedTransparencyKey;
            }

            UpdateCaptionAreaButtonsBackColor();
        }

        private void UpdateCaptionAreaButtonsBackColor()
        {
            if (MainMenuStrip != null)
            {
                MainMenuStrip.BackColor = ObservableStyle.BackColor;
                MainMenuStrip.ForeColor = ObservableStyle.ForeColor;

                foreach (var mainMenuItem in MainMenuStrip.Items.OfType<ToolStripDropDownItem>())
                {
                    mainMenuItem.ForeColor = ObservableStyle.ForeColor;
                }
            }

            new[] { minimizeButton, maximizeButton, saveButton, closeButton }.ForEach(StyleButton);

            if (closeButtonHoverColorOverride != null)
            {
                // Reapply override after StyleButton reset the hover color.
                closeButton.FlatAppearance.MouseOverBackColor = closeButtonHoverColorOverride.Value;
            }

            if (ObservableStyle.InDarkMode)
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
        }

        private void StyleButton(Button titleBarButton)
        {
            titleBarButton.BackColor = ObservableStyle.BackColor;
            titleBarButton.ForeColor = ObservableStyle.ForeColor;
            titleBarButton.FlatAppearance.MouseOverBackColor = ObservableStyle.HoverColor;
            StyleTitleBarButtonBorder(titleBarButton);
        }

        private void StyleTitleBarButtonBorder(Button titleBarButton)
        {
            if (titleBarButton != null)
            {
                titleBarButton.FlatAppearance.BorderColor
                    = titleBarButton == currentHoverButton && titleBarButton.Enabled
                    ? ObservableStyle.HoverBorderColor
                    : ObservableStyle.BackColor;
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
            ((ToolStripDropDownItem)sender).ForeColor = ObservableStyle.ForeColor;
        }

        private void UpdateMaximizeButtonIcon()
        {
            maximizeButton.Image
                = WindowState == FormWindowState.Maximized
                ? (ObservableStyle.InDarkMode ? SharedResources.demaximize_white : SharedResources.demaximize)
                : (ObservableStyle.InDarkMode ? SharedResources.maximize_white : SharedResources.maximize);
        }

        protected override void OnActivated(EventArgs e)
        {
            ObservableStyle.IsActive = true;
            base.OnActivated(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            ObservableStyle.IsActive = false;
            base.OnDeactivate(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            var clientSize = ClientSize;

            currentMetrics.TotalWidth = clientSize.Width;
            currentMetrics.TotalHeight = clientSize.Height;

            // If maximized, Windows will make a Form bigger than the screen to hide the drop shadow used for resizing.
            // If a Form such as this one is drawn without that drop shadow, we need to compensate for that.
            if (WindowState == FormWindowState.Maximized)
            {
                currentMetrics.IsMaximized = true;
                currentMetrics.HorizontalResizeBorderThickness = SystemInformation.HorizontalResizeBorderThickness * 2;
                currentMetrics.VerticalResizeBorderThickness = SystemInformation.VerticalResizeBorderThickness * 2;
            }
            else
            {
                currentMetrics.IsMaximized = false;
                currentMetrics.HorizontalResizeBorderThickness = SystemInformation.HorizontalResizeBorderThickness;
                currentMetrics.VerticalResizeBorderThickness = SystemInformation.VerticalResizeBorderThickness;
            }

            currentMetrics.CaptionHeight = CaptionHeight;

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
                MainMenuStrip.SetBounds(
                    currentMetrics.MainMenuLeft,
                    currentMetrics.MainMenuTop,
                    currentMetrics.MainMenuWidth,
                    currentMetrics.MainMenuHeight);
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

            foreach (Control control in Controls)
            {
                if (control != MainMenuStrip
                    && control != closeButton
                    && control != saveButton
                    && control != maximizeButton
                    && control != minimizeButton
                    && control.Visible)
                {
                    control.SetBounds(
                        currentMetrics.ClientAreaLeft,
                        currentMetrics.ClientAreaTop,
                        currentMetrics.ClientAreaWidth,
                        currentMetrics.ClientAreaHeight);
                }
            }

            // Update maximize button because Aero snap changes the client size directly and updates
            // the window state, but does not seem to call WndProc with e.g. a WM_SYSCOMMAND.
            UpdateMaximizeButtonIcon();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (currentMetrics.IsMaximized && !IsMdiContainer)
            {
                // Draw the window using the current transparency key.
                // Unfortunately this doesn't work for MdiContainers, so then we need to revert back to default behavior.
                using (var captionAreaColorBrush = new SolidBrush(TransparencyKey))
                {
                    g.FillRectangle(captionAreaColorBrush, new Rectangle(
                        0,
                        0,
                        currentMetrics.TotalWidth,
                        currentMetrics.TotalHeight));
                }

                // Block out only the visible area of the window.
                using (var captionAreaColorBrush = new SolidBrush(ObservableStyle.BackColor))
                {
                    int horizontalInvisibleBorderWidth = currentMetrics.HorizontalResizeBorderThickness / 2;
                    int verticalInvisibleBorderWidth = currentMetrics.VerticalResizeBorderThickness / 2;

                    g.FillRectangle(captionAreaColorBrush, new Rectangle(
                        horizontalInvisibleBorderWidth,
                        verticalInvisibleBorderWidth,
                        currentMetrics.TotalWidth - horizontalInvisibleBorderWidth * 2,
                        currentMetrics.TotalHeight - verticalInvisibleBorderWidth * 2));
                }
            }
            else
            {
                // Block out the entire client area, then draw a 1-pizel border around it.
                using (var captionAreaColorBrush = new SolidBrush(ObservableStyle.BackColor))
                {
                    g.FillRectangle(captionAreaColorBrush, new Rectangle(
                        0,
                        0,
                        currentMetrics.TotalWidth,
                        currentMetrics.TotalHeight));
                }

                g.DrawRectangle(Pens.DimGray,
                    0,
                    0,
                    currentMetrics.TotalWidth - 1,
                    currentMetrics.TotalHeight - 1);
            }

            string text = Text;

            if (!string.IsNullOrWhiteSpace(text))
            {
                // (0, width) places the Text in the center of the whole caption area bar, which is the most natural.
                int textAreaLeftEdge = currentMetrics.MainMenuLeft;
                int textAreaWidth = currentMetrics.ClientAreaWidth;

                // Measure the text. If its left edge is about to disappear under the main menu,
                // use a different Rectangle to center the text in.
                // See also e.g. Visual Studio Code where it works the same way.
                // Also use an extra MainMenuHorizontalMargin.
                Size measuredTextSize = TextRenderer.MeasureText(text, ObservableStyle.Font);
                int probableLeftTextEdge = (textAreaWidth - measuredTextSize.Width) / 2;

                if (probableLeftTextEdge < currentMetrics.MainMenuWidth)
                {
                    // Now place it in the middle between the outer menu right edge and the system buttons.
                    textAreaLeftEdge = currentMetrics.MainMenuLeft + currentMetrics.MainMenuWidth;
                    textAreaWidth = currentMetrics.MinimizeButtonLeft - textAreaLeftEdge;
                }

                Rectangle textAreaRectangle = new Rectangle(
                    textAreaLeftEdge,
                    currentMetrics.MainMenuTop,
                    textAreaWidth,
                    currentMetrics.MainMenuHeight - 2);

                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                TextRenderer.DrawText(
                    g,
                    text,
                    ObservableStyle.Font,
                    textAreaRectangle,
                    ObservableStyle.ForeColor,
                    ObservableStyle.BackColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg >= WM.NCCALCSIZE && m.Msg <= WM.NCHITTEST)
            {
                if (m.Msg == WM.NCCALCSIZE)
                {
                    m.Result = IntPtr.Zero;
                    return;
                }
                else
                {
                    Point position = PointToClient(new Point(m.LParam.ToInt32()));

                    // Any of the resize borders?
                    if (position.Y < currentMetrics.VerticalResizeBorderThickness)
                    {
                        if (position.X < currentMetrics.HorizontalResizeBorderThickness) m.Result = (IntPtr)HT.TOPLEFT;
                        else if (position.X >= currentMetrics.TotalWidth - currentMetrics.HorizontalResizeBorderThickness) m.Result = (IntPtr)HT.TOPRIGHT;
                        else m.Result = (IntPtr)HT.TOP;
                    }
                    else if (position.Y >= currentMetrics.TotalHeight - currentMetrics.VerticalResizeBorderThickness)
                    {
                        if (position.X < currentMetrics.HorizontalResizeBorderThickness) m.Result = (IntPtr)HT.BOTTOMLEFT;
                        else if (position.X >= currentMetrics.TotalWidth - currentMetrics.HorizontalResizeBorderThickness) m.Result = (IntPtr)HT.BOTTOMRIGHT;
                        else m.Result = (IntPtr)HT.BOTTOM;
                    }
                    else if (position.X < currentMetrics.HorizontalResizeBorderThickness)
                    {
                        m.Result = (IntPtr)HT.LEFT;
                    }
                    else if (position.X >= currentMetrics.TotalWidth - currentMetrics.HorizontalResizeBorderThickness)
                    {
                        m.Result = (IntPtr)HT.RIGHT;
                    }
                    else if (position.Y >= currentMetrics.CaptionHeight)
                    {
                        // Client area.
                        m.Result = (IntPtr)HT.CLIENT;
                    }
                    else if (position.X < currentMetrics.MainMenuLeft + currentMetrics.MainMenuWidth || position.X >= currentMetrics.MinimizeButtonLeft)
                    {
                        // Main menu or system button.
                        m.Result = (IntPtr)HT.CLIENT;
                    }
                    else
                    {
                        // This is the draggable 'caption' area.
                        m.Result = (IntPtr)HT.CAPTION;
                    }

                    return;
                }
            }
            else if (m.Msg == WM.NCRBUTTONUP)
            {
                // Emulate the system behavior of clicking the right mouse button over the caption area to bring up the system menu.
                if (m.WParam.ToInt32() == HT.CAPTION)
                {
                    this.ShowSystemMenu(new Point(m.LParam.ToInt32()));
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) ObservableStyle.Dispose();
            base.Dispose(disposing);
        }
    }

    // OldMenuCaptionBarForm retained while there are still client area controls that do not implement IDockableControl.
    public class MenuCaptionBarForm : OldMenuCaptionBarForm
    {
    }

    /// <summary>
    /// <see cref="UIActionForm"/> which contains a single client control, and displays a main menu in the top left area of a custom caption bar.
    /// </summary>
    /// <typeparam name="TDockableControl">
    /// The type of the client control. It must derive from <see cref="Control"/> and implements the <see cref="IDockableControl"/>
    /// interface, which allows it to be hosted in a <see cref="MdiTabControl"/> as well.
    /// </typeparam>
    /// <remarks>
    /// Some non-client area handling code is adapted from:
    /// https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Shell/WindowChromeWorker.cs,369313199b0de06c
    /// </remarks>
    public sealed class MenuCaptionBarForm<TDockableControl> : MenuCaptionBarForm
        where TDockableControl : Control, IDockableControl
    {
        public TDockableControl DockedControl { get; }

        public MenuCaptionBarForm(TDockableControl dockableControl)
        {
            DockedControl = dockableControl ?? throw new ArgumentNullException(nameof(dockableControl));
            Controls.Add(DockedControl);
        }
    }
}
