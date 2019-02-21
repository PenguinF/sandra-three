#region License
/*********************************************************************************
 * Localizers.cs
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

using Eutherion;
using Eutherion.Localization;
using Eutherion.Text.Json;
using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.AppTemplate;
using Eutherion.Win.Storage;
using Eutherion.Win.UIActions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Sandra.UI
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
        internal static readonly LocalizedStringKey DeveloperTools = new LocalizedStringKey(nameof(DeveloperTools));
        internal static readonly LocalizedStringKey Edit = new LocalizedStringKey(nameof(Edit));
        internal static readonly LocalizedStringKey EditCurrentLanguage = new LocalizedStringKey(nameof(EditCurrentLanguage));
        internal static readonly LocalizedStringKey EditPreferencesFile = new LocalizedStringKey(nameof(EditPreferencesFile));
        internal static readonly LocalizedStringKey EndOfGame = new LocalizedStringKey(nameof(EndOfGame));
        internal static readonly LocalizedStringKey ErrorLocation = new LocalizedStringKey(nameof(ErrorLocation));
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
        internal static readonly LocalizedStringKey NoErrorsMessage = new LocalizedStringKey(nameof(NoErrorsMessage));
        internal static readonly LocalizedStringKey Paste = new LocalizedStringKey(nameof(Paste));
        internal static readonly LocalizedStringKey PieceSymbols = new LocalizedStringKey(nameof(PieceSymbols));
        internal static readonly LocalizedStringKey PreviousLine = new LocalizedStringKey(nameof(PreviousLine));
        internal static readonly LocalizedStringKey PreviousMove = new LocalizedStringKey(nameof(PreviousMove));
        internal static readonly LocalizedStringKey PromoteLine = new LocalizedStringKey(nameof(PromoteLine));
        internal static readonly LocalizedStringKey Redo = new LocalizedStringKey(nameof(Redo));
        internal static readonly LocalizedStringKey Save = new LocalizedStringKey(nameof(Save));
        internal static readonly LocalizedStringKey SelectAll = new LocalizedStringKey(nameof(SelectAll));
        internal static readonly LocalizedStringKey ShowDefaultSettingsFile = new LocalizedStringKey(nameof(ShowDefaultSettingsFile));
        internal static readonly LocalizedStringKey StartOfGame = new LocalizedStringKey(nameof(StartOfGame));
        internal static readonly LocalizedStringKey Undo = new LocalizedStringKey(nameof(Undo));
        internal static readonly LocalizedStringKey UseLongAlgebraicNotation = new LocalizedStringKey(nameof(UseLongAlgebraicNotation));
        internal static readonly LocalizedStringKey UsePGNPieceSymbols = new LocalizedStringKey(nameof(UsePGNPieceSymbols));
        internal static readonly LocalizedStringKey View = new LocalizedStringKey(nameof(View));
        internal static readonly LocalizedStringKey ZoomIn = new LocalizedStringKey(nameof(ZoomIn));
        internal static readonly LocalizedStringKey ZoomOut = new LocalizedStringKey(nameof(ZoomOut));
    }

    internal static class Localizers
    {
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

        public static IEnumerable<string> LocalizerKeyCandidates(CultureInfo uiCulture)
        {
            yield return uiCulture.NativeName;
            yield return uiCulture.Name;
            yield return uiCulture.ThreeLetterISOLanguageName;
            yield return uiCulture.TwoLetterISOLanguageName;
            yield return uiCulture.LCID.ToString();

            // Recursively check parents, stop at the invariant culture which has no name.
            if (uiCulture.Parent.Name.Length > 0)
            {
                foreach (var candidateKey in LocalizerKeyCandidates(uiCulture.Parent))
                {
                    yield return candidateKey;
                }
            }
        }

        /// <summary>
        /// Initializes the available localizers.
        /// </summary>
        /// <param name="defaultLangDirectory">
        /// Path to the directory to scan for language files.
        /// </param>
        public static Dictionary<string, FileLocalizer> ScanLocalizers(string defaultLangDirectory)
        {
            var foundLocalizers = new Dictionary<string, FileLocalizer>(StringComparer.OrdinalIgnoreCase);

            var languageFileSchema = CreateLanguageFileSchema();

            try
            {
                DirectoryInfo defaultDir = new DirectoryInfo(defaultLangDirectory);

                if (defaultDir.Exists)
                {
                    foreach (var fileInfo in defaultDir.EnumerateFiles("*.json"))
                    {
                        try
                        {
                            var languageFile = SettingsFile.Create(fileInfo.FullName, new SettingCopy(languageFileSchema));
                            languageFile.Settings.TryGetValue(FlagIconFile, out string flagIconFileName);

                            foundLocalizers.Add(
                                Path.GetFileNameWithoutExtension(fileInfo.Name),
                                new FileLocalizer(languageFile));
                        }
                        catch (Exception exc)
                        {
                            exc.Trace();
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                exc.Trace();
            }

            return foundLocalizers;
        }

        public static FileLocalizer BestFit(Dictionary<string, FileLocalizer> localizers)
        {
            if (localizers.Count > 0)
            {
                // Try keys based on CurrentUICulture.
                foreach (var candidateKey in LocalizerKeyCandidates(CultureInfo.CurrentUICulture))
                {
                    if (localizers.TryGetValue(candidateKey, out FileLocalizer localizerMatch))
                    {
                        return localizerMatch;
                    }
                }

                return localizers.First().Value;
            }

            return null;
        }
    }

    /// <summary>
    /// Apart from being a <see cref="Localizer"/>, contains properties
    /// to allow construction of <see cref="UIAction"/> bindings and interact with settings.
    /// </summary>
    public sealed class FileLocalizer : Localizer, IWeakEventTarget
    {
        /// <summary>
        /// Gets the settings file from which this localizer is loaded.
        /// </summary>
        public SettingsFile LanguageFile { get; }

        /// <summary>
        /// Gets the name of the language in the language itself, e.g. "English", "Español", "Deutsch", ...
        /// </summary>
        public string LanguageName { get; }

        /// <summary>
        /// Gets the file name without extension of the flag icon.
        /// </summary>
        public string FlagIconFileName { get; }

        public Dictionary<LocalizedStringKey, string> Dictionary { get; private set; }

        public FileLocalizer(SettingsFile languageFile)
        {
            LanguageFile = languageFile;
            LanguageName = languageFile.Settings.GetValue(Localizers.NativeName);
            FlagIconFileName = languageFile.Settings.TryGetValue(Localizers.FlagIconFile, out string flagIconFile) ? flagIconFile : string.Empty;
            UpdateDictionary();
        }

        private void UpdateDictionary()
        {
            Dictionary = LanguageFile.Settings.TryGetValue(Localizers.Translations, out Dictionary<LocalizedStringKey, string> dict) ? dict : new Dictionary<LocalizedStringKey, string>();
        }

        public override string Localize(LocalizedStringKey localizedStringKey, string[] parameters)
            => Dictionary.TryGetValue(localizedStringKey, out string displayText)
            ? LocalizedString.ConditionalFormat(displayText, parameters)
            : Default.Localize(localizedStringKey, parameters);

        private DefaultUIActionBinding switchToLangUIActionBinding;

        public DefaultUIActionBinding SwitchToLangUIActionBinding
        {
            get
            {
                if (switchToLangUIActionBinding == null)
                {
                    Image menuIcon = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(FlagIconFileName))
                        {
                            menuIcon = Image.FromFile(Path.Combine(Path.GetDirectoryName(LanguageFile.AbsoluteFilePath), FlagIconFileName));
                        }
                    }
                    catch (Exception exc)
                    {
                        exc.Trace();
                    }

                    switchToLangUIActionBinding = new DefaultUIActionBinding(
                        new UIAction(nameof(Localizers) + "." + LanguageName),
                        new ImplementationSet<IUIActionInterface>
                        {
                            new ContextMenuUIActionInterface
                            {
                                MenuCaptionKey = LocalizedStringKey.Unlocalizable(LanguageName),
                                MenuIcon = menuIcon,
                            },
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
                Session.Current.AutoSave.Persist(Session.Current.LangSetting, this);
            }

            return new UIActionState(UIActionVisibility.Enabled, Current == this);
        }

        public void EnableLiveUpdates()
        {
            // Can only happen after a message loop has been started.
            LanguageFile.RegisterSettingsChangedHandler(Localizers.Translations, TranslationsChanged);
        }

        private void TranslationsChanged(object sender, EventArgs e)
        {
            UpdateDictionary();
            NotifyChanged();
        }

        // TranslationsChanged only raises another WeakEvent so this does not indirectly leak.
        // FileLocalizers do not go out of scope during the lifetime of the application.
        bool IWeakEventTarget.IsDisposed => false;
    }

    internal sealed class BuiltInEnglishLocalizer : Localizer
    {
        public static readonly BuiltInEnglishLocalizer Instance = new BuiltInEnglishLocalizer();

        public readonly Dictionary<LocalizedStringKey, string> Dictionary;

        public override string Localize(LocalizedStringKey localizedStringKey, string[] parameters)
            => Dictionary.TryGetValue(localizedStringKey, out string displayText)
            ? LocalizedString.ConditionalFormat(displayText, parameters)
            : Default.Localize(localizedStringKey, parameters);

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
                { LocalizedStringKeys.DeveloperTools, "Tools" },
                { LocalizedStringKeys.Edit, "Edit" },
                { LocalizedStringKeys.EditCurrentLanguage, "Edit current language" },
                { LocalizedStringKeys.EditPreferencesFile, "Edit preferences" },
                { LocalizedStringKeys.EndOfGame, "End of game" },
                { LocalizedStringKeys.ErrorLocation, "{0} at line {1}, position {2}" },
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
                { LocalizedStringKeys.NoErrorsMessage, "(No errors)" },
                { LocalizedStringKeys.Paste, "Paste" },
                { LocalizedStringKeys.PieceSymbols, "NBRQK" },
                { LocalizedStringKeys.PreviousLine, "Previous line" },
                { LocalizedStringKeys.PreviousMove, "Previous move" },
                { LocalizedStringKeys.PromoteLine, "Promote line" },
                { LocalizedStringKeys.Redo, "Redo" },
                { LocalizedStringKeys.Save, "Save" },
                { LocalizedStringKeys.SelectAll, "Select All" },
                { LocalizedStringKeys.ShowDefaultSettingsFile, "Show default settings" },
                { LocalizedStringKeys.StartOfGame, "Start of game" },
                { LocalizedStringKeys.UseLongAlgebraicNotation, "Use long algebraic notation" },
                { LocalizedStringKeys.UsePGNPieceSymbols, "Use PGN notation" },
                { LocalizedStringKeys.Undo, "Undo" },
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

                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.UnexpectedSymbol), "Unexpected symbol '{0}'" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.UnterminatedMultiLineComment), "Unterminated multi-line comment" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.UnterminatedString), "Unterminated string" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.UnrecognizedEscapeSequence), "Unrecognized escape sequence ('{0}')" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.IllegalControlCharacterInString), "Illegal control character '{0}' in string" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.ExpectedEof), "End of file expected" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.UnexpectedEofInObject), "Unexpected end of file, expected '}'" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.UnexpectedEofInArray), "Unexpected end of file, expected ']'" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.ControlSymbolInObject), "'}' expected" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.ControlSymbolInArray), "']' expected" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.InvalidPropertyKey), "Invalid property key" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.PropertyKeyAlreadyExists), "Key '{0}' already exists in object" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.MissingPropertyKey), "Missing property key" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.MissingValue), "Missing value" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.UnrecognizedValue), "Unrecognized value '{0}'" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.MultiplePropertyKeySections), "Unexpected ':', expected ',' or '}'" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.MultiplePropertyKeys), "':' expected" },
                { JsonErrorInfoExtensions.GetLocalizedStringKey(JsonErrorCode.MultipleValues), "',' expected" },

                { SettingReader.RootValueShouldBeObjectTypeError.LocalizedMessageKey, "Expected object ('{{ \"a\" = 1, \"b\" = 2, ... }}')" },

                { PType.JsonArray, "a value array ('[1, 2, ...]')" },
                { PType.JsonObject, "an object ('{{ \"a\" = 1, \"b\" = 2, ... }}')" },
                { PType.JsonUndefinedValue, "an undefined value" },

                { PType.BooleanTypeError.LocalizedMessageKey, "Expected '" + JsonValue.False + "' or '" + JsonValue.True + "' value for {0}, but found {1}" },
                { PType.IntegerTypeError.LocalizedMessageKey, "Expected integer value for {0}, but found {1}" },
                { PType.StringTypeError.LocalizedMessageKey, "Expected string value for {0}, but found {1}" },
                { PType.MapTypeError.LocalizedMessageKey, "Expected object ('{{ \"a\" = 1, \"b\" = 2, ... }}') for {0}, but found {1}" },

                { PTypeErrorBuilder.NoLegalValues, "Found value {1}, but there exist no legal values for {0}" },
                { PTypeErrorBuilder.EnumerateWithOr, "{0} or {1}" },
                { PType.EnumerationTypeError, "Expected {2} for {0}, but found {1}" },
                { PType.KeyedSetTypeError, "Expected {2} for {0}, but found {1}" },
                { PType.RangedIntegerTypeError, "Expected integer value between {2} and {3} for {0}, but found {1}" },

                //{ PersistableFormState.PersistableFormStateTypeError.LocalizedMessageKey, "" }, // PersistableFormState only used for auto-save.
                { OpaqueColorType.OpaqueColorTypeError.LocalizedMessageKey, "Expected string in the HTML color format (e.g. \"#808000\", or \"#DC143C\") for {0}, but found {1}" },
                { FileNameType.FileNameTypeError.LocalizedMessageKey, "Expected valid file name for {0}, but found {1}" },
                { SubFolderNameType.SubFolderNameTypeError.LocalizedMessageKey, "Expected valid subfolder name for {0}, but found {1}" },
                { TrimmedStringType.TrimmedStringTypeError.LocalizedMessageKey, "Expected string value which contains at least one non white-space character for {0}, but found {1}" },
                { TranslationDictionaryTypeError.LocalizedMessageKey, "Expected string translations for {0}, but found {2} at key {1}" },
            };
        }
    }

    public static class JsonErrorInfoExtensions
    {
        public static LocalizedStringKey GetLocalizedStringKey(JsonErrorCode jsonErrorCode)
        {
            const string UnspecifiedMessage = "Unspecified error";

            if (jsonErrorCode == JsonErrorCode.Unspecified)
            {
                return LocalizedStringKey.Unlocalizable(UnspecifiedMessage);
            }
            else
            {
                return new LocalizedStringKey($"JsonError{jsonErrorCode}");
            }
        }

        /// <summary>
        /// Gets the formatted and localized error message of a <see cref="JsonErrorInfo"/>.
        /// </summary>
        public static string Message(this JsonErrorInfo jsonErrorInfo, Localizer localizer)
        {
            if (jsonErrorInfo is PTypeError typeError)
            {
                return typeError.GetLocalizedMessage(localizer);
            }

            return localizer.Localize(GetLocalizedStringKey(jsonErrorInfo.ErrorCode), jsonErrorInfo.Parameters);
        }
    }

    public sealed class TrimmedStringType : PType.Derived<string, string>
    {
        public static readonly PTypeErrorBuilder TrimmedStringTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(TrimmedStringTypeError)));

        public static TrimmedStringType Instance = new TrimmedStringType();

        private TrimmedStringType() : base(PType.CLR.String) { }

        public override string GetBaseValue(string value) => value;

        public override Union<ITypeErrorBuilder, string> TryGetTargetValue(string value)
            => !string.IsNullOrWhiteSpace(value)
            ? ValidValue(value.Trim())
            : InvalidValue(TrimmedStringTypeError);
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

        public override Union<ITypeErrorBuilder, Dictionary<LocalizedStringKey, string>> TryGetTargetValue(PMap value)
        {
            var dictionary = new Dictionary<LocalizedStringKey, string>();

            foreach (var kv in value)
            {
                if (!PType.String.TryGetValidValue(kv.Value).IsOption2(out PString stringValue))
                {
                    return InvalidValue(new TranslationDictionaryTypeError(kv.Key, kv.Value));
                }

                dictionary.Add(new LocalizedStringKey(kv.Key), stringValue.Value);
            }

            return ValidValue(dictionary);
        }
    }

    /// <summary>
    /// Represents the result of a failed typecheck of <see cref="TranslationDictionaryType"/>.
    /// </summary>
    public class TranslationDictionaryTypeError : PValueVisitor<Localizer, string>, ITypeErrorBuilder
    {
        /// <summary>
        /// Gets the translation key for this error message.
        /// </summary>
        public static readonly LocalizedStringKey LocalizedMessageKey = new LocalizedStringKey(nameof(TranslationDictionaryTypeError));

        /// <summary>
        /// Gets the key with the illegal value.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the illegal value.
        /// </summary>
        public PValue InvalidStringValue { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="KeyedSetTypeError"/>.
        /// </summary>
        internal TranslationDictionaryTypeError(string key, PValue invalidStringValue)
        {
            Key = key;
            InvalidStringValue = invalidStringValue;
        }

        /// <summary>
        /// Gets the localized error message in the current language.
        /// </summary>
        public string GetLocalizedTypeErrorMessage(Localizer localizer, string propertyKey, string valueString)
            => localizer.Localize(LocalizedMessageKey, new[]
            {
                propertyKey,
                PTypeErrorBuilder.QuoteStringValue(Key),
                Visit(InvalidStringValue, localizer)
            });


        public override string DefaultVisit(PValue value, Localizer localizer) => localizer.Localize(PType.JsonUndefinedValue);
        public override string VisitBoolean(PBoolean value, Localizer localizer) => PTypeErrorBuilder.QuoteValue(JsonValue.BoolSymbol(value.Value));
        public override string VisitInteger(PInteger value, Localizer localizer) => PTypeErrorBuilder.QuoteValue(value.Value.ToString(CultureInfo.InvariantCulture));
        public override string VisitList(PList value, Localizer localizer) => localizer.Localize(PType.JsonArray);
        public override string VisitMap(PMap value, Localizer localizer) => localizer.Localize(PType.JsonObject);
    }
}
