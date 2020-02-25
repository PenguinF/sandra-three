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
        // Offset with value 1, because 0 is the illegal character class in the PGN parser.
        internal const int UppercaseLetterCharacter = 1;
        internal const int LowercaseLetterCharacter = 2;
        internal const int DigitCharacter = 3;
        internal const int OtherSymbolCharacter = 4;

        internal const int CharacterClassLength = 18;

        // The default state 0 indicates that the machine has entered an invalid state, and will never become valid again.
        private const int StateStart = 1;

        private const int StateValidTagName = 40;

        private const int StateLength = 41;

        private const ulong ValidTagNameStates
            = 1ul << StateValidTagName;

        // This table is used to transition from state to state given a character class index.
        // It is a low level implementation of a regular expression; it is a theorem
        // that regular expressions and finite automata are equivalent in their expressive power.
        private static readonly int[,] StateTransitionTable;

        static PgnSymbolStateMachine()
        {
            StateTransitionTable = new int[StateLength, CharacterClassLength];

            // Tag names must start with a letter or underscore.
            // This deviates from the PGN standard which only allows tag names to start with uppercase letters.
            StateTransitionTable[StateStart, UppercaseLetterCharacter] = StateValidTagName;
            StateTransitionTable[StateStart, LowercaseLetterCharacter] = StateValidTagName;

            // Allow only digits, letters or the underscore character in tag names.
            StateTransitionTable[StateValidTagName, DigitCharacter] = StateValidTagName;
            StateTransitionTable[StateValidTagName, UppercaseLetterCharacter] = StateValidTagName;
            StateTransitionTable[StateValidTagName, LowercaseLetterCharacter] = StateValidTagName;
        }

        private int CurrentState;

        public void Start(int characterClass)
            => CurrentState = StateTransitionTable[StateStart, characterClass];

        public void Transition(int characterClass)
            => CurrentState = StateTransitionTable[CurrentState, characterClass];

        public IGreenPgnSymbol Yield(int length)
        {
            ulong resultState = 1ul << CurrentState;
            if (ValidTagNameStates.Test(resultState)) return new GreenPgnTagNameSyntax(length);
            return null;
        }
    }
}
