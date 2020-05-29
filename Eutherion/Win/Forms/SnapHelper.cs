#region License
/*********************************************************************************
 * SnapHelper.cs
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

using System.ComponentModel;

namespace Eutherion.Win.Forms
{
    /// <summary>
    /// Modifies a Form's behavior, such that it snaps to other forms.
    /// </summary>
    public abstract class SnapHelper
    {
        /// <summary>
        /// Gets the default value for the <see cref="MaxSnapDistance"/> property.
        /// </summary>
        public const int DefaultMaxSnapDistance = 4;

        /// <summary>
        /// Gets or sets the maximum distance between form edges within which they will be sensitive to snapping together. The default value is <see cref="DefaultMaxSnapDistance"/> (4).
        /// </summary>
        [DefaultValue(DefaultMaxSnapDistance)]
        public int MaxSnapDistance { get; set; } = DefaultMaxSnapDistance;

        /// <summary>
        /// Gets the default value for the <see cref="InsensitiveBorderEndLength"/> property.
        /// </summary>
        public const int DefaultInsensitiveBorderEndLength = 16;

        /// <summary>
        /// Gets or sets the length of the ends of the borders of the form that are insensitive to snapping. The default value is <see cref="DefaultInsensitiveBorderEndLength"/> (16).
        /// </summary>
        [DefaultValue(DefaultInsensitiveBorderEndLength)]
        public int InsensitiveBorderEndLength { get; set; } = DefaultInsensitiveBorderEndLength;

        /// <summary>
        /// Gets or sets if the form will snap to other forms while it's being moved. The default value is true.
        /// </summary>
        public bool SnapWhileMoving { get; set; } = true;

        /// <summary>
        /// Gets or sets if the form will snap to other forms while it's being resized. The default value is true.
        /// </summary>
        public bool SnapWhileResizing { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="ConstrainedMoveResizeForm"/> with the snapping behavior.
        /// </summary>
        public ConstrainedMoveResizeForm Form { get; }

        protected SnapHelper(ConstrainedMoveResizeForm form) => Form = form;
    }
}
