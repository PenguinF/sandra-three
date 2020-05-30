﻿#region License
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
    }
}
