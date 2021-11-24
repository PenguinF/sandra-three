#region License
/*********************************************************************************
 * OpaqueColorType.cs
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

using Eutherion.Localization;
using System;
using System.Drawing;
using System.Globalization;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Type that accepts strings in the HTML color format "#xxxxxx" where all x'es are hexadecimal characters,
    /// and converts those values to and from opaque colors.
    /// </summary>
    public sealed class OpaqueColorType : PType.Derived<string, Color>
    {
        public static readonly PTypeErrorBuilder OpaqueColorTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(OpaqueColorTypeError)));

        public static readonly OpaqueColorType Instance = new OpaqueColorType();

        private OpaqueColorType() : base(PType.CLR.String) { }

        public override Union<ITypeErrorBuilder, Color> TryGetTargetValue(string value)
        {
            if (value != null && value.Length == 7 && value[0] == '#')
            {
                string hexString = value.Substring(1);
                if (int.TryParse(hexString, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int rgb))
                {
                    return Color.FromArgb(255, Color.FromArgb(rgb));
                }
            }

            return InvalidValue(OpaqueColorTypeError);
        }

        public override string GetBaseValue(Color value)
            => $"#{value.R.ToString("X2")}{value.G.ToString("X2")}{value.B.ToString("X2")}";
    }
}
