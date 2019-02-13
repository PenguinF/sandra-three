#region License
/*********************************************************************************
 * FormExtensions.cs
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

using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Eutherion.Win
{
    public static class FormExtensions
    {
        /// <summary>
        /// If a <see cref="Form"/> is minimized, restores it to its previous state.
        /// </summary>
        /// <param name="form">
        /// The <see cref="Form"/> to restore.
        /// </param>
        public static void Deminimize(this Form form)
        {
            const int SW_RESTORE = 0x09;

            if (form.IsHandleCreated
                && !form.IsDisposed
                && form.WindowState == FormWindowState.Minimized)
            {
                WinAPI.ShowWindow(new HandleRef(form, form.Handle), SW_RESTORE);
            }
        }
    }
}
