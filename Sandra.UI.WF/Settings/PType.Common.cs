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
using System.Collections.Generic;

namespace Sandra.UI.WF
{
    public static partial class PType
    {
        /// <summary>
        /// Contains <see cref="PType"/>s which convert to and from common .NET data types.
        /// </summary>
        public static class CLR
        {
            public static readonly PType<bool> Boolean = new _BooleanCLRType();
            public static readonly PType<int> Int32 = new _Int32CLRType();
            public static readonly PType<string> String = new _StringCLRType();

            private sealed class _BooleanCLRType : PType<bool>
            {
                public override bool TryGetValidValue(PValue value, out bool targetValue)
                {
                    PBoolean boolean;
                    if (PType.Boolean.TryGetValidValue(value, out boolean))
                    {
                        targetValue = boolean.Value;
                        return true;
                    }

                    targetValue = default(bool);
                    return false;
                }

                public override PValue GetPValue(bool value) => PType.Boolean.GetPValue(new PBoolean(value));
            }

            private sealed class _Int32CLRType : PType<int>
            {
                public override bool TryGetValidValue(PValue value, out int targetValue)
                {
                    PInteger integer;
                    if (PType.Integer.TryGetValidValue(value, out integer)
                        && int.MinValue <= integer.Value && integer.Value <= int.MaxValue)
                    {
                        targetValue = (int)integer.Value;
                        return true;
                    }

                    targetValue = default(int);
                    return false;
                }

                public override PValue GetPValue(int value) => PType.Integer.GetPValue(new PInteger(value));
            }

            private sealed class _StringCLRType : PType<string>
            {
                public override bool TryGetValidValue(PValue value, out string targetValue)
                {
                    PString stringValue;
                    if (PType.String.TryGetValidValue(value, out stringValue))
                    {
                        targetValue = stringValue.Value;
                        return true;
                    }

                    targetValue = default(string);
                    return false;
                }

                public override PValue GetPValue(string value) => PType.String.GetPValue(new PString(value));
            }
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
                PInteger integer;
                if (PType.Integer.TryGetValidValue(value, out integer)
                    && MinDiscreteValue <= integer.Value && integer.Value <= MaxDiscreteValue)
                {
                    targetValue = (int)integer.Value;
                    return true;
                }

                targetValue = default(int);
                return false;
            }

            public override PValue GetPValue(int value) => PType.Integer.GetPValue(new PInteger(value));
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
                string stringValue;
                if (PType.CLR.String.TryGetValidValue(value, out stringValue)
                    && stringToEnum.TryGetValue(stringValue, out targetValue))
                {
                    return true;
                }

                targetValue = default(TEnum);
                return false;
            }

            public override PValue GetPValue(TEnum value) => PType.CLR.String.GetPValue(enumToString[value]);
        }

        public sealed class KeyedSet<T> : PType<T> where T : class
        {
            private readonly Dictionary<string, T> stringToTarget = new Dictionary<string, T>();

            public KeyedSet(IEnumerable<KeyValuePair<string, T>> keyedValues)
            {
                foreach (var keyedValue in keyedValues)
                {
                    stringToTarget.Add(keyedValue.Key, keyedValue.Value);
                }
            }

            public override bool TryGetValidValue(PValue value, out T targetValue)
            {
                string stringValue;
                if (PType.CLR.String.TryGetValidValue(value, out stringValue)
                    && stringToTarget.TryGetValue(stringValue, out targetValue))
                {
                    return true;
                }

                targetValue = default(T);
                return false;
            }

            public override PValue GetPValue(T value)
            {
                foreach (var kv in stringToTarget)
                {
                    if (kv.Value == value) return PType.CLR.String.GetPValue(kv.Key);
                }

                throw new ArgumentException("Target value not found.");
            }
        }
    }
}
