﻿#region License
/*********************************************************************************
 * UIAutoHideMainMenu.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Eutherion.Win.UIActions
{
    /// <summary>
    /// When attached to a <see cref="Form"/>, displays a predefined standard main menu when the ALT key is pressed.
    /// Exposes a number of globally available <see cref="UIAction"/>s.
    /// </summary>
    public class UIAutoHideMainMenu : IWeakEventTarget, IDisposable
    {
        private readonly Form Owner;

        private readonly List<UIAutoHideMainMenuItem> MenuItems = new List<UIAutoHideMainMenuItem>();

        private readonly Action<MenuStrip> OnMenuKey;

        public UIAutoHideMainMenu(Form owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));

            // Internal method called when ALT key is pressed - if the MenuStrip was visible in the first place.
            var methodInfo = typeof(MenuStrip).GetMethod(nameof(OnMenuKey), BindingFlags.NonPublic | BindingFlags.Instance);

            // Assume it can be found, but don't crash if not.
            if (methodInfo != null) OnMenuKey = x => methodInfo.Invoke(x, null);

            // Track focus to detect when main menu items must be updated.
            FocusHelper.Instance.FocusChanged += FocusHelper_FocusChanged;
        }

        private void FocusHelper_FocusChanged(FocusHelper sender, FocusChangedEventArgs e)
        {
            HideMainMenu();
        }

        public UIAutoHideMainMenuItem AddMenuItem(UIMenuNode.Container containerNode)
        {
            if (containerNode == null) throw new ArgumentNullException(nameof(containerNode));

            UIAutoHideMainMenuItem menuItem = new UIAutoHideMainMenuItem(this, containerNode);
            MenuItems.Add(menuItem);
            return menuItem;
        }

        private MenuStrip BuildMainMenu()
        {
            var mainMenuStrip = new MenuStrip();
            mainMenuStrip.Items.AddRange(MenuItems.Select(x => x.InitializedMenuItem()).ToArray());
            return mainMenuStrip;
        }

        internal void HideMainMenu()
        {
            // Hide the menu when:
            // 1) The ALT key is pressed again.
            // 2) A menu item is clicked/activated.
            // 3) The focus changed.

            if (Owner.MainMenuStrip != null)
            {
                Owner.MainMenuStrip.Dispose();
                Owner.Controls.Remove(Owner.MainMenuStrip);
                Owner.MainMenuStrip = null;
            }
        }

        public void ToggleMainMenu()
        {
            if (Owner.MainMenuStrip == null)
            {
                Owner.MainMenuStrip = BuildMainMenu();
                Owner.Controls.Add(Owner.MainMenuStrip);
                OnMenuKey?.Invoke(Owner.MainMenuStrip);
            }
            else
            {
                HideMainMenu();
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
            FocusHelper.Instance.FocusChanged -= FocusHelper_FocusChanged;
        }
    }

    public class UIAutoHideMainMenuItem
    {
        private readonly UIAutoHideMainMenu Owner;
        private readonly UIMenuNode.Container ContainerNode;
        private readonly UIActionHandler DropDownItemsActionHandler;
        private LocalizedToolStripMenuItem MenuItem;

        internal UIAutoHideMainMenuItem(UIAutoHideMainMenu owner, UIMenuNode.Container containerNode)
        {
            Owner = owner;
            ContainerNode = containerNode;
            DropDownItemsActionHandler = new UIActionHandler();
        }

        public void BindAction(UIAction action, bool alwaysVisible)
        {
            if (action.DefaultInterfaces.TryGet(out IContextMenuUIActionInterface _))
            {
                DropDownItemsActionHandler.BindAction(new UIActionBinding(action, perform =>
                {
                    try
                    {
                        if (perform) Owner.HideMainMenu();

                        // Try to find a UIActionHandler that is willing to validate/perform the given action.
                        foreach (var actionHandler in UIActionUtilities.EnumerateUIActionHandlers(FocusHelper.GetFocusedControl()))
                        {
                            UIActionState currentActionState = actionHandler.TryPerformAction(action.Key, perform);
                            if (currentActionState.UIActionVisibility != UIActionVisibility.Undetermined)
                            {
                                return currentActionState.UIActionVisibility == UIActionVisibility.Hidden && alwaysVisible
                                    ? UIActionVisibility.Disabled
                                    : currentActionState;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }

                    // No handler in the chain that processes the UIAction actively, so set to hidden/disabled.
                    return alwaysVisible ? UIActionVisibility.Disabled : UIActionVisibility.Hidden;
                }));
            }
        }

        public void BindAction(UIAction action)
            => BindAction(action, alwaysVisible: true);

        public void BindActions(params UIAction[] actions)
            => actions.ForEach(BindAction);

        private bool UpdateMenu()
        {
            bool atLeastOneItemEnabled = false;

            foreach (var menuItem in MenuItem.DropDownItems.OfType<UIActionToolStripMenuItem>())
            {
                var state = DropDownItemsActionHandler.TryPerformAction(menuItem.ActionKey, false);
                menuItem.Update(state);
                atLeastOneItemEnabled |= state.Enabled;
            }

            return atLeastOneItemEnabled;
        }

        internal LocalizedToolStripMenuItem InitializedMenuItem()
        {
            if (MenuItem == null)
            {
                MenuItem = LocalizedToolStripMenuItem.CreateFrom(ContainerNode);
                UIMenuBuilder.BuildMenu(DropDownItemsActionHandler, MenuItem.DropDownItems);
                MenuItem.Visible = UpdateMenu();
                MenuItem.DropDownOpening += (_, __) => UpdateMenu();
                MenuItem.Disposed += (_, __) => MenuItem = null;
            }

            return MenuItem;
        }
    }
}
