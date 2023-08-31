#region License
/*********************************************************************************
 * PersistableFormState.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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
using System.Windows.Forms;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Captures the state of a <see cref="Form"/> for an auto-save file.
    /// This is a combination of its last location and whether or not it was maximized.
    /// </summary>
    public class PersistableFormState
    {
        /// <summary>
        /// Gets the <see cref="PType"/> of a <see cref="PersistableFormState"/>.
        /// </summary>
        public static readonly PType<PersistableFormState> Type = new FormStatePType();

        private sealed class FormStatePType : PType.Derived<(bool, int, int, int, int), PersistableFormState>
        {
            public FormStatePType()
                : base(new PType.TupleType<bool, int, int, int, int>(
                                          (PType.CLR.Boolean, PType.CLR.Int32, PType.CLR.Int32, PType.CLR.Int32, PType.CLR.Int32)))
            {
            }

            public override Union<ITypeErrorBuilder, PersistableFormState> TryGetTargetValue((bool, int, int, int, int) value)
            {
                var (maximized, left, top, width, height) = value;
                return new PersistableFormState(maximized, new Rectangle(left, top, width, height));
            }

            public override (bool, int, int, int, int) ConvertToBaseValue(PersistableFormState value)
                => (value.Maximized, value.Bounds.Left, value.Bounds.Top, value.Bounds.Width, value.Bounds.Height);
        }

        private Form form;

        public PersistableFormState(bool maximized, Rectangle bounds)
        {
            Maximized = maximized;
            Bounds = bounds;
        }

        /// <summary>
        /// Gets if the <see cref="Form"/> is currently maximized.
        /// </summary>
        public bool Maximized { get; private set; }

        /// <summary>
        /// Gets the current location of the <see cref="Form"/>,
        /// or the location of the <see cref="Form"/> before it was maximized.
        /// </summary>
        public Rectangle Bounds { get; private set; }

        private void Update()
        {
            // Remember settings before changing them.
            bool oldMaximized = Maximized;
            Rectangle oldBounds = Bounds;

            // Don't store anything if the form is minimized.
            // If the application is then closed and reopened, it will restore to the state before it was minimized.
            if (form.WindowState == FormWindowState.Maximized)
            {
                Maximized = true;
            }
            else if (form.WindowState == FormWindowState.Normal)
            {
                Maximized = false;
                Bounds = form.Bounds;
            }

            if (oldMaximized != Maximized || oldBounds != Bounds)
            {
                // Raise event if change is detected.
                OnChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Attaches to events of the <see cref="Form"/> to detect changes in the Form's window state.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="form"/> is null.
        /// </exception>
        public void AttachTo(Form form)
        {
            // Suppress "use 'throw' expression" message, throw must happen before Detach, and this.form cannot be assigned earlier.
#pragma warning disable IDE0016
            if (form == null) throw new ArgumentNullException(nameof(form));
#pragma warning restore IDE0016

            // Assume caller will deal with unregistering Changed event.
            Detach();
            this.form = form;

            // Initialize with updated values.
            Update();

            form.ResizeEnd += Form_ResizeEnd;
            form.LocationChanged += Form_LocationChanged;
            form.FormClosed += Form_FormClosed;
        }

        private void Detach()
        {
            if (form != null)
            {
                // Clean up events.
                form.ResizeEnd -= Form_ResizeEnd;
                form.LocationChanged -= Form_LocationChanged;
                form.FormClosed -= Form_FormClosed;
                form = null;
            }
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Detach();
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            Update();
        }

        private void Form_LocationChanged(object sender, EventArgs e)
        {
            Update();
        }

        /// <summary>
        /// Occurs when the value of <see cref="Maximized"/> and/or <see cref="Bounds"/> changed.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Raises the <see cref="OnChanged(EventArgs)]"/> event.
        /// </summary>
        /// <param name="e">
        /// The arguments of the event.
        /// </param>
        protected virtual void OnChanged(EventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }
}
