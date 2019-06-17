#region License
/*********************************************************************************
 * FileUtilities.cs
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

using System;
using System.IO;

namespace Eutherion.Utils
{
    /// <summary>
    /// Contains utility methods for files and related classes.
    /// </summary>
    public static class FileUtilities
    {
        /// <summary>
        /// Documented default value of the 'bufferSize' parameter of the <see cref="FileStream"/> constructor.
        /// </summary>
        public const int DefaultFileStreamBufferSize = 4096;

        /// <summary>
        /// Returns a normalized absolute path for the specified file path string.
        /// Do not use this for directories.
        /// </summary>
        /// <param name="path">
        /// The path to normalize.
        /// </param>
        /// <returns>
        /// The normalized path.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is empty, contains only whitespace, or contains invalid characters
        /// (see also <seealso cref="Path.GetInvalidPathChars"/>), or is in an invalid format,
        /// or is a relative path and its absolute path could not be resolved.
        /// </exception>
        /// <exception cref="IOException">
        /// <paramref name="path"/> is longer than its maximum length (this is OS specific).
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have sufficient permissions.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="path"/> is in an invalid format.
        /// </exception>
        public static string NormalizeFilePath(string path)
        {
            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Creates a new file with a guaranteed unique name and opens it for reading and writing.
        /// The generated unique file name is in the format: {file name}_{hexadecimal counter}.{extension}
        /// Only this process can write to the returned <see cref="FileStream"/>, other processes can still read from it.
        /// When the application exits or crashes, the returned <see cref="FileStream"/> is closed automatically.
        /// </summary>
        /// <param name="path">
        /// The path of the file to create.
        /// If the file already exists, an underscore character followed by a hexadecimal value is appended
        /// at the end of the path.
        /// Check <see cref="FileStream.Name"/> for the name of the file that was actually created.
        /// If its directory does not exist, it is created.
        /// </param>
        /// <param name="fileOptions">
        /// Specifies custom file options.
        /// </param>
        /// <param name="counter">
        /// The (initial) integer value to use for generating a candidate file name. Its hexadecimal
        /// value is appended to <paramref name="path"/>.
        /// Only values higher than 0 are appended, so for value 0 the file name is unchanged.
        /// </param>
        /// <returns>
        /// The created <see cref="FileStream"/> that can be used for reading and writing.
        /// Check its <see cref="FileStream.Name"/> property for the name of the file that was actually created.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is empty, contains only whitespace, or contains invalid characters
        /// (see also <seealso cref="Path.GetInvalidPathChars"/>), or is in an invalid format,
        /// or is a relative path and its absolute path could not be resolved.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="fileOptions"/> has an invalid value.
        /// </exception>
        /// <exception cref="IOException">
        /// Part of the path which is expected to be a directory is actually a file,
        /// -or- The path contains a network name which cannot be resolved,
        /// -or- <paramref name="path"/> is longer than its maximum length (this is OS specific).
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have sufficient permissions to create the file or its directory.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have sufficient permissions to create the file or its directory.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// <paramref name="path"/> is invalid.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="path"/> is in an invalid format.
        /// </exception>
        public static FileStream CreateUniqueFile(string path, FileOptions fileOptions, ref uint counter)
        {
            // Normalize path.
            path = Path.GetFullPath(path);

            // Create the directory first if it does not exist.
            string directory = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            // This counter is appended to the file name to ensure its uniqueness.
            while (true)
            {
                string attemptPath
                    = counter == 0 ? path
                    : Path.Combine(directory, $"{fileNameWithoutExtension}_{counter.ToString("X")}{extension}");

                try
                {
                    // Always increment counter so the next call to CreateUniqueFile() is more likely to succeed the first time.
                    counter++;

                    // If FileMode.CreateNew and this succeeds, the file name was unique.
                    return new FileStream(attemptPath,
                                          FileMode.CreateNew,
                                          FileAccess.ReadWrite,
                                          FileShare.Read,
                                          DefaultFileStreamBufferSize,
                                          fileOptions);
                }
                catch (IOException)
                {
                    // File already exists and is required to have a unique name,
                    // retry with the incremented counter.
                    // Also recreate the directory if it was deleted in the meantime,
                    // or else we'll be in an infinite loop.
                    Directory.CreateDirectory(directory);
                }
            }
        }
    }
}
