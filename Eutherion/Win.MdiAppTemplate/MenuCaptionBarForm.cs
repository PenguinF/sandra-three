#region License
/*********************************************************************************
 * MenuCaptionBarForm.cs
 *
 * Copyright (c) 2004-2025 Henk Nicolai
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

using Eutherion.Text;
using Eutherion.UIActions;
using Eutherion.Win.Controls;
using Eutherion.Win.Forms;
using Eutherion.Win.Native;
using Eutherion.Win.UIActions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// <see cref="UIActionForm"/> which contains a single client control, and displays a main menu in the top left area of a custom caption bar.
    /// </summary>
    /// <remarks>
    /// Some non-client area handling code is adapted from:
    /// https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Shell/WindowChromeWorker.cs,369313199b0de06c
    /// </remarks>
    public abstract class MenuCaptionBarForm : UIActionForm, IWeakEventTarget
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

            public int MinimumWidth;
            public int MinimumHeight;

            public void UpdateSystemButtonMetrics(bool saveButtonVisible, bool maximizeButtonVisible, bool mininizeButtonVisible)
            {
                // Calculate top edge position for all caption buttons: 1 pixel above center.
                SystemButtonTop = CaptionHeight - captionButtonHeight - 2;

                if (SystemButtonTop < 0)
                {
                    SystemButtonTop = 0;
                    SystemButtonHeight = CaptionHeight - VerticalResizeBorderThickness;
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
                MaximizeButtonLeft = SaveButtonLeft;
                if (maximizeButtonVisible) MaximizeButtonLeft -= captionButtonWidth + closeButtonMargin;
                MinimizeButtonLeft = MaximizeButtonLeft;
                if (mininizeButtonVisible) MinimizeButtonLeft -= captionButtonWidth;

                // An appopriate MinimumSize is a size which allows the entire menu and the system buttons to be displayed,
                // taking border thickness into account.
                MinimumWidth = MainMenuLeft + MainMenuWidth + TotalWidth - MinimizeButtonLeft;
                MinimumHeight = CaptionHeight + VerticalResizeBorderThickness * 2;
            }
        }

        private static readonly Color UnsavedModificationsCloseButtonHoverColor = Color.FromArgb(0xff, 0xc0, 0xc0);

        // Access to a private field in Form such that the effects of RestoreWindowBoundsIfNecessary() can be controlled.
        private static readonly FieldInfo restoredWindowBoundsSpecifiedFieldInfo;

        static MenuCaptionBarForm()
        {
            try
            {
                // Depending on the version, the field may have different names.
                restoredWindowBoundsSpecifiedFieldInfo
                    = typeof(Form).GetField("_restoredWindowBoundsSpecified", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? typeof(Form).GetField("restoredWindowBoundsSpecified", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
            catch (Exception e)
            {
                // Instead of generating a TypeLoadException from not being able to complete
                // the static initializer, just trace the exception and move on.
                // The bug that can be fixed by having access to restoredWindowBoundsSpecified just isn't that important.
                e.Trace();
            }
        }

        private const int MainMenuHorizontalMargin = 8;

        public const int DefaultCaptionHeight = 30;
        public int CaptionHeight { get; set; } = DefaultCaptionHeight;

        private readonly NonSelectableButton minimizeButton;
        private readonly NonSelectableButton maximizeButton;
        private readonly NonSelectableButton saveButton;
        private readonly NonSelectableButton closeButton;

        private readonly ToolTip ToolTip;

        private readonly UIActionHandler mainMenuActionHandler;

        private Button currentHoverButton;
        private Color? closeButtonHoverColorOverride;

        private Metrics currentMetrics;

        private protected MenuCaptionBarForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.UserMouse
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.FixedHeight
                | ControlStyles.FixedWidth
                | ControlStyles.ResizeRedraw
                | ControlStyles.Opaque, true);

            MinimumSize = new Size(20, 20);

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
                    ActionHandler.TryPerformAction(SharedUIAction.SaveToFile.Key, true);
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
                UIActionState currentActionState = ActionHandler.TryPerformAction(SharedUIAction.SaveToFile.Key, false);
                saveButton.Visible = currentActionState.Visible;
                saveButton.Enabled = currentActionState.Enabled;
                if (!saveButton.Enabled) saveButton.FlatAppearance.BorderColor = ObservableStyle.BackColor;
            };

            closeButton = CreateCaptionButton();
            closeButton.Click += (_, __) => Close();

            MainMenuStrip = new MenuStrip();

            SuspendLayout();

            Controls.Add(minimizeButton);
            Controls.Add(maximizeButton);
            Controls.Add(saveButton);
            Controls.Add(closeButton);
            Controls.Add(MainMenuStrip);

            ResumeLayout();

            ToolTip = new ToolTip();
            UpdateToolTips();

            ObservableStyle.NotifyChange += ObservableStyle_NotifyChange;

            mainMenuActionHandler = new UIActionHandler();

            this.BindActions(StandardUIActionBindings);

            Session.Current.CurrentLocalizerChanged += CurrentLocalizerChanged;
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
        /// Gets the docked <see cref="Control"/> and <see cref="IDockableControl"/>.
        /// </summary>
        public abstract IDockableControl DockedAsDockable { get; }

        /// <summary>
        /// Gets the docked <see cref="Control"/> and <see cref="IDockableControl"/>.
        /// </summary>
        public abstract Control DockedAsControl { get; }

        /// <summary>
        /// Gets the size of the client area of the form.
        /// </summary>
        public Size ClientAreaSize => new Size(currentMetrics.ClientAreaWidth, currentMetrics.ClientAreaHeight);

        /// <summary>
        /// Returns the style of the title bar which can be observed for changes.
        /// </summary>
        public MenuCaptionBarFormStyle ObservableStyle { get; } = new MenuCaptionBarFormStyle();

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

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnLoad(EventArgs e)
        {
            // For when another window is active when this form is first shown.
            UpdateCaptionAreaButtonsBackColor();
            base.OnLoad(e);

            // If the docked control is a ContainerControl, it will move the focus to its own ActiveControl.
            ActiveControl = DockedAsControl;
        }

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
            var mainMenuItem = (ToolStripDropDownItem)sender;

            // Use DefaultForeColor rather than titleBarForeColor when dropped down.
            mainMenuItem.ForeColor = DefaultForeColor;

            // Remember last toolstrip separator before a developer tool item with FirstInGroup set.
            ToolStripSeparator lastSeparator = null;

            foreach (ToolStripItem toolStripItem in mainMenuItem.DropDownItems)
            {
                if (toolStripItem is UIActionToolStripMenuItem menuItem)
                {
                    UIActionState actionState = mainMenuActionHandler.TryPerformAction(menuItem.ActionKey, false);
                    menuItem.Update(actionState);

                    if (Session.IsDeveloperTool(menuItem.ActionKey))
                    {
                        // Hide instead of disable developer tool items.
                        bool visible = actionState.UIActionVisibility == UIActionVisibility.Enabled;
                        menuItem.Visible = visible;
                        if (lastSeparator != null) lastSeparator.Visible = visible;
                    }
                }

                lastSeparator = toolStripItem as ToolStripSeparator;
            }
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

            maximizeButton.Visible = MaximizeBox;
            minimizeButton.Visible = MinimizeBox;
            currentMetrics.UpdateSystemButtonMetrics(saveButton.Visible, MaximizeBox, MinimizeBox);

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

            if (maximizeButton.Visible)
            {
                maximizeButton.SetBounds(
                    currentMetrics.MaximizeButtonLeft,
                    currentMetrics.SystemButtonTop,
                    currentMetrics.SystemButtonWidth,
                    currentMetrics.SystemButtonHeight);
            }

            if (minimizeButton.Visible)
            {
                minimizeButton.SetBounds(
                    currentMetrics.MinimizeButtonLeft,
                    currentMetrics.SystemButtonTop,
                    currentMetrics.SystemButtonWidth,
                    currentMetrics.SystemButtonHeight);
            }

            // Do a null-check in case this is called from our constructor, it may not yet been set.
            DockedAsControl?.SetBounds(
                currentMetrics.ClientAreaLeft,
                currentMetrics.ClientAreaTop,
                currentMetrics.ClientAreaWidth,
                currentMetrics.ClientAreaHeight);

            // Setting MinimumSize on an inactive Form will activate it.
            // To work around this issue, I tried delaying updates to MinimumSize until the window is active.
            // However, this prevents a window from being restored to a maximized state from an auto-save file,
            // even if e.g. a nonce is used.
            //
            // See also:
            // https://github.com/dotnet/winforms/issues/3020
            // https://social.msdn.microsoft.com/Forums/en-US/9625570d-4bd7-48f2-b158-15dcbd96c60b/bug-in-form-class-setting-formminimumsize-triggers-an-activated-event-even-when-the-form-is
            // https://social.msdn.microsoft.com/Forums/en-US/31f9d493-f8d6-4b39-92c9-912399ed3964/why-parentformdeactivate-event-getting-triggered-when-try-to-set-the-minimum-size-for-child-form

            // As a workaround, instead of setting MinimumSize, update MinimumWidth/MinimumHeight, and prevent resizes to smaller sizes.
            // MinimumSize = new Size(MinimumWidth, MinimumHeight)

            // Update maximize button because Aero snap changes the client size directly and updates
            // the window state, but does not seem to call WndProc with e.g. a WM_SYSCOMMAND.
            UpdateMaximizeButtonIcon();
        }

        protected override void OnResizing(ResizeEventArgs e)
        {
            base.OnResizing(e);

            int minimumRight = e.MoveResizeRect.Left + currentMetrics.MinimumWidth;
            if (e.MoveResizeRect.Right < minimumRight)
            {
                e.MoveResizeRect.Right = minimumRight;
            }

            int minimumBottom = e.MoveResizeRect.Top + currentMetrics.MinimumHeight;
            if (e.MoveResizeRect.Bottom < minimumBottom)
            {
                e.MoveResizeRect.Bottom = minimumBottom;
            }
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
                // Place the text in the middle between the outer menu right edge and the system buttons.
                // Also 1 pixel above center to align with the main menu.
                int textAreaLeftEdge = currentMetrics.MainMenuLeft + currentMetrics.MainMenuWidth;
                int textAreaWidth = currentMetrics.MinimizeButtonLeft - textAreaLeftEdge;

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

            // Only after the first paint allow window to be drawn in layered mode.
            if (!AllowTransparency)
            {
                TransparencyKey = ObservableStyle.SuggestedTransparencyKey;
                AllowTransparency = true;
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
            else if (m.Msg == WM.WINDOWPOSCHANGED)
            {
                base.WndProc(ref m);

                if (restoredWindowBoundsSpecifiedFieldInfo != null)
                {
                    // Erase all BoundsSpecified values from restoredWindowBoundsSpecified,
                    // so that this window restores at the expected size+location after maximize->minimize->deminimize->demaximize.
                    // RestoreWindowBoundsIfNecessary() incorrectly assumes things about this window's behavior.
                    if (restoredWindowBoundsSpecifiedFieldInfo.GetValue(this) is BoundsSpecified restoredWindowBoundsSpecified)
                    {
                        restoredWindowBoundsSpecifiedFieldInfo.SetValue(this, restoredWindowBoundsSpecified & ~BoundsSpecified.All);
                    }
                }
            }
            else if (m.Msg == WM.DWMCOMPOSITIONCHANGED)
            {
                // Windows 7 and lower only. This message is not used in Windows 8 and higher.
                // Make sure to update the accent colors as well.
                UpdateCaptionAreaButtonsBackColor();
            }
            else if (m.Msg == WM.SYSCOMMAND)
            {
                int wParam = SC.MASK & m.WParam.ToInt32();

                base.WndProc(ref m);

                // Make sure the maximize button icon is updated too when the FormWindowState is updated externally.
                if (wParam == SC.MAXIMIZE || wParam == SC.MINIMIZE || wParam == SC.RESTORE)
                {
                    UpdateMaximizeButtonIcon();
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void UpdateToolTips()
        {
            TextFormatter currentLocalizer = Session.Current.CurrentLocalizer;

            new (NonSelectableButton, StringKey<ForFormattedText>)[]
            {
                (minimizeButton, SharedLocalizedStringKeys.WindowMinimize),
                (maximizeButton, SharedLocalizedStringKeys.WindowMaximize),
                (saveButton, SharedLocalizedStringKeys.Save),
                (closeButton, SharedLocalizedStringKeys.Close),
            }
            .ForEach(x => ToolTip.SetToolTip(x.Item1, currentLocalizer.Format(x.Item2)));
        }

        private void CurrentLocalizerChanged(object sender, EventArgs e)
        {
            UIMenu.UpdateMenu(MainMenuStrip.Items);
            UpdateToolTips();
        }

        private List<UIMenuNode> BindMainMenuItemActions(IEnumerable<Union<UIAction, MainMenuDropDownItem>> dropDownItems)
        {
            var menuNodes = new List<UIMenuNode>();

            foreach (var dropDownItem in dropDownItems)
            {
                if (dropDownItem.IsOption1(out UIAction action))
                {
                    if (action.DefaultInterfaces.TryGet(out IContextMenuUIActionInterface contextMenuInterface))
                    {
                        menuNodes.Add(new UIMenuNode.Element(action.Key, contextMenuInterface));

                        mainMenuActionHandler.BindAction(new UIActionBinding(action, perform =>
                        {
                            try
                            {
                                // Try to find a UIActionHandler that is willing to validate/perform the given action.
                                foreach (var actionHandler in UIActionUtilities.EnumerateUIActionHandlers(FocusHelper.GetFocusedControl()))
                                {
                                    UIActionState currentActionState = actionHandler.TryPerformAction(action.Key, perform);
                                    if (currentActionState.UIActionVisibility != UIActionVisibility.Undetermined)
                                    {
                                        return currentActionState.UIActionVisibility == UIActionVisibility.Hidden
                                            ? UIActionVisibility.Disabled
                                            : currentActionState;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }

                            // No handler in the chain that processes the UIAction actively, so set to disabled.
                            return UIActionVisibility.Disabled;
                        }));
                    }
                }
                else
                {
                    var menuDropDownItem = dropDownItem.ToOption2();
                    menuNodes.Add(menuDropDownItem.Container);
                    menuDropDownItem.Container.Nodes.AddRange(BindMainMenuItemActions(menuDropDownItem.DropDownItems));
                }
            }

            return menuNodes;
        }

        private protected void UpdateFromDockProperties(DockProperties dockProperties)
        {
            CaptionHeight = dockProperties.CaptionHeight;
            Text = dockProperties.CaptionText;

            Icon = dockProperties.Icon;
            ShowIcon = dockProperties.Icon != null;

            // Invalidate to update the save button.
            ActionHandler.Invalidate();

            // If something can be saved, closing is dangerous, therefore use a reddish hover color.
            if (dockProperties.IsModified)
            {
                SetCloseButtonHoverColor(UnsavedModificationsCloseButtonHoverColor);
            }
            else
            {
                ResetCloseButtonHoverColor();
            }

            // Only fill MainMenuStrip once, it's not really supposed to change.
            if (dockProperties.MainMenuItems != null && MainMenuStrip.Items.Count == 0)
            {
                var topLevelContainers = new List<UIMenuNode.Container>();

                foreach (var mainMenuItem in dockProperties.MainMenuItems)
                {
                    if (mainMenuItem.Container != null && mainMenuItem.DropDownItems != null && mainMenuItem.DropDownItems.Any())
                    {
                        topLevelContainers.Add(mainMenuItem.Container);
                        mainMenuItem.Container.Nodes.AddRange(BindMainMenuItemActions(mainMenuItem.DropDownItems));
                    }
                }

                UIMenuBuilder.BuildMenu(mainMenuActionHandler, topLevelContainers, MainMenuStrip.Items);

                foreach (ToolStripDropDownItem mainMenuItem in MainMenuStrip.Items)
                {
                    mainMenuItem.DropDownOpening += MainMenuItem_DropDownOpening;
                    mainMenuItem.DropDownClosed += MainMenuItem_DropDownClosed;
                }
            }

            // CaptionHeight and/or MainMenuStrip can affect the layout.
            PerformLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            bool cancel = e.Cancel;
            DockedAsDockable.CanClose(e.CloseReason, ref cancel);
            e.Cancel = cancel;
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ToolTip.Dispose();
                ObservableStyle.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the regular UIActions for this Form.
        /// </summary>
        public UIActionBindings StandardUIActionBindings => new UIActionBindings
        {
            { SharedUIAction.WindowMenuRestore, TryWindowRestore },
            { SharedUIAction.WindowMenuMove, TryWindowMove },
            { SharedUIAction.WindowMenuSize, TryWindowSize },
            { SharedUIAction.WindowMenuMinimize, TryWindowMinimize },
            { SharedUIAction.WindowMenuMaximize, TryWindowMaximize },
            { SharedUIAction.Close, TryClose },
        };

        public UIActionState TryWindowRestore(bool perform)
        {
            if (WindowState == FormWindowState.Normal) return UIActionVisibility.Disabled;
            if (perform) this.SendWindowCommand(WindowCommand.Restore);
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryWindowMove(bool perform)
        {
            // It appears that the system menu when opened from the caption bar sometimes enables this item when the Form is maximized.
            // I'm not sure if this is a bug, but to be safe disable this action when maximized.
            if (WindowState != FormWindowState.Normal) return UIActionVisibility.Disabled;
            if (perform) this.SendWindowCommand(WindowCommand.Move);
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryWindowSize(bool perform)
        {
            if (FormBorderStyle != FormBorderStyle.Sizable && FormBorderStyle != FormBorderStyle.SizableToolWindow) return UIActionVisibility.Disabled;
            if (WindowState == FormWindowState.Maximized) return UIActionVisibility.Disabled;
            if (perform) this.SendWindowCommand(WindowCommand.Size);
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryWindowMinimize(bool perform)
        {
            if (WindowState == FormWindowState.Minimized || !MinimizeBox) return UIActionVisibility.Disabled;
            if (perform) this.SendWindowCommand(WindowCommand.Minimize);
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryWindowMaximize(bool perform)
        {
            if (WindowState == FormWindowState.Maximized || !MaximizeBox) return UIActionVisibility.Disabled;
            if (perform) this.SendWindowCommand(WindowCommand.Maximize);
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryClose(bool perform)
        {
            if (!ControlBox) return UIActionVisibility.Disabled;
            if (perform) Close();
            return UIActionVisibility.Enabled;
        }
    }

    /// <summary>
    /// <see cref="UIActionForm"/> which contains a single client control, and displays a main menu in the top left area of a custom caption bar.
    /// </summary>
    /// <typeparam name="TDockableControl">
    /// The type of the client control. It must derive from <see cref="Control"/> and implements the <see cref="IDockableControl"/>
    /// interface, which allows it to be hosted in a <see cref="MdiTabControl"/> as well.
    /// </typeparam>
    public class MenuCaptionBarForm<TDockableControl> : MenuCaptionBarForm
        where TDockableControl : Control, IDockableControl
    {
        /// <summary>
        /// Gets the docked <see cref="Control"/> and <see cref="IDockableControl"/>.
        /// </summary>
        public TDockableControl DockedControl { get; }

        /// <summary>
        /// Gets the docked <see cref="Control"/> and <see cref="IDockableControl"/>.
        /// </summary>
        public override IDockableControl DockedAsDockable => DockedControl;

        /// <summary>
        /// Gets the docked <see cref="Control"/> and <see cref="IDockableControl"/>.
        /// </summary>
        public override Control DockedAsControl => DockedControl;

        public MenuCaptionBarForm(TDockableControl dockableControl)
        {
            DockedControl = dockableControl ?? throw new ArgumentNullException(nameof(dockableControl));
            Controls.Add(DockedControl);

            UpdateFromDockProperties(dockableControl.DockProperties);
            dockableControl.DockPropertiesChanged += DockedControl_DockPropertiesChanged;
        }

        private void DockedControl_DockPropertiesChanged() => UpdateFromDockProperties(DockedControl.DockProperties);

        protected override void Dispose(bool disposing)
        {
            if (disposing) DockedControl.DockPropertiesChanged -= DockedControl_DockPropertiesChanged;
            base.Dispose(disposing);
        }
    }
}
