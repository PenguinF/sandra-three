#region License
/*********************************************************************************
 * UpdatableRichTextBox.cs
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

using Eutherion.Win.Native;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Eutherion.Win.Controls
{
    /// <summary>
    /// Standard Windows <see cref="RichTextBox"/> with a <see cref="BeginUpdate"/> method to suspend repainting of the <see cref="RichTextBox"/>. 
    /// </summary>
    public class UpdatableRichTextBox : RichTextBox
    {
        private const int WM_SETREDRAW = 0x0b;

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

            public _UpdateToken(UpdatableRichTextBox owner)
            {
                this.owner = owner;
            }

            public override void Dispose()
            {
                owner.EndUpdate();
                GC.SuppressFinalize(this);
            }

            ~_UpdateToken()
            {
                // Make sure that _UpdateTokens which go out of scope without being disposed
                // stop blocking an UpdatableRichTextBox when they are garbage collected.
                if (owner != null) owner.EndUpdate();
            }
        }

        private uint blockingUpdateTokenCount;

        /// <summary>
        /// Gets if the <see cref="UpdatableRichTextBox"/> is currently being updated.
        /// </summary>
        public bool IsUpdating => blockingUpdateTokenCount > 0;

        /// <summary>
        /// Suspends repainting of the <see cref="UpdatableRichTextBox"/> while it's being updated.
        /// Use this method when the <see cref="Text"/> is about to change.
        /// </summary>
        public UpdateToken BeginUpdate()
        {
            if (blockingUpdateTokenCount == 0 && !IsDisposed && !Disposing && IsHandleCreated)
            {
                NativeMethods.HideCaret(new HandleRef(this, Handle));
                NativeMethods.SendMessage(new HandleRef(this, Handle), WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            }
            ++blockingUpdateTokenCount;

            return new _UpdateToken(this);
        }

        /// <summary>
        /// Resumes repainting of the <see cref="UpdatableRichTextBox"/> after it's being updated.
        /// </summary>
        private void EndUpdate()
        {
            --blockingUpdateTokenCount;
            if (blockingUpdateTokenCount == 0 && !IsDisposed && !Disposing && IsHandleCreated)
            {
                NativeMethods.SendMessage(new HandleRef(this, Handle), WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                NativeMethods.ShowCaret(new HandleRef(this, Handle));
                Invalidate();
            }
        }
    }
}
