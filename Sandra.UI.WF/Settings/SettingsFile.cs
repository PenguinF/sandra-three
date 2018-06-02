/*********************************************************************************
 * SettingsFile.cs
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
using SysExtensions;
using System;
using System.IO;
using System.Security;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Reads settings from a file.
    /// </summary>
    public class SettingsFile
    {
        /// <summary>
        /// Creates a <see cref="SettingsFile"/> given a valid file path.
        /// </summary>
        /// <param name="absoluteFilePath">
        /// The absolute file path from which to load the settings file.
        /// If the file does not exist, if access rights are insufficient,
        /// or if the settings file is corrupt, an empty <see cref="SettingsFile"/>
        /// object is returned.
        /// </param>
        /// <param name="workingCopy">
        /// The optional <see cref="SettingCopy"/> to load the settings into.
        /// </param>
        /// <returns>
        /// The created <see cref="SettingsFile"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="absoluteFilePath"/> is a zero-length string, contains only white space,
        /// or contains one or more invalid characters as defined by <see cref="Path.InvalidPathChars"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="absoluteFilePath"/> is null.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="absoluteFilePath"/> is in an invalid format.
        /// </exception>
        public static SettingsFile Create(string absoluteFilePath, SettingCopy workingCopy)
        {
            if (workingCopy == null) workingCopy = new SettingCopy();

            string fileText = null;
            try
            {
                fileText = File.ReadAllText(absoluteFilePath);
            }
            catch (Exception exception)
            {
                if (exception is IOException ||
                    exception is UnauthorizedAccessException ||
                    exception is FileNotFoundException ||
                    exception is DirectoryNotFoundException ||
                    exception is SecurityException)
                {
                    // 'Expected' exceptions can be traced.
                    exception.Trace();
                }
                else
                {
                    // Other exceptions are developer errors.
                    throw;
                }
            }

            if (fileText != null)
            {
                workingCopy.LoadFromText(new StringReader(fileText));
            }

            return new SettingsFile(absoluteFilePath, workingCopy.Commit());
        }

        private readonly string absoluteFilePath;

        public SettingObject Settings { get; }

        private SettingsFile(string absoluteFilePath, SettingObject settings)
        {
            this.absoluteFilePath = absoluteFilePath;
            Settings = settings;
        }
    }
}
