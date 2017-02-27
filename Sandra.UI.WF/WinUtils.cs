﻿/*********************************************************************************
 * WinUtils.cs
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
using System.Runtime.InteropServices;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Encapsulates the RECT structure which is used by the Windows API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

#if DEBUG
        /// <summary>
        /// For debugging purposes.
        /// </summary>
        public override string ToString()
        {
            return string.Format("({0},{2})-({1},{3})", Left, Right, Top, Bottom);
        }
#endif
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
    public struct ICONINFO
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
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr CreateIconIndirect([In] ref ICONINFO iconInfo);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteObject(HandleRef hObject);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern bool DestroyIcon(HandleRef hIcon);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GetIconInfo(HandleRef hIcon, ref ICONINFO info);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool HideCaret(HandleRef hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ShowCaret(HandleRef hWnd);
    }
}
