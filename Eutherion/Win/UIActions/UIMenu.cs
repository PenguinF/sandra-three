#region License
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

            /// <summary>
            /// Indicates if a modal dialog will be displayed if the action is invoked.
            /// If true, the display text of the menu item is followed by "...".
            /// </summary>
            public readonly bool OpensDialog;

            public Element(UIAction action, ShortcutKeysUIActionInterface shortcutKeysInterface, ContextMenuUIActionInterface contextMenuInterface)
                : base(contextMenuInterface.MenuCaptionKey, contextMenuInterface.MenuIcon)
            {
                Action = action ?? throw new ArgumentNullException(nameof(action));

                if (shortcutKeysInterface != null && shortcutKeysInterface.Shortcuts != null)
                {
                    Shortcut = shortcutKeysInterface.Shortcuts.FirstOrDefault(x => !x.IsEmpty);
                }

                IsFirstInGroup = contextMenuInterface.IsFirstInGroup;
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

        public LocalizedString LocalizedText;
        public List<LocalizedString> ShortcutKeyDisplayStringParts;

        /// <summary>
        /// Sets up this menu item with the caption and icon from the menu node.
        /// </summary>
        /// <param name="node">
        /// The <see cref="UIMenuNode"/> to initialize from.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="node"/> is null.
        /// </exception>
        public void InitializeFrom(UIMenuNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            InitializeFrom(
                node.CaptionKey,
                node.Icon,
                node is UIMenuNode.Element element && element.OpensDialog);
        }

        /// <summary>
        /// Sets up this menu item with the given caption and icon.
        /// </summary>
        /// <param name="captionKey">
        /// The localized text to use for the caption.
        /// </param>
        /// <param name="icon">
        /// The icon to show in the menu item.
        /// </param>
        /// <param name="opensDialog">
        /// Whether or not the display text of the menu item is followed by a "...".
        /// </param>
        public void InitializeFrom(LocalizedStringKey captionKey, Image icon, bool opensDialog)
        {
            if (LocalizedText != null) LocalizedText.Dispose();

            if (captionKey != null)
            {
                LocalizedText = new LocalizedString(captionKey);

                // Put opensDialog in an if-condition so it doesn't need to be captured into the closure.
                if (!opensDialog)
                {
                    // Make sure ampersand characters are shown in menu items, instead of giving rise to a mnemonic.
                    LocalizedText.DisplayText.ValueChanged += displayText => Text = displayText.Replace("&", "&&");
                }
                else
                {
                    // Add OpensDialogIndicatorSuffix.
                    LocalizedText.DisplayText.ValueChanged += displayText => Text = displayText.Replace("&", "&&") + OpensDialogIndicatorSuffix;
                }
            }
            else
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image;
            }

            ImageScaling = ToolStripItemImageScaling.None;
            Image = icon;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (LocalizedText != null) LocalizedText.Dispose();
            }

            base.Dispose(disposing);
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
        /// given the set of <see cref="ContextMenuUIActionInterface"/>s which are defined in a <see cref="UIActionHandler"/>.
        /// </summary>
        /// <param name="actionHandler">
        /// The <see cref="UIActionHandler"/> which performs actions and defines the <see cref="ContextMenuUIActionInterface"/>s.
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
                if (interfaceSet.TryGet(out ContextMenuUIActionInterface contextMenuInterface))
                {
                    var shortcutKeysInterface = interfaceSet.Get<ShortcutKeysUIActionInterface>();
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

            var menuItem = new UIActionToolStripMenuItem(element.Action);
            menuItem.InitializeFrom(element);

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
            menuItem.InitializeFrom(container);

            BuildMenu(container.Nodes, menuItem.DropDownItems);

            // No empty submenu items.
            if (menuItem.DropDownItems.Count == 0) return null;

            return menuItem;
        }
    }
}
