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
using Eutherion.Utils;
using Eutherion.Win.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Eutherion.Win.AppTemplate
{
    public static class Localizers
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
        /// <param name="session">
        /// The session in which to register the language files.
        /// </param>
        /// <param name="defaultLangDirectory">
        /// Path to the directory to scan for language files.
        /// </param>
        public static Dictionary<string, FileLocalizer> ScanLocalizers(Session session, string defaultLangDirectory)
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
                                new FileLocalizer(session, languageFile));
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

    public sealed class TrimmedStringType : PType.Derived<string, string>
    {
        public static readonly PTypeErrorBuilder TrimmedStringTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(TrimmedStringTypeError)));

        public static readonly TrimmedStringType Instance = new TrimmedStringType();

        private TrimmedStringType() : base(PType.CLR.String) { }

        public override string GetBaseValue(string value) => value;

        public override Union<ITypeErrorBuilder, string> TryGetTargetValue(string value)
            => !string.IsNullOrWhiteSpace(value)
            ? ValidValue(value.Trim())
            : InvalidValue(TrimmedStringTypeError);
    }

    public sealed class TranslationDictionaryType : PType.Derived<PMap, Dictionary<LocalizedStringKey, string>>
    {
        public static readonly TranslationDictionaryType Instance = new TranslationDictionaryType();

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
