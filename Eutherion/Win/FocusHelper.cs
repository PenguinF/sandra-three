#region License
/*********************************************************************************
 * FocusHelper.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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

using Eutherion.Threading;
using Eutherion.Win.Native;
using System;
using System.Windows.Forms;

namespace Eutherion.Win
{
    public sealed class FocusHelper
    {
        /// <summary>
        /// Returns the .NET control that has the keyboard focus in this application.
        /// </summary>
        /// <returns>
        /// The .NET control that has the keyboard focus, if this application is active,
        /// or a null reference if this application is not active,
        /// or the focused control is not a .NET control.
        /// </returns>
        public static Control GetFocusedControl()
        {
            IntPtr focusedHandle = NativeMethods.GetFocus();
            if (focusedHandle != IntPtr.Zero)
            {
                // If the focused control is not a .NET control, then this will return null.
                return Control.FromHandle(focusedHandle);
            }
            return null;
        }

        Control currentFocusedControl;

        FocusHelper()
        {
            Application.Idle += (_, __) =>
            {
                FocusChangedEventArgs e = null;

                try
                {
                    Control newFocusedControl = GetFocusedControl();
                    if (currentFocusedControl != newFocusedControl)
                    {
                        e = new FocusChangedEventArgs(currentFocusedControl, newFocusedControl);
                        currentFocusedControl = newFocusedControl;
                    }
                }
                catch (Exception ex)
                {
                    // Do not crash if GetFocusedControl failed.
                    // Just trace the exception and assume the focus was unchanged.
                    ex.Trace();
                }

                if (e != null)
                {
                    try
                    {
                        event_FocusChanged.Raise(this, e);
                    }
                    catch (Exception ex)
                    {
                        Application.OnThreadException(ex);
                    }
                }
            };
        }

        // SafeLazy<T> is thread-safe.
        static readonly SafeLazy<FocusHelper> instance = new SafeLazy<FocusHelper>(() => new FocusHelper());

        /// <summary>
        /// Returns the singleton <see cref="FocusHelper"/> which is created on the thread which first uses it.
        /// </summary>
        public static FocusHelper Instance => instance.Value;

        readonly WeakEvent<FocusHelper, FocusChangedEventArgs> event_FocusChanged = new WeakEvent<FocusHelper, FocusChangedEventArgs>();

        /// <summary>
        /// <see cref="WeakEvent"/> which occurs when the focused control changed.
        /// </summary>
        public event Action<FocusHelper, FocusChangedEventArgs> FocusChanged
        {
            add { event_FocusChanged.AddListener(value); }
            remove { event_FocusChanged.RemoveListener(value); }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="FocusHelper.FocusChanged"/> event.
    /// </summary>
    public class FocusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="Control"/> that previously contained focus.
        /// </summary>
        public Control PreviousFocusedControl { get; }

        /// <summary>
        /// Gets the <see cref="Control"/> that currently contains focus.
        /// </summary>
        public Control CurrentFocusedControl { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusChangedEventArgs"/> class.
        /// </summary>
        /// <param name="previousFocusedControl">
        /// The <see cref="Control"/> that previously contained focus.
        /// </param>
        /// <param name="currentFocusedControl">
        /// The <see cref="Control"/> that currently contains focus.
        /// </param>
        public FocusChangedEventArgs(Control previousFocusedControl, Control currentFocusedControl)
        {
            PreviousFocusedControl = previousFocusedControl;
            CurrentFocusedControl = currentFocusedControl;
        }
    }
}
