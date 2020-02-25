#region License
/*********************************************************************************
 * PgnSymbolStateMachine.cs
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Contains a state machine which is used during parsing of <see cref="IGreenPgnSymbol"/> instances.
    /// </summary>
    internal struct PgnSymbolStateMachine
    {
        internal const int UppercaseLetterCharacter = 1;
        internal const int LowercaseLetterCharacter = 2;
        internal const int DigitCharacter = 3;
        internal const int OtherSymbolCharacter = 4;

        private bool AllLegalTagNameCharacters;

        public void Start(int characterClass)
            // Tag names must start with an uppercase letter.
            => AllLegalTagNameCharacters = characterClass == UppercaseLetterCharacter || characterClass == LowercaseLetterCharacter;

        public void Transition(int characterClass)
            // Allow only digits, letters or the underscore character in tag names.
            => AllLegalTagNameCharacters = AllLegalTagNameCharacters &&
                (characterClass == UppercaseLetterCharacter
                || characterClass == LowercaseLetterCharacter
                || characterClass == DigitCharacter);

        public IGreenPgnSymbol Yield(int length)
        {
            if (AllLegalTagNameCharacters) return new GreenPgnTagNameSyntax(length);
            return null;
        }
    }
}
