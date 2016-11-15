/*********************************************************************************
 * PlayingBoard.EventArgs.cs
 * 
 * Copyright (c) 2004-2016 Henk Nicolai
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
    [DebuggerDisplay("(x = {X}, y = {Y})")]
    public class SquareEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the X-coordinate of the square.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y-coordinate of the square.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SquareEventArgs"/> class.
        /// </summary>
        /// <param name="x">
        /// The X-coordinate of the square.
        /// </param>
        /// <param name="y">
        /// The Y-coordinate of the square.
        /// </param>
        public SquareEventArgs(int x, int y)
        {
            X = x; Y = y;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveStart"/> event.
    /// </summary>
    public class CancellableSquareEventArgs : SquareEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableSquareEventArgs"/> class.
        /// </summary>
        /// <param name="x">
        /// The X-coordinate of the square.
        /// </param>
        /// <param name="y">
        /// The Y-coordinate of the square.
        /// </param>
        public CancellableSquareEventArgs(int x, int y) : base(x, y)
        {
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveCancel"/> event.
    /// </summary>
    [DebuggerDisplay("From (x = {StartX}, y = {StartY})")]
    public class MoveEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the X-coordinate of the square where moving started.
        /// </summary>
        public int StartX { get; }

        /// <summary>
        /// Gets the Y-coordinate of the square where moving started.
        /// </summary>
        public int StartY { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveEventArgs"/> class.
        /// </summary>
        /// <param name="startX">
        /// The X-coordinate of the square where moving started.
        /// </param>
        /// <param name="startY">
        /// The Y-coordinate of the square where moving started.
        /// </param>
        public MoveEventArgs(int startX, int startY)
        {
            StartX = startX; StartY = startY;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveCommit"/> event.
    /// </summary>
    [DebuggerDisplay("From (x = {StartX}, y = {StartY}) to (x = {TargetX}, y = {TargetY})")]
    public class MoveCommitEventArgs : MoveEventArgs
    {
        /// <summary>
        /// Gets the X-coordinate of the square where the mouse cursor currently is.
        /// </summary>
        public int TargetX { get; }

        /// <summary>
        /// Gets the Y-coordinate of the square where the mouse cursor currently is.
        /// </summary>
        public int TargetY { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveCommitEventArgs"/> class.
        /// </summary>
        /// <param name="startX">
        /// The X-coordinate of the square where moving started.
        /// </param>
        /// <param name="startY">
        /// The Y-coordinate of the square where moving started.
        /// </param>
        /// <param name="targetX">
        /// The X-coordinate of the square where the mouse cursor currently is.
        /// </param>
        /// <param name="targetY">
        /// The Y-coordinate of the square where the mouse cursor currently is.
        /// </param>
        public MoveCommitEventArgs(int startX, int startY, int targetX, int targetY) : base(startX, startY)
        {
            TargetX = targetX; TargetY = targetY;
        }
    }
}
