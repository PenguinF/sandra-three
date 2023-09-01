#region License
/*********************************************************************************
 * Localizers.cs
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

using Eutherion.Text;
using Eutherion.Win.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Eutherion.Win.MdiAppTemplate
{
    public static class Localizers
    {
        private static readonly string NativeNameDescription
            = "Name of the language in the language itself. This property is mandatory.";

        public static readonly SettingProperty<string> NativeName = new SettingProperty<string>(
            SettingKey.ToSnakeCaseKey(nameof(NativeName)),
            TrimmedStringType.Instance,
            new SettingComment(NativeNameDescription));

        private static readonly string FlagIconFileDescription
            = "File name of the flag icon to display in the menu.";

        public static readonly SettingProperty<string> FlagIconFile = new SettingProperty<string>(
            SettingKey.ToSnakeCaseKey(nameof(FlagIconFile)),
            FileNameType.Instance,
            new SettingComment(FlagIconFileDescription));

        private static readonly string TranslationsDescription
            = "List of translations.";

        public static readonly SettingProperty<Dictionary<StringKey<ForFormattedText>, string>> Translations = new SettingProperty<Dictionary<StringKey<ForFormattedText>, string>>(
            SettingKey.ToSnakeCaseKey(nameof(Translations)),
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
                            var languageFile = SettingsFile.Create(fileInfo.FullName, SettingObject.CreateEmpty(languageFileSchema));
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
            = new PTypeErrorBuilder(new StringKey<ForFormattedText>(nameof(TrimmedStringTypeError)));

        public static readonly TrimmedStringType Instance = new TrimmedStringType();

        private TrimmedStringType() : base(PType.String) { }

        public override string ConvertToBaseValue(string value) => value;

        public override Union<ITypeErrorBuilder, string> TryGetTargetValue(string value)
            => !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : InvalidValue(TrimmedStringTypeError);
    }

    public class TranslationDictionaryType : PType.Derived<Dictionary<string, string>, Dictionary<StringKey<ForFormattedText>, string>>
    {
        public static readonly TranslationDictionaryType Instance = new TranslationDictionaryType();

        private TranslationDictionaryType()
            : base(new PType.ValueMap<string>(PType.String))
        {
        }

        public override Union<ITypeErrorBuilder, Dictionary<StringKey<ForFormattedText>, string>> TryGetTargetValue(Dictionary<string, string> value)
        {
            var dictionary = new Dictionary<StringKey<ForFormattedText>, string>();

            foreach (var kv in value)
            {
                dictionary.Add(new StringKey<ForFormattedText>(kv.Key), kv.Value);
            }

            return dictionary;
        }

        public override Dictionary<string, string> ConvertToBaseValue(Dictionary<StringKey<ForFormattedText>, string> value)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var kv in value)
            {
                dictionary.Add(kv.Key.Key, kv.Value);
            }

            return dictionary;
        }
    }
}
