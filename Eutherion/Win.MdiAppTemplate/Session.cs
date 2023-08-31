#region License
/*********************************************************************************
 * Session.cs
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Contains all ambient state which is global to a single user session.
    /// This includes e.g. an auto-save file, settings and preferences.
    /// </summary>
    public partial class Session : IDisposable
    {
        // Constants for managing the lock file.
        private const int WindowHandleLengthInBytes = 8;
        private const int MagicLengthInBytes = 16;
        private const int MaxRetryAttemptsForLockFile = 20;
        private const int PauseTimeBeforeLockRetry = 100;

        public static readonly string DefaultSettingsFileName = "DefaultSettings.json";

        public static readonly string LangSettingKey = "lang";

        public static readonly string ExecutableFolder;

        public static readonly string ExecutableFileName;

        public static readonly string ExecutableFileNameWithoutExtension;

        public static readonly FileVersionInfo ExecutableFileVersion;

        /// <summary>
        /// Gets the name of the file which acts as an exclusive lock between different instances
        /// of this process which might race to obtain a reference to the auto-save files.
        /// </summary>
        public static readonly string LockFileName = ".lock";

        /// <summary>
        /// Gets the name of the first auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName1 = ".autosave1";

        /// <summary>
        /// Gets the name of the second auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName2 = ".autosave2";

        static Session()
        {
            // Store executable folder/filename for later use.
            string exePath = Assembly.GetEntryAssembly().Location;

            ExecutableFolder = Path.GetDirectoryName(exePath);
            ExecutableFileName = Path.GetFileName(exePath);
            ExecutableFileNameWithoutExtension = Path.GetFileNameWithoutExtension(exePath);
            ExecutableFileVersion = FileVersionInfo.GetVersionInfo(exePath);
        }

        public static Session Current { get; private set; }

        public static Session Configure(SingleInstanceMainForm singleInstanceMainForm,
                                        ISettingsProvider settingsProvider,
                                        TextFormatter defaultLocalizer,
                                        Dictionary<StringKey<ForFormattedText>, string> defaultLocalizerDictionary,
                                        Icon applicationIcon)
        {
            var session = new Session(singleInstanceMainForm,
                                      settingsProvider,
                                      defaultLocalizer,
                                      defaultLocalizerDictionary,
                                      applicationIcon);

            // Use nullcheck on AutoSave to check if the initialization sequence completed.
            if (session.AutoSave != null)
            {
                Current = session;
            }
            else
            {
                session.Dispose();
            }

            return Current;
        }

        private readonly Dictionary<StringKey<ForFormattedText>, string> defaultLocalizerDictionary;
        private readonly Dictionary<string, FileLocalizer> registeredLocalizers;

        /// <summary>
        /// The lock file to grant access to the auto-save files by at most one instance of this process.
        /// </summary>
        private readonly FileStream lockFile;

        /// <summary>
        /// Contains a generated sequence of 16 bytes which is written to the lock file,
        /// and is different for each new instance of the application.
        /// </summary>
        internal readonly byte[] TodaysMagic;

        private TextFormatter currentLocalizer;

        private Session(SingleInstanceMainForm singleInstanceMainForm,
                        ISettingsProvider settingsProvider,
                        TextFormatter defaultLocalizer,
                        Dictionary<StringKey<ForFormattedText>, string> defaultLocalizerDictionary,
                        Icon applicationIcon)
        {
            if (settingsProvider == null) throw new ArgumentNullException(nameof(settingsProvider));
            if (singleInstanceMainForm == null) throw new ArgumentNullException(nameof(singleInstanceMainForm));

            if (!singleInstanceMainForm.IsHandleCreated
                || singleInstanceMainForm.Disposing
                || singleInstanceMainForm.IsDisposed)
            {
                throw new InvalidOperationException($"{nameof(singleInstanceMainForm)} should have its handle created.");
            }

            // May be null.
            this.defaultLocalizerDictionary = defaultLocalizerDictionary;
            ApplicationIcon = applicationIcon;

            // This depends on ExecutableFileName.
            DeveloperMode = new SettingProperty<bool>(
                SettingKey.ToSnakeCaseKey(nameof(DeveloperMode)),
                PType.CLR.Boolean,
                new SettingComment($"Enables tools which assist with {ExecutableFileNameWithoutExtension} development and debugging."));

            // Attempt to load default settings.
            DefaultSettings = SettingsFile.Create(
                Path.Combine(ExecutableFolder, DefaultSettingsFileName),
                settingsProvider.CreateBuiltIn(this).Commit());

            // Save name of LOCALAPPDATA subfolder for persistent files.
            AppDataSubFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                GetDefaultSetting(SharedSettings.AppDataSubFolderName));

#if DEBUG
            // In debug mode, generate default json configuration files from hard coded settings.
            DeployRuntimeConfigurationFiles();
#endif

            // Scan Languages subdirectory to load localizers.
            var langFolderName = GetDefaultSetting(SharedSettings.LangFolderName);
            registeredLocalizers = Localizers.ScanLocalizers(this, Path.Combine(ExecutableFolder, langFolderName));

            LangSetting = new SettingProperty<FileLocalizer>(
                new StringKey<SettingProperty>(LangSettingKey),
                new PType.KeyedSet<FileLocalizer>(registeredLocalizers));

            // Now attempt to get exclusive write access to the .lock file so it becomes a safe mutex.
            string lockFileName = Path.Combine(AppDataSubFolder, LockFileName);
            FileStreamPair autoSaveFiles = null;

            // Retry a maximum number of times.
            int remainingRetryAttempts = MaxRetryAttemptsForLockFile;
            while (remainingRetryAttempts >= 0)
            {
                try
                {
                    // Create the folder on startup.
                    Directory.CreateDirectory(AppDataSubFolder);

                    // If this call doesn't throw, exclusive access to the mutex file is obtained.
                    // Then this process is the first instance.
                    // Use a buffer size of 24 rather than the default 4096.
                    lockFile = new FileStream(
                        lockFileName,
                        FileMode.OpenOrCreate,
                        FileAccess.Write,
                        FileShare.Read,
                        WindowHandleLengthInBytes + MagicLengthInBytes);

                    try
                    {
                        // Immediately empty the file.
                        lockFile.SetLength(0);

                        // Write the window handle to the lock file.
                        // Use BitConverter to convert to and from byte[].
                        byte[] buffer = BitConverter.GetBytes(singleInstanceMainForm.Handle.ToInt64());
                        lockFile.Write(buffer, 0, WindowHandleLengthInBytes);

                        // Generate a magic GUID for this instance.
                        // The byte array has a length of 16.
                        TodaysMagic = Guid.NewGuid().ToByteArray();
                        lockFile.Write(TodaysMagic, 0, MagicLengthInBytes);
                        lockFile.Flush();

                        autoSaveFiles = OpenAutoSaveFileStreamPair(new AutoSaveFileNamePair(AutoSaveFileName1, AutoSaveFileName2));

                        // Loop exit point 1: successful write to lockFile. Can auto-save.
                        break;
                    }
                    catch
                    {
                        ReleaseLockFile(lockFile);
                        lockFile = null;
                    }
                }
                catch
                {
                    // Not the first instance.
                    // Then try opening the lock file as read-only.
                    try
                    {
                        // If opening as read-only succeeds, read the contents as bytes.
                        FileStream existingLockFile = new FileStream(
                            lockFileName,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.ReadWrite,
                            WindowHandleLengthInBytes + MagicLengthInBytes);

                        using (existingLockFile)
                        {
                            byte[] lockFileBytes = new byte[WindowHandleLengthInBytes + MagicLengthInBytes];
                            int totalBytesRead = 0;
                            int remainingBytes = WindowHandleLengthInBytes + MagicLengthInBytes;
                            while (remainingBytes > 0)
                            {
                                int bytesRead = existingLockFile.Read(lockFileBytes, totalBytesRead, remainingBytes);
                                if (bytesRead == 0) break; // Unexpected EOF?
                                totalBytesRead += bytesRead;
                                remainingBytes -= bytesRead;
                            }

                            // For checking that EOF has been reached, evaluate ReadByte() == -1.
                            if (remainingBytes == 0 && existingLockFile.ReadByte() == -1)
                            {
                                // Parse out the remote window handle and the magic bytes it is expecting.
                                long longValue = BitConverter.ToInt64(lockFileBytes, 0);
                                HandleRef remoteWindowHandle = new HandleRef(null, new IntPtr(longValue));

                                byte[] remoteExpectedMagic = new byte[MagicLengthInBytes];
                                Array.Copy(lockFileBytes, 8, remoteExpectedMagic, 0, MagicLengthInBytes);

                                // Tell the singleInstanceMainForm that another instance is active.
                                // Not a clean design to have callbacks going back and forth.
                                // Hard to refactor since we're inside a loop.
                                singleInstanceMainForm.NotifyExistingInstance(remoteWindowHandle, remoteExpectedMagic);

                                // Reference remoteWindowHandle here so it won't be GC'ed until method returns.
                                GC.KeepAlive(remoteWindowHandle);

                                // Loop exit point 2: successful read of the lock file owned by an existing instance.
                                return;
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                // If any of the above steps fail, this might be caused by the other instance
                // shutting down for example, or still being in its startup phase.
                // In this case, sleep for 100 ms and retry the whole process.
                Thread.Sleep(PauseTimeBeforeLockRetry);
                remainingRetryAttempts--;

                // Loop exit point 3: proceed without auto-saving settings if even after 2 seconds the lock file couldn't be accessed.
                // This can happen for example if the first instance is running as Administrator and this instance is not.
            }

            try
            {
                AutoSave = new SettingsAutoSave(settingsProvider.CreateAutoSaveSchema(this), autoSaveFiles);

                // After creating the auto-save file, look for a local preferences file.
                // Create a working copy with correct schema first.
                SettingCopy localSettingsCopy = new SettingCopy(settingsProvider.CreateLocalSettingsSchema(this));

                // And then create the local settings file which can overwrite values in default settings.
                LocalSettings = SettingsFile.Create(
                    Path.Combine(AppDataSubFolder, GetDefaultSetting(SharedSettings.LocalPreferencesFileName)),
                    localSettingsCopy.Commit());

                if (TryGetAutoSaveValue(LangSetting, out FileLocalizer localizer))
                {
                    currentLocalizer = localizer;
                }
                else
                {
                    // Select best fit.
                    currentLocalizer = Localizers.BestFit(registeredLocalizers);
                }

                // Fall back onto defaults if still null.
                currentLocalizer = currentLocalizer ?? defaultLocalizer ?? TextFormatter.Default;
            }
            catch
            {
                // Must dispose here, because Dispose() is never called if an exception is thrown in a constructor.
                ReleaseLockFile(lockFile);
                throw;
            }
        }

        public Icon ApplicationIcon { get; }

        public SettingProperty<bool> DeveloperMode { get; }

        public string AppDataSubFolder { get; }

        public SettingsFile DefaultSettings { get; }

        public SettingsFile LocalSettings { get; }

        public SettingsAutoSave AutoSave { get; }

        public SettingProperty<FileLocalizer> LangSetting { get; }

        public IEnumerable<FileLocalizer> RegisteredLocalizers => registeredLocalizers.Select(kv => kv.Value);

        private string LocalApplicationDataPath(bool isLocalSchema)
            => !isLocalSchema ? string.Empty :
            $" ({AppDataSubFolder})";

        public SettingComment DefaultSettingsSchemaDescription(bool isLocalSchema) => new SettingComment(
            "There are generally two copies of this file, one in the directory where "
            + ExecutableFileName
            + " is located ("
            + DefaultSettingsFileName
            + "), and one that lives in the local application data folder"
            + LocalApplicationDataPath(isLocalSchema)
            + ".",
            "Preferences in the latter file override those that are specified in the default. "
            + "In the majority of cases, only the latter file is changed, while the default "
            + "settings serve as a template.");

        public TValue GetDefaultSetting<TValue>(SettingProperty<TValue> property)
            => DefaultSettings.Settings.GetValue(property);

        public TValue GetSetting<TValue>(SettingProperty<TValue> property)
        {
            return LocalSettings.Settings.TryGetValue(property, out TValue result)
                ? result
                : GetDefaultSetting(property);
        }

        public bool TryGetAutoSaveValue<TValue>(SettingProperty<TValue> property, out TValue value)
            => AutoSave.CurrentSettings.TryGetValue(property, out value);

        private FileStreamPair OpenAutoSaveFileStreamPair(AutoSaveFileNamePair autoSaveFileNamePair)
        {
            try
            {
                var fileStreamPair = FileStreamPair.Create(
                    AutoSaveTextFile.OpenExistingAutoSaveFile,
                    Path.Combine(AppDataSubFolder, autoSaveFileNamePair.FileName1),
                    Path.Combine(AppDataSubFolder, autoSaveFileNamePair.FileName2));

                if (AutoSaveTextFile.CanAutoSaveTo(fileStreamPair.FileStream1)
                    && AutoSaveTextFile.CanAutoSaveTo(fileStreamPair.FileStream2))
                {
                    return fileStreamPair;
                }

                fileStreamPair.Dispose();
            }
            catch (Exception autoSaveLoadException)
            {
                // Only trace exceptions resulting from e.g. a missing LOCALAPPDATA subfolder or insufficient access.
                autoSaveLoadException.Trace();
            }

            return null;
        }

        public FileStreamPair OpenAutoSaveFileStreamPair(SettingProperty<AutoSaveFileNamePair> autoSaveProperty)
        {
            if (autoSaveProperty != null && TryGetAutoSaveValue(autoSaveProperty, out AutoSaveFileNamePair autoSaveFileNamePair))
            {
                return OpenAutoSaveFileStreamPair(autoSaveFileNamePair);
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the current <see cref="TextFormatter"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// The provided new value for <see cref="CurrentLocalizer"/> is null.
        /// </exception>
        public TextFormatter CurrentLocalizer
        {
            get => currentLocalizer;
            set
            {
                if (currentLocalizer != value)
                {
                    currentLocalizer = value ?? throw new ArgumentNullException(nameof(value));
                    event_CurrentLocalizerChanged.Raise(null, EventArgs.Empty);
                }
            }
        }

        private readonly WeakEvent<object, EventArgs> event_CurrentLocalizerChanged = new WeakEvent<object, EventArgs>();

        /// <summary>
        /// Occurs when the value of <see cref="CurrentLocalizer"/> is updated.
        /// </summary>
        public event Action<object, EventArgs> CurrentLocalizerChanged
        {
            add => event_CurrentLocalizerChanged.AddListener(value);
            remove => event_CurrentLocalizerChanged.RemoveListener(value);
        }

        /// <summary>
        /// Notifies listeners that the translations of the given localizer were updated.
        /// </summary>
        public void NotifyCurrentLocalizerChanged()
        {
            event_CurrentLocalizerChanged.Raise(null, EventArgs.Empty);
        }

        private static void ReleaseLockFile(FileStream lockFile)
        {
            if (lockFile != null)
            {
                // Clear the lock file, release the lock on it, and attempt to delete it.
                lockFile.SetLength(0);
                lockFile.Dispose();

                try
                {
                    File.Delete(lockFile.Name);
                }
                catch
                {
                    // Just ignore any exception thrown from File.Delete.
                }
            }
        }

        public void Dispose()
        {
            // Wait until the auto-save background task has finished.
            AutoSave?.Close();

            // Only after AutoSave has completed its work can the lock file be closed.
            ReleaseLockFile(lockFile);

            // Stop watching settings files.
            LocalSettings?.Dispose();
            DefaultSettings.Dispose();
            registeredLocalizers.Values.ForEach(x => x.Dispose());
        }

#if DEBUG
        /// <summary>
        /// Generates DefaultSettings.json from the loaded default settings in memory,
        /// and generates Bin/Languages/en.json from the BuiltInEnglishLocalizer.
        /// </summary>
        private void DeployRuntimeConfigurationFiles()
        {
            // No exception handler for both WriteToFiles.
            DefaultSettings.WriteToFile(
                DefaultSettings.TemplateSettings,
                SettingWriterOptions.Default);

            using (SettingsFile englishFileFromBuiltIn = SettingsFile.Create(
                Path.Combine(ExecutableFolder, "Languages", "en.json"),
                new SettingCopy(Localizers.CreateLanguageFileSchema()).Commit()))
            {
                var settingCopy = new SettingCopy(englishFileFromBuiltIn.TemplateSettings.Schema);
                settingCopy.AddOrReplace(Localizers.NativeName, "English");
                settingCopy.AddOrReplace(Localizers.FlagIconFile, "flag-uk.png");
                settingCopy.AddOrReplace(Localizers.Translations, defaultLocalizerDictionary);

                englishFileFromBuiltIn.WriteToFile(
                    settingCopy.Commit(),
                    SettingWriterOptions.SuppressSettingComments);
            }

            // Generate current version number in README.txt.
            string readMeFilePath = Path.Combine(ExecutableFolder, "README.txt");
            string readMe = File.ReadAllText(readMeFilePath);
            int firstNewLine = readMe.IndexOf('\n');
            int secondNewLine = readMe.IndexOf('\n', firstNewLine + 1);

            // Leave out private build if zero.
            string displayFileVersion = ExecutableFileVersion.FilePrivatePart == 0
                ? $"{ExecutableFileVersion.FileMajorPart}.{ExecutableFileVersion.FileMinorPart}.{ExecutableFileVersion.FileBuildPart}"
                : ExecutableFileVersion.FileVersion;
            string heading = $"\r\n*** {ExecutableFileNameWithoutExtension} v{displayFileVersion} ***\r\n";
            File.WriteAllText(readMeFilePath, heading + readMe.Substring(secondNewLine + 1));
        }
#endif
    }

    public interface ISettingsProvider
    {
        /// <summary>
        /// Gets the built-in default settings. Its schema is used for the default settings file.
        /// </summary>
        SettingCopy CreateBuiltIn(Session session);

        /// <summary>
        /// Gets the schema to use for the local preferences file.
        /// </summary>
        SettingSchema CreateLocalSettingsSchema(Session session);

        /// <summary>
        /// Gets the schema to use for the auto-save file.
        /// </summary>
        SettingSchema CreateAutoSaveSchema(Session session);
    }
}
