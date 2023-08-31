#region License
/*********************************************************************************
 * SettingsFile.cs
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

using System;
using System.Collections.Generic;
using System.IO;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Reads settings from a file.
    /// </summary>
    public class SettingsFile : LiveTextFile
    {
        /// <summary>
        /// Creates a <see cref="SettingsFile"/> given a valid file path.
        /// The <see cref="LiveTextFile.FileUpdated"/> event is not raised until
        /// the current synchronization context is captured explicitly.
        /// </summary>
        /// <param name="absoluteFilePath">
        /// The absolute file path from which to load the settings file.
        /// If the file does not exist, if access rights are insufficient,
        /// or if the settings file is corrupt, an empty <see cref="SettingsFile"/>
        /// object is returned.
        /// </param>
        /// <param name="templateSettings">
        /// The <see cref="SettingObject"/> in which the initial values are stored.
        /// If the file is loaded, its JSON must match the schema in these template settings.
        /// </param>
        /// <returns>
        /// The created <see cref="SettingsFile"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="absoluteFilePath"/> is empty, contains only whitespace, or contains invalid characters
        /// (see also <seealso cref="Path.GetInvalidPathChars"/>), or is in an invalid format,
        /// or is a relative path and its absolute path could not be resolved.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="absoluteFilePath"/> and/or <paramref name="templateSettings"/> are <see langword="null"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// <paramref name="absoluteFilePath"/> is longer than its maximum length (this is OS specific).
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have sufficient permissions to read the file.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="absoluteFilePath"/> is in an invalid format.
        /// </exception>
        public static SettingsFile Create(string absoluteFilePath, SettingObject templateSettings)
        {
            if (templateSettings == null) throw new ArgumentNullException(nameof(templateSettings));

            var settingsFile = new SettingsFile(absoluteFilePath, templateSettings);
            settingsFile.Settings = settingsFile.ReadSettingObject(settingsFile.LoadedText);
            return settingsFile;
        }

        /// <summary>
        /// Gets the template settings into which the values from the settings file are loaded.
        /// </summary>
        public SettingObject TemplateSettings { get; }

        /// <summary>
        /// Gets the most recent version of the settings.
        /// </summary>
        public SettingObject Settings { get; private set; }

        private SettingsFile(string absoluteFilePath, SettingObject templateSettings)
            : base(absoluteFilePath)
        {
            TemplateSettings = templateSettings;
        }

        private SettingObject ReadSettingObject(Union<Exception, string> fileTextOrException)
        {
            SettingCopy workingCopy = TemplateSettings.CreateWorkingCopy();

            fileTextOrException.Match(
                whenOption1: exception => exception.Trace(),
                whenOption2: fileText => workingCopy.TryLoadFromText(fileText));

            return workingCopy.Commit();
        }

        private readonly Dictionary<StringKey<SettingProperty>, WeakEvent<object, EventArgs>> settingsChangedEvents
            = new Dictionary<StringKey<SettingProperty>, WeakEvent<object, EventArgs>>();

        /// <summary>
        /// Registers a handler for the weak event which occurs after the <see cref="Settings"/> have been updated in the file.
        /// The event handler cannot be an anonymous method.
        /// For best performance, the class in which the event handler method is defined should implement <see cref="IWeakEventTarget"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> and/or <paramref name="eventHandler"/> are <see langword="null"/>.
        /// </exception>
        public void RegisterSettingsChangedHandler(SettingProperty property, Action<object, EventArgs> eventHandler)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (eventHandler == null) throw new ArgumentNullException(nameof(eventHandler));

            WeakEvent<object, EventArgs> keyedEvent = settingsChangedEvents.GetOrAdd(property.Name, key => new WeakEvent<object, EventArgs>());
            keyedEvent.AddListener(eventHandler);
        }

        private IEnumerable<SettingProperty> ChangedProperties(SettingObject newSettings)
        {
            PValueEqualityComparer eq = PValueEqualityComparer.Instance;

            foreach (var property in Settings.Schema.AllProperties)
            {
                // Change if added, updated or deleted.
                if (Settings.TryGetRawValue(property, out PValue oldValue))
                {
                    if (newSettings.TryGetRawValue(property, out PValue newValue))
                    {
                        if (!eq.AreEqual(oldValue, newValue))
                        {
                            // Updated.
                            yield return property;
                        }
                    }
                    else
                    {
                        // Deleted.
                        yield return property;
                    }
                }
                else if (newSettings.TryGetRawValue(property, out _))
                {
                    // Added.
                    yield return property;
                }
            }
        }

        protected override void OnFileUpdated(EventArgs e)
        {
            base.OnFileUpdated(e);

            SettingObject newSettings = ReadSettingObject(LoadedText);

            foreach (var property in ChangedProperties(newSettings))
            {
                // Update settings if at least one property changed.
                Settings = newSettings;

                if (settingsChangedEvents.TryGetValue(property.Name, out WeakEvent<object, EventArgs> keyedEvent))
                {
                    keyedEvent.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Attempts to overwrite the setting file with the current values in <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">
        /// The settings to write.
        /// </param>
        /// <param name="options">
        /// Specifies options for writing the settings.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settings"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="settings"/> has an unexpected schema.
        /// </exception>
        public void WriteToFile(SettingObject settings, SettingWriterOptions options)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            if (settings.Schema != TemplateSettings.Schema)
            {
                throw new ArgumentException(
                    $"{nameof(settings)} has an unexpected schema",
                    nameof(settings));
            }

            Save(SettingWriter.ConvertToJson(settings, options));
        }
    }
}
