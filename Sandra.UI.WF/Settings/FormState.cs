/*********************************************************************************
 * FormState.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Captures the state of a <see cref="Form"/> for an auto-save file.
    /// This is a combination of its last location and whether or not it was maximized.
    /// </summary>
    public class FormState
    {
        /// <summary>
        /// Gets the <see cref="PType"/> of a <see cref="FormState"/>.
        /// </summary>
        public static readonly PType<Rectangle> Type = new FormStatePType();

        private sealed class FormStatePType : PType<Rectangle>
        {
            public override bool TryGetValidValue(PValue value, out Rectangle targetValue)
            {
                PList windowBoundsList = value as PList;
                int left, top, width, height;
                if (windowBoundsList != null
                    && windowBoundsList.Count == 4
                    && PType.Int32.Instance.TryGetValidValue(windowBoundsList[0], out left)
                    && PType.Int32.Instance.TryGetValidValue(windowBoundsList[1], out top)
                    && PType.Int32.Instance.TryGetValidValue(windowBoundsList[2], out width)
                    && PType.Int32.Instance.TryGetValidValue(windowBoundsList[3], out height))
                {
                    targetValue = new Rectangle(left, top, width, height);
                    return true;
                }

                targetValue = default(Rectangle);
                return false;
            }

            public override PValue GetPValue(Rectangle value)
                => new PList(new List<PValue>
                {
                    PType.Int32.Instance.GetPValue(value.Left),
                    PType.Int32.Instance.GetPValue(value.Top),
                    PType.Int32.Instance.GetPValue(value.Width),
                    PType.Int32.Instance.GetPValue(value.Height),
                });
        }
    }
}
