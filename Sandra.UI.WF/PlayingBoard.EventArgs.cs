/*********************************************************************************
 * PlayingBoard.EventArgs.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
using System;
using System.Diagnostics;
using System.Drawing;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MouseEnterSquare"/>
    /// or <see cref="PlayingBoard.MouseLeaveSquare"/> event.
    /// </summary>
    [DebuggerDisplay("(x = {Location.X}, y = {Location.Y})")]
    public class SquareEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the location of the square.
        /// </summary>
        public SquareLocation Location { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SquareEventArgs"/> class.
        /// </summary>
        /// <param name="location">
        /// The location of the square.
        /// </param>
        public SquareEventArgs(SquareLocation location)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));
            Location = location;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveStart"/> event.
    /// </summary>
    public class CancellableMoveEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the location of the square where moving started.
        /// </summary>
        public SquareLocation Start { get; }

        /// <summary>
        /// Gets the position of the mouse relative to the top left corner of the control when dragging started.
        /// </summary>
        public Point MouseStartPosition { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableMoveEventArgs"/> class.
        /// </summary>
        /// <param name="start">
        /// The location of the square where moving started.
        /// </param>
        /// <param name="mouseStartPosition">
        /// The position of the mouse relative to the top left corner of the control when dragging started.
        /// </param>
        public CancellableMoveEventArgs(SquareLocation start, Point mouseStartPosition)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            Start = start;
            MouseStartPosition = mouseStartPosition;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveCommit"/> event.
    /// </summary>
    public class MoveCommitEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the location of the square where the mouse cursor currently is,
        /// or null if the mouse cursor is elsewhere.
        /// </summary>
        public SquareLocation Target { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveCommitEventArgs"/> class.
        /// </summary>
        /// <param name="target">
        /// The location of the square where the mouse cursor currently is,
        /// or null if the mouse cursor is elsewhere.
        /// </param>
        public MoveCommitEventArgs(SquareLocation target)
        {
            Target = target;
        }
    }
}
