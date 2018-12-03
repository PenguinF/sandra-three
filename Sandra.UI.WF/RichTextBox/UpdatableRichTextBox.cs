#region License
/*********************************************************************************
 * UpdatableRichTextBox.cs
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
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Standard Windows <see cref="RichTextBox"/> with a <see cref="BeginUpdate"/> method to suspend repainting of the <see cref="RichTextBox"/>. 
    /// </summary>
    public class UpdatableRichTextBox : RichTextBox
    {
        private const int WM_SETREDRAW = 0x0b;
        private const int EM_SETCHARFORMAT = 0x0444;
        private const int SCF_SELECTION = 0x0001;
        private const int CFM_UNDERLINETYPE = 0x800000;
        private const byte WAVY_RED = 0x58;

        /// <summary>
        /// Represents a unique update token returned from <see cref="BeginUpdate"/>().
        /// Repainting of the <see cref="UpdatableRichTextBox"/> is suspended until <see cref="Dispose()"/> is called.
        /// </summary>
        public abstract class UpdateToken : IDisposable
        {
            public abstract void Dispose();
        }

        private sealed class _UpdateToken : UpdateToken
        {
            private readonly UpdatableRichTextBox owner;
            private readonly bool restore;
            private readonly int selectionStart;
            private readonly int firstVisibleLine;

            public _UpdateToken(UpdatableRichTextBox owner,
                                bool restore,
                                int selectionStart,
                                int firstVisibleLine)
            {
                this.owner = owner;
                this.restore = restore;
                this.selectionStart = selectionStart;
                this.firstVisibleLine = firstVisibleLine;
            }

            public override void Dispose()
            {
                owner.EndUpdate(restore, selectionStart, firstVisibleLine);
                GC.SuppressFinalize(this);
            }

            ~_UpdateToken()
            {
                // Make sure that _UpdateTokens which go out of scope without being disposed
                // stop blocking an UpdatableRichTextBox when they are garbage collected.
                if (owner != null) owner.EndUpdate(false, 0, 0);
            }
        }

        private uint blockingUpdateTokenCount;

        /// <summary>
        /// Gets if the <see cref="UpdatableRichTextBox"/> is currently being updated.
        /// </summary>
        public bool IsUpdating => blockingUpdateTokenCount > 0;

        private UpdateToken beginUpdate(bool restore)
        {
            if (blockingUpdateTokenCount == 0 && !IsDisposed && !Disposing && IsHandleCreated)
            {
                WinAPI.HideCaret(new HandleRef(this, Handle));
                WinAPI.SendMessage(new HandleRef(this, Handle), WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            }
            ++blockingUpdateTokenCount;

            int selectionStartToRestore = 0;
            int firstVisibleLineToRestore = 0;
            if (restore)
            {
                selectionStartToRestore = SelectionStart;
                firstVisibleLineToRestore = GetLineFromCharIndex(GetCharIndexFromPosition(Point.Empty));
            }

            return new _UpdateToken(this, restore, selectionStartToRestore, firstVisibleLineToRestore);
        }

        /// <summary>
        /// Suspends repainting of the <see cref="UpdatableRichTextBox"/> while it's being updated.
        /// Use this method when the <see cref="Text"/> is about to change.
        /// </summary>
        public UpdateToken BeginUpdate() => beginUpdate(false);

        /// <summary>
        /// Suspends repainting of the <see cref="UpdatableRichTextBox"/> while it's being updated.
        /// Attempts to restore the current position of the caret when the token is disposed.
        /// As a result, the current selection state will always be reset.
        /// Use this method when the <see cref="Text"/> remains unchanged and the style is updated.
        /// </summary>
        /// <remarks>
        /// The RichTextBox API seems to have this hiatus that makes it impossible to decide what the direction was in which the text was selected.
        /// For this reason, the SelectionLength is always set to zero when the token is disposed.
        /// </remarks>
        public UpdateToken BeginUpdateRememberState() => beginUpdate(true);

        /// <summary>
        /// Resumes repainting of the <see cref="UpdatableRichTextBox"/> after it's being updated.
        /// </summary>
        private void EndUpdate(bool restore, int selectionStart, int firstVisibleLine)
        {
            if (restore)
            {
                // Sort of restore first visible char index with this trick:
                // a) Move to end to get downwards as much as possible so step (b) has the best chance to succeed.
                // b) Move to first character of the line to restore so it becomes the first visible line again.
                Select(TextLength, 0);
                Select(GetFirstCharIndexFromLine(firstVisibleLine), 0);

                // Only then move back to selectionStart.
                Select(selectionStart, 0);
            }

            --blockingUpdateTokenCount;
            if (blockingUpdateTokenCount == 0 && !IsDisposed && !Disposing && IsHandleCreated)
            {
                WinAPI.SendMessage(new HandleRef(this, Handle), WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                WinAPI.ShowCaret(new HandleRef(this, Handle));
                Invalidate();
            }
        }

        /// <summary>
        /// Underlines the current selection with a red wavy line.
        /// </summary>
        /// <summary>
        /// From: https://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/RichTextBox.cs
        /// </summary>
        public void SetErrorUnderline()
        {
            if (IsHandleCreated)
            {
                CHARFORMAT2A cf2 = new CHARFORMAT2A();
                cf2.dwMask = CFM_UNDERLINETYPE;
                cf2.bUnderlineType = WAVY_RED;
                WinAPI.SendMessage(new HandleRef(this, Handle), EM_SETCHARFORMAT, SCF_SELECTION, cf2);
            }
        }

        /// <summary>
        /// Removes any underline from the current selection.
        /// </summary>
        /// <summary>
        /// From: https://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/RichTextBox.cs
        /// </summary>
        public void ClearErrorUnderline()
        {
            if (IsHandleCreated)
            {
                CHARFORMAT2A cf2 = new CHARFORMAT2A();
                cf2.dwMask = CFM_UNDERLINETYPE;
                cf2.bUnderlineType = 0;
                WinAPI.SendMessage(new HandleRef(this, Handle), EM_SETCHARFORMAT, SCF_SELECTION, cf2);
            }
        }
    }
}
