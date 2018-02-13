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
using System.Windows.Forms;

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
            Location = location;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.SquareMouseDown"/>
    /// or <see cref="PlayingBoard.SquareMouseUp"/> event.
    /// </summary>
    public class SquareMouseEventArgs : SquareEventArgs
    {
        /// <summary>
        /// Gets which mouse button was pressed.
        /// </summary>
        public MouseButtons Button { get; }

        /// <summary>
        /// Gets the position of the mouse in pixels relative to the top left corner of the control.
        /// </summary>
        public Point MouseLocation { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SquareMouseEventArgs"/> class.
        /// </summary>
        /// <param name="location">
        /// The location of the square.
        /// </param>
        /// <param name="button">
        /// Which mouse button was pressed.
        /// </param>
        /// <param name="mouseLocation">
        /// The position of the mouse in pixels relative to the top left corner of the control.
        /// </param>
        public SquareMouseEventArgs(SquareLocation location, MouseButtons button, Point mouseLocation) : base(location)
        {
            Button = button;
            MouseLocation = mouseLocation;
        }
    }
}
