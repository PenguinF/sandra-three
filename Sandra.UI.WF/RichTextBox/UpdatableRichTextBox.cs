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
using SysExtensions;
using SysExtensions.SyntaxRenderer;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Standard Windows <see cref="RichTextBox"/> with a <see cref="BeginUpdate"/> method to suspend repainting of the <see cref="RichTextBox"/>. 
    /// </summary>
    public class UpdatableRichTextBox : RichTextBox, ISyntaxRenderTarget
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

        public ObservableValue<int> CaretPosition { get; } = new ObservableValue<int>();

        public void BringIntoView(int caretPosition)
        {
            if (SelectionStart != caretPosition)
            {
                Select(caretPosition, 0);
                ScrollToCaret();
            }
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            // Ignore updates as a result of all kinds of calls to Select()/SelectAll().
            // This is only to detect caret updates by interacting with the control.
            // Also check SelectionLength so the event is not raised for non-empty selections.
            if (!IsUpdating && SelectionLength == 0)
            {
                CaretPosition.Value = SelectionStart;
            }

            base.OnSelectionChanged(e);
        }

        public void InsertText(int textPosition, string text)
        {
            if (textPosition < 0) textPosition = 0;
            if (textPosition > TextLength) textPosition = TextLength;

            if (textPosition == TextLength)
            {
                AppendText(text);
            }
            else
            {
                Select(textPosition, 0);
                // This only works if not read-only, so temporarily turn it off.
                ReadOnly = false;
                SelectedText = text;
                ReadOnly = true;
            }
        }

        public void RemoveText(int textStart, int textLength)
        {
            if (textStart >= TextLength || textLength <= 0) return;

            if (textStart < 0) textStart = 0;
            if (textLength > TextLength) textLength = TextLength;

            Select(textStart, textLength);

            // This only works if not read-only, so temporarily turn it off.
            ReadOnly = false;
            SelectedText = string.Empty;
            ReadOnly = true;
        }

        public UpdatableRichTextBox()
        {
            CaretPosition.ValueChanged += BringIntoView;
        }
    }
}
