#region License
/*********************************************************************************
 * NonSelectableButton.cs
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

using System.Windows.Forms;

namespace Eutherion.Win.Controls
{
    /// <summary>
    /// Standard Windows <see cref="Button"/> which cannot receive keyboard focus. 
    /// </summary>
    public class NonSelectableButton : Button
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NonSelectableButton"/>.
        /// </summary>
        public NonSelectableButton() => SetStyle(ControlStyles.Selectable, false);
    }
}
