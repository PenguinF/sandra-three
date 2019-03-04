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
using Eutherion.UIActions;
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
        public LocalizedStringKey CaptionKey { get; }

        /// <summary>
        /// Gets the icon to display for this node.
        /// </summary>
        public Image Icon { get; }

        /// <summary>
        /// Gets or sets if this node is the first in a group of nodes.
        /// </summary>
        public bool IsFirstInGroup { get; set; }

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
            public readonly IEnumerable<LocalizedStringKey> Shortcut;

            /// <summary>
            /// Indicates if a modal dialog will be displayed if the action is invoked.
            /// If true, the display text of the menu item is followed by "...".
            /// </summary>
            public readonly bool OpensDialog;

            public Element(UIAction action, IShortcutKeysUIActionInterface shortcutKeysInterface, IContextMenuUIActionInterface contextMenuInterface)
                : base(contextMenuInterface.MenuCaptionKey, contextMenuInterface.MenuIcon)
            {
                Action = action ?? throw new ArgumentNullException(nameof(action));

                if (shortcutKeysInterface != null && shortcutKeysInterface.Shortcuts != null)
                {
                    Shortcut = shortcutKeysInterface.Shortcuts.FirstOrDefault(x => !x.IsEmpty).DisplayStringParts();
                }

                IsFirstInGroup = contextMenuInterface.IsFirstInGroup;
                OpensDialog = contextMenuInterface.OpensDialog;
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
        public static readonly string OpensDialogIndicatorSuffix = "...";

        public LocalizedStringKey CaptionKey { get; }

        public IEnumerable<LocalizedStringKey> ShortcutKeyDisplayStringParts { get; }

        /// <summary>
        /// Indicates if a modal dialog will be displayed if the action is invoked.
        /// If true, the display text of the menu item is followed by "...".
        /// </summary>
        public bool OpensDialog { get; }

        protected LocalizedToolStripMenuItem(LocalizedStringKey captionKey,
                                             Image icon,
                                             IEnumerable<LocalizedStringKey> displayStringParts,
                                             bool opensDialog)
        {
            ImageScaling = ToolStripItemImageScaling.None;

            CaptionKey = captionKey;
            Image = icon;
            ShortcutKeyDisplayStringParts = displayStringParts;
            OpensDialog = opensDialog;

            Update();
        }

        /// <summary>
        /// Updates this menu item's properties after a definition change.
        /// </summary>
        public void Update()
        {
            string displayText = CaptionKey == null ? null : Localizer.Current.Localize(CaptionKey);

            if (!string.IsNullOrWhiteSpace(displayText))
            {
                if (!OpensDialog)
                {
                    // Make sure ampersand characters are shown in menu items, instead of giving rise to a mnemonic.
                    Text = displayText.Replace("&", "&&");
                }
                else
                {
                    // Add OpensDialogIndicatorSuffix.
                    Text = displayText.Replace("&", "&&") + OpensDialogIndicatorSuffix;
                }
            }
            else
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image;
                Text = string.Empty;
            }

            if (ShortcutKeyDisplayStringParts != null)
            {
                ShortcutKeyDisplayString = string.Join("+", ShortcutKeyDisplayStringParts.Select(x => Localizer.Current.Localize(x)));
            }
            else
            {
                ShortcutKeyDisplayString = string.Empty;
            }
        }

        /// <summary>
        /// Creates a menu item with the caption and icon from the menu node.
        /// </summary>
        /// <param name="container">
        /// The <see cref="UIMenuNode.Container"/> to initialize from.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="container"/> is null.
        /// </exception>
        public static LocalizedToolStripMenuItem CreateFrom(UIMenuNode.Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return new LocalizedToolStripMenuItem(container.CaptionKey, container.Icon, null, false);
        }
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

        private UIActionToolStripMenuItem(UIMenuNode.Element element)
            : base(element.CaptionKey,
                   element.Icon,
                   element.Shortcut,
                   element.OpensDialog)
        {
            Action = element.Action;
        }

        /// <summary>
        /// Creates a new menu item with the caption and icon from the menu node.
        /// </summary>
        /// <param name="element">
        /// The <see cref="UIMenuNode.Element"/> to initialize from.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is null.
        /// </exception>
        public static UIActionToolStripMenuItem CreateFrom(UIMenuNode.Element element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            return new UIActionToolStripMenuItem(element);
        }

        public void Update(UIActionState currentActionState)
        {
            Enabled = currentActionState.Enabled;
            Checked = currentActionState.Checked;
        }
    }

    public static class UIMenu
    {
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

        /// <summary>
        /// Updates all menu items in a collection recursively.
        /// </summary>
        /// <param name="toolStripItems">
        /// The items to update.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="toolStripItems"/> is null.
        /// </exception>
        public static void UpdateMenu(ToolStripItemCollection toolStripItems)
        {
            if (toolStripItems == null) throw new ArgumentNullException(nameof(toolStripItems));

            foreach (ToolStripItem toolStripItem in toolStripItems)
            {
                if (toolStripItem is ToolStripDropDownItem dropDownItem)
                {
                    if (dropDownItem is LocalizedToolStripMenuItem localizedItem) localizedItem.Update();
                    UpdateMenu(dropDownItem.DropDownItems);
                }
            }
        }
    }

    public struct UIMenuBuilder : IUIMenuTreeVisitor<ToolStripMenuItem>
    {
        /// <summary>
        /// Dynamically adds menu items to a <see cref="ToolStripItemCollection"/>
        /// given the set of <see cref="IContextMenuUIActionInterface"/>s which are defined in a <see cref="UIActionHandler"/>.
        /// </summary>
        /// <param name="actionHandler">
        /// The <see cref="UIActionHandler"/> which performs actions and defines the <see cref="IContextMenuUIActionInterface"/>s.
        /// </param>
        /// <param name="destination">
        /// The <see cref="ToolStripItemCollection"/> in which to generate the menu items.
        /// </param>
        public static void BuildMenu(UIActionHandler actionHandler, ToolStripItemCollection destination)
        {
            var rootMenuNodes = new List<UIMenuNode>();

            // Extract all ContextMenuUIActionInterfaces from the handler.
            foreach (var (interfaceSet, action) in actionHandler.InterfaceSets)
            {
                if (interfaceSet.TryGet(out IContextMenuUIActionInterface contextMenuInterface))
                {
                    var shortcutKeysInterface = interfaceSet.Get<IShortcutKeysUIActionInterface>();
                    rootMenuNodes.Add(new UIMenuNode.Element(action, shortcutKeysInterface, contextMenuInterface));
                }
            }

            BuildMenu(actionHandler, rootMenuNodes, destination);
        }

        /// <summary>
        /// Dynamically adds menu items to a <see cref="ToolStripItemCollection"/>.
        /// </summary>
        /// <param name="actionHandler">
        /// The <see cref="UIActionHandler"/> which performs actions and defines the blueprint <see cref="UIMenuNode"/>.
        /// </param>
        /// <param name="rootMenuNodes">
        /// Collection of the menu items to generate.
        /// </param>
        /// <param name="destination">
        /// The <see cref="ToolStripItemCollection"/> in which to generate the menu items.
        /// </param>
        public static void BuildMenu(UIActionHandler actionHandler, IEnumerable<UIMenuNode> rootMenuNodes, ToolStripItemCollection destination)
            => new UIMenuBuilder(actionHandler).BuildMenu(rootMenuNodes, destination);

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

        ToolStripMenuItem IUIMenuTreeVisitor<ToolStripMenuItem>.VisitElement(UIMenuNode.Element element)
        {
            if (element.CaptionKey == null && element.Icon == null) return null;

            UIActionState currentActionState = ActionHandler.TryPerformAction(element.Action, false);

            if (!currentActionState.Visible) return null;

            var menuItem = UIActionToolStripMenuItem.CreateFrom(element);
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

            var menuItem = LocalizedToolStripMenuItem.CreateFrom(container);
            BuildMenu(container.Nodes, menuItem.DropDownItems);

            // No empty submenu items.
            if (menuItem.DropDownItems.Count == 0) return null;

            return menuItem;
        }
    }
}
