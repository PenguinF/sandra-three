#region License
/*********************************************************************************
 * WindowCommand.cs
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

namespace Eutherion.Win.Native
{
    /// <summary>
    /// Enumerates available items in the standard window menu.
    /// </summary>
    public enum WindowCommand : uint
    {
        /// <summary>
        /// Represents the 'Size' window command.
        /// </summary>
        Size = SC.SIZE,

        /// <summary>
        /// Represents the 'Move' window command.
        /// </summary>
        Move = SC.MOVE,

        /// <summary>
        /// Represents the 'Minimize' window command.
        /// </summary>
        Minimize = SC.MINIMIZE,

        /// <summary>
        /// Represents the 'Maximize' window command.
        /// </summary>
        Maximize = SC.MAXIMIZE,

        /// <summary>
        /// Represents the 'Close' window command.
        /// </summary>
        Close = SC.CLOSE,

        /// <summary>
        /// Represents the 'Restore' window command.
        /// </summary>
        Restore = SC.RESTORE,
    }
}
