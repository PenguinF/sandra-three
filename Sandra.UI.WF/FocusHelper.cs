/*********************************************************************************
 * FocusHelper.cs
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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public sealed class FocusHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetFocus();

        /// <summary>
        /// Returns the .NET control that has the keyboard focus in this application.
        /// </summary>
        /// <returns>
        /// The .NET control that has the keyboard focus, if this application is active,
        /// or a null reference if this application is not active,
        /// or the focused control is not a .NET control.
        /// </returns>
        public static Control GetFocusedControl()
        {
            IntPtr focusedHandle = GetFocus();

            int lastError = Marshal.GetLastWin32Error();
            if (lastError != 0)
            {
                throw new Win32Exception(lastError);
            }

            if (focusedHandle != IntPtr.Zero)
            {
                // If the focused control is not a .NET control, then this will return null.
                return Control.FromHandle(focusedHandle);
            }
            return null;
        }
    }
}
