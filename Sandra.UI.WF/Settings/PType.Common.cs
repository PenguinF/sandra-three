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

            private sealed class _BooleanCLRType : Derived<PBoolean, bool>
            {
                public _BooleanCLRType() : base(PType.Boolean) { }

                public override bool TryGetTargetValue(PBoolean boolean, out bool targetValue)
                {
                    targetValue = boolean.Value;
                    return true;
                }

                public override PBoolean GetBaseValue(bool value) => new PBoolean(value);
            }

            private sealed class _Int32CLRType : Derived<PInteger, int>
            {
                public _Int32CLRType() : base(new RangedInteger(int.MinValue, int.MaxValue)) { }

                public override bool TryGetTargetValue(PInteger integer, out int targetValue)
                {
                    targetValue = (int)integer.Value;
                    return true;
                }

                public override PInteger GetBaseValue(int value) => new PInteger(value);
            }

            private sealed class _StringCLRType : Derived<PString, string>
            {
                public _StringCLRType() : base(PType.String) { }

                public override bool TryGetTargetValue(PString stringValue, out string targetValue)
                {
                    targetValue = stringValue.Value;
                    return true;
                }

                public override PString GetBaseValue(string value) => new PString(value);
            }
        }

        public sealed class RichTextZoomFactor : Derived<PInteger, int>
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

            private RichTextZoomFactor() : base(new RangedInteger(MinDiscreteValue, MaxDiscreteValue)) { }

            public override bool TryGetTargetValue(PInteger integer, out int targetValue)
            {
                targetValue = (int)integer.Value;
                return true;
            }

            public override PInteger GetBaseValue(int value) => new PInteger(value);
        }

        public sealed class Enumeration<TEnum> : Derived<string, TEnum> where TEnum : struct
        {
            private readonly Dictionary<TEnum, string> enumToString = new Dictionary<TEnum, string>();
            private readonly Dictionary<string, TEnum> stringToEnum = new Dictionary<string, TEnum>();

            public Enumeration(IEnumerable<TEnum> enumValues) : base(CLR.String)
            {
                Type enumType = typeof(TEnum);
                foreach (var enumValue in enumValues)
                {
                    string name = Enum.GetName(enumType, enumValue);
                    enumToString.Add(enumValue, name);
                    stringToEnum.Add(name, enumValue);
                }
            }

            public override bool TryGetTargetValue(string stringValue, out TEnum targetValue)
                => stringToEnum.TryGetValue(stringValue, out targetValue);

            public override string GetBaseValue(TEnum value) => enumToString[value];
        }

        public sealed class KeyedSet<T> : Derived<string, T> where T : class
        {
            private readonly Dictionary<string, T> stringToTarget = new Dictionary<string, T>();

            public KeyedSet(IEnumerable<KeyValuePair<string, T>> keyedValues) : base(CLR.String)
            {
                foreach (var keyedValue in keyedValues)
                {
                    stringToTarget.Add(keyedValue.Key, keyedValue.Value);
                }
            }

            public override bool TryGetTargetValue(string stringValue, out T targetValue)
                => stringToTarget.TryGetValue(stringValue, out targetValue);

            public override string GetBaseValue(T value)
            {
                foreach (var kv in stringToTarget)
                {
                    if (kv.Value == value) return kv.Key;
                }

                throw new ArgumentException("Target value not found.");
            }
        }
    }
}
