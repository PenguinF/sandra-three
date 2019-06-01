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

using Eutherion.Utils;
using System;
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

    public class FileFixture : IDisposable
    {
        private class TargetFileState
        {
            public readonly string FilePath;

            public TargetFileState(string fileName)
            {
                // This prepends the working directory, which is the location of the compiled unit test dll.
                FilePath = Path.GetFullPath(fileName);
            }
        }

        private readonly TargetFileState textFileState;
        private readonly TargetFileState autoSaveFileState1;
        private readonly TargetFileState autoSaveFileState2;

        /// <summary>
        /// Primary target text file path.
        /// </summary>
        public string TextFilePath => GetPath(TargetFile.PrimaryTextFile);

        /// <summary>
        /// Gets the path to the primary auto-save file.
        /// </summary>
        public string AutoSaveFilePath1 => GetPath(TargetFile.AutoSaveFile1);

        /// <summary>
        /// Gets the path to the secondary auto-save file.
        /// </summary>
        public string AutoSaveFilePath2 => GetPath(TargetFile.AutoSaveFile2);

        public void PrepareAutoSaveFiles(byte[] autoSaveBytes1, byte[] autoSaveBytes2)
        {
            File.WriteAllBytes(AutoSaveFilePath1, autoSaveBytes1);
            File.WriteAllBytes(AutoSaveFilePath2, autoSaveBytes2);
        }

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

        public void Dispose()
        {
            File.Delete(TextFilePath);
            File.Delete(AutoSaveFilePath1);
            File.Delete(AutoSaveFilePath2);
        }
    }

    public class WorkingCopyTextFileTests : IClassFixture<FileFixture>
    {
        private static FileStream CreateAutoSaveFileStream(string autoSaveFilePath) => new FileStream(
            autoSaveFilePath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.Read,
            FileUtilities.DefaultFileStreamBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        private readonly FileFixture fileFixture;

        public WorkingCopyTextFileTests(FileFixture fileFixture)
        {
            this.fileFixture = fileFixture;
        }

        private FileStreamPair AutoSaveFiles()
            => FileStreamPair.Create(
                CreateAutoSaveFileStream,
                fileFixture.AutoSaveFilePath1,
                fileFixture.AutoSaveFilePath2);

        [Fact]
        public void LiveTextFilePathUnchanged()
        {
            using (var textFile = new LiveTextFile(fileFixture.TextFilePath))
            {
                Assert.Equal(fileFixture.TextFilePath, textFile.AbsoluteFilePath);
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
            fileFixture.PrepareAutoSaveFiles(invalidFile, validFile);
            var remoteState1 = new WorkingCopyTextFile.TextAutoSaveState();
            using (var autoSaveTextFile = new AutoSaveTextFile<string>(remoteState1, AutoSaveFiles()))
            {
                Assert.Equal(expectedAutoSaveText, remoteState1.LastAutoSavedText);
            }

            fileFixture.PrepareAutoSaveFiles(validFile, invalidFile);
            var remoteState2 = new WorkingCopyTextFile.TextAutoSaveState();
            using (var autoSaveTextFile = new AutoSaveTextFile<string>(remoteState2, AutoSaveFiles()))
            {
                Assert.Equal(expectedAutoSaveText, remoteState2.LastAutoSavedText);
            }
        }
    }
}
