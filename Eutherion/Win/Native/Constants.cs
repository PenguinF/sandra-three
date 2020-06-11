#region License
/*********************************************************************************
 * Constants.cs
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

namespace Eutherion.Win.Native
{
    /// <summary>
    /// Contains constants for results of the <see cref="WM.NCHITTEST"/> message.
    /// </summary>
    public static class HT
    {
        public const int CLIENT = 1;
        public const int CAPTION = 2;
        public const int LEFT = 10;
        public const int RIGHT = 11;
        public const int TOP = 12;
        public const int TOPLEFT = 13;
        public const int TOPRIGHT = 14;
        public const int BOTTOM = 15;
        public const int BOTTOMLEFT = 16;
        public const int BOTTOMRIGHT = 17;
    }

    /// <summary>
    /// Constains constants for native system commands, e.g. the WParam of the <see cref="WM.SYSCOMMAND"/> message.
    /// See also: https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand
    /// </summary>
    public static class SC
    {
        public const int MASK = 0xfff0;

        public const int MINIMIZE = 0xf020;
        public const int MAXIMIZE = 0xf030;
        public const int RESTORE = 0xf120;
    }

    /// <summary>
    /// Constains constants for the <see cref="NativeMethods.TrackPopupMenuEx"/> P/Invoke call.
    /// </summary>
    public static class TPM
    {
        public const uint RETURNCMD = 0x0100;
        public const uint LEFTBUTTON = 0x0;
    }

    /// <summary>
    /// Contains constants for native Windows messages.
    /// </summary>
    public static class WM
    {
        public const int QUERYOPEN = 0x13;

        public const int WINDOWPOSCHANGED = 0x47;

        public const int COPYDATA = 0x4A;

        public const int NCCALCSIZE = 0x83;
        public const int NCHITTEST = 0x84;
        public const int NCRBUTTONUP = 0xa5;

        public const int SYSCOMMAND = 0x0112;

        public const int SIZING = 0x214;
        public const int MOVING = 0x216;

        public const int DWMCOMPOSITIONCHANGED = 0x031e;
    }
}
