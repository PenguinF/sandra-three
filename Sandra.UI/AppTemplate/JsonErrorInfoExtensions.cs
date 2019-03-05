#region License
/*********************************************************************************
 * JsonErrorInfoExtensions.cs
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
using Eutherion.Win.Storage;
using System.Collections.Generic;

namespace Eutherion.Win.AppTemplate
{
    public static class JsonErrorInfoExtensions
    {
        public static LocalizedStringKey GetLocalizedStringKey(JsonErrorCode jsonErrorCode)
            => new LocalizedStringKey($"JsonError{jsonErrorCode}");

        /// <summary>
        /// Gets the formatted and localized error message of a <see cref="JsonErrorInfo"/>.
        /// </summary>
        public static string Message(this JsonErrorInfo jsonErrorInfo, Localizer localizer)
        {
            const string UnspecifiedMessage = "Unspecified error";

            if (jsonErrorInfo is PTypeError typeError)
            {
                return typeError.GetLocalizedMessage(localizer);
            }

            if (jsonErrorInfo.ErrorCode != JsonErrorCode.Unspecified)
            {
                return localizer.Localize(GetLocalizedStringKey(jsonErrorInfo.ErrorCode), jsonErrorInfo.Parameters);
            }

            return UnspecifiedMessage;
        }

        public static IEnumerable<KeyValuePair<LocalizedStringKey, string>> DefaultEnglishJsonErrorTranslations => new Dictionary<LocalizedStringKey, string>
        {
            { GetLocalizedStringKey(JsonErrorCode.UnexpectedSymbol), "Unexpected symbol '{0}'" },
            { GetLocalizedStringKey(JsonErrorCode.UnterminatedMultiLineComment), "Unterminated multi-line comment" },
            { GetLocalizedStringKey(JsonErrorCode.UnterminatedString), "Unterminated string" },
            { GetLocalizedStringKey(JsonErrorCode.UnrecognizedEscapeSequence), "Unrecognized escape sequence ('{0}')" },
            { GetLocalizedStringKey(JsonErrorCode.IllegalControlCharacterInString), "Illegal control character '{0}' in string" },
            { GetLocalizedStringKey(JsonErrorCode.ExpectedEof), "End of file expected" },
            { GetLocalizedStringKey(JsonErrorCode.UnexpectedEofInObject), "Unexpected end of file, expected '}'" },
            { GetLocalizedStringKey(JsonErrorCode.UnexpectedEofInArray), "Unexpected end of file, expected ']'" },
            { GetLocalizedStringKey(JsonErrorCode.ControlSymbolInObject), "'}' expected" },
            { GetLocalizedStringKey(JsonErrorCode.ControlSymbolInArray), "']' expected" },
            { GetLocalizedStringKey(JsonErrorCode.InvalidPropertyKey), "Invalid property key" },
            { GetLocalizedStringKey(JsonErrorCode.PropertyKeyAlreadyExists), "Key '{0}' already exists in object" },
            { GetLocalizedStringKey(JsonErrorCode.MissingPropertyKey), "Missing property key" },
            { GetLocalizedStringKey(JsonErrorCode.MissingValue), "Missing value" },
            { GetLocalizedStringKey(JsonErrorCode.UnrecognizedValue), "Unrecognized value '{0}'" },
            { GetLocalizedStringKey(JsonErrorCode.MultiplePropertyKeySections), "Unexpected ':', expected ',' or '}'" },
            { GetLocalizedStringKey(JsonErrorCode.MultiplePropertyKeys), "':' expected" },
            { GetLocalizedStringKey(JsonErrorCode.MultipleValues), "',' expected" },

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
