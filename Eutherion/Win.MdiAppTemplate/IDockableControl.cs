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

using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Represents a <see cref="Control"/> that can be hosted both as a top level control
    /// in a <see cref="MenuCaptionBarForm"/>, and as a tab page in a <see cref="MdiTabControl"/>.
    /// </summary>
    public interface IDockableControl
    {
    }
}
