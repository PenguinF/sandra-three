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
        public DockProperties DockProperties { get; } = new DockProperties();

        public event Action DockPropertiesChanged;

        public void OnClosing(CloseReason closeReason, ref bool cancel)
        {
        }
    }
}
