/*********************************************************************************
 * SubFolderNameType.cs
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
using System;
using System.IO;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Specialized PType that only accepts a certain class of subfolder names.
    /// </summary>
    public sealed class SubFolderNameType : PType.Filter<string>
    {
        public static SubFolderNameType Instance = new SubFolderNameType();

        private SubFolderNameType() : base(PType.CLR.String) { }

        public override bool IsValid(string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath)
                && folderPath.IndexOfAny(Path.GetInvalidPathChars()) < 0
                && !Path.IsPathRooted(folderPath))
            {
                var localApplicationFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                var subFolder = new DirectoryInfo(Path.Combine(localApplicationFolder.FullName, folderPath));

                for (var parentFolder = subFolder.Parent; parentFolder != null; parentFolder = parentFolder.Parent)
                {
                    // Indeed a subfolder?
                    if (localApplicationFolder.FullName == parentFolder.FullName) return true;
                }
            }
            return false;
        }
    }
}
