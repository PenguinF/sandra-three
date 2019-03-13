#region License
/*********************************************************************************
 * FileStreamPair.cs
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

namespace Eutherion.Win
{
    /// <summary>
    /// Holds a pair of <see cref="FileStream"/> objects.
    /// </summary>
    public class FileStreamPair : IDisposable
    {
        /// <summary>
        /// The primary <see cref="FileStream"/>.
        /// </summary>
        public FileStream FileStream1 { get; }

        /// <summary>
        /// The secondary <see cref="FileStream"/>.
        /// </summary>
        public FileStream FileStream2 { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="FileStreamPair"/> from a pair of <see cref="FileStream"/> objects.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fileStream1"/> and/or <paramref name="fileStream2"/> are null.
        /// </exception>
        public FileStreamPair(FileStream fileStream1, FileStream fileStream2)
        {
            FileStream1 = fileStream1 ?? throw new ArgumentNullException(nameof(fileStream1));
            FileStream2 = fileStream2 ?? throw new ArgumentNullException(nameof(fileStream2));
        }

        /// <summary>
        /// Returns either <see cref="FileStream1"/> or <see cref="FileStream2"/>, guaranteeing
        /// that the returned <see cref="FileStream"/> is different from <paramref name="fileStream"/>.
        /// </summary>
        /// <param name="fileStream">
        /// The <see cref="FileStream"/> not to return.
        /// </param>
        /// <returns>
        /// Either <see cref="FileStream1"/> or <see cref="FileStream2"/>, different from <paramref name="fileStream"/>.
        /// </returns>
        public FileStream Different(FileStream fileStream)
            => fileStream == FileStream1 ? FileStream2 : FileStream1;

        /// <summary>
        /// Disposes of both <see cref="FileStream"/>.
        /// </summary>
        public void Dispose()
        {
            // Dispose in opposite order of opening the files,
            // so that inner files can only be locked if outer files are locked too.
            FileStream2.Dispose();
            FileStream1.Dispose();
        }
    }
}
