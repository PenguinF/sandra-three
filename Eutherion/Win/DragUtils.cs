#region License
/*********************************************************************************
 * DragUtils.cs
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public static class DragUtils
    {
        /// <summary>
        /// Resizes an image, overlays it with an existing <see cref="Cursor"/>, and creates a new <see cref="Cursor"/> from the result.
        /// </summary>
        public static CursorFromHandle CreateDragCursorFromImage(Image image, Size imageSize, Cursor overlayCursor, Point hotSpot)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (overlayCursor == null) throw new ArgumentNullException(nameof(overlayCursor));

            // Use the overlay cursor's size and hot spot to determine where to draw overlayCursor.
            Size overlaySize = overlayCursor.Size;
            Point overlayHotSpot = overlayCursor.HotSpot;
            Rectangle cursorRectangle = new Rectangle(hotSpot.X - overlayHotSpot.X,
                                                      hotSpot.Y - overlayHotSpot.Y,
                                                      overlaySize.Width,
                                                      overlaySize.Height);
            Rectangle imageRectangle = new Rectangle(0, 0, imageSize.Width, imageSize.Height);
            Size cursorSize = new Size(imageSize.Width, imageSize.Height);

            // Now expand cursorSize so it can contain both rectangles.
            int diff = cursorRectangle.X;
            if (diff < 0)
            {
                hotSpot.X -= diff;
                cursorRectangle.X -= diff;
                imageRectangle.X -= diff;
                cursorSize.Width -= diff;
            }
            diff = cursorRectangle.Y;
            if (diff < 0)
            {
                hotSpot.Y -= diff;
                cursorRectangle.Y -= diff;
                imageRectangle.Y -= diff;
                cursorSize.Height -= diff;
            }
            diff = cursorRectangle.Right - imageRectangle.Right;
            if (diff > 0) cursorSize.Width += diff;
            diff = cursorRectangle.Bottom - imageRectangle.Bottom;
            if (diff > 0) cursorSize.Height += diff;

            using (Bitmap copy = new Bitmap(cursorSize.Width, cursorSize.Height))
            {
                using (var g = Graphics.FromImage(copy))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawImage(image, imageRectangle);
                    overlayCursor.Draw(g, cursorRectangle);
                }

                IntPtr iconHandle = copy.GetHicon();
                ICONINFO iconInfo = new ICONINFO();
                try
                {
                    // Obtain icon-like info using the Windows API, and create a new icon from it to change its hot spot.
                    WinAPI.GetIconInfo(new HandleRef(copy, iconHandle), ref iconInfo);
                    iconInfo.fIcon = false;
                    iconInfo.xHotspot = hotSpot.X;
                    iconInfo.yHotspot = hotSpot.Y;

                    IntPtr cursorIconHandle = WinAPI.CreateIconIndirect(ref iconInfo);
                    return cursorIconHandle != IntPtr.Zero ? new CursorFromHandle(cursorIconHandle) : null;
                }
                finally
                {
                    if (iconInfo.hbmColor != IntPtr.Zero) WinAPI.DeleteObject(new HandleRef(null, iconInfo.hbmColor));
                    if (iconInfo.hbmMask != IntPtr.Zero) WinAPI.DeleteObject(new HandleRef(null, iconInfo.hbmMask));
                    if (iconHandle != IntPtr.Zero) WinAPI.DestroyIcon(new HandleRef(copy, iconHandle));
                }
            }
        }
    }
}
