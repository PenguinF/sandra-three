#region License
/*********************************************************************************
 * SingleInstanceMainForm.cs
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
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Hidden window which uses a mutex file to ensure that at most one instance
    /// of the Windows Forms application is running.
    /// </summary>
    public abstract class SingleInstanceMainForm : Form
    {
        private Session session;

        /// <summary>
        /// Called when a handle is created and a <see cref="Session"/> for this
        /// <see cref="SingleInstanceMainForm"/> must be configured.
        /// </summary>
        /// <returns>
        /// The initialized session.
        /// </returns>
        public abstract Session RequestSession();

        protected override void OnHandleCreated(EventArgs e)
        {
            session = RequestSession();
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            session?.Dispose();
            session = null;
        }
    }
}
