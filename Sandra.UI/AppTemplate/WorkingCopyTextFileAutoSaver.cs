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

using Eutherion.Win.Storage;
using System;
using System.IO;

namespace Eutherion.Win.AppTemplate
{
    public class WorkingCopyTextFileAutoSaver
    {
        internal static FileStreamPair OpenAutoSaveFileStreamPair(SettingProperty<AutoSaveFileNamePair> autoSaveProperty)
        {
            if (autoSaveProperty != null && Session.Current.TryGetAutoSaveValue(autoSaveProperty, out AutoSaveFileNamePair autoSaveFileNamePair))
            {
                return FileStreamPair.Create(
                    AutoSaveTextFile.OpenExistingAutoSaveFile,
                    Path.Combine(Session.Current.AppDataSubFolder, autoSaveFileNamePair.FileName1),
                    Path.Combine(Session.Current.AppDataSubFolder, autoSaveFileNamePair.FileName2));
            }

            return null;
        }

        /// <summary>
        /// Setting to use when an auto-save file name pair is generated.
        /// </summary>
        internal readonly SettingProperty<AutoSaveFileNamePair> autoSaveProperty;

        public WorkingCopyTextFileAutoSaver(SettingProperty<AutoSaveFileNamePair> autoSaveProperty)
        {
            this.autoSaveProperty = autoSaveProperty ?? throw new ArgumentNullException(nameof(autoSaveProperty));
        }
    }
}
