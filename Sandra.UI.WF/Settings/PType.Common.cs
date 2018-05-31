/*********************************************************************************
 * PType.Common.cs
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

namespace Sandra.UI.WF
{
    public static partial class PType
    {
        public sealed class RichTextZoomFactor : PType<int>
        {
            /// <summary>
            /// Returns the minimum discrete integer value which when converted with
            /// <see cref="FromDiscreteZoomFactor"/> is still a valid value for the ZoomFactor property
            /// of a <see cref="System.Windows.Forms.RichTextBox"/> which is between 1/64 and 64,
            /// endpoints not included.
            /// </summary>
            public static readonly int MinDiscreteValue = -9;

            /// <summary>
            /// Returns the maximum discrete integer value which when converted with
            /// <see cref="FromDiscreteZoomFactor"/> is still a valid value for the ZoomFactor property
            /// of a <see cref="System.Windows.Forms.RichTextBox"/> which is between 1/64 and 64,
            /// endpoints not included.
            /// </summary>
            public static readonly int MaxDiscreteValue = 649;

            public static float FromDiscreteZoomFactor(int zoomFactor)
                => (zoomFactor + 10) / 10f;

            public static int ToDiscreteZoomFactor(float zoomFactor)
                // Assume discrete deltas of 0.1f.
                // Set 0 to be the default, so 1.0f should map to 0.
                => (int)Math.Round(zoomFactor * 10f) - 10;

            public static readonly RichTextZoomFactor Instance = new RichTextZoomFactor();

            private RichTextZoomFactor() { }

            public override bool TryGetValidValue(PValue value, out int targetValue)
            {
                if (value is PInteger)
                {
                    PInteger integer = (PInteger)value;
                    if (MinDiscreteValue <= integer.Value && integer.Value <= MaxDiscreteValue)
                    {
                        targetValue = (int)integer.Value;
                        return true;
                    }
                }

                targetValue = default(int);
                return false;
            }

            public override PValue GetPValue(int value) => new PInteger(value);
        }
    }
}
