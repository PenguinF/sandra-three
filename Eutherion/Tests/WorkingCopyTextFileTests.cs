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
using System.Text;
using System.Threading;
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
        /// The secondary target text file.
        /// </summary>
        SecondaryTextFile,

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

        // All variations on "test1.txt".
        public static IEnumerable<object[]> AlternativePrimaryTextFilePaths()
        {
            yield return new object[] { "test1.txt" };
            yield return new object[] { "TEST1.TXT" };
#if DEBUG
            // It's not very healthy to take dependencies on build paths but impact is low.
            yield return new object[] { "../Debug/TEST1.TXT" };
            yield return new object[] { "..\\DEBUG\\test1.TXT" };
#else
            yield return new object[] { "../Release/TEST1.TXT" };
            yield return new object[] { "..\\RELEASE\\test1.TXT" };
#endif
        }

        private readonly TargetFileState textFileState1;
        private readonly TargetFileState textFileState2;
        private readonly TargetFileState autoSaveFileState1;
        private readonly TargetFileState autoSaveFileState2;

        public FileFixture()
        {
            textFileState1 = new TargetFileState("test1.txt");
            textFileState2 = new TargetFileState("test2.txt");
            autoSaveFileState1 = new TargetFileState("autosave1.txt");
            autoSaveFileState2 = new TargetFileState("autosave2.txt");
        }

        private TargetFileState GetTargetFileState(TargetFile targetFile)
        {
            switch (targetFile)
            {
                default:
                case TargetFile.PrimaryTextFile:
                    return textFileState1;
                case TargetFile.SecondaryTextFile:
                    return textFileState2;
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

        public void PrepareTargetFile(TargetFile targetFile, string text, Encoding encoding = null)
        {
            PrepareTargetFile(targetFile, FileState.Text);

            if (encoding == null)
            {
                File.WriteAllText(GetPath(targetFile), text);
            }
            else
            {
                File.WriteAllText(GetPath(targetFile), text, encoding);
            }
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
            PrepareTargetFile(TargetFile.SecondaryTextFile, FileState.DoesNotExist);
            PrepareTargetFile(TargetFile.AutoSaveFile1, FileState.DoesNotExist);
            PrepareTargetFile(TargetFile.AutoSaveFile2, FileState.DoesNotExist);
        }
    }

    public class WorkingCopyTextFileTests : IClassFixture<FileFixture>
    {
        private static string GenerateAutoSaveFileText(string targetResultText)
            => $"{targetResultText.Length}\n{targetResultText}";

        private readonly FileFixture fileFixture;

        public WorkingCopyTextFileTests(FileFixture fileFixture)
        {
            this.fileFixture = fileFixture;
        }

        private FileStreamPair AutoSaveFiles() => FileStreamPair.Create(
            AutoSaveTextFile.OpenExistingAutoSaveFile,
            fileFixture.GetPath(TargetFile.AutoSaveFile1),
            fileFixture.GetPath(TargetFile.AutoSaveFile2));

        /// <summary>
        /// Prepares the first auto-save file with the text, and the second to be empty.
        /// </summary>
        private void PrepareAutoSave(string autoSaveText)
        {
            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile1, GenerateAutoSaveFileText(autoSaveText), Encoding.UTF8);
            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile2, GenerateAutoSaveFileText(string.Empty), Encoding.UTF8);
        }

        private void AssertLiveTextFileSuccessfulLoad(string loadedText, WorkingCopyTextFile wcFile)
        {
            Assert.False(wcFile.IsDisposed);
            Assert.Null(wcFile.AutoSaveFile);
            Assert.Equal(loadedText, wcFile.LoadedText);
            Assert.Null(wcFile.LoadException);
            Assert.Equal(loadedText, wcFile.LocalCopyText);
            Assert.False(wcFile.ContainsChanges);
        }

        private void AssertLiveTextFileSuccessfulLoadWithAutoSave(string loadedText, string autoSavedText, WorkingCopyTextFile wcFile)
        {
            Assert.False(wcFile.IsDisposed);
            Assert.NotNull(wcFile.AutoSaveFile);
            Assert.False(wcFile.AutoSaveFile.IsDisposed);
            Assert.Equal(loadedText, wcFile.LoadedText);
            Assert.Null(wcFile.LoadException);

            // Convention is that when autoSavedText is empty, it means there were no local changes, so LocalCopyText == LoadedText.
            if (string.IsNullOrEmpty(autoSavedText))
            {
                Assert.Equal(loadedText, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);
            }
            else
            {
                Assert.Equal(autoSavedText, wcFile.LocalCopyText);
                Assert.True(wcFile.ContainsChanges);
            }
        }

        private void AssertNoAutoSaveFiles(WorkingCopyTextFile wcFile)
        {
            // Assert that the auto-save files have been deleted.
            Assert.Null(wcFile.AutoSaveFile);
            Assert.False(wcFile.ContainsChanges);
            Assert.False(File.Exists(fileFixture.GetPath(TargetFile.AutoSaveFile1)));
            Assert.False(File.Exists(fileFixture.GetPath(TargetFile.AutoSaveFile2)));
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

        [Fact]
        public void NewFileInitialState()
        {
            using (var wcFile = WorkingCopyTextFile.Open(null, null))
            {
                Assert.True(wcFile.IsTextFileOwner);
                Assert.Null(wcFile.OpenTextFile);
                Assert.Null(wcFile.OpenTextFilePath);

                AssertLiveTextFileSuccessfulLoad(string.Empty, wcFile);
            }
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
            using (var wcFile = WorkingCopyTextFile.FromLiveTextFile(textFile, null))
            {
                Assert.False(wcFile.IsTextFileOwner);
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
            using (var wcFile = WorkingCopyTextFile.FromLiveTextFile(textFile, null))
            {
                Assert.False(wcFile.IsTextFileOwner);
                Assert.Same(textFile, wcFile.OpenTextFile);
                Assert.Equal(filePath, wcFile.OpenTextFilePath);
                Assert.Null(wcFile.AutoSaveFile);

                Assert.Equal(string.Empty, wcFile.LoadedText);
                Assert.IsType(exceptionType, wcFile.LoadException);
                Assert.Equal(string.Empty, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);
            }
        }

        private void TestInaccessibleFileWithAutoSaveInitialState(FileState fileState, Type exceptionType, string autoSavedText)
        {
            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, fileState);

            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                Assert.True(wcFile.IsTextFileOwner);
                Assert.Equal(filePath, wcFile.OpenTextFilePath);
                Assert.NotNull(wcFile.AutoSaveFile);

                Assert.Equal(string.Empty, wcFile.LoadedText);
                Assert.IsType(exceptionType, wcFile.LoadException);
                Assert.Equal(autoSavedText, wcFile.LocalCopyText);
                Assert.Equal(!string.IsNullOrEmpty(autoSavedText), wcFile.ContainsChanges);
            }
        }

        [Theory]
        [InlineData(FileState.DoesNotExist, typeof(FileNotFoundException))]
        [InlineData(FileState.LockedByAnotherProcess, typeof(IOException))]
        [InlineData(FileState.IsDirectory, typeof(UnauthorizedAccessException))]
        public void InaccessibleFileWithAutoSaveInitialState(FileState fileState, Type exceptionType)
        {
            // Test both empty and non-empty auto-save files.
            string autoSaveText = string.Empty;
            PrepareAutoSave(autoSaveText);
            TestInaccessibleFileWithAutoSaveInitialState(fileState, exceptionType, autoSaveText);

            autoSaveText = "A";
            PrepareAutoSave(autoSaveText);
            TestInaccessibleFileWithAutoSaveInitialState(fileState, exceptionType, autoSaveText);
        }

        [Theory]
        [MemberData(nameof(Texts))]
        public void AutoSavedNewFileInitialState(string autoSaveFileText)
        {
            PrepareAutoSave(autoSaveFileText);

            using (var wcFile = WorkingCopyTextFile.Open(null, AutoSaveFiles()))
            {
                Assert.True(wcFile.IsTextFileOwner);
                Assert.Null(wcFile.OpenTextFile);
                Assert.Null(wcFile.OpenTextFilePath);

                AssertLiveTextFileSuccessfulLoadWithAutoSave(string.Empty, autoSaveFileText, wcFile);
            }
        }

        [Theory]
        [MemberData(nameof(Texts))]
        public void AutoSavedExistingFileInitialState(string autoSaveFileText)
        {
            string expectedLoadedText = "A";

            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, expectedLoadedText);
            PrepareAutoSave(autoSaveFileText);

            using (var textFile = new LiveTextFile(filePath))
            using (var wcFile = WorkingCopyTextFile.FromLiveTextFile(textFile, AutoSaveFiles()))
            {
                Assert.False(wcFile.IsTextFileOwner);
                Assert.Same(textFile, wcFile.OpenTextFile);
                Assert.Equal(filePath, wcFile.OpenTextFilePath);

                AssertLiveTextFileSuccessfulLoadWithAutoSave(expectedLoadedText, autoSaveFileText, wcFile);
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
            using (var wcFile = WorkingCopyTextFile.Open(null, AutoSaveFiles()))
            {
                AssertLiveTextFileSuccessfulLoadWithAutoSave(string.Empty, expectedAutoSaveText, wcFile);
            }

            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile1, validFile);
            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile2, invalidFile);
            using (var wcFile = WorkingCopyTextFile.Open(null, AutoSaveFiles()))
            {
                AssertLiveTextFileSuccessfulLoadWithAutoSave(string.Empty, expectedAutoSaveText, wcFile);
            }
        }

        [Fact]
        public void ExistingFileOwnership()
        {
            string text = "A";

            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, text);

            // wcFile does not own textFile: textFile must still be open after wcFile is disposed.
            using (var textFile = new LiveTextFile(filePath))
            using (var wcFile = WorkingCopyTextFile.FromLiveTextFile(textFile, null))
            {
                Assert.False(wcFile.IsTextFileOwner);
                wcFile.Dispose();
                Assert.NotNull(wcFile.OpenTextFile);
                Assert.False(textFile.IsDisposed);
                textFile.Dispose();
                Assert.True(textFile.IsDisposed);
            }

            // wcFile owns textFile: textFile must be disposed together with wcFile.
            using (var wcFile = WorkingCopyTextFile.Open(filePath, null))
            {
                var textFile = wcFile.OpenTextFile;
                Assert.NotNull(textFile);
                Assert.False(textFile.IsDisposed);
                wcFile.Dispose();
                Assert.Same(textFile, wcFile.OpenTextFile);
                Assert.True(textFile.IsDisposed);
            }
        }

        [Fact]
        public void ModificationsAreAutoSaved()
        {
            string expectedLoadedText = "A";
            string expectedAutoSaveText = "B";

            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, expectedLoadedText);
            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile1, FileState.DoesNotExist);
            fileFixture.PrepareTargetFile(TargetFile.AutoSaveFile2, FileState.DoesNotExist);

            using (var wcFile = WorkingCopyTextFile.Open(filePath, null))
            {
                wcFile.QueryAutoSaveFile += (_, e) => e.AutoSaveFileStreamPair = AutoSaveFiles();
                wcFile.UpdateLocalCopyText(expectedAutoSaveText, containsChanges: true);
                Assert.True(wcFile.ContainsChanges);
            }

            // Reloading the WorkingCopyTextFile should restore the auto saved text, even when the original file is deleted.
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, FileState.DoesNotExist);

            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                Assert.Equal(expectedAutoSaveText, wcFile.LocalCopyText);
                Assert.True(wcFile.ContainsChanges);
                Assert.NotNull(wcFile.AutoSaveFile);

                // Assert that the auto-save file is still there, but disposed.
                wcFile.Dispose();
                Assert.NotNull(wcFile.AutoSaveFile);
                Assert.True(wcFile.AutoSaveFile.IsDisposed);
                Assert.True(wcFile.ContainsChanges);
            }
        }

        [Fact]
        public void DiscardAutosavedModificationsIfFileUpdatedWithSameContents()
        {
            // This asserts that there are no changes after the following transitions:
            // 1) Open existing file with text "A".
            // 2) Make local modification, change text to "B", this is auto-saved.
            // 3) Dispose the WorkingCopyTextFile.
            // 4) Update existing file with contents "B".
            // 5) Reload WorkingCopyTextFile from the auto-save files.
            //    -> Should detect that the contents of the file match the auto-saved modifications.

            string expectedLoadedText = "A";
            string expectedAutoSaveText = "B";

            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, expectedLoadedText);
            PrepareAutoSave(string.Empty);

            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                wcFile.UpdateLocalCopyText(expectedAutoSaveText, containsChanges: true);
                Assert.True(wcFile.ContainsChanges);
            }

            // Assert that the auto-save actually happened.
            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                Assert.Equal(expectedAutoSaveText, wcFile.LocalCopyText);
                Assert.True(wcFile.ContainsChanges);
            }

            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, expectedAutoSaveText);

            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                Assert.Equal(expectedAutoSaveText, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);
                Assert.NotNull(wcFile.AutoSaveFile);
                wcFile.Dispose();
                AssertNoAutoSaveFiles(wcFile);
            }
        }

        [Fact]
        public void AllowMultipleDispose()
        {
            using (var wcFile = WorkingCopyTextFile.Open(null, null))
            {
                wcFile.Dispose();
                Assert.True(wcFile.IsDisposed);
                wcFile.Dispose();
                Assert.True(wcFile.IsDisposed);
            }
        }

        [Fact]
        public void UpdatesBlockedAfterDispose()
        {
            using (var wcFile = WorkingCopyTextFile.Open(null, null))
            {
                Assert.Throws<InvalidOperationException>(() => wcFile.Save());
                wcFile.Dispose();
                Assert.Throws<ObjectDisposedException>(() => wcFile.Save());
                Assert.Throws<ObjectDisposedException>(() => wcFile.UpdateLocalCopyText(string.Empty, false));
            }
        }

        [Fact]
        public void AutoSaveCleanedUpIfNoModifications()
        {
            string loadedText = "A";
            string autoSaveText = "B";

            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, loadedText);
            PrepareAutoSave(autoSaveText);

            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                // Assert transitions between auto-save states.
                Assert.NotNull(wcFile.AutoSaveFile);
                Assert.Equal(autoSaveText, wcFile.LocalCopyText);
                Assert.True(wcFile.ContainsChanges);
                wcFile.UpdateLocalCopyText(loadedText, containsChanges: false);
                Assert.NotNull(wcFile.AutoSaveFile);
                Assert.Equal(loadedText, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);
                wcFile.Dispose();

                // Assert that the auto-save files have been deleted.
                AssertNoAutoSaveFiles(wcFile);
            }
        }

        [Fact]
        public void TextAutoUpdated()
        {
            string oldLoadedText = "A";
            string newLoadedText = "B";

            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, oldLoadedText);

            using (var ewh = new ManualResetEvent(false))
            using (var wcFile = WorkingCopyTextFile.Open(filePath, null))
            {
                // Use an EventWaitHandle to wait for the event to occur.
                // This works because the event will get raised on a background thread.
                wcFile.LoadedTextChanged += (_, __) => ewh.Set();

                // Simulate that the opened text file was updated remotely.
                fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, newLoadedText);
                ewh.WaitOne();

                // Assert that the loaded text is updated automatically.
                Assert.Equal(newLoadedText, wcFile.LoadedText);
                Assert.Equal(newLoadedText, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);
            }
        }

        [Theory]
        // First and last value must be different or WorkingCopyTextFile will load with ContaisChanges == false.
        [InlineData("A", "B", "C")]
        [InlineData("A", "A", "C")]
        [InlineData("A", "C", "C")]
        public void TextNotAutoUpdatedWithLocalChanges(string oldLoadedText, string newLoadedText, string autoSavedText)
        {
            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, oldLoadedText);
            PrepareAutoSave(autoSavedText);

            using (var ewh = new ManualResetEvent(false))
            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                wcFile.LoadedTextChanged += (_, __) => ewh.Set();
                fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, newLoadedText);
                ewh.WaitOne();

                // Assert that LoadedText is updated but LocalCopyText is not.
                Assert.Equal(newLoadedText, wcFile.LoadedText);
                Assert.Equal(autoSavedText, wcFile.LocalCopyText);
                Assert.True(wcFile.ContainsChanges);
            }
        }

        [Fact]
        public void SaveRemovesAutoSave()
        {
            string loadedText = "A";
            string autoSavedText = "B";

            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, loadedText);
            PrepareAutoSave(autoSavedText);

            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                wcFile.Save();
                wcFile.Dispose();
                AssertNoAutoSaveFiles(wcFile);
            }
        }

        [Theory]
        [InlineData(FileState.DoesNotExist)]
        [InlineData(FileState.IsDirectory)]
        // Skip LockedByAnotherProcess, because then the LoadedTextChanged event isn't raised.
        public void FailedAutoUpdate(FileState fileState)
        {
            string loadedText = "A";

            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, loadedText);
            PrepareAutoSave(string.Empty);

            using (var ewh = new ManualResetEvent(false))
            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                wcFile.LoadedTextChanged += (_, __) => ewh.Set();
                fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, fileState);
                ewh.WaitOne();

                // LocalCopyText should still be intact even though LoadedText is cleared.
                wcFile.Dispose();
                Assert.Equal(string.Empty, wcFile.LoadedText);
                Assert.Equal(loadedText, wcFile.LocalCopyText);
                Assert.True(wcFile.ContainsChanges);
                Assert.NotNull(wcFile.AutoSaveFile);
            }
        }

        [Fact]
        public void FailedSaveThenAutoUpdate()
        {
            string oldLoadedText = "A";
            string newLoadedText = "B";

            string filePath = fileFixture.GetPath(TargetFile.PrimaryTextFile);
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, oldLoadedText);
            PrepareAutoSave(string.Empty);

            using (var ewh = new ManualResetEvent(false))
            using (var wcFile = WorkingCopyTextFile.Open(filePath, AutoSaveFiles()))
            {
                fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, FileState.LockedByAnotherProcess);
                Assert.Throws<IOException>(() => wcFile.Save());

                wcFile.LoadedTextChanged += (_, __) => ewh.Set();
                fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, newLoadedText);
                ewh.WaitOne();
                Assert.Equal(newLoadedText, wcFile.LoadedText);
                Assert.Equal(newLoadedText, wcFile.LocalCopyText);

                // Auto-updating should still work after being locked by another process.
                Assert.False(wcFile.ContainsChanges);
            }
        }

        private void PrepareForReplaceTest(string primaryLoadedText, string secondaryLoadedText)
        {
            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, primaryLoadedText);
            fileFixture.PrepareTargetFile(TargetFile.SecondaryTextFile, secondaryLoadedText);
        }

        [Fact]
        public void ReplaceFailsWhenDisposed()
        {
            PrepareForReplaceTest("A", "B");

            var wcFile = WorkingCopyTextFile.Open(fileFixture.GetPath(TargetFile.PrimaryTextFile), null);
            var liveTextFile = wcFile.OpenTextFile;
            wcFile.Dispose();

            Assert.Throws<ObjectDisposedException>(() => wcFile.Replace(fileFixture.GetPath(TargetFile.SecondaryTextFile)));
            Assert.Same(liveTextFile, wcFile.OpenTextFile);
        }

        [Fact]
        public void ReplaceFailsWhenNotOwner()
        {
            PrepareForReplaceTest("A", "B");

            using (var textFile = new LiveTextFile(fileFixture.GetPath(TargetFile.PrimaryTextFile)))
            {
                string replacePath = fileFixture.GetPath(TargetFile.SecondaryTextFile);

                var wcFile = WorkingCopyTextFile.FromLiveTextFile(textFile, null);
                using (wcFile)
                {
                    Assert.Throws<InvalidOperationException>(() => wcFile.Replace(replacePath));
                    Assert.Same(textFile, wcFile.OpenTextFile);
                }

                // Also assert that the ObjectDisposedException has precedence.
                Assert.Throws<ObjectDisposedException>(() => wcFile.Replace(replacePath));
            }
        }

        public static IEnumerable<object[]> PrimaryTextFilePaths() => FileFixture.AlternativePrimaryTextFilePaths();

        [Theory]
        [MemberData(nameof(PrimaryTextFilePaths))]
        public void ReplaceWithSamePath(string filePath)
        {
            string text1 = "A";
            string text2 = "B";
            string text3 = "C";

            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, text1);

            using (var ewh = new AutoResetEvent(false))
            using (var wcFile = WorkingCopyTextFile.Open(fileFixture.GetPath(TargetFile.PrimaryTextFile), null))
            {
                var liveTextFile = wcFile.OpenTextFile;

                // Evaluate LoadedText so IsDirty becomes false.
                Assert.Equal(text1, wcFile.LoadedText);

                // This should have no effect other than that the local changes are saved.
                wcFile.UpdateLocalCopyText(text2, containsChanges: true);
                bool eventRaised = false;
                wcFile.OpenTextFilePathChanged += (_, __) => eventRaised = true;
                wcFile.Replace(filePath);

                // Assert that the OpenTextFile is unchanged.
                Assert.Same(liveTextFile, wcFile.OpenTextFile);
                Assert.False(liveTextFile.IsDisposed);
                Assert.Equal(text2, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);
                Assert.False(eventRaised);

                // Verify that auto-updates still work.
                wcFile.LoadedTextChanged += (_, __) => ewh.Set();
                fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, text3);
                ewh.WaitOne();

                Assert.Equal(text3, wcFile.LoadedText);
                Assert.Equal(text3, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);
            }
        }

        [Fact]
        public void ReplaceWithDifferentPath()
        {
            string text1 = "A";
            string text2 = "B";
            string text3 = "C";

            PrepareForReplaceTest(text1, text2);

            using (var ewh = new AutoResetEvent(false))
            using (var wcFile = WorkingCopyTextFile.Open(fileFixture.GetPath(TargetFile.PrimaryTextFile), null))
            {
                var liveTextFile = wcFile.OpenTextFile;
                string previousOpenTextFilePath = null;
                wcFile.OpenTextFilePathChanged += (_, e) => previousOpenTextFilePath = e.PreviousOpenTextFilePath;
                wcFile.Replace(fileFixture.GetPath(TargetFile.SecondaryTextFile));

                // Assert that the OpenTextFile was replaced, and the old one disposed.
                Assert.NotSame(liveTextFile, wcFile.OpenTextFile);
                Assert.True(liveTextFile.IsDisposed);

                // Assert that the second file was overwritten.
                Assert.Equal(text1, wcFile.LoadedText);
                Assert.Equal(text1, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);

                // Assert that the OpenTextFilePathChanged event was raised.
                Assert.Equal(fileFixture.GetPath(TargetFile.PrimaryTextFile), previousOpenTextFilePath);

                // Verify that auto-updates work on the second file, not the first.
                wcFile.LoadedTextChanged += (_, __) => ewh.Set();
                fileFixture.PrepareTargetFile(TargetFile.SecondaryTextFile, text2);
                fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, text3);
                ewh.WaitOne();

                Assert.Equal(text2, wcFile.LoadedText);
                Assert.Equal(text2, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);
            }
        }

        [Fact]
        public void ReplaceWithIllegalPath()
        {
            string text1 = "A";
            string text2 = "B";

            fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, text1);

            using (var ewh = new AutoResetEvent(false))
            using (var wcFile = WorkingCopyTextFile.Open(fileFixture.GetPath(TargetFile.PrimaryTextFile), null))
            {
                var liveTextFile = wcFile.OpenTextFile;

                // Lock the second file before attempting to overwrite it.
                fileFixture.PrepareTargetFile(TargetFile.SecondaryTextFile, FileState.LockedByAnotherProcess);

                // This should throw, and leave the old OpenTextFile intact.
                Assert.Throws<IOException>(() => wcFile.Replace(fileFixture.GetPath(TargetFile.SecondaryTextFile)));

                Assert.Same(liveTextFile, wcFile.OpenTextFile);
                Assert.False(liveTextFile.IsDisposed);
                Assert.Equal(text1, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);

                // Verify that auto-updates still work.
                wcFile.LoadedTextChanged += (_, __) => ewh.Set();
                fileFixture.PrepareTargetFile(TargetFile.PrimaryTextFile, text2);
                ewh.WaitOne();

                Assert.Equal(text2, wcFile.LoadedText);
                Assert.Equal(text2, wcFile.LocalCopyText);
                Assert.False(wcFile.ContainsChanges);
            }
        }
    }
}
