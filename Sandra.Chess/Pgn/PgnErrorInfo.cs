#region License
/*********************************************************************************
 * PgnErrorInfo.cs
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

using Eutherion.Text;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Reports an error at a certain location in a source PGN.
    /// </summary>
    public class PgnErrorInfo : ISpan
    {
        public string Message { get; }
        public int Start { get; }
        public int Length { get; }

        public PgnErrorInfo(string message, int start, int length)
        {
            Message = message;
            Start = start;
            Length = length;
        }
    }
}
