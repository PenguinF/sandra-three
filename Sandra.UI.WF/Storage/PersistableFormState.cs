/*********************************************************************************
 * PersistableFormState.cs
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI.WF
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

        private sealed class FormStatePType : PType<PersistableFormState>
        {
            public override bool TryGetValidValue(PValue value, out PersistableFormState targetValue)
            {
                PList windowBoundsList = value as PList;
                bool maximized;
                int left, top, width, height;
                if (windowBoundsList != null
                    && windowBoundsList.Count == 5
                    && PType.CLR.Boolean.TryGetValidValue(windowBoundsList[0], out maximized)
                    && PType.CLR.Int32.TryGetValidValue(windowBoundsList[1], out left)
                    && PType.CLR.Int32.TryGetValidValue(windowBoundsList[2], out top)
                    && PType.CLR.Int32.TryGetValidValue(windowBoundsList[3], out width)
                    && PType.CLR.Int32.TryGetValidValue(windowBoundsList[4], out height))
                {
                    targetValue = new PersistableFormState(maximized, new Rectangle(left, top, width, height));
                    return true;
                }

                targetValue = default(PersistableFormState);
                return false;
            }

            public override PValue GetPValue(PersistableFormState value) => new PList(
                new List<PValue>
                {
                    PType.CLR.Boolean.GetPValue(value.Maximized),
                    PType.CLR.Int32.GetPValue(value.Bounds.Left),
                    PType.CLR.Int32.GetPValue(value.Bounds.Top),
                    PType.CLR.Int32.GetPValue(value.Bounds.Width),
                    PType.CLR.Int32.GetPValue(value.Bounds.Height),
                });
        }

        private Form form;

        private bool maximized;

        private Rectangle bounds;

        public PersistableFormState(bool maximized, Rectangle bounds)
        {
            this.maximized = maximized;
            this.bounds = bounds;
        }

        /// <summary>
        /// Gets if the <see cref="Form"/> is currently maximized.
        /// </summary>
        public bool Maximized => maximized;

        /// <summary>
        /// Gets the current location of the <see cref="Form"/>,
        /// or the location of the <see cref="Form"/> before it was maximized.
        /// </summary>
        public Rectangle Bounds => bounds;

        private void Update()
        {
            // Remember settings before changing them.
            bool oldMaximized = maximized;
            Rectangle oldBounds = bounds;

            // Don't store anything if the form is minimized.
            // If the application is then closed and reopened, it will restore to the state before it was minimized.
            if (form.WindowState == FormWindowState.Maximized)
            {
                maximized = true;
            }
            else if (form.WindowState == FormWindowState.Normal)
            {
                maximized = false;
                bounds = form.Bounds;
            }

            if (oldMaximized != maximized || oldBounds != bounds)
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
            if (form == null) throw new ArgumentNullException(nameof(form));

            // Assume caller will deal with unregistering Changed event.
            detach();
            this.form = form;

            // Initialize with updated values.
            Update();

            form.ResizeEnd += Form_ResizeEnd;
            form.LocationChanged += Form_LocationChanged;
            form.FormClosed += Form_FormClosed;
        }

        private void detach()
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
            detach();
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
