#region License
/*********************************************************************************
 * FileStreamPair.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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
        /// Creates a <see cref="FileStreamPair"/> from a pair of <see cref="FileStream"/> constructors
        /// and ensures that if the second constructor fails, the <see cref="FileStream"/> returned
        /// by the first constructor is properly closed.
        /// </summary>
        /// <param name="constructor1">
        /// The first <see cref="FileStream"/> constructor to use.
        /// </param>
        /// <param name="constructor2">
        /// The second <see cref="FileStream"/> constructor to use.
        /// </param>
        /// <returns>
        /// An initialized <see cref="FileStreamPair"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="constructor1"/> and/or <paramref name="constructor2"/> are null.
        /// </exception>
        public static FileStreamPair Create(Func<FileStream> constructor1, Func<FileStream> constructor2)
        {
            if (constructor1 == null) throw new ArgumentNullException(nameof(constructor1));
            if (constructor2 == null) throw new ArgumentNullException(nameof(constructor2));

            FileStream fileStream1 = constructor1();
            try
            {
                return new FileStreamPair(fileStream1, constructor2());
            }
            catch
            {
                // Who knows, someone may at some point return null from constructor1.
                // FileStreamPair constructor then throws but need to not turn that
                // into a NullReferenceException over here.
                fileStream1?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a <see cref="FileStreamPair"/> from a parametrized <see cref="FileStream"/> constructor
        /// and ensures that if creation of the second <see cref="FileStream"/> fails, the first <see cref="FileStream"/>
        /// is properly closed.
        /// </summary>
        /// <typeparam name="T">
        /// The type of parameter of the <paramref name="constructor"/>.
        /// </typeparam>
        /// <param name="constructor">
        /// The <see cref="FileStream"/> constructor to use.
        /// </param>
        /// <param name="parameter1">
        /// The value to pass to the <paramref name="constructor"/> to create the first <see cref="FileStream"/>.
        /// </param>
        /// <param name="parameter2">
        /// The value to pass to the <paramref name="constructor"/> to create the second <see cref="FileStream"/>.
        /// </param>
        /// <returns>
        /// An initialized <see cref="FileStreamPair"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="constructor"/> is null.
        /// </exception>
        public static FileStreamPair Create<T>(Func<T, FileStream> constructor, T parameter1, T parameter2)
        {
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));

            FileStream fileStream1 = constructor(parameter1);
            try
            {
                return new FileStreamPair(fileStream1, constructor(parameter2));
            }
            catch
            {
                // See remark at other Create() overload.
                fileStream1?.Dispose();
                throw;
            }
        }

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
