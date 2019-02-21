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

using Eutherion.Localization;
using Eutherion.Text.Json;
using Eutherion.UIActions;
using Eutherion.Win.AppTemplate;
using Eutherion.Win.Storage;
using System.Collections.Generic;

namespace Sandra.UI
{
    internal static class LocalizedStringKeys
    {
        internal static readonly LocalizedStringKey BreakAtCurrentPosition = new LocalizedStringKey(nameof(BreakAtCurrentPosition));
        internal static readonly LocalizedStringKey Chessboard = new LocalizedStringKey(nameof(Chessboard));
        internal static readonly LocalizedStringKey CopyDiagramToClipboard = new LocalizedStringKey(nameof(CopyDiagramToClipboard));
        internal static readonly LocalizedStringKey DeleteLine = new LocalizedStringKey(nameof(DeleteLine));
        internal static readonly LocalizedStringKey DemoteLine = new LocalizedStringKey(nameof(DemoteLine));
        internal static readonly LocalizedStringKey EndOfGame = new LocalizedStringKey(nameof(EndOfGame));
        internal static readonly LocalizedStringKey FastBackward = new LocalizedStringKey(nameof(FastBackward));
        internal static readonly LocalizedStringKey FastForward = new LocalizedStringKey(nameof(FastForward));
        internal static readonly LocalizedStringKey FirstMove = new LocalizedStringKey(nameof(FirstMove));
        internal static readonly LocalizedStringKey FlipBoard = new LocalizedStringKey(nameof(FlipBoard));
        internal static readonly LocalizedStringKey Game = new LocalizedStringKey(nameof(Game));
        internal static readonly LocalizedStringKey GoTo = new LocalizedStringKey(nameof(GoTo));
        internal static readonly LocalizedStringKey LastMove = new LocalizedStringKey(nameof(LastMove));
        internal static readonly LocalizedStringKey Moves = new LocalizedStringKey(nameof(Moves));
        internal static readonly LocalizedStringKey NewGame = new LocalizedStringKey(nameof(NewGame));
        internal static readonly LocalizedStringKey NextLine = new LocalizedStringKey(nameof(NextLine));
        internal static readonly LocalizedStringKey NextMove = new LocalizedStringKey(nameof(NextMove));
        internal static readonly LocalizedStringKey PieceSymbols = new LocalizedStringKey(nameof(PieceSymbols));
        internal static readonly LocalizedStringKey PreviousLine = new LocalizedStringKey(nameof(PreviousLine));
        internal static readonly LocalizedStringKey PreviousMove = new LocalizedStringKey(nameof(PreviousMove));
        internal static readonly LocalizedStringKey PromoteLine = new LocalizedStringKey(nameof(PromoteLine));
        internal static readonly LocalizedStringKey StartOfGame = new LocalizedStringKey(nameof(StartOfGame));
        internal static readonly LocalizedStringKey UseLongAlgebraicNotation = new LocalizedStringKey(nameof(UseLongAlgebraicNotation));
        internal static readonly LocalizedStringKey UsePGNPieceSymbols = new LocalizedStringKey(nameof(UsePGNPieceSymbols));
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
                { SharedLocalizedStringKeys.About, "About SandraChess" },
                { LocalizedStringKeys.BreakAtCurrentPosition, "Break at current position" },
                { LocalizedStringKeys.Chessboard, "Chessboard" },
                { SharedLocalizedStringKeys.Copy, "Copy" },
                { LocalizedStringKeys.CopyDiagramToClipboard, "Copy diagram to clipboard" },
                { SharedLocalizedStringKeys.Credits, "Show credits" },
                { SharedLocalizedStringKeys.Cut, "Cut" },
                { LocalizedStringKeys.DeleteLine, "Delete line" },
                { LocalizedStringKeys.DemoteLine, "Demote line" },
                { SharedLocalizedStringKeys.Edit, "Edit" },
                { SharedLocalizedStringKeys.EditCurrentLanguage, "Edit current language" },
                { SharedLocalizedStringKeys.EditPreferencesFile, "Edit preferences" },
                { LocalizedStringKeys.EndOfGame, "End of game" },
                { SharedLocalizedStringKeys.ErrorLocation, "{0} at line {1}, position {2}" },
                { SharedLocalizedStringKeys.Exit, "Exit" },
                { LocalizedStringKeys.FastBackward, "Fast backward" },
                { LocalizedStringKeys.FastForward, "Fast forward" },
                { SharedLocalizedStringKeys.File, "File" },
                { LocalizedStringKeys.FirstMove, "First move" },
                { LocalizedStringKeys.FlipBoard, "Flip board" },
                { LocalizedStringKeys.Game, "Game" },
                { LocalizedStringKeys.GoTo, "Go to" },
                { SharedLocalizedStringKeys.GoToNextLocation, "Go to next location" },
                { SharedLocalizedStringKeys.GoToPreviousLocation, "Go to previous location" },
                { SharedLocalizedStringKeys.Help, "Help" },
                { LocalizedStringKeys.LastMove, "Last move" },
                { LocalizedStringKeys.Moves, "Moves" },
                { LocalizedStringKeys.NewGame, "New game" },
                { LocalizedStringKeys.NextLine, "Next line" },
                { LocalizedStringKeys.NextMove, "Next move" },
                { SharedLocalizedStringKeys.NoErrorsMessage, "(No errors)" },
                { SharedLocalizedStringKeys.Paste, "Paste" },
                { LocalizedStringKeys.PieceSymbols, "NBRQK" },
                { LocalizedStringKeys.PreviousLine, "Previous line" },
                { LocalizedStringKeys.PreviousMove, "Previous move" },
                { LocalizedStringKeys.PromoteLine, "Promote line" },
                { SharedLocalizedStringKeys.Redo, "Redo" },
                { SharedLocalizedStringKeys.Save, "Save" },
                { SharedLocalizedStringKeys.SelectAll, "Select All" },
                { SharedLocalizedStringKeys.ShowDefaultSettingsFile, "Show default settings" },
                { LocalizedStringKeys.StartOfGame, "Start of game" },
                { SharedLocalizedStringKeys.Tools, "Tools" },
                { LocalizedStringKeys.UseLongAlgebraicNotation, "Use long algebraic notation" },
                { LocalizedStringKeys.UsePGNPieceSymbols, "Use PGN notation" },
                { SharedLocalizedStringKeys.Undo, "Undo" },
                { SharedLocalizedStringKeys.View, "View" },
                { SharedLocalizedStringKeys.ZoomIn, "Zoom in" },
                { SharedLocalizedStringKeys.ZoomOut, "Zoom out" },

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
}
