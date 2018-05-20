﻿/*********************************************************************************
 * AutoSave.cs
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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Manages an auto-save file local to every non-roaming user.
    /// This class is assumed to have a lifetime equal to the application.
    /// See also: <seealso cref="Environment.SpecialFolder.LocalApplicationData"/>
    /// </summary>
    public sealed class AutoSave
    {
        // These values seem to be recommended.
        private const int CharBufferSize = 1024;
        private const int FileStreamBufferSize = 4096;

        /// <summary>
        /// Gets the name of the auto save file.
        /// </summary>
        public static readonly string AutoSaveFileName = ".autosave";

        private readonly FileStream autoSaveFileStream;
        private readonly Encoding encoding;
        private readonly Encoder encoder;

        /// <summary>
        /// Serves as input for the encoder.
        /// </summary>
        private readonly char[] buffer;

        /// <summary>
        /// Serves as output for the encoder.
        /// </summary>
        private readonly byte[] encodedBuffer;

        /// <summary>
        /// Initializes a new instance of <see cref="AutoSave"/>.
        /// </summary>
        /// <param name="appSubFolderName">
        /// The name of the subfolder to use in <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appSubFolderName"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="appSubFolderName"/> is <see cref="string.Empty"/>,
        /// or contains one or more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="appSubFolderName"/> contains a colon character (:) that is not part of a drive label ("C:\").
        /// </exception>
        public AutoSave(string appSubFolderName)
        {
            // Have to check for string.Empty because Path.Combine will not.
            if (appSubFolderName == null)
            {
                throw new ArgumentNullException(nameof(appSubFolderName));
            }

            if (appSubFolderName.Length == 0)
            {
                throw new ArgumentException($"{nameof(appSubFolderName)} is string.Empty.", nameof(appSubFolderName));
            }

            // If creation of the auto-save file fails, because e.g. an instance is already running,
            // don't throw but just disable auto-saving and use default initial settings.
            try
            {
                var localApplicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var baseDir = Directory.CreateDirectory(Path.Combine(localApplicationFolder, appSubFolderName));

                // Create fileStream in such a way that:
                // a) Create if it doesn't exist, open if it already exists.
                // b) Only this process can access it. Protects the folder from deletion as well.
                // It gets automatically closed when the application exits, i.e. no need for IDisposable.
                autoSaveFileStream = new FileStream(Path.Combine(baseDir.FullName, AutoSaveFileName),
                                                    FileMode.OpenOrCreate,
                                                    FileAccess.ReadWrite,
                                                    FileShare.Read,
                                                    FileStreamBufferSize,
                                                    FileOptions.Asynchronous);

                // Assert capabilities of the file stream.
                Debug.Assert(autoSaveFileStream.CanSeek
                    && autoSaveFileStream.CanRead
                    && autoSaveFileStream.CanWrite
                    && !autoSaveFileStream.CanTimeout);

                // Initialize encoders and buffers.
                encoding = new UTF8Encoding();
                encoder = encoding.GetEncoder();
                buffer = new char[CharBufferSize];
                encodedBuffer = new byte[encoding.GetMaxByteCount(CharBufferSize)];
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception initAutoSaveException)
            {
                // Throw exceptions caused by dev errors.
                // Trace the rest. (IOException, PlatformNotSupportedException, UnauthorizedAccessException, ...)
                initAutoSaveException.Trace();
            }
        }

        /// <summary>
        /// Creates and returns an update operation for the auto-save file.
        /// </summary>
        public SettingUpdateOperation CreateUpdate()
        {
            return new SettingUpdateOperation(this);
        }

        internal void Persist(SettingUpdateOperation settingUpdateOperation)
        {
        }
    }
}
