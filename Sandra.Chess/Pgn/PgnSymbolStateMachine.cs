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

using System.Linq;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Contains a state machine which is used during parsing of <see cref="IGreenPgnSymbol"/> instances.
    /// </summary>
    internal struct PgnSymbolStateMachine
    {
        // Offset with value 1, because 0 is the illegal character class in the PGN parser.
        internal const int Digit0 = 1;
        internal const int Digit1 = 2;
        internal const int Digit2 = 3;
        internal const int Digit3_8 = 4;
        internal const int Digit9 = 5;

        internal const int LetterO = 6;
        internal const int LetterP = 7;
        internal const int OtherPieceLetter = 8;
        internal const int OtherUpperCaseLetter = 9;

        internal const int LowercaseAtoH = 10;
        internal const int LowercaseX = 11;
        internal const int OtherLowercaseLetter = 12;

        internal const int Dash = 13;
        internal const int Slash = 14;
        internal const int EqualitySign = 15;
        internal const int PlusOrOctothorpe = 16;
        internal const int ExclamationOrQuestionMark = 17;

        internal const int CharacterClassLength = 18;

        // The default state 0 indicates that the machine has entered an invalid state, and will never become valid again.
        private const int StateStart = 1;

        // Game termination marker accepting states placed before any other accepting state, for inequality check in Yield().
        // State1 '1' -> StateDraw2 '1/' -> ... -> StateDraw7 '1/2-1/2'
        private const int StateDraw2 = 2;
        private const int StateDraw3 = 3;
        private const int StateDraw4 = 4;
        private const int StateDraw5 = 5;
        private const int StateDraw6 = 6;
        private const int StateDraw7 = 7;

        // Distinct states after the first character was a digit.
        private const int State0 = 8;
        private const int State1 = 9;

        // All digits.
        private const int StateValidMoveNumber = 10;

        private const int StateValidTagName = 39;

        private const int StateLength = 40;

        private const ulong ValidMoveNumberStates
            = 1ul << State0
            | 1ul << State1
            | 1ul << StateValidMoveNumber;

        private const ulong ValidTagNameStates
            = 1ul << StateValidTagName;

        // This table is used to transition from state to state given a character class index.
        // It is a low level implementation of a regular expression; it is a theorem
        // that regular expressions and finite automata are equivalent in their expressive power.
        private static readonly int[,] StateTransitionTable;

        static PgnSymbolStateMachine()
        {
            StateTransitionTable = new int[StateLength, CharacterClassLength];

            // When the first character is a digit.
            StateTransitionTable[StateStart, Digit0] = State0;
            StateTransitionTable[StateStart, Digit1] = State1;
            StateTransitionTable[StateStart, Digit2] = StateValidMoveNumber;
            StateTransitionTable[StateStart, Digit3_8] = StateValidMoveNumber;
            StateTransitionTable[StateStart, Digit9] = StateValidMoveNumber;

            // Valid move numbers are digits only.
            new[] { State0, State1, StateValidMoveNumber }.ForEach(state =>
            {
                StateTransitionTable[state, Digit0] = StateValidMoveNumber;
                StateTransitionTable[state, Digit1] = StateValidMoveNumber;
                StateTransitionTable[state, Digit2] = StateValidMoveNumber;
                StateTransitionTable[state, Digit3_8] = StateValidMoveNumber;
                StateTransitionTable[state, Digit9] = StateValidMoveNumber;
            });

            // Termination markers.
            StateTransitionTable[State1, Slash] = StateDraw2;
            StateTransitionTable[StateDraw2, Digit2] = StateDraw3;
            StateTransitionTable[StateDraw3, Dash] = StateDraw4;
            StateTransitionTable[StateDraw4, Digit1] = StateDraw5;
            StateTransitionTable[StateDraw5, Slash] = StateDraw6;
            StateTransitionTable[StateDraw6, Digit2] = StateDraw7;

            // Tag names must start with a letter or underscore.
            // This deviates from the PGN standard which only allows tag names to start with uppercase letters.
            StateTransitionTable[StateStart, LetterO] = StateValidTagName;
            StateTransitionTable[StateStart, LetterP] = StateValidTagName;
            StateTransitionTable[StateStart, OtherPieceLetter] = StateValidTagName;
            StateTransitionTable[StateStart, OtherUpperCaseLetter] = StateValidTagName;
            StateTransitionTable[StateStart, LowercaseAtoH] = StateValidTagName;
            StateTransitionTable[StateStart, LowercaseX] = StateValidTagName;
            StateTransitionTable[StateStart, OtherLowercaseLetter] = StateValidTagName;

            // Allow only digits, letters or the underscore character in tag names.
            StateTransitionTable[StateValidTagName, Digit0] = StateValidTagName;
            StateTransitionTable[StateValidTagName, Digit1] = StateValidTagName;
            StateTransitionTable[StateValidTagName, Digit2] = StateValidTagName;
            StateTransitionTable[StateValidTagName, Digit3_8] = StateValidTagName;
            StateTransitionTable[StateValidTagName, Digit9] = StateValidTagName;
            StateTransitionTable[StateValidTagName, LetterO] = StateValidTagName;
            StateTransitionTable[StateValidTagName, LetterP] = StateValidTagName;
            StateTransitionTable[StateValidTagName, OtherPieceLetter] = StateValidTagName;
            StateTransitionTable[StateValidTagName, OtherUpperCaseLetter] = StateValidTagName;
            StateTransitionTable[StateValidTagName, LowercaseAtoH] = StateValidTagName;
            StateTransitionTable[StateValidTagName, LowercaseX] = StateValidTagName;
            StateTransitionTable[StateValidTagName, OtherLowercaseLetter] = StateValidTagName;
        }

        private int CurrentState;

        public void Start(int characterClass)
            => CurrentState = StateTransitionTable[StateStart, characterClass];

        public void Transition(int characterClass)
            => CurrentState = StateTransitionTable[CurrentState, characterClass];

        public IGreenPgnSymbol Yield(int length)
        {
            if (CurrentState < State0)
            {
                if (CurrentState == StateDraw7) return GreenPgnDrawMarkerSyntax.Value;
            }
            else
            {
                ulong resultState = 1ul << CurrentState;
                if (ValidMoveNumberStates.Test(resultState)) return new GreenPgnMoveNumberSyntax(length);
                if (ValidTagNameStates.Test(resultState)) return new GreenPgnTagNameSyntax(length);
            }

            return null;
        }
    }
}
