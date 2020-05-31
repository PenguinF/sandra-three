#region License
/*********************************************************************************
 * GlyphTabControl.cs
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Eutherion.Win.Controls
{
    /// <summary>
    /// Non-selectable control which displays a set of tab pages, and draws a modified-close glyph in each tab header.
    /// </summary>
    public partial class GlyphTabControl : ContainerControl
    {
        /// <summary>
        /// Gets the character which represents a modified state.
        /// </summary>
        public static string ModifiedMarkerCharacter { get; } = "•";

        private static int Checked(int value, int minimumValue, string propertyName)
        {
            if (value < minimumValue) throw new ArgumentOutOfRangeException(propertyName, value, $"{propertyName} must be {minimumValue} or higher.");
            return value;
        }

        /// <summary>
        /// Gets or sets the tab header height. The minimum value is 1. The default value is <see cref="DefaultTabHeaderHeight"/> (26).
        /// </summary>
        [DefaultValue(DefaultTabHeaderHeight)]
        public int TabHeaderHeight
        {
            get => tabHeaderHeight;
            set { if (tabHeaderHeight != value) { tabHeaderHeight = Checked(value, 1, nameof(TabHeaderHeight)); PerformLayout(); } }
        }

        private int tabHeaderHeight = DefaultTabHeaderHeight;

        /// <summary>
        /// The default value for the <see cref="TabHeaderHeight"/> property.
        /// </summary>
        public const int DefaultTabHeaderHeight = 26;

        /// <summary>
        /// Gets or sets the width a tab header will occupy if not constrained. The minimum value is 1. The default value is <see cref="DefaultTabWidth"/> (120).
        /// </summary>
        [DefaultValue(DefaultTabWidth)]
        public int TabWidth
        {
            get => tabWidth;
            set { if (tabWidth != value) { tabWidth = Checked(value, 1, nameof(TabWidth)); HeaderPanel.UpdateMetrics(); } }
        }

        private int tabWidth = DefaultTabWidth;

        /// <summary>
        /// The default value for the <see cref="TabWidth"/> property.
        /// </summary>
        public const int DefaultTabWidth = 120;

        /// <summary>
        /// Gets or sets the horizontal margin for text in a tab header. The minimum value is 0. The default value is <see cref="DefaultHorizontalTabTextMargin"/> (6).
        /// </summary>
        [DefaultValue(DefaultHorizontalTabTextMargin)]
        public int HorizontalTabTextMargin
        {
            get => horizontalTabTextMargin;
            set { if (horizontalTabTextMargin != value) { horizontalTabTextMargin = Checked(value, 0, nameof(HorizontalTabTextMargin)); HeaderPanel.UpdateMetrics(); } }
        }

        private int horizontalTabTextMargin = DefaultHorizontalTabTextMargin;

        /// <summary>
        /// The default value for the <see cref="HorizontalTabTextMargin"/> property.
        /// </summary>
        public const int DefaultHorizontalTabTextMargin = 6;

        /// <summary>
        /// Gets or sets the background color for the inactive header area of the control.
        /// </summary>
        public override Color BackColor { get => base.BackColor; set { if (base.BackColor != value) { base.BackColor = value; HeaderPanel.UpdateNonMetrics(); } } }

        /// <summary>
        /// Gets or sets the foreground color for the inactive header area of the control.
        /// </summary>
        public override Color ForeColor { get => base.ForeColor; set { if (base.ForeColor != value) { base.ForeColor = value; HeaderPanel.UpdateNonMetrics(); } } }

        /// <summary>
        /// Gets or sets the font of the text displayed in all header areas of the control.
        /// </summary>
        public override Font Font { get => base.Font; set { if (base.Font != value) { base.Font = value; HeaderPanel.UpdateNonMetrics(); } } }

        /// <summary>
        /// Gets or sets the background color to display when the mouse is hovering over an inactive tab header.
        /// </summary>
        public Color InactiveTabHeaderHoverColor
        {
            get => inactiveTabHeaderHoverColor;
            set { if (inactiveTabHeaderHoverColor != value) { inactiveTabHeaderHoverColor = value; HeaderPanel.UpdateNonMetrics(); } }
        }

        private Color inactiveTabHeaderHoverColor;

        /// <summary>
        /// Gets or sets the border color to display when the mouse is hovering over an inactive tab header.
        /// </summary>
        public Color InactiveTabHeaderHoverBorderColor
        {
            get => inactiveTabHeaderHoverBorderColor;
            set { if (inactiveTabHeaderHoverBorderColor != value) { inactiveTabHeaderHoverBorderColor = value; HeaderPanel.UpdateNonMetrics(); } }
        }

        private Color inactiveTabHeaderHoverBorderColor;

        /// <summary>
        /// Gets or sets the foreground color to display for a glyph when the mouse is hovering over an inactive tab header but not over the glyph itself.
        /// If this color is empty, <see cref="ForeColor"/> is used.
        /// </summary>
        public Color InactiveTabHeaderGlyphForeColor
        {
            get => inactiveTabHeaderGlyphForeColor;
            set { if (inactiveTabHeaderGlyphForeColor != value) { inactiveTabHeaderGlyphForeColor = value; HeaderPanel.UpdateNonMetrics(); } }
        }

        private Color inactiveTabHeaderGlyphForeColor;

        /// <summary>
        /// Gets or sets the foreground color to display for a glyph when the mouse is hovering over it on an inactive tab header.
        /// If this color is empty, a lighter version of <see cref="InactiveTabHeaderGlyphForeColor"/> is used.
        /// </summary>
        public Color InactiveTabHeaderGlyphHoverColor
        {
            get => inactiveTabHeaderGlyphHoverColor;
            set { if (inactiveTabHeaderGlyphHoverColor != value) { inactiveTabHeaderGlyphHoverColor = value; HeaderPanel.UpdateNonMetrics(); } }
        }

        private Color inactiveTabHeaderGlyphHoverColor;

        /// <summary>
        /// Gets the collection of tab pages in this control.
        /// </summary>
        public TabPageCollection TabPages { get; }

        /// <summary>
        /// Gets the index of the active tab page, or -1 if none is active.
        /// </summary>
        public int ActiveTabPageIndex { get; private set; } = -1;

        #region Ignored properties

        /// <summary>
        /// This property is ignored.
        /// </summary>
        [Browsable(false)]
        public override Image BackgroundImage { get => base.BackgroundImage; set => base.BackgroundImage = value; }

        /// <summary>
        /// This property is ignored.
        /// </summary>
        [Browsable(false)]
        public override ImageLayout BackgroundImageLayout { get => base.BackgroundImageLayout; set => base.BackgroundImageLayout = value; }

        /// <summary>
        /// This property is ignored.
        /// </summary>
        [Browsable(false)]
        public override string Text { get => base.Text; set => base.Text = value; }

        #endregion Ignored properties

        private readonly TabHeaderPanel HeaderPanel;

        /// <summary>
        /// Initializes a new instance of <see cref="GlyphTabControl"/>.
        /// </summary>
        public GlyphTabControl()
        {
            HeaderPanel = new TabHeaderPanel(this);
            TabPages = new TabPageCollection(this);

            Controls.Add(HeaderPanel);
        }

        private void TabInserted(TabPage tabPage, int tabPageIndex)
        {
            // Make sure the same page remains activated.
            if (ActiveTabPageIndex >= tabPageIndex) ActiveTabPageIndex++;

            tabPage.ClientControl.Visible = false;
            Controls.Add(tabPage.ClientControl);
            HeaderPanel.UpdateMetrics();
            tabPage.NotifyChange += Tab_NotifyChange;

            OnAfterTabInserted(new GlyphTabControlEventArgs(tabPage, tabPageIndex));
        }

        private void TabRemoved(TabPage tabPage, int tabPageIndex, bool disposeClientControl)
        {
            // Make sure the same page remains activated.
            // If the active tab page is closed, just deselect.
            if (tabPageIndex == ActiveTabPageIndex) ActiveTabPageIndex = -1;
            else if (tabPageIndex < ActiveTabPageIndex) ActiveTabPageIndex--;

            tabPage.NotifyChange -= Tab_NotifyChange;
            if (disposeClientControl) tabPage.ClientControl.Dispose();
            Controls.Remove(tabPage.ClientControl);
            HeaderPanel.UpdateMetrics();

            var e = new GlyphTabControlEventArgs(tabPage, tabPageIndex);
            if (disposeClientControl) OnAfterTabClosed(e); else OnAfterTabRemoved(e);
        }

        private void Tab_NotifyChange(TabPage tabPage)
        {
            int tabPageIndex = TabPages.IndexOf(tabPage);
            if (tabPageIndex >= 0)
            {
                HeaderPanel.UpdateNonMetrics();
                OnTabNotifyChange(new GlyphTabControlEventArgs(tabPage, tabPageIndex));
            }
        }

        private void TabHeaderClicked(int tabPageIndex, bool mouseOverGlyph)
        {
            if (!mouseOverGlyph)
            {
                // Default is to activate the tab.
                ActivateTab(tabPageIndex);
            }
            else
            {
                // Close the tab page rather than activate it.
                TabPage tabPage = TabPages[tabPageIndex];
                var cancelEventArgs = new GlyphTabControlCancelEventArgs(tabPage, tabPageIndex);
                OnTabHeaderGlyphClick(cancelEventArgs);

                if (!cancelEventArgs.Cancel)
                {
                    // Need to dispose the client control, this is an actual close action.
                    TabPages.RemoveTab(tabPageIndex, disposeClientControl: true);
                }
            }
        }

        /// <summary>
        /// Activates the <see cref="TabPage"/> with the given index.
        /// </summary>
        /// <param name="tabPageIndex">
        /// The index of the tab page to activate.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="tabPageIndex"/> is less than 0, or greater than or equal to the number of tab pages in <see cref="TabPages"/>.
        /// </exception>
        public void ActivateTab(int tabPageIndex)
        {
            if (tabPageIndex < 0 || tabPageIndex >= TabPages.Count) throw new ArgumentOutOfRangeException(nameof(tabPageIndex));

            if (ActiveTabPageIndex != tabPageIndex)
            {
                TabPage newActiveTabPage = TabPages[tabPageIndex];

                Control oldActiveControl = ActiveTabPageIndex >= 0 ? TabPages[ActiveTabPageIndex].ClientControl : null;
                Control newActiveControl = newActiveTabPage.ClientControl;

                ActiveTabPageIndex = tabPageIndex;
                newActiveControl.Visible = true;
                if (oldActiveControl != null) oldActiveControl.Visible = false;
                ActiveControl = newActiveControl;
                HeaderPanel.UpdateNonMetrics();

                OnAfterTabActivated(new GlyphTabControlEventArgs(newActiveTabPage, tabPageIndex));
            }
        }

        /// <summary>
        /// Occurs after a property of a tab page in the <see cref="TabPages"/> collection is modified.
        /// </summary>
        public event EventHandler<GlyphTabControlEventArgs> TabNotifyChange;

        /// <summary>
        /// Raises the <see cref="TabNotifyChange"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="GlyphTabControlEventArgs"/> providing the event data.
        /// </param>
        protected virtual void OnTabNotifyChange(GlyphTabControlEventArgs e) => TabNotifyChange?.Invoke(this, e);

        /// <summary>
        /// Occurs after a new tab page is inserted.
        /// </summary>
        public event EventHandler<GlyphTabControlEventArgs> AfterTabInserted;

        /// <summary>
        /// Raises the <see cref="AfterTabInserted"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="GlyphTabControlEventArgs"/> providing the event data.
        /// </param>
        protected virtual void OnAfterTabInserted(GlyphTabControlEventArgs e) => AfterTabInserted?.Invoke(this, e);

        /// <summary>
        /// Occurs after a new tab page is removed.
        /// The tab page in the event data is not part of the <see cref="TabPages"/> collection anymore.
        /// </summary>
        public event EventHandler<GlyphTabControlEventArgs> AfterTabRemoved;

        /// <summary>
        /// Raises the <see cref="AfterTabRemoved"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="GlyphTabControlEventArgs"/> providing the event data.
        /// </param>
        protected virtual void OnAfterTabRemoved(GlyphTabControlEventArgs e) => AfterTabRemoved?.Invoke(this, e);

        /// <summary>
        /// Occurs after a new tab page is closed. This is similar to the <see cref="AfterTabRemoved"/> event,
        /// but in this case the client control on the closed tab page has been disposed.
        /// The <see cref="AfterTabRemoved"/> event is not raised for the closed tab page.
        /// The tab page in the event data is not part of the <see cref="TabPages"/> collection anymore.
        /// </summary>
        public event EventHandler<GlyphTabControlEventArgs> AfterTabClosed;

        /// <summary>
        /// Raises the <see cref="AfterTabClosed"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="GlyphTabControlEventArgs"/> providing the event data.
        /// </param>
        protected virtual void OnAfterTabClosed(GlyphTabControlEventArgs e) => AfterTabClosed?.Invoke(this, e);

        /// <summary>
        /// Occurs after a tab page is activated.
        /// </summary>
        public event EventHandler<GlyphTabControlEventArgs> AfterTabActivated;

        /// <summary>
        /// Raises the <see cref="AfterTabActivated"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="GlyphTabControlEventArgs"/> providing the event data.
        /// </param>
        protected virtual void OnAfterTabActivated(GlyphTabControlEventArgs e) => AfterTabActivated?.Invoke(this, e);

        /// <summary>
        /// Occurs a the tab header glyph is clicked or otherwise invoked.
        /// </summary>
        public event EventHandler<GlyphTabControlCancelEventArgs> TabHeaderGlyphClick;

        /// <summary>
        /// Raises the <see cref="TabHeaderGlyphClick"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="GlyphTabControlCancelEventArgs"/> providing the event data.
        /// </param>
        protected virtual void OnTabHeaderGlyphClick(GlyphTabControlCancelEventArgs e) => TabHeaderGlyphClick?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="Control.Layout"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="LayoutEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            var clientSize = ClientSize;
            HeaderPanel.SetBounds(0, 0, clientSize.Width, TabHeaderHeight);

            foreach (var tabPage in TabPages)
            {
                tabPage.ClientControl.SetBounds(0, TabHeaderHeight, clientSize.Width, clientSize.Height - TabHeaderHeight);
            }

            base.OnLayout(e);
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">
        /// A <see cref="PaintEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (ActiveTabPageIndex < 0)
            {
                // Default behavior if there is no control to display in the tab client area.
                var clientSize = ClientSize;

                e.Graphics.FillRectangle(Brushes.Black, new Rectangle(
                    0, TabHeaderHeight, clientSize.Width, clientSize.Height - TabHeaderHeight));
            }
        }
    }

    /// <summary>
    /// Provides data for <see cref="GlyphTabControl"/> events.
    /// </summary>
    public class GlyphTabControlEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="GlyphTabControl.TabPage"/> the event is occurring for.
        /// </summary>
        public GlyphTabControl.TabPage TabPage { get; }

        /// <summary>
        /// Gets the zero-based index of the <see cref="GlyphTabControl.TabPage"/> the event is occurring for.
        /// </summary>
        public int TabPageIndex { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GlyphTabControlEventArgs"/>.
        /// </summary>
        /// <param name="tabPage">
        /// The <see cref="GlyphTabControl.TabPage"/> the event is occurring for.
        /// </param>
        /// <param name="tabPageIndex">
        /// The zero-based index of the <see cref="GlyphTabControl.TabPage"/> the event is occurring for.
        /// </param>
        public GlyphTabControlEventArgs(GlyphTabControl.TabPage tabPage, int tabPageIndex)
        {
            TabPage = tabPage;
            TabPageIndex = tabPageIndex;
        }
    }

    /// <summary>
    /// Provides data for cancelable <see cref="GlyphTabControl"/> events.
    /// </summary>
    public class GlyphTabControlCancelEventArgs : GlyphTabControlEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="GlyphTabControlCancelEventArgs"/>
        /// with the <see cref="Cancel"/> property set to false.
        /// </summary>
        public GlyphTabControlCancelEventArgs(GlyphTabControl.TabPage tabPage, int tabPageIndex) : base(tabPage, tabPageIndex) { }

        /// <summary>
        /// Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
