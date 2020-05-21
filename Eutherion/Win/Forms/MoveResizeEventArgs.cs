#region License
/*********************************************************************************
 * MoveResizeEventArgs.cs
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

using Eutherion.Win.Native;
using System;

namespace Eutherion.Win.Forms
{
    /// <summary>
    /// Contains data for the <see cref="ConstrainedMoveResizeForm.Moving"/> event.
    /// </summary>
    public class MoveResizeEventArgs : EventArgs
    {
        /// <summary>
        /// Contains the bounding rectangle of the form, relative to the top left corner of the screen.
        /// </summary>
        public RECT MoveResizeRect;

        /// <summary>
        /// Initializes a new instance of <see cref="MoveResizeEventArgs"/>.
        /// </summary>
        /// <param name="moveResizeRect">
        /// The bounding rectangle of the form, relative to the top left corner of the screen.
        /// </param>
        public MoveResizeEventArgs(RECT moveResizeRect) => MoveResizeRect = moveResizeRect;
    }

    /// <summary>
    /// Contains data for the <see cref="ConstrainedMoveResizeForm.Resizing"/> event.
    /// </summary>
    public class ResizeEventArgs : MoveResizeEventArgs
    {
        /// <summary>
        /// Gets or sets the mode in which the form is being resized.
        /// </summary>
        public ResizeMode ResizeMode { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ResizeEventArgs"/>.
        /// </summary>
        /// <param name="moveResizeRect">
        /// The bounding rectangle of the form, relative to the top left corner of the screen.
        /// </param>
        /// <param name="resizeMode">
        /// The mode in which the form is being resized.
        /// </param>
        public ResizeEventArgs(RECT moveResizeRect, ResizeMode resizeMode) : base(moveResizeRect) => ResizeMode = resizeMode;
    }
}
