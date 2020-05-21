#region License
/*********************************************************************************
 * AutoSaveFileNamePair.cs
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

using Eutherion.Win.Storage;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Represents two file names in the local application data folder.
    /// </summary>
    public struct AutoSaveFileNamePair
    {
        public string FileName1;
        public string FileName2;

        public AutoSaveFileNamePair(string fileName1, string fileName2)
        {
            FileName1 = fileName1;
            FileName2 = fileName2;
        }

        public AutoSaveFileNamePair((string, string) fileNames)
        {
            FileName1 = fileNames.Item1;
            FileName2 = fileNames.Item2;
        }
    }

    /// <summary>
    /// Specialized PType that accepts pairs of legal file names that are used for auto-saving changes in text files.
    /// </summary>
    public sealed class AutoSaveFilePairPType : PType.Derived<(string, string), AutoSaveFileNamePair>
    {
        public static readonly AutoSaveFilePairPType Instance = new AutoSaveFilePairPType();

        private AutoSaveFilePairPType()
            : base(new PType.TupleType<string, string>(
                (FileNameType.InstanceAllowStartWithDots, FileNameType.InstanceAllowStartWithDots)))
        {
        }

        public override Union<ITypeErrorBuilder, AutoSaveFileNamePair> TryGetTargetValue((string, string) value)
            => new AutoSaveFileNamePair(value);

        public override (string, string) GetBaseValue(AutoSaveFileNamePair value)
            => (value.FileName1, value.FileName2);
    }
}
