﻿/*********************************************************************************
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
using SysExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sandra.UI.WF
{
    public static partial class PType
    {
        public sealed class Boolean : PType<bool>
        {
            public static readonly Boolean Instance = new Boolean();

            private Boolean() { }

            public override bool TryGetValidValue(PValue value, out bool targetValue)
            {
                if (value is PBoolean)
                {
                    targetValue = ((PBoolean)value).Value;
                    return true;
                }

                targetValue = default(bool);
                return false;
            }

            public override PValue GetPValue(bool value) => new PBoolean(value);
        }

        public sealed class Int32 : PType<int>
        {
            public static readonly Int32 Instance = new Int32();

            private Int32() { }

            public override bool TryGetValidValue(PValue value, out int targetValue)
            {
                if (value is PInteger)
                {
                    PInteger integer = (PInteger)value;
                    if (int.MinValue <= integer.Value && integer.Value <= int.MaxValue)
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

        public sealed class WindowRectangle : PType<Rectangle>
        {
            public static readonly WindowRectangle Instance = new WindowRectangle();

            private WindowRectangle() { }

            public override bool TryGetValidValue(PValue value, out Rectangle targetValue)
            {
                PList windowBoundsList = value as PList;
                int left, top, width, height;
                if (windowBoundsList != null
                    && windowBoundsList.Count == 4
                    && Int32.Instance.TryGetValidValue(windowBoundsList[0], out left)
                    && Int32.Instance.TryGetValidValue(windowBoundsList[1], out top)
                    && Int32.Instance.TryGetValidValue(windowBoundsList[2], out width)
                    && Int32.Instance.TryGetValidValue(windowBoundsList[3], out height))
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
                    Int32.Instance.GetPValue(value.Left),
                    Int32.Instance.GetPValue(value.Top),
                    Int32.Instance.GetPValue(value.Width),
                    Int32.Instance.GetPValue(value.Height),
                });
        }

        public sealed class TwoConstants : PType<OptionValue<_void, _void>>
        {
            private readonly PValue Constant1;
            private readonly PValue Constant2;

            public TwoConstants(PValue constant1, PValue constant2)
            {
                if (constant1 == null) throw new ArgumentNullException(nameof(constant1));
                if (constant2 == null) throw new ArgumentNullException(nameof(constant2));

                Constant1 = constant1;
                Constant2 = constant2;
            }

            public override bool TryGetValidValue(PValue value, out OptionValue<_void, _void> targetValue)
            {
                if (SettingHelper.AreEqual(Constant1, value))
                {
                    targetValue = OptionValue<_void, _void>.Option1(_void._);
                    return true;
                }
                else if (SettingHelper.AreEqual(Constant2, value))
                {
                    targetValue = OptionValue<_void, _void>.Option2(_void._);
                    return true;
                }

                targetValue = default(OptionValue<_void, _void>);
                return false;
            }

            public override PValue GetPValue(OptionValue<_void, _void> value)
                => value.Case(
                    whenOption1: _ => Constant1,
                    whenOption2: _ => Constant2);
        }

        public sealed class ThreeConstants : PType<OptionValue<_void, _void, _void>>
        {
            private readonly PValue Constant1;
            private readonly PValue Constant2;
            private readonly PValue Constant3;

            public ThreeConstants(PValue constant1, PValue constant2, PValue constant3)
            {
                if (constant1 == null) throw new ArgumentNullException(nameof(constant1));
                if (constant2 == null) throw new ArgumentNullException(nameof(constant2));
                if (constant2 == null) throw new ArgumentNullException(nameof(constant2));

                Constant1 = constant1;
                Constant2 = constant2;
                Constant3 = constant3;
            }

            public override bool TryGetValidValue(PValue value, out OptionValue<_void, _void, _void> targetValue)
            {
                if (SettingHelper.AreEqual(Constant1, value))
                {
                    targetValue = OptionValue<_void, _void, _void>.Option1(_void._);
                    return true;
                }
                else if (SettingHelper.AreEqual(Constant2, value))
                {
                    targetValue = OptionValue<_void, _void, _void>.Option2(_void._);
                    return true;
                }
                else if (SettingHelper.AreEqual(Constant3, value))
                {
                    targetValue = OptionValue<_void, _void, _void>.Option3(_void._);
                    return true;
                }

                targetValue = default(OptionValue<_void, _void, _void>);
                return false;
            }

            public override PValue GetPValue(OptionValue<_void, _void, _void> value)
                => value.Case(
                    whenOption1: _ => Constant1,
                    whenOption2: _ => Constant2,
                    whenOption3: _ => Constant3);
        }

        public sealed class Enumeration<TEnum> : PType<TEnum> where TEnum : struct
        {
            private readonly Dictionary<TEnum, string> enumToString = new Dictionary<TEnum, string>();
            private readonly Dictionary<string, TEnum> stringToEnum = new Dictionary<string, TEnum>();

            public Enumeration(IEnumerable<TEnum> enumValues)
            {
                Type enumType = typeof(TEnum);
                foreach (var enumValue in enumValues)
                {
                    string name = Enum.GetName(enumType, enumValue);
                    enumToString.Add(enumValue, name);
                    stringToEnum.Add(name, enumValue);
                }
            }

            public override bool TryGetValidValue(PValue value, out TEnum targetValue)
            {
                PString stringValue;
                if (value is PString)
                {
                    stringValue = (PString)value;
                    if (stringToEnum.TryGetValue(stringValue.Value, out targetValue))
                    {
                        return true;
                    }
                }

                targetValue = default(TEnum);
                return false;
            }

            public override PValue GetPValue(TEnum value) => new PString(enumToString[value]);
        }
    }
}
