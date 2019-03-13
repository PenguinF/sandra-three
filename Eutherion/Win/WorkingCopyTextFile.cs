#region License
/*********************************************************************************
 * WorkingCopyTextFile.cs
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

namespace Eutherion.Win
{
    /// <summary>
    /// Combines a <see cref="AutoSaveTextFile{TUpdate}"/> with a <see cref="LiveTextFile"/>
    /// to create a working copy of a text file with local changes which persists across sessions,
    /// and is aware of external updates to the text file on the file system.
    /// </summary>
    public sealed class WorkingCopyTextFile
    {
        /// <summary>
        /// Initializes a new <see cref="WorkingCopyTextFile"/> from an open <see cref="LiveTextFile"/>.
        /// </summary>
        /// <param name="openTextFile">
        /// The open text file.
        /// </param>
        /// <returns>
        /// The new <see cref="WorkingCopyTextFile"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="openTextFile"/> is null.
        /// </exception>
        public static WorkingCopyTextFile OpenExisting(LiveTextFile openTextFile)
        {
            return new WorkingCopyTextFile(openTextFile);
        }

        private WorkingCopyTextFile(LiveTextFile openTextFile)
        {
            OpenTextFile = openTextFile ?? throw new ArgumentNullException(nameof(openTextFile));
        }

        /// <summary>
        /// Gets the opened <see cref="LiveTextFile"/>.
        /// </summary>
        public LiveTextFile OpenTextFile { get; }

        /// <summary>
        /// Gets the loaded file as text in memory. If it could not be loaded, returns string.Empty.
        /// </summary>
        public string LoadedText => OpenTextFile.LoadedText.Match(whenOption1: e => string.Empty, whenOption2: text => text);

        /// <summary>
        /// Returns the <see cref="Exception"/> from an unsuccessful attempt to read the file from the file system.
        /// </summary>
        public Exception LoadException => OpenTextFile.LoadedText.Match(whenOption1: e => e, whenOption2: _ => null);

        /// <summary>
        /// Saves the text to the file.
        /// </summary>
        public void Save(string localCopyText)
        {
            OpenTextFile.Save(localCopyText);
        }
    }
}
