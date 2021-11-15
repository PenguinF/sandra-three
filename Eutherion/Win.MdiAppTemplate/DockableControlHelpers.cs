#region License
/*********************************************************************************
 * DockableControlHelpers.cs
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

using System;
using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Contains helper extension methods for features shared by <see cref="IDockableControl"/> instances
    /// regardless of whether they are docked within a <see cref="MenuCaptionBarForm"/> or a <see cref="MdiTabControl"/>.
    /// </summary>
    public static class DockableControlHelpers
    {
        /// <summary>
        /// Gets any of three options for a <typeparamref name="TDockableControl"/> host:
        /// docked in a <see cref="MenuCaptionBarForm"/>, docked in a <see cref="MdiTabControl"/>, or not docked anywhere at all.
        /// </summary>
        public static Union<MenuCaptionBarForm<TDockableControl>, MdiTabControl, _void> GetDockHost<TDockableControl>(this TDockableControl dockedControl)
            where TDockableControl : Control, IDockableControl
        {
            if (dockedControl.Parent is MenuCaptionBarForm<TDockableControl> menuCaptionBarForm)
            {
                return menuCaptionBarForm;
            }
            else if (dockedControl.Parent is MdiTabControl mdiTabControl)
            {
                return mdiTabControl;
            }
            else
            {
                return _void._;
            }
        }

        /// <summary>
        /// Ensures that a docked control is activated. If it is not docked, this method has no effect.
        /// </summary>
        public static void EnsureActivated<TDockableControl>(this TDockableControl dockedControl)
            where TDockableControl : Control, IDockableControl
        {
            dockedControl.GetDockHost().Match(
                whenOption1: menuCaptionBarForm =>
                {
                    // Activate the Form, then also the control.
                    menuCaptionBarForm.EnsureActivated();
                    menuCaptionBarForm.ActiveControl = dockedControl;
                },
                whenOption2: mdiTabControl =>
                {
                    mdiTabControl.EnsureActivated();
                    mdiTabControl.Activate(dockedControl);
                });
        }
    }
}
