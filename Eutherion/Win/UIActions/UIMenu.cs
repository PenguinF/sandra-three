﻿#region License
/*********************************************************************************
 * UIMenu.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

using Eutherion.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.UIActions
{
    public abstract class UIMenuNode
    {
        public abstract TResult Accept<TResult>(IUIMenuTreeVisitor<TResult> visitor);

        /// <summary>
        /// Gets the caption for this node. If null or empty, no menu item is generated for this node.
        /// </summary>
        public readonly LocalizedStringKey CaptionKey;

        /// <summary>
        /// Gets the icon to display for this node.
        /// </summary>
        public readonly Image Icon;

        /// <summary>
        /// Gets or sets if this node is the first in a group of nodes.
        /// </summary>
        public bool IsFirstInGroup;

        protected UIMenuNode(LocalizedStringKey captionKey)
        {
            CaptionKey = captionKey;
        }

        protected UIMenuNode(LocalizedStringKey captionKey, Image icon) : this(captionKey)
        {
            Icon = icon;
        }

        public sealed class Element : UIMenuNode
        {
            public readonly UIAction Action;
            public readonly ShortcutKeys Shortcut;

            public Element(UIAction action, UIActionBinding binding) : base(binding.MenuCaptionKey, binding.MenuIcon)
            {
                Action = action ?? throw new ArgumentNullException(nameof(action));
                if (binding.Shortcuts != null) Shortcut = binding.Shortcuts.FirstOrDefault(x => !x.IsEmpty);
                IsFirstInGroup = binding.IsFirstInGroup;
            }

            public override TResult Accept<TResult>(IUIMenuTreeVisitor<TResult> visitor) => visitor.VisitElement(this);
        }

        public sealed class Container : UIMenuNode
        {
            public readonly List<UIMenuNode> Nodes = new List<UIMenuNode>();

            public Container(LocalizedStringKey captionKey) : base(captionKey)
            {
            }

            public Container(LocalizedStringKey captionKey, Image icon) : base(captionKey, icon)
            {
            }

            public override TResult Accept<TResult>(IUIMenuTreeVisitor<TResult> visitor) => visitor.VisitContainer(this);
        }
    }

    public interface IUIMenuTreeVisitor<TResult>
    {
        TResult VisitContainer(UIMenuNode.Container container);
        TResult VisitElement(UIMenuNode.Element element);
    }

    /// <summary>
    /// <see cref="ToolStripMenuItem"/> override which contains localized strings.
    /// </summary>
    public class LocalizedToolStripMenuItem : ToolStripMenuItem
    {
        public LocalizedString LocalizedText;
        public List<LocalizedString> ShortcutKeyDisplayStringParts;
    }

    /// <summary>
    /// <see cref="ToolStripMenuItem"/> override which contains a reference to an <see cref="UIAction"/>.
    /// </summary>
    public class UIActionToolStripMenuItem : LocalizedToolStripMenuItem
    {
        /// <summary>
        /// Gets action for this menu item.
        /// </summary>
        public UIAction Action { get; }

        public UIActionToolStripMenuItem(UIAction action)
        {
            Action = action;
        }

        public void Update(UIActionState currentActionState)
        {
            Enabled = currentActionState.Enabled;
            Checked = currentActionState.Checked;
        }
    }

    public static class UIMenu
    {
        /// <summary>
        /// Adds a dynamic <see cref="ContextMenuStrip"/> to a control if it implements <see cref="IUIActionHandlerProvider"/>.
        /// </summary>
        public static void AddTo<TUIActionControl>(TUIActionControl control)
            where TUIActionControl : Control, IUIActionHandlerProvider
        {
            if (control != null && control.ActionHandler != null)
            {
                control.ContextMenuStrip = new UIMenuStrip<TUIActionControl>(control);
            }
        }

        private class UIMenuStrip<TUIActionControl> : ContextMenuStrip
            where TUIActionControl : Control, IUIActionHandlerProvider
        {
            private readonly ToolStripMenuItem dummyItem = new ToolStripMenuItem(nameof(dummyItem));
            private readonly UIActionHandler ActionHandler;

            public UIMenuStrip(TUIActionControl ownerControl)
            {
                ActionHandler = ownerControl.ActionHandler;

                // Add a dummy item, or else no context menu strip will be opened even if the opening event is fired.
                Items.Add(dummyItem);

                // Dispose when the action provider is disposed.
                this.ownerControl = ownerControl;
                this.ownerControl.Disposed += Parent_Disposed;
            }

            private Control ownerControl;

            protected override void OnParentChanged(EventArgs e)
            {
                base.OnParentChanged(e);
                if (ownerControl != Parent)
                {
                    if (ownerControl != null) ownerControl.Disposed -= Parent_Disposed;
                    if (Parent != null) Parent.Disposed += Parent_Disposed;
                    ownerControl = Parent;
                }
            }

            void Parent_Disposed(object sender, EventArgs e)
            {
                // Dispose automatically to prevent memory leaks when the owner of the context menu strip is disposed.
                Dispose();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    dummyItem.Dispose();
                }

                base.Dispose(disposing);
            }

            protected override void OnOpening(CancelEventArgs e)
            {
                base.OnOpening(e);

                // Dynamically build the context menu here.
                base.Items.Clear();
                if (ownerControl != null && ownerControl.IsHandleCreated)
                {
                    UIMenuBuilder.BuildMenu(ActionHandler, Items);
                }

                // If no items, add the dummy item again, or else no context menu strip will be opened even if the opening event is fired.
                if (Items.Count == 0)
                {
                    Items.Add(dummyItem);
                    e.Cancel = true;
                }
            }
        }
    }

    public struct UIMenuBuilder : IUIMenuTreeVisitor<ToolStripMenuItem>
    {
        /// <summary>
        /// Dynamically adds menu items to a <see cref="ToolStripItemCollection"/>
        /// given the <see cref="UIMenuNode"/> which is defined in a <see cref="UIActionHandler"/>.
        /// </summary>
        /// <param name="actionHandler">
        /// The <see cref="UIActionHandler"/> which performs actions and defines the blueprint <see cref="UIMenuNode"/>.
        /// </param>
        /// <param name="destination">
        /// The <see cref="ToolStripItemCollection"/> in which to generate the menu items.
        /// </param>
        public static void BuildMenu(UIActionHandler actionHandler, ToolStripItemCollection destination)
        {
            new UIMenuBuilder(actionHandler).BuildMenu(actionHandler.RootMenuNode.Nodes, destination);
        }

        UIActionHandler ActionHandler;

        UIMenuBuilder(UIActionHandler actionHandler)
        {
            ActionHandler = actionHandler;
        }

        void BuildMenu(IEnumerable<UIMenuNode> actionList, ToolStripItemCollection destination)
        {
            bool first = true;
            bool firstInGroup = false;

            foreach (var node in actionList)
            {
                // Remember to add a ToolStripSeparator, but prevent multiple separators in a row
                // by only adding the separator right before a new visible menu item.
                if (node.IsFirstInGroup) firstInGroup = true;

                var generatedItem = node.Accept(this);
                if (generatedItem != null)
                {
                    // Add separator between this menu-item and the previous one?
                    if (firstInGroup && !first)
                    {
                        destination.Add(new ToolStripSeparator());
                    }
                    destination.Add(generatedItem);
                    first = false;
                    firstInGroup = false;
                }
            }
        }

        void InitializeMenuItem(UIMenuNode node, LocalizedToolStripMenuItem menuItem)
        {
            // Make sure ampersand characters are shown in menu items, instead of giving rise to a mnemonic.
            if (node.CaptionKey != null)
            {
                menuItem.LocalizedText = new LocalizedString(node.CaptionKey);
                menuItem.LocalizedText.DisplayText.ValueChanged += displayText => menuItem.Text = displayText.Replace("&", "&&");
                menuItem.Disposed += (_, __) => menuItem.LocalizedText.Dispose();
            }
            else
            {
                menuItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            }

            menuItem.ImageScaling = ToolStripItemImageScaling.None;
            menuItem.Image = node.Icon;
        }

        ToolStripMenuItem IUIMenuTreeVisitor<ToolStripMenuItem>.VisitElement(UIMenuNode.Element element)
        {
            if (element.CaptionKey == null && element.Icon == null) return null;

            UIActionState currentActionState = ActionHandler.TryPerformAction(element.Action, false);

            if (!currentActionState.Visible) return null;

            var menuItem = new UIActionToolStripMenuItem(element.Action);
            InitializeMenuItem(element, menuItem);

            menuItem.ShortcutKeyDisplayStringParts = element.Shortcut.DisplayStringParts().Select(x => new LocalizedString(x)).ToList();
            menuItem.ShortcutKeyDisplayStringParts.ForEach(
                x => x.DisplayText.ValueChanged += __ =>
                menuItem.ShortcutKeyDisplayString = string.Join("+", menuItem.ShortcutKeyDisplayStringParts.Select(y => y.DisplayText.Value)));
            menuItem.Disposed += (_, __) => menuItem.ShortcutKeyDisplayStringParts.ForEach(x => x.Dispose());
            menuItem.Update(currentActionState);

            var actionHandler = ActionHandler;
            menuItem.Click += (_, __) =>
            {
                try
                {
                    actionHandler.TryPerformAction(element.Action, true);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            };

            return menuItem;
        }

        ToolStripMenuItem IUIMenuTreeVisitor<ToolStripMenuItem>.VisitContainer(UIMenuNode.Container container)
        {
            if (container.CaptionKey == null && container.Icon == null) return null;

            var menuItem = new LocalizedToolStripMenuItem();
            InitializeMenuItem(container, menuItem);

            BuildMenu(container.Nodes, menuItem.DropDownItems);

            // No empty submenu items.
            if (menuItem.DropDownItems.Count == 0) return null;

            return menuItem;
        }
    }
}