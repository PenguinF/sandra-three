#region License
/*********************************************************************************
 * NativeMethods.cs
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

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Eutherion.Win.Native
{
    /// <summary>
    /// Contains P/Invoke definitions for the Windows API.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class NativeMethods
    {
        const string DwmApi = "dwmapi.dll";

        [DllImport(DwmApi, PreserveSig = false)]
        public static extern void DwmIsCompositionEnabled(out bool enabled);

        const string Gdi32 = "gdi32.dll";

        [DllImport(Gdi32, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteObject(HandleRef hObject);

        const string User32 = "user32.dll";

        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr CreateIconIndirect([In] ref ICONINFO iconInfo);

        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern bool DestroyIcon(HandleRef hIcon);

        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetFocus();

        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GetIconInfo(HandleRef hIcon, ref ICONINFO info);

        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool HideCaret(HandleRef hWnd);

        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ShowCaret(HandleRef hWnd);

        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ShowWindow(HandleRef hWnd, int nCmdShow);
    }
}
