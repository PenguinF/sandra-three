#region License
/*********************************************************************************
 * Localizers.cs
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
#endregion

using Sandra.UI.WF.Storage;
using System.Collections.Generic;
using System.Linq;

namespace Sandra.UI.WF
{
    internal static class LocalizedStringKeys
    {
        internal static readonly LocalizedStringKey About = new LocalizedStringKey(nameof(About));
        internal static readonly LocalizedStringKey BreakAtCurrentPosition = new LocalizedStringKey(nameof(BreakAtCurrentPosition));
        internal static readonly LocalizedStringKey Chessboard = new LocalizedStringKey(nameof(Chessboard));
        internal static readonly LocalizedStringKey Copy = new LocalizedStringKey(nameof(Copy));
        internal static readonly LocalizedStringKey CopyDiagramToClipboard = new LocalizedStringKey(nameof(CopyDiagramToClipboard));
        internal static readonly LocalizedStringKey Credits = new LocalizedStringKey(nameof(Credits));
        internal static readonly LocalizedStringKey Cut = new LocalizedStringKey(nameof(Cut));
        internal static readonly LocalizedStringKey DeleteLine = new LocalizedStringKey(nameof(DeleteLine));
        internal static readonly LocalizedStringKey DemoteLine = new LocalizedStringKey(nameof(DemoteLine));
        internal static readonly LocalizedStringKey EditPreferencesFile = new LocalizedStringKey(nameof(EditPreferencesFile));
        internal static readonly LocalizedStringKey EndOfGame = new LocalizedStringKey(nameof(EndOfGame));
        internal static readonly LocalizedStringKey Exit = new LocalizedStringKey(nameof(Exit));
        internal static readonly LocalizedStringKey FastBackward = new LocalizedStringKey(nameof(FastBackward));
        internal static readonly LocalizedStringKey FastForward = new LocalizedStringKey(nameof(FastForward));
        internal static readonly LocalizedStringKey File = new LocalizedStringKey(nameof(File));
        internal static readonly LocalizedStringKey FirstMove = new LocalizedStringKey(nameof(FirstMove));
        internal static readonly LocalizedStringKey FlipBoard = new LocalizedStringKey(nameof(FlipBoard));
        internal static readonly LocalizedStringKey Game = new LocalizedStringKey(nameof(Game));
        internal static readonly LocalizedStringKey GoTo = new LocalizedStringKey(nameof(GoTo));
        internal static readonly LocalizedStringKey GoToNextLocation = new LocalizedStringKey(nameof(GoToNextLocation));
        internal static readonly LocalizedStringKey GoToPreviousLocation = new LocalizedStringKey(nameof(GoToPreviousLocation));
        internal static readonly LocalizedStringKey Help = new LocalizedStringKey(nameof(Help));
        internal static readonly LocalizedStringKey LastMove = new LocalizedStringKey(nameof(LastMove));
        internal static readonly LocalizedStringKey Moves = new LocalizedStringKey(nameof(Moves));
        internal static readonly LocalizedStringKey NewGame = new LocalizedStringKey(nameof(NewGame));
        internal static readonly LocalizedStringKey NextLine = new LocalizedStringKey(nameof(NextLine));
        internal static readonly LocalizedStringKey NextMove = new LocalizedStringKey(nameof(NextMove));
        internal static readonly LocalizedStringKey Paste = new LocalizedStringKey(nameof(Paste));
        internal static readonly LocalizedStringKey PieceSymbols = new LocalizedStringKey(nameof(PieceSymbols));
        internal static readonly LocalizedStringKey PreviousLine = new LocalizedStringKey(nameof(PreviousLine));
        internal static readonly LocalizedStringKey PreviousMove = new LocalizedStringKey(nameof(PreviousMove));
        internal static readonly LocalizedStringKey PromoteLine = new LocalizedStringKey(nameof(PromoteLine));
        internal static readonly LocalizedStringKey Save = new LocalizedStringKey(nameof(Save));
        internal static readonly LocalizedStringKey SelectAll = new LocalizedStringKey(nameof(SelectAll));
        internal static readonly LocalizedStringKey ShowDefaultSettingsFile = new LocalizedStringKey(nameof(ShowDefaultSettingsFile));
        internal static readonly LocalizedStringKey StartOfGame = new LocalizedStringKey(nameof(StartOfGame));
        internal static readonly LocalizedStringKey UseLongAlgebraicNotation = new LocalizedStringKey(nameof(UseLongAlgebraicNotation));
        internal static readonly LocalizedStringKey UsePGNPieceSymbols = new LocalizedStringKey(nameof(UsePGNPieceSymbols));
        internal static readonly LocalizedStringKey View = new LocalizedStringKey(nameof(View));
        internal static readonly LocalizedStringKey ZoomIn = new LocalizedStringKey(nameof(ZoomIn));
        internal static readonly LocalizedStringKey ZoomOut = new LocalizedStringKey(nameof(ZoomOut));
    }

    internal static class Localizers
    {
        public static readonly string LangSettingKey = "lang";

        private static KeyedLocalizer[] registered;

        public static IEnumerable<KeyedLocalizer> Registered => registered.Enumerate();

        /// <summary>
        /// This setting key is moved to this class to ensure the localizers are set up before the auto-save setting is loaded.
        /// </summary>
        public static SettingProperty<Localizer> LangSetting { get; private set; }

        private static readonly string NativeNameDescription
            = "Name of the language in the language itself. This property is mandatory.";

        public static readonly SettingProperty<string> NativeName = new SettingProperty<string>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(NativeName))),
            TrimmedStringType.Instance,
            new SettingComment(NativeNameDescription));

        private static readonly string FlagIconFileDescription
            = "File name of the flag icon to display in the menu.";

        public static readonly SettingProperty<string> FlagIconFile = new SettingProperty<string>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(FlagIconFile))),
            FileNameType.Instance,
            new SettingComment(FlagIconFileDescription));

        private static readonly string TranslationsDescription
            = "List of translations.";

        public static readonly SettingProperty<Dictionary<LocalizedStringKey, string>> Translations = new SettingProperty<Dictionary<LocalizedStringKey, string>>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(Translations))),
            TranslationDictionaryType.Instance,
            new SettingComment(TranslationsDescription));

        public static SettingSchema CreateLanguageFileSchema()
        {
            return new SettingSchema(
                NativeName,
                FlagIconFile,
                Translations);
        }

        /// <summary>
        /// Registers a set of localizers. The first localizer will be set as the current localizer.
        /// </summary>
        public static void Register(params KeyedLocalizer[] localizers)
        {
            if (localizers == null || localizers.Length == 0)
            {
                // Use built-in localizer if none is provided.
                localizers = new KeyedLocalizer[] { BuiltInEnglishLocalizer.Instance };
            }

            registered = localizers;

            LangSetting = new SettingProperty<Localizer>(
                new SettingKey(LangSettingKey),
                new PType.KeyedSet<Localizer>(Registered.Select(x => new KeyValuePair<string, Localizer>(x.AutoSaveSettingValue, x))));

            Localizer.Current = Registered.First();
        }
    }

    /// <summary>
    /// Apart from being a <see cref="Localizer"/>, contains abstract properties
    /// to allow construction of <see cref="UIActionBinding"/>s and interact with settings.
    /// </summary>
    public abstract class KeyedLocalizer : Localizer
    {
        /// <summary>
        /// Gets the name of the language in the language itself, e.g. "English", "Español", "Deutsch", ...
        /// </summary>
        public abstract string LanguageName { get; }

        /// <summary>
        /// Gets the value of the <see cref="Localizers.LangSetting"/> in the auto-save file.
        /// </summary>
        public abstract string AutoSaveSettingValue { get; }

        /// <summary>
        /// Gets the file name without extension of the flag icon.
        /// </summary>
        public abstract string FlagIconFileName { get; }

        private DefaultUIActionBinding switchToLangUIActionBinding;

        public DefaultUIActionBinding SwitchToLangUIActionBinding
        {
            get
            {
                if (switchToLangUIActionBinding == null)
                {
                    switchToLangUIActionBinding = new DefaultUIActionBinding(
                        new UIAction(nameof(Localizers) + "." + LanguageName),
                        new UIActionBinding
                        {
                            ShowInMenu = true,
                            MenuCaptionKey = LocalizedStringKey.Unlocalizable(LanguageName),
                            MenuIcon = Program.LoadImage(FlagIconFileName),
                        });
                }

                return switchToLangUIActionBinding;
            }
        }

        public UIActionState TrySwitchToLang(bool perform)
        {
            if (perform)
            {
                Current = this;
                Program.AutoSave.Persist(Localizers.LangSetting, this);
            }

            return new UIActionState(UIActionVisibility.Enabled, Current == this);
        }
    }

    internal sealed class BuiltInEnglishLocalizer : KeyedLocalizer
    {
        public static readonly BuiltInEnglishLocalizer Instance = new BuiltInEnglishLocalizer();

        public readonly Dictionary<LocalizedStringKey, string> Dictionary;

        public override string LanguageName => "English";

        public override string AutoSaveSettingValue => "en";

        public override string FlagIconFileName => "flag-uk";

        public override string Localize(LocalizedStringKey localizedStringKey)
            => Dictionary.TryGetValue(localizedStringKey, out string displayText) ? displayText
            : Default.Localize(localizedStringKey);

        private BuiltInEnglishLocalizer()
        {
            Dictionary = new Dictionary<LocalizedStringKey, string>
            {
                { LocalizedStringKeys.About, "About SandraChess" },
                { LocalizedStringKeys.BreakAtCurrentPosition, "Break at current position" },
                { LocalizedStringKeys.Chessboard, "Chessboard" },
                { LocalizedStringKeys.Copy, "Copy" },
                { LocalizedStringKeys.CopyDiagramToClipboard, "Copy diagram to clipboard" },
                { LocalizedStringKeys.Credits, "Show credits" },
                { LocalizedStringKeys.Cut, "Cut" },
                { LocalizedStringKeys.DeleteLine, "Delete line" },
                { LocalizedStringKeys.DemoteLine, "Demote line" },
                { LocalizedStringKeys.EditPreferencesFile, "Edit preferences" },
                { LocalizedStringKeys.EndOfGame, "End of game" },
                { LocalizedStringKeys.Exit, "Exit" },
                { LocalizedStringKeys.FastBackward, "Fast backward" },
                { LocalizedStringKeys.FastForward, "Fast forward" },
                { LocalizedStringKeys.File, "File" },
                { LocalizedStringKeys.FirstMove, "First move" },
                { LocalizedStringKeys.FlipBoard, "Flip board" },
                { LocalizedStringKeys.Game, "Game" },
                { LocalizedStringKeys.GoTo, "Go to" },
                { LocalizedStringKeys.GoToNextLocation, "Go to next location" },
                { LocalizedStringKeys.GoToPreviousLocation, "Go to previous location" },
                { LocalizedStringKeys.Help, "Help" },
                { LocalizedStringKeys.LastMove, "Last move" },
                { LocalizedStringKeys.Moves, "Moves" },
                { LocalizedStringKeys.NewGame, "New game" },
                { LocalizedStringKeys.NextLine, "Next line" },
                { LocalizedStringKeys.NextMove, "Next move" },
                { LocalizedStringKeys.Paste, "Paste" },
                { LocalizedStringKeys.PieceSymbols, "NBRQK" },
                { LocalizedStringKeys.PreviousLine, "Previous line" },
                { LocalizedStringKeys.PreviousMove, "Previous move" },
                { LocalizedStringKeys.PromoteLine, "Promote line" },
                { LocalizedStringKeys.Save, "Save" },
                { LocalizedStringKeys.SelectAll, "Select All" },
                { LocalizedStringKeys.ShowDefaultSettingsFile, "Show default settings" },
                { LocalizedStringKeys.StartOfGame, "Start of game" },
                { LocalizedStringKeys.UseLongAlgebraicNotation, "Use long algebraic notation" },
                { LocalizedStringKeys.UsePGNPieceSymbols, "Use PGN notation" },
                { LocalizedStringKeys.View, "View" },
                { LocalizedStringKeys.ZoomIn, "Zoom in" },
                { LocalizedStringKeys.ZoomOut, "Zoom out" },

                { LocalizedConsoleKeys.ConsoleKeyCtrl, "Ctrl" },
                { LocalizedConsoleKeys.ConsoleKeyShift, "Shift" },
                { LocalizedConsoleKeys.ConsoleKeyAlt, "Alt" },

                { LocalizedConsoleKeys.ConsoleKeyLeftArrow, "Left Arrow" },
                { LocalizedConsoleKeys.ConsoleKeyRightArrow, "Right Arrow" },
                { LocalizedConsoleKeys.ConsoleKeyUpArrow, "Up Arrow" },
                { LocalizedConsoleKeys.ConsoleKeyDownArrow, "Down Arrow" },

                { LocalizedConsoleKeys.ConsoleKeyDelete, "Del" },
                { LocalizedConsoleKeys.ConsoleKeyHome, "Home" },
                { LocalizedConsoleKeys.ConsoleKeyEnd, "End" },
                { LocalizedConsoleKeys.ConsoleKeyPageDown, "PageDown" },
                { LocalizedConsoleKeys.ConsoleKeyPageUp, "PageUp" },
            };
        }
    }

    public sealed class TrimmedStringType : PType.Derived<string, string>
    {
        public static TrimmedStringType Instance = new TrimmedStringType();

        private TrimmedStringType() : base(PType.CLR.String) { }

        public override string GetBaseValue(string value) => value;

        public override bool TryGetTargetValue(string value, out string targetValue)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                targetValue = value.Trim();
                return true;
            }

            targetValue = default(string);
            return false;
        }
    }

    public sealed class TranslationDictionaryType : PType.Derived<PMap, Dictionary<LocalizedStringKey, string>>
    {
        public static TranslationDictionaryType Instance = new TranslationDictionaryType();

        private TranslationDictionaryType() : base(PType.Map) { }

        public override PMap GetBaseValue(Dictionary<LocalizedStringKey, string> value)
        {
            Dictionary<string, PValue> dictionary = new Dictionary<string, PValue>();

            if (value != null)
            {
                foreach (var kv in value)
                {
                    dictionary.Add(kv.Key.Key, new PString(kv.Value));
                }
            }

            return new PMap(dictionary);
        }

        public override bool TryGetTargetValue(PMap value, out Dictionary<LocalizedStringKey, string> targetValue)
        {
            targetValue = new Dictionary<LocalizedStringKey, string>();

            foreach (var kv in value)
            {
                if (!PType.String.TryGetValidValue(kv.Value, out PString stringValue))
                {
                    targetValue = default(Dictionary<LocalizedStringKey, string>);
                    return false;
                }

                targetValue.Add(new LocalizedStringKey(kv.Key), stringValue.Value);
            }

            return true;
        }
    }

    internal sealed class DutchLocalizer : KeyedLocalizer
    {
        private readonly Dictionary<LocalizedStringKey, string> dutchDictionary;

        public override string LanguageName => "Nederlands";

        public override string AutoSaveSettingValue => "nl";

        public override string FlagIconFileName => "flag-nl";

        public override string Localize(LocalizedStringKey localizedStringKey)
            => dutchDictionary.TryGetValue(localizedStringKey, out string displayText) ? displayText
            : Default.Localize(localizedStringKey);

        public DutchLocalizer()
        {
            dutchDictionary = new Dictionary<LocalizedStringKey, string>
            {
                { LocalizedStringKeys.About, "Over SandraChess" },
                { LocalizedStringKeys.BreakAtCurrentPosition, "Afbreken in huidige stelling" },
                { LocalizedStringKeys.Chessboard, "Schaakbord" },
                { LocalizedStringKeys.Copy, "Kopiëren" },
                { LocalizedStringKeys.CopyDiagramToClipboard, "Diagram naar klembord kopiëren" },
                { LocalizedStringKeys.Credits, "Dankwoord (Engels)" },
                { LocalizedStringKeys.Cut, "Knippen" },
                { LocalizedStringKeys.DeleteLine, "Variant verwijderen" },
                { LocalizedStringKeys.DemoteLine, "Variant degraderen" },
                { LocalizedStringKeys.EditPreferencesFile, "Wijzig voorkeuren" },
                { LocalizedStringKeys.Exit, "Afsluiten" },
                { LocalizedStringKeys.EndOfGame, "Naar einde partij" },
                { LocalizedStringKeys.FastBackward, "Snel achterwaarts" },
                { LocalizedStringKeys.FastForward, "Snel voorwaarts" },
                { LocalizedStringKeys.File, "Bestand" },
                { LocalizedStringKeys.FirstMove, "Eerste zet" },
                { LocalizedStringKeys.FlipBoard, "Bord omkeren" },
                { LocalizedStringKeys.Game, "Partij" },
                { LocalizedStringKeys.GoTo, "Navigeren" },
                { LocalizedStringKeys.GoToNextLocation, "Ga naar volgende locatie" },
                { LocalizedStringKeys.GoToPreviousLocation, "Ga naar vorige locatie" },
                { LocalizedStringKeys.Help, "Help" },
                { LocalizedStringKeys.LastMove, "Laatste zet" },
                { LocalizedStringKeys.Moves, "Zetten" },
                { LocalizedStringKeys.NewGame, "Nieuwe partij" },
                { LocalizedStringKeys.NextLine, "Volgende variant" },
                { LocalizedStringKeys.NextMove, "Volgende zet" },
                { LocalizedStringKeys.Paste, "Plakken" },
                { LocalizedStringKeys.PieceSymbols, "PLTDK" },
                { LocalizedStringKeys.PreviousLine, "Vorige variant" },
                { LocalizedStringKeys.PreviousMove, "Vorige zet" },
                { LocalizedStringKeys.PromoteLine, "Variant promoveren" },
                { LocalizedStringKeys.Save, "Opslaan" },
                { LocalizedStringKeys.SelectAll, "Alles selecteren" },
                { LocalizedStringKeys.ShowDefaultSettingsFile, "Standaard instellingen tonen" },
                { LocalizedStringKeys.StartOfGame, "Naar begin partij" },
                { LocalizedStringKeys.UseLongAlgebraicNotation, "Lange notatie gebruiken" },
                { LocalizedStringKeys.UsePGNPieceSymbols, "PGN notatie gebruiken" },
                { LocalizedStringKeys.View, "Weergave" },
                { LocalizedStringKeys.ZoomIn, "Inzoomen" },
                { LocalizedStringKeys.ZoomOut, "Uitzoomen" },

                { LocalizedConsoleKeys.ConsoleKeyCtrl, "Ctrl" },
                { LocalizedConsoleKeys.ConsoleKeyShift, "Shift" },
                { LocalizedConsoleKeys.ConsoleKeyAlt, "Alt" },

                { LocalizedConsoleKeys.ConsoleKeyLeftArrow, "Pijl Links" },
                { LocalizedConsoleKeys.ConsoleKeyRightArrow, "Pijl Rechts" },
                { LocalizedConsoleKeys.ConsoleKeyUpArrow, "Pijl Omhoog" },
                { LocalizedConsoleKeys.ConsoleKeyDownArrow, "Pijl Omlaag" },

                { LocalizedConsoleKeys.ConsoleKeyDelete, "Del" },
                { LocalizedConsoleKeys.ConsoleKeyHome, "Home" },
                { LocalizedConsoleKeys.ConsoleKeyEnd, "End" },
                { LocalizedConsoleKeys.ConsoleKeyPageDown, "PageDown" },
                { LocalizedConsoleKeys.ConsoleKeyPageUp, "PageUp" },
            };
        }
    }
}
