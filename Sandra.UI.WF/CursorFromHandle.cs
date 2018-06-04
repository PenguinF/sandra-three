/*********************************************************************************
 * CursorFromHandle.cs
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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Creates a <see cref="System.Windows.Forms.Cursor"/> from a Windows handle.
    /// Ensures that both the cursor and its unmanaged handle are cleaned up after use.
    /// </summary>
    public sealed class CursorFromHandle : IDisposable
    {
        // Keep a reference to the handle which created the cursor.
        // To not leak GDI objects, it needs to be explicitly released when the cursor is disposed.
        private readonly IntPtr cursorIconHandle;

        /// <summary>
        /// Gets a reference to the generated <see cref="System.Windows.Forms.Cursor"/>.
        /// </summary>
        public Cursor Cursor { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CursorFromHandle"/> from the Windows handle of an icon.
        /// </summary>
        /// <param name="cursorIconHandle">
        /// The <see cref="IntPtr"/> that represents the Windows handle of an icon.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="cursorIconHandle"/> is <see cref="IntPtr.Zero"/>.
        /// </exception>
        internal CursorFromHandle(IntPtr cursorIconHandle)
        {
            this.cursorIconHandle = cursorIconHandle;
            Cursor = new Cursor(cursorIconHandle);
        }

        /// <summary>
        /// Releases all resources used by this <see cref="CursorFromHandle"/>.
        /// </summary>
        public void Dispose()
        {
            Cursor.Dispose();
            WinAPI.DestroyIcon(new HandleRef(null, cursorIconHandle));
            GC.SuppressFinalize(this);
        }

        ~CursorFromHandle()
        {
            // Release cursorIconHandle, which is an unmanaged resource.
            if (cursorIconHandle != IntPtr.Zero)
            {
                WinAPI.DestroyIcon(new HandleRef(null, cursorIconHandle));
            }
        }
    }
}
