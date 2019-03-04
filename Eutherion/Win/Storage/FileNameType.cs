#region License
/*********************************************************************************
 * FileNameType.cs
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

using Eutherion.Localization;
using System.IO;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Specialized PType that only accepts strings that are legal file names.
    /// </summary>
    public sealed class FileNameType : PType.Filter<string>
    {
        public static readonly PTypeErrorBuilder FileNameTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(FileNameTypeError)));

        public static readonly FileNameType Instance = new FileNameType();

        private FileNameType() : base(PType.CLR.String) { }

        public override bool IsValid(string fileName, out ITypeErrorBuilder typeError)
            => !string.IsNullOrEmpty(fileName)
            && !fileName.StartsWith(".")
            && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
            ? ValidValue(out typeError)
            : InvalidValue(FileNameTypeError, out typeError);
    }
}
