﻿#region License
/*********************************************************************************
 * IDockableControl.cs
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

using Eutherion.UIActions;
using Eutherion.Win.UIActions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Represents a <see cref="Control"/> that can be hosted both as a top level control
    /// in a <see cref="MenuCaptionBarForm"/>, and as a tab page in a <see cref="MdiTabControl"/>.
    /// </summary>
    public interface IDockableControl
    {
        /// <summary>
        /// Gets a set of properties which are relevant when docked.
        /// These are owned by the client control, and are read-only for the host control.
        /// </summary>
        DockProperties DockProperties { get; }

        /// <summary>
        /// Occurs when the dock properties have been updated by the client control.
        /// </summary>
        event Action DockPropertiesChanged;

        /// <summary>
        /// Called when the control is about to be closed.
        /// </summary>
        /// <param name="closeReason">
        /// The reason why the control is being closed.
        /// </param>
        /// <param name="cancel">
        /// Whether or not to cancel the closing event.
        /// </param>
        void CanClose(CloseReason closeReason, ref bool cancel);
    }

    /// <summary>
    /// Exposes a set of properties which are relevant when docked.
    /// </summary>
    public class DockProperties
    {
        /// <summary>
        /// Gets or sets the height of the caption area.
        /// </summary>
        public int CaptionHeight { get; set; }

        /// <summary>
        /// Gets or sets the text to display in a caption bar.
        /// </summary>
        public string CaptionText { get; set; }

        /// <summary>
        /// Gets or sets the icon to display in a caption bar.
        /// </summary>
        public Icon Icon { get; set; }

        /// <summary>
        /// Gets or sets if the control contains any unsaved modifications.
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        /// Gets or sets an enumeration of main menu items to build.
        /// </summary>
        public IEnumerable<MainMenuDropDownItem> MainMenuItems { get; set; }

        /// <summary>
        /// Gets or sets the text to display in a tab page header.
        /// If this property is not set, tab page headers will display <see cref="CaptionText"/> instead.
        /// </summary>
        public string TabPageTextOverride { get; set; }

        /// <summary>
        /// Gets the text to display in a tab page header.
        /// </summary>
        public string TabPageText => string.IsNullOrWhiteSpace(TabPageTextOverride) ? CaptionText : TabPageTextOverride;

        /// <summary>
        /// Gets or sets the background color to display in a tab header if the control is docked on an active tab page.
        /// </summary>
        public Color TabBackColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color to display in a tab header if the control is docked on an active tab page.
        /// </summary>
        public Color TabForeColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color to display for the glyph on the active tab page.
        /// If this color is empty, <see cref="TabForeColor"/> is used.
        /// </summary>
        public Color GlyphForeColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color to display for the glyph on the active tab page if the mouse is positioned over it.
        /// If this color is empty, a lighter version of <see cref="GlyphForeColor"/> is used.
        /// </summary>
        public Color GlyphHoverColor { get; set; }
    }

    /// <summary>
    /// Contains a high level definition of a main menu item.
    /// </summary>
    public struct MainMenuDropDownItem
    {
        /// <summary>
        /// Represents the main menu container item. It is assumed to be empty.
        /// </summary>
        public UIMenuNode.Container Container;

        /// <summary>
        /// Enumerates the collection of drop down item definitions and action bindings to add to the container.
        /// </summary>
        public IEnumerable<Union<UIAction, MainMenuDropDownItem>> DropDownItems;
    }
}
