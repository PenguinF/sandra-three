#region License
/*********************************************************************************
 * MdiTabControl.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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

using Eutherion.Win.Controls;
using System;
using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Represents a control with <see cref="TabControl"/>-like capabilities, that hosts a <see cref="IDockableControl"/>
    /// in each of its tab pages. Its tab headers cannot receive focus, instead the control exposes keyboard shortcuts
    /// to navigate between tab pages.
    /// </summary>
    public class MdiTabControl : GlyphTabControl, IDockableControl
    {
        public string ApplicationTitle { get; }

        public DockProperties DockProperties { get; } = new DockProperties();

        public event Action DockPropertiesChanged;

        /// <summary>
        /// Raised when a <see cref="MdiTabPage"/> is about to be undocked.
        /// An event handler should reparent the <see cref="MdiTabPage"/> into a different control tree.
        /// The expected behaviour is that a new <see cref="Form"/> is created which contains the previously docked control.
        /// If the previously docked control is not reparented, the undocking operation fails and the resulting UI/UX is undefined.
        /// </summary>
        public event Action<MdiTabPage> RequestUndock;

        public MdiTabControl(string applicationTitle)
        {
            ApplicationTitle = applicationTitle;
            UpdateCaptionText(-1);
        }

        private void UpdateCaptionText(int activeTabIndex)
        {
            string newCaptionText;

            // Don't support non-MdiTabPages here.
            if (activeTabIndex < 0 || !(TabPages[activeTabIndex] is MdiTabPage mdiTabPage))
            {
                newCaptionText = ApplicationTitle;
            }
            else if (mdiTabPage.DockedControl.DockProperties.IsModified)
            {
                newCaptionText = $"{ModifiedMarkerCharacter} {mdiTabPage.Text} - {ApplicationTitle}";
            }
            else
            {
                newCaptionText = $"{mdiTabPage.Text} - {ApplicationTitle}";
            }

            if (DockProperties.CaptionText != newCaptionText)
            {
                DockProperties.CaptionText = newCaptionText;
                DockPropertiesChanged?.Invoke();
            }
        }

        public void CanClose(CloseReason closeReason, ref bool cancel)
        {
            // Use correct CloseReason value.
            if (closeReason == CloseReason.UserClosing) closeReason = CloseReason.MdiFormClosing;

            // Go through each tab page to see if they can close.
            for (int i = 0; i < TabPages.Count; i++)
            {
                if (TabPages[i] is MdiTabPage mdiTabPage)
                {
                    mdiTabPage.DockedControl.CanClose(closeReason, ref cancel);
                    if (cancel) break;
                }
            }
        }

        /// <summary>
        /// Acivates a control docked in one of the tab pages, then activates the control itself.
        /// </summary>
        public void Activate<TDockableControl>(TDockableControl dockedControl)
            where TDockableControl : Control, IDockableControl
        {
            for (int i = 0; i < TabPages.Count; i++)
            {
                TabPage tabPage = TabPages[i];
                if (tabPage.ClientControl == dockedControl)
                {
                    // Select tab page, move focus.
                    ActivateTab(i);
                    if (i == ActiveTabPageIndex) ActiveControl = dockedControl;
                }
            }
        }

        protected override void OnTabNotifyChange(GlyphTabControlEventArgs e)
        {
            // Text of the active tab page may have been updated.
            if (e.TabPageIndex == ActiveTabPageIndex) UpdateCaptionText(e.TabPageIndex);
            base.OnTabNotifyChange(e);
        }

        protected override void OnAfterTabInserted(GlyphTabControlEventArgs e)
        {
            if (e.TabPage is MdiTabPage mdiTabPage) mdiTabPage.RegisterDockPropertiesChangedEvent();
            base.OnAfterTabInserted(e);
        }

        protected override void OnAfterTabRemoved(GlyphTabControlEventArgs e)
        {
            if (e.TabPage is MdiTabPage mdiTabPage) mdiTabPage.DeregisterDockPropertiesChangedEvent();
            base.OnAfterTabRemoved(e);
        }

        protected override void OnAfterTabClosed(GlyphTabControlEventArgs e)
        {
            if (e.TabPage is MdiTabPage mdiTabPage) mdiTabPage.DeregisterDockPropertiesChangedEvent();
            base.OnAfterTabClosed(e);
        }

        protected override void OnAfterTabActivated(GlyphTabControlEventArgs e)
        {
            UpdateCaptionText(e.TabPageIndex);
            base.OnAfterTabActivated(e);
        }

        protected override void OnTabHeaderGlyphClick(GlyphTabControlCancelEventArgs e)
        {
            if (TabPages[e.TabPageIndex] is MdiTabPage mdiTabPage)
            {
                if (TabPages.Count == 1)
                {
                    // If it's the last tab page, close it.
                    bool cancel = e.Cancel;
                    mdiTabPage.DockedControl.CanClose(CloseReason.UserClosing, ref cancel);
                    e.Cancel = cancel;
                }
                else
                {
                    Undock(mdiTabPage);
                    e.Cancel = true;
                }
            }

            base.OnTabHeaderGlyphClick(e);
        }

        /// <summary>
        /// Undocks a tab page. Returns true if the operation succeeded, otherwise false.
        /// </summary>
        public bool Undock(MdiTabPage mdiTabPage)
        {
            if (RequestUndock != null)
            {
                // To be able to restore state.
                int oldActiveTabPageIndex = ActiveTabPageIndex;
                int removedTabPageIndex = TabPages.IndexOf(mdiTabPage);

                if (TabPages.Remove(mdiTabPage))
                {
                    // Set Visible to true in case an inactive tab page is undocked.
                    mdiTabPage.ClientControl.Visible = true;

                    // Redraw immediately before waiting for the end of the any caller.
                    // Necessary because moving the Control to a different parent is going to force an update as well.
                    Update();

                    // Call the handler and check if the previously docked Control has been properly reparented.
                    RequestUndock(mdiTabPage);

                    if (mdiTabPage.ClientControl.Parent == null)
                    {
                        // Restore original state.
                        TabPages.Insert(removedTabPageIndex, mdiTabPage);
                        if (oldActiveTabPageIndex == removedTabPageIndex) ActivateTab(oldActiveTabPageIndex);
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }
    }

    public abstract class MdiTabPage : GlyphTabControl.TabPage
    {
        public IDockableControl DockedControl { get; }

        private protected MdiTabPage(IDockableControl dockedControl, Control clientControl)
            : base(clientControl)
        {
            DockedControl = dockedControl;
        }

        internal void RegisterDockPropertiesChangedEvent()
        {
            UpdateFromDockProperties();
            DockedControl.DockPropertiesChanged += UpdateFromDockProperties;
        }

        internal void DeregisterDockPropertiesChangedEvent()
        {
            DockedControl.DockPropertiesChanged -= UpdateFromDockProperties;
        }

        private void UpdateFromDockProperties()
        {
            DockProperties dockProperties = DockedControl.DockProperties;
            Text = dockProperties.TabPageText;
            IsModified = dockProperties.IsModified;
            ActiveBackColor = dockProperties.TabBackColor;
            ActiveForeColor = dockProperties.TabForeColor;
            GlyphForeColor = dockProperties.GlyphForeColor;
            GlyphHoverColor = dockProperties.GlyphHoverColor;
        }
    }

    public class MdiTabPage<TDockableControl> : MdiTabPage
        where TDockableControl : Control, IDockableControl
    {
        public MdiTabPage(TDockableControl clientControl)
            : base(clientControl, clientControl)
        {
        }
    }
}
