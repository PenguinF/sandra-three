#region License
/*********************************************************************************
 * WinAPI.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Encapsulates the RECT structure which is used by the Windows API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("({Left},{Top})-({Right},{Bottom})")]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// Specifies the mode in which a form is being resized.
    /// </summary>
    public enum ResizeMode
    {
        /// <summary>
        /// The left border of the form is being dragged.
        /// </summary>
        /// <remarks>
        /// This enumeration value encapsulates the WMSZ_LEFT constant.
        /// </remarks>
        Left = 1,

        /// <summary>
        /// The right border of the form is being dragged.
        /// </summary>
        /// <remarks>
        /// This enumeration value encapsulates the WMSZ_RIGHT constant.
        /// </remarks>
        Right = 2,

        /// <summary>
        /// The top border of the form is being dragged.
        /// </summary>
        /// <remarks>
        /// This enumeration value encapsulates the WMSZ_TOP constant.
        /// </remarks>
        Top = 3,

        /// <summary>
        /// The top and left borders of the form are being dragged.
        /// </summary>
        /// <remarks>
        /// This enumeration value is equal to WMSZ_TOP + WMSZ_LEFT.
        /// </remarks>
        TopLeft = 4,

        /// <summary>
        /// The top and right borders of the form are being dragged.
        /// </summary>
        /// <remarks>
        /// This enumeration value is equal to WMSZ_TOP + WMSZ_RIGHT.
        /// </remarks>
        TopRight = 5,

        /// <summary>
        /// The bottom border of the form is being dragged.
        /// </summary>
        /// <remarks>
        /// This enumeration value encapsulates the WMSZ_BOTTOM constant.
        /// </remarks>
        Bottom = 6,

        /// <summary>
        /// The bottom and left borders of the form are being dragged.
        /// </summary>
        /// <remarks>
        /// This enumeration value is equal to WMSZ_BOTTOM + WMSZ_LEFT.
        /// </remarks>
        BottomLeft = 7,

        /// <summary>
        /// The bottom and right borders of the form are being dragged.
        /// </summary>
        /// <remarks>
        /// This enumeration value is equal to WMSZ_BOTTOM + WMSZ_RIGHT.
        /// </remarks>
        BottomRight = 8,
    }

    /// <summary>
    /// Encapsulates the ICONINFO structure which is used by the Windows API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ICONINFO
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    /// <summary>
    /// Contains P/Invoke definitions for the Windows API.
    /// </summary>
    internal static class WinAPI
    {
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

        [DllImport(User32, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ShowCaret(HandleRef hWnd);
    }
}
