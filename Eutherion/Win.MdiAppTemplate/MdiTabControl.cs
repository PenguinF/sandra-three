#region License
/*********************************************************************************
 * MdiTabControl.cs
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

        public void OnClosing(CloseReason closeReason, ref bool cancel)
        {
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
                    ActiveControl = tabPage.ClientControl;
                }
            }
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

        protected override void OnAfterTabActivated(GlyphTabControlEventArgs e)
        {
            UpdateCaptionText(e.TabPageIndex);
            base.OnAfterTabActivated(e);
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
            Text = dockProperties.CaptionText;
            ActiveBackColor = dockProperties.TabBackColor;
            ActiveForeColor = dockProperties.TabForeColor;
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
