#region License
/*********************************************************************************
 * WorkingCopyTextFileTests.cs
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
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Eutherion.Win.Tests
{
    public enum TargetFile
    {
        /// <summary>
        /// The primary target text file.
        /// </summary>
        PrimaryTextFile,

        /// <summary>
        /// The primary auto-save file.
        /// </summary>
        AutoSaveFile1,

        /// <summary>
        /// The secondary auto-save file.
        /// </summary>
        AutoSaveFile2,
    }

    public enum FileState
    {
        /// <summary>
        /// The file does not exist.
        /// </summary>
        DoesNotExist,

        /// <summary>
        /// The file is locked and cannot be read from.
        /// </summary>
        LockedByAnotherProcess,

        /// <summary>
        /// The file is a directory.
        /// </summary>
        IsDirectory,

        /// <summary>
        /// The file exists and is readable.
        /// </summary>
        Text,
    }

    public class FileFixture : IDisposable
    {
        private class TargetFileState
        {
            public readonly string FilePath;
            public FileState CurrentFileState;
            public FileStream LockedFileStream;

            public TargetFileState(string fileName)
            {
                // This prepends the working directory, which is the location of the compiled unit test dll.
                FilePath = Path.GetFullPath(fileName);
            }
        }

        private readonly TargetFileState textFileState;
        private readonly TargetFileState autoSaveFileState1;
        private readonly TargetFileState autoSaveFileState2;

        public FileFixture()
        {
            textFileState = new TargetFileState("test.txt");
            autoSaveFileState1 = new TargetFileState("autosave1.txt");
            autoSaveFileState2 = new TargetFileState("autosave2.txt");
        }

        private TargetFileState GetTargetFileState(TargetFile targetFile)
        {
            switch (targetFile)
            {
                default:
                case TargetFile.PrimaryTextFile:
                    return textFileState;
                case TargetFile.AutoSaveFile1:
                    return autoSaveFileState1;
                case TargetFile.AutoSaveFile2:
                    return autoSaveFileState2;
            }
        }

        public string GetPath(TargetFile targetFile) => GetTargetFileState(targetFile).FilePath;

        public void PrepareTargetFile(TargetFile targetFile, FileState newFileState)
        {
            var targetFileState = GetTargetFileState(targetFile);

            if (targetFileState.CurrentFileState != newFileState)
            {
                if (targetFileState.CurrentFileState == FileState.LockedByAnotherProcess)
                {
                    targetFileState.LockedFileStream.Dispose();
                    targetFileState.LockedFileStream = null;
                }
                else if (targetFileState.CurrentFileState == FileState.IsDirectory)
                {
                    Directory.Delete(targetFileState.FilePath);
                }

                targetFileState.CurrentFileState = newFileState;

                if (newFileState == FileState.DoesNotExist)
                {
                    File.Delete(targetFileState.FilePath);
                }
                else if (newFileState == FileState.LockedByAnotherProcess)
                {
                    targetFileState.LockedFileStream = new FileStream(
                        targetFileState.FilePath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None);
                }
                else if (newFileState == FileState.IsDirectory)
                {
                    File.Delete(targetFileState.FilePath);
                    Directory.CreateDirectory(targetFileState.FilePath);
                }
            }
        }

        public void PrepareTargetFile(TargetFile targetFile, string text)
        {
            PrepareTargetFile(targetFile, FileState.Text);
            File.WriteAllText(GetPath(targetFile), text);
        }

        public void PrepareTargetFile(TargetFile targetFile, byte[] fileBytes)
        {
            PrepareTargetFile(targetFile, FileState.Text);
            File.WriteAllBytes(GetPath(targetFile), fileBytes);
        }

        public void Dispose()
        {
            // This removes locks and deletes the files.
            PrepareTargetFile(TargetFile.PrimaryTextFile, FileState.DoesNotExist);
            PrepareTargetFile(TargetFile.AutoSaveFile1, FileState.DoesNotExist);
            PrepareTargetFile(TargetFile.AutoSaveFile2, FileState.DoesNotExist);
        }
    }

    public class WorkingCopyTextFileTests : IClassFixture<FileFixture>
    {
        private readonly FileFixture fileFixture;

        public WorkingCopyTextFileTests(FileFixture fileFixture)
        {
            this.fileFixture = fileFixture;
        }

        private FileStreamPair AutoSaveFiles() => FileStreamPair.Create(
            AutoSaveTextFile.OpenExistingAutoSaveFile,
            fileFixture.GetPath(TargetFile.AutoSaveFile1),
            fileFixture.GetPath(TargetFile.AutoSaveFile2));

        private void AssertLiveTextFileSuccessfulLoad(string loadedText, WorkingCopyTextFile wcFile)
        {
            Assert.Null(wcFile.AutoSaveFile);
            Assert.Equal(loadedText, wcFile.LoadedText);
            Assert.Null(wcFile.LoadException);
            Assert.Equal(loadedText, wcFile.LocalCopyText);
        }

        [Fact]
        public void LiveTextFilePathUnchanged()
        {
            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            using (var textFile = new LiveTextFile(filePath))
            {
                Assert.Equal(filePath, textFile.AbsoluteFilePath);
            }
        }

        [Fact]
        public void IllegalFileNames()
        {
            // Shouldn't really test what kind of exception System.IO classes throw,
            // but since they're documented in summaries it's nice to have an overview.
            // Main thing to be tested here is that creating the LiveTextFile does throw and not pretend to be OK.
            Assert.Throws<ArgumentNullException>(() => new LiveTextFile(null).Dispose());
            Assert.Throws<ArgumentException>(() => new LiveTextFile(string.Empty).Dispose());
            Assert.Throws<ArgumentException>(() => new LiveTextFile("*.?").Dispose());
            Assert.Throws<ArgumentException>(() => new LiveTextFile(new string(' ', 261)).Dispose());
            Assert.Throws<PathTooLongException>(() => new LiveTextFile(new string('x', 261)).Dispose());
        }

        public static IEnumerable<object[]> Texts()
        {
            yield return new object[] { "" };
            yield return new object[] { "0" };

            // Null character only.
            yield return new object[] { "\u0000" };

            // Whitespace and newlines.
            yield return new object[] { "\n\r\n\n" };
            yield return new object[] { "\t\v\u000c\u0085\u1680\u3000" };

            // Higher code points, see also JsonTokenizerTests.
            yield return new object[] { "もの" };
        }

        // Regular existing files. Make sure encoding or newline convention is not updated as a result of a load.
        [Theory]
        [MemberData(nameof(Texts))]
        public void ExistingFileInitialState(string text)
        {
            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, text);

            using (var textFile = new LiveTextFile(filePath))
            using (var wcFile = WorkingCopyTextFile.OpenExisting(textFile))
            {
                Assert.Same(textFile, wcFile.OpenTextFile);
                Assert.Equal(filePath, wcFile.OpenTextFilePath);

                AssertLiveTextFileSuccessfulLoad(text, wcFile);
            }
        }

        [Theory]
        // "Could not find file '...\test.txt'."
        [InlineData(FileState.DoesNotExist, typeof(FileNotFoundException))]
        // "The process cannot access the file '...\test.txt' because it is being used by another process."
        [InlineData(FileState.LockedByAnotherProcess, typeof(IOException))]
        // "Access to the path '...' is denied."
        [InlineData(FileState.IsDirectory, typeof(UnauthorizedAccessException))]
        public void InaccessibleFileInitialState(FileState fileState, Type exceptionType)
        {
            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, fileState);

            using (var textFile = new LiveTextFile(filePath))
            using (var wcFile = WorkingCopyTextFile.OpenExisting(textFile))
            {
                Assert.Same(textFile, wcFile.OpenTextFile);
                Assert.Equal(filePath, wcFile.OpenTextFilePath);
                Assert.Null(wcFile.AutoSaveFile);

                Assert.Equal(string.Empty, wcFile.LoadedText);
                Assert.IsType(exceptionType, wcFile.LoadException);
                Assert.Equal(string.Empty, wcFile.LocalCopyText);
            }
        }

        [Theory]
        // Wrong lengths, incomplete auto-saves, with and without UTF8 byte order marker.
        [InlineData(new byte[] { })]
        [InlineData(new byte[] { 0x30 })]
        [InlineData(new byte[] { 0x30, 0x0a })]
        [InlineData(new byte[] { 0x31, 0x0a })]
        [InlineData(new byte[] { 0x33, 0x0a, 0x0a, 0x0a, 0x0a, 0x0a })]
        [InlineData(new byte[] { 0xef })]
        [InlineData(new byte[] { 0xef, 0xbb })]
        [InlineData(new byte[] { 0xef, 0xbb, 0xbf })]
        [InlineData(new byte[] { 0xef, 0xbb, 0xbf, 0x30 })]
        [InlineData(new byte[] { 0xef, 0xbb, 0xbf, 0x30, 0x0a })]
        [InlineData(new byte[] { 0xef, 0xbb, 0xbf, 0x31, 0x0a })]
        [InlineData(new byte[] { 0xef, 0xbb, 0xbf, 0x33, 0x0a, 0x0a, 0x0a, 0x0a, 0x0a })]
        // Do not accept UTF-16.
        [InlineData(new byte[] { 0xff, 0xfe, 0x31, 0x00, 0x0a, 0x00, 0x42, 0x00 })]
        [InlineData(new byte[] { 0xfe, 0xff, 0x00, 0x31, 0x00, 0x0a, 0x00, 0x42 })]
        //http://www.cl.cam.ac.uk/~mgk25/ucs/examples/UTF-8-test.txt
        // 0xe0, 0x80, 0xaf is replaced by 0xfd, 0xfd which has length 2 (0x32).
        [InlineData(new byte[] { 0x31, 0x0a, 0xe0, 0x80, 0xaf })]
        [InlineData(new byte[] { 0x33, 0x0a, 0xe0, 0x80, 0xaf })]
        [InlineData(new byte[] { 0xef, 0xbb, 0xbf, 0x31, 0x0a, 0xe0, 0x80, 0xaf })]
        [InlineData(new byte[] { 0xef, 0xbb, 0xbf, 0x33, 0x0a, 0xe0, 0x80, 0xaf })]
        public void InvalidAutoSaveFiles(byte[] invalidFile)
        {
            // Small valid non-empty file, should be preferred over invalid or empty files.
            // UTF8 BOM "ï»¿", followed by length 1, a newline character, and an 'A'.
            byte[] validFile = new byte[] { 0xef, 0xbb, 0xbf, 0x31, 0x0a, 0x41 };
            string expectedAutoSaveText = "A";

            // Both file permutations should yield the same result.
            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile1, invalidFile);
            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile2, validFile);
            var remoteState1 = new WorkingCopyTextFile.TextAutoSaveState();
            using (var autoSaveTextFile = new AutoSaveTextFile<string>(remoteState1, AutoSaveFiles()))
            {
                Assert.Equal(expectedAutoSaveText, remoteState1.LastAutoSavedText);
            }

            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile1, validFile);
            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile2, invalidFile);
            var remoteState2 = new WorkingCopyTextFile.TextAutoSaveState();
            using (var autoSaveTextFile = new AutoSaveTextFile<string>(remoteState2, AutoSaveFiles()))
            {
                Assert.Equal(expectedAutoSaveText, remoteState2.LastAutoSavedText);
            }
        }
    }
}
