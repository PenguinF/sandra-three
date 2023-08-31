#region License
/*********************************************************************************
 * Settings.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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

using Eutherion.Win.MdiAppTemplate;
using Eutherion.Win.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sandra.UI
{
    internal static class SettingKeys
    {
        public static readonly string DefaultAppDataSubFolderName = "SandraChess";

        private static readonly string VersionDescription
            = "Identifies the version of the set of recognized properties. The only allowed value is 1.";

        public static readonly SettingProperty<int> Version = new SettingProperty<int>(
            SettingKey.ToSnakeCaseKey(nameof(Version)),
            VersionRange.Instance,
            new SettingComment(VersionDescription));

        private sealed class VersionRange : PType.Derived<PInteger, int>
        {
            public static readonly VersionRange Instance = new VersionRange();

            private VersionRange() : base(new PType.RangedInteger(1, 1)) { }

            public override Union<ITypeErrorBuilder, int> TryGetTargetValue(PInteger integer)
                => (int)integer.Value;

            public override PInteger ConvertToBaseValue(int value)
                => new PInteger(value);
        }

        public static readonly SettingProperty<IEnumerable<PersistableFormState>> Windows = new SettingProperty<IEnumerable<PersistableFormState>>(
            SettingKey.ToSnakeCaseKey(nameof(Windows)),
            new PType.ValueList<PersistableFormState>(PersistableFormState.Type));

        public static readonly SettingProperty<int> PgnZoom = new SettingProperty<int>(
            SettingKey.ToSnakeCaseKey(nameof(PgnZoom)),
            ScintillaZoomFactor.Instance);

        private static readonly string FastNavigationPlyCountDescription
            = "The number of plies (=half moves) to move forward or backward in a game for fast navigation. "
            + $"This value must be between {FastNavigationPlyCountRange.MinPlyCount} and {FastNavigationPlyCountRange.MaxPlyCount}.";

        public static readonly SettingProperty<int> FastNavigationPlyCount = new SettingProperty<int>(
            SettingKey.ToSnakeCaseKey(nameof(FastNavigationPlyCount)),
            FastNavigationPlyCountRange.Instance,
            new SettingComment(FastNavigationPlyCountDescription));

        private sealed class FastNavigationPlyCountRange : PType.Derived<PInteger, int>
        {
            public static readonly int MinPlyCount = 2;
            public static readonly int MaxPlyCount = 40;

            public static readonly FastNavigationPlyCountRange Instance = new FastNavigationPlyCountRange();

            private FastNavigationPlyCountRange() : base(new PType.RangedInteger(MinPlyCount, MaxPlyCount)) { }

            public override Union<ITypeErrorBuilder, int> TryGetTargetValue(PInteger integer)
                => (int)integer.Value;

            public override PInteger ConvertToBaseValue(int value)
                => new PInteger(value);
        }

        private static readonly string DarkSquareColorDescription
            = "The color of the dark squares. This value must be in the HTML color format, "
            + "for example \"#808000\" is the Olive color.";

        public static readonly SettingProperty<Color> DarkSquareColor = new SettingProperty<Color>(
            SettingKey.ToSnakeCaseKey(nameof(DarkSquareColor)),
            OpaqueColorType.Instance,
            new SettingComment(DarkSquareColorDescription));

        private static readonly string LightSquareColorDescription
            = "The color of the light squares. This value must be in the HTML color format, "
            + "for example \"#F0E68C\" is the Khaki color.";

        public static readonly SettingProperty<Color> LightSquareColor = new SettingProperty<Color>(
            SettingKey.ToSnakeCaseKey(nameof(LightSquareColor)),
            OpaqueColorType.Instance,
            new SettingComment(LightSquareColorDescription));

        private static readonly string LastMoveArrowColorDescription
            = "The color of the arrow which displays the last move. This value must be in the HTML color format, "
            + "for example \"#DC143C\" is the Crimson color.";

        public static readonly SettingProperty<Color> LastMoveArrowColor = new SettingProperty<Color>(
            SettingKey.ToSnakeCaseKey(nameof(LastMoveArrowColor)),
            OpaqueColorType.Instance,
            new SettingComment(LastMoveArrowColorDescription));

        private static readonly string DisplayLegalTargetSquaresDescription
            = "Whether or not to display all legal target squares of a piece when it is selected.";

        public static readonly SettingProperty<bool> DisplayLegalTargetSquares = new SettingProperty<bool>(
            SettingKey.ToSnakeCaseKey(nameof(DisplayLegalTargetSquares)),
            PType.CLR.Boolean,
            new SettingComment(DisplayLegalTargetSquaresDescription));

        private static readonly string LegalTargetSquaresColorDescription
            = "Overlay color used to display legal target squares. This value must be in the HTML color format, "
            + "for example \"#B0E0E6\" is the PowderBlue color.";

        public static readonly SettingProperty<Color> LegalTargetSquaresColor = new SettingProperty<Color>(
            SettingKey.ToSnakeCaseKey(nameof(LegalTargetSquaresColor)),
            OpaqueColorType.Instance,
            new SettingComment(LegalTargetSquaresColorDescription));
    }

    public class SettingsProvider : ISettingsProvider
    {
        public SettingSchema CreateAutoSaveSchema(Session session)
        {
            return new SettingSchema(
                session.LangSetting,
                SettingKeys.Windows,
                SharedSettings.AutoSaveCounter,
                SettingKeys.PgnZoom,
                SharedSettings.DefaultSettingsAutoSave,
                SharedSettings.PreferencesAutoSave,
                SharedSettings.JsonZoom);
        }

        public SettingSchema CreateDefaultSettingsSchema(Session session)
        {
            return new SettingSchema(
                session.DefaultSettingsSchemaDescription(isLocalSchema: false),
                SettingKeys.Version,
                SharedSettings.AppDataSubFolderName,
                SharedSettings.LocalPreferencesFileName,
                session.DeveloperMode,
                SharedSettings.LangFolderName,
                SettingKeys.DarkSquareColor,
                SettingKeys.LightSquareColor,
                SettingKeys.LastMoveArrowColor,
                SettingKeys.DisplayLegalTargetSquares,
                SettingKeys.LegalTargetSquaresColor,
                SettingKeys.FastNavigationPlyCount);
        }

        public SettingSchema CreateLocalSettingsSchema(Session session)
        {
            return new SettingSchema(
                session.DefaultSettingsSchemaDescription(isLocalSchema: true),
                SettingKeys.DarkSquareColor,
                SettingKeys.LightSquareColor,
                SettingKeys.LastMoveArrowColor,
                SettingKeys.DisplayLegalTargetSquares,
                SettingKeys.LegalTargetSquaresColor,
                SettingKeys.FastNavigationPlyCount,
                session.DeveloperMode);
        }

        public SettingObject CreateBuiltIn(Session session)
        {
            SettingCopy defaultSettings = new SettingCopy(CreateDefaultSettingsSchema(session));

            defaultSettings.AddOrReplace(SettingKeys.Version, 1);
            defaultSettings.AddOrReplace(SharedSettings.AppDataSubFolderName, SettingKeys.DefaultAppDataSubFolderName);
            defaultSettings.AddOrReplace(SharedSettings.LocalPreferencesFileName, SharedSettings.DefaultLocalPreferencesFileName);
            defaultSettings.AddOrReplace(session.DeveloperMode, false);
            defaultSettings.AddOrReplace(SharedSettings.LangFolderName, SharedSettings.DefaultLangFolderName);
            defaultSettings.AddOrReplace(SettingKeys.DarkSquareColor, Color.LightBlue);
            defaultSettings.AddOrReplace(SettingKeys.LightSquareColor, Color.Azure);
            defaultSettings.AddOrReplace(SettingKeys.LastMoveArrowColor, Color.DimGray);
            defaultSettings.AddOrReplace(SettingKeys.DisplayLegalTargetSquares, true);
            defaultSettings.AddOrReplace(SettingKeys.LegalTargetSquaresColor, Color.FromArgb(240, 90, 90));
            defaultSettings.AddOrReplace(SettingKeys.FastNavigationPlyCount, 10);

            return defaultSettings.Commit();
        }
    }
}
