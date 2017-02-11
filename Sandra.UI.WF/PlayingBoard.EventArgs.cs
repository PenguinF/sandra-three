/*********************************************************************************
 * PlayingBoard.EventArgs.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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
    /// Provides data for the <see cref="PlayingBoard.MoveCancel"/> event.
    /// </summary>
    [DebuggerDisplay("From (x = {Start.X}, y = {Start.Y})")]
    public class MoveEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the location of the square where moving started.
        /// </summary>
        public SquareLocation Start { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveEventArgs"/> class.
        /// </summary>
        /// <param name="start">
        /// The location of the square where moving started.
        /// </param>
        public MoveEventArgs(SquareLocation start)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            Start = start;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveStart"/> event.
    /// </summary>
    public class CancellableMoveEventArgs : MoveEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableMoveEventArgs"/> class.
        /// </summary>
        /// <param name="start">
        /// The location of the square where moving started.
        /// </param>
        public CancellableMoveEventArgs(SquareLocation start) : base(start)
        {
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveCommit"/> event.
    /// </summary>
    [DebuggerDisplay("From (x = {StartX}, y = {StartY}) to (x = {TargetX}, y = {TargetY})")]
    public class MoveCommitEventArgs : MoveEventArgs
    {
        /// <summary>
        /// Gets the location of the square where the mouse cursor currently is.
        /// </summary>
        public SquareLocation Target { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveCommitEventArgs"/> class.
        /// </summary>
        /// <param name="start">
        /// The location of the square where moving started.
        /// </param>
        /// <param name="target">
        /// The location of the square where the mouse cursor currently is.
        /// </param>
        public MoveCommitEventArgs(SquareLocation start, SquareLocation target) : base(start)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            Target = target;
        }
    }
}
