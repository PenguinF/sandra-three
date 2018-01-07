/*********************************************************************************
 * UpdatableRichTextBox.cs
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
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Standard Windows <see cref="RichTextBox"/> with a <see cref="BeginUpdate"/> method to suspend repainting of the <see cref="RichTextBox"/>. 
    /// </summary>
    public class UpdatableRichTextBox : RichTextBox
    {
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
            private readonly bool rememberSelectionStart;
            private readonly int selectionStart;

            public _UpdateToken(UpdatableRichTextBox owner,
                                bool rememberSelectionStart,
                                int selectionStart)
            {
                this.owner = owner;
                this.rememberSelectionStart = rememberSelectionStart;
                this.selectionStart = selectionStart;
            }

            public override void Dispose()
            {
                owner.EndUpdate(rememberSelectionStart, selectionStart);
                GC.SuppressFinalize(this);
            }

            ~_UpdateToken()
            {
                // Make sure that _UpdateTokens which go out of scope without being disposed
                // stop blocking an UpdatableRichTextBox when they are garbage collected.
                if (owner != null) owner.EndUpdate(false, 0);
            }
        }

        private uint blockingUpdateTokenCount;

        /// <summary>
        /// Gets if the <see cref="UpdatableRichTextBox"/> is currently being updated.
        /// </summary>
        public bool IsUpdating => blockingUpdateTokenCount > 0;

        const int WM_SETREDRAW = 0x0b;

        private UpdateToken beginUpdate(bool rememberCaret)
        {
            if (blockingUpdateTokenCount == 0 && !IsDisposed && !Disposing && IsHandleCreated)
            {
                WinAPI.HideCaret(new HandleRef(this, Handle));
                WinAPI.SendMessage(new HandleRef(this, Handle), WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            }
            ++blockingUpdateTokenCount;
            return new _UpdateToken(this, rememberCaret, rememberCaret ? SelectionStart : 0);
        }

        /// <summary>
        /// Suspends repainting of the <see cref="UpdatableRichTextBox"/> while it's being updated.
        /// </summary>
        public UpdateToken BeginUpdate() => beginUpdate(false);

        /// <summary>
        /// Suspends repainting of the <see cref="UpdatableRichTextBox"/> while it's being updated.
        /// Attempts to restore the current position of the caret when the token is disposed.
        /// As a result, the current selection state will always be reset.
        /// </summary>
        /// <remarks>
        /// The RichTextBox API seems to have this hiatus that makes it impossible to decide what the direction was in which the text was selected.
        /// For this reason, the SelectionLength is always set to zero when the token is disposed.
        /// </remarks>
        public UpdateToken BeginUpdateRememberCaret() => beginUpdate(true);

        /// <summary>
        /// Resumes repainting of the <see cref="UpdatableRichTextBox"/> after it's being updated.
        /// </summary>
        private void EndUpdate(bool setSelectionStart, int selectionStart)
        {
            if (setSelectionStart) Select(selectionStart, 0);
            --blockingUpdateTokenCount;
            if (blockingUpdateTokenCount == 0 && !IsDisposed && !Disposing && IsHandleCreated)
            {
                WinAPI.SendMessage(new HandleRef(this, Handle), WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                WinAPI.ShowCaret(new HandleRef(this, Handle));
                Invalidate();
            }
        }
    }
}
