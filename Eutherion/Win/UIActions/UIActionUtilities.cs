#region License
/*********************************************************************************
 * UIActionUtilities.cs
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

using Eutherion.UIActions;
using Eutherion.Win.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.UIActions
{
    /// <summary>
    /// Contains utility methods for handling <see cref="UIAction"/>s.
    /// </summary>
    public static class UIActionUtilities
    {
        /// <summary>
        /// Helper function which enumerates all <see cref="UIActionHandler"/> instances
        /// which are available on any parent of a <see cref="Control"/>.
        /// </summary>
        /// <param name="startControl">
        /// <see cref="Control"/> where to start searching.
        /// </param>
        public static IEnumerable<UIActionHandler> EnumerateUIActionHandlers(Control startControl)
        {
            Control control = startControl;

            while (control != null)
            {
                if (control is IUIActionHandlerProvider provider && provider.ActionHandler != null)
                {
                    yield return provider.ActionHandler;
                }

                control = control.Parent;
            }
        }

        /// <summary>
        /// Searches for an <see cref="UIActionHandler"/> to convert a user command key to a <see cref="UIAction"/>
        /// and then perform the action.
        /// </summary>
        /// <param name="shortcut">
        /// The shortcut key pressed by the user.
        /// </param>
        /// <param name="bottomLevelControl">
        /// The <see cref="Control"/> from which to initiate searching for a handler for the shortcut key.
        /// Generally this is the control which has the keyboard focus.
        /// </param>
        /// <returns>
        /// Whether or not a <see cref="UIActionHandler"/> was found which processed the key successfully.
        /// </returns>
        public static bool TryExecute(Keys shortcut, Control bottomLevelControl)
        {
            // Try to find an action with given shortcut.
            return (from actionHandler in EnumerateUIActionHandlers(bottomLevelControl)
                    from interfaceActionPair in actionHandler.InterfaceSets
                    let shortcuts = interfaceActionPair.Item1.Get<IShortcutKeysUIActionInterface>()?.Shortcuts
                    where shortcuts != null
                    from registeredShortcut in shortcuts
                        // If the shortcut matches, then try to perform the action.
                        // If the handler does not return UIActionVisibility.Parent, then swallow the key by returning true.
                    where KeyUtilities.IsMatch(registeredShortcut, shortcut)
                    select actionHandler.TryPerformAction(interfaceActionPair.Item2, true).UIActionVisibility)
                    .Any(x => x != UIActionVisibility.Parent);
        }
    }
}
