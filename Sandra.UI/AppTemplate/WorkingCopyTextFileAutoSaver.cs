#region License
/*********************************************************************************
 * WorkingCopyTextFileAutoSaver.cs
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

using Eutherion.Utils;
using Eutherion.Win.Storage;
using System;
using System.IO;

namespace Eutherion.Win.AppTemplate
{
    public class WorkingCopyTextFileAutoSaver
    {
        /// <summary>
        /// This results in file names such as ".%_A8.tmp".
        /// </summary>
        private static readonly string AutoSavedLocalChangesFileName = ".%.tmp";

        private static FileStream CreateUniqueNewAutoSaveFileStream()
        {
            if (!Session.Current.TryGetAutoSaveValue(SharedSettings.AutoSaveCounter, out uint autoSaveFileCounter))
            {
                autoSaveFileCounter = 1;
            };

            var file = FileUtilities.CreateUniqueFile(
                Path.Combine(Session.Current.AppDataSubFolder, AutoSavedLocalChangesFileName),
                FileOptions.SequentialScan | FileOptions.Asynchronous,
                ref autoSaveFileCounter);

            Session.Current.AutoSave.Persist(SharedSettings.AutoSaveCounter, autoSaveFileCounter);

            return file;
        }

        internal static FileStreamPair OpenAutoSaveFileStreamPair(SettingProperty<AutoSaveFileNamePair> autoSaveProperty)
        {
            try
            {
                if (autoSaveProperty != null && Session.Current.TryGetAutoSaveValue(autoSaveProperty, out AutoSaveFileNamePair autoSaveFileNamePair))
                {
                    var fileStreamPair = FileStreamPair.Create(
                        AutoSaveTextFile.OpenExistingAutoSaveFile,
                        Path.Combine(Session.Current.AppDataSubFolder, autoSaveFileNamePair.FileName1),
                        Path.Combine(Session.Current.AppDataSubFolder, autoSaveFileNamePair.FileName2));

                    if (AutoSaveTextFile.CanAutoSaveTo(fileStreamPair.FileStream1)
                        && AutoSaveTextFile.CanAutoSaveTo(fileStreamPair.FileStream2))
                    {
                        return fileStreamPair;
                    }

                    fileStreamPair.Dispose();
                }
            }
            catch (Exception autoSaveLoadException)
            {
                // Only trace exceptions resulting from e.g. a missing LOCALAPPDATA subfolder or insufficient access.
                autoSaveLoadException.Trace();
            }

            return null;
        }

        private readonly Session ownerSession;
        internal readonly SettingProperty<AutoSaveFileNamePair> autoSaveProperty;

        /// <summary>
        /// Gets the <see cref="FileStreamPair"/> that is currently used for auto-saving.
        /// </summary>
        public FileStreamPair AutoSaveFileStreamPair { get; private set; }

        private readonly WorkingCopyTextFile workingCopyTextFile;

        public WorkingCopyTextFileAutoSaver(
            Session ownerSession,
            SettingProperty<AutoSaveFileNamePair> autoSaveProperty,
            FileStreamPair autoSaveFileStreamPair,
            WorkingCopyTextFile workingCopyTextFile)
        {
            this.ownerSession = ownerSession ?? throw new ArgumentNullException(nameof(ownerSession));
            this.autoSaveProperty = autoSaveProperty ?? throw new ArgumentNullException(nameof(autoSaveProperty));
            AutoSaveFileStreamPair = autoSaveFileStreamPair;
            this.workingCopyTextFile = workingCopyTextFile ?? throw new ArgumentNullException(nameof(workingCopyTextFile));

            workingCopyTextFile.QueryAutoSaveFile += QueryAutoSaveFile;
        }

        private void QueryAutoSaveFile(WorkingCopyTextFile sender, QueryAutoSaveFileEventArgs e)
        {
            // Only open auto-save files if they can be stored in autoSaveSetting.
            FileStreamPair fileStreamPair = null;

            try
            {
                fileStreamPair = FileStreamPair.Create(CreateUniqueNewAutoSaveFileStream, CreateUniqueNewAutoSaveFileStream);
                e.AutoSaveFileStreamPair = fileStreamPair;

                ownerSession.AutoSave.Persist(
                    autoSaveProperty,
                    new AutoSaveFileNamePair(
                        Path.GetFileName(fileStreamPair.FileStream1.Name),
                        Path.GetFileName(fileStreamPair.FileStream2.Name)));
            }
            catch (Exception autoSaveLoadException)
            {
                if (fileStreamPair != null) fileStreamPair.Dispose();

                // Only trace exceptions resulting from e.g. a missing LOCALAPPDATA subfolder or insufficient access.
                autoSaveLoadException.Trace();
            }
        }
    }
}
