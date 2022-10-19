#region License
/*********************************************************************************
 * PgnParserCharacterClass.cs
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

using Eutherion.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sandra.Chess.Pgn
{
    internal static class PgnParserCharacterClass
    {
        public const int IllegalCharacter = 0;

        // Symbol characters in discrete partitions of character sets.
        public const int SymbolCharacterMask = 0x3f;

        public const int SpecialCharacter = 1 << 6;
        public const int WhitespaceCharacter = 1 << 7;

        /// <summary>
        /// Contains a bitfield of character classes relevant for PGN, for each 8-bit character.
        /// A value of 0 means the character is not allowed.
        /// </summary>
        private static readonly int[] PgnCharacterClassTable = new int[0x100];

        static PgnParserCharacterClass()
        {
            // 0x00..0x20: treat 4 control characters and ' ' as whitespace.
            PgnCharacterClassTable['\t'] = WhitespaceCharacter;
            PgnCharacterClassTable['\n'] = WhitespaceCharacter;
            PgnCharacterClassTable['\v'] = WhitespaceCharacter;
            PgnCharacterClassTable['\r'] = WhitespaceCharacter;
            PgnCharacterClassTable[' '] = WhitespaceCharacter;

            // Treat 0xa0 as a space separator too.
            PgnCharacterClassTable[0xa0] = WhitespaceCharacter;

            new[]
            {
                PgnGameResultSyntax.AsteriskCharacter,
                PgnBracketOpenSyntax.BracketOpenCharacter,
                PgnBracketCloseSyntax.BracketCloseCharacter,
                PgnParenthesisCloseSyntax.ParenthesisCloseCharacter,
                PgnParenthesisOpenSyntax.ParenthesisOpenCharacter,
                PgnPeriodSyntax.PeriodCharacter,
                CStyleStringLiteral.QuoteCharacter,
                PgnCommentSyntax.EndOfLineCommentStartCharacter,
                PgnCommentSyntax.MultiLineCommentStartCharacter,
                PgnNagSyntax.NagCharacter,
                PgnEscapeSyntax.EscapeCharacter,
            }.ForEach(c => PgnCharacterClassTable[c] = SpecialCharacter);

            // Digits.
            PgnCharacterClassTable['0'] = PgnSymbolStateMachine.Digit0;
            PgnCharacterClassTable['1'] = PgnSymbolStateMachine.Digit1;
            PgnCharacterClassTable['2'] = PgnSymbolStateMachine.Digit2;
            for (char c = '3'; c <= '8'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.Digit3_8;
            PgnCharacterClassTable['9'] = PgnSymbolStateMachine.Digit9;

            // Letters.
            for (char c = 'A'; c <= 'Z'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherUpperCaseLetter;
            for (char c = 'À'; c <= 'Ö'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherUpperCaseLetter;  //0xc0-0xd6
            for (char c = 'Ø'; c <= 'Þ'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherUpperCaseLetter;  //0xd8-0xde
            for (char c = 'a'; c <= 'h'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.LowercaseAtoH;
            for (char c = 'i'; c <= 'z'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherLowercaseLetter;
            for (char c = 'ß'; c <= 'ö'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherLowercaseLetter;  //0xdf-0xf6
            for (char c = 'ø'; c <= 'ÿ'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherLowercaseLetter;  //0xf8-0xff

            // Treat the underscore as a lower case character.
            PgnCharacterClassTable['_'] = PgnSymbolStateMachine.OtherLowercaseLetter;

            // Special cases.
            PgnCharacterClassTable['O'] = PgnSymbolStateMachine.LetterO;
            PgnCharacterClassTable['P'] = PgnSymbolStateMachine.LetterP;
            PgnMoveFormatter.PieceSymbols.ForEach(c => PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherPieceLetter);
            PgnCharacterClassTable['x'] = PgnSymbolStateMachine.LowercaseX;
            PgnCharacterClassTable['-'] = PgnSymbolStateMachine.Dash;
            PgnCharacterClassTable['/'] = PgnSymbolStateMachine.Slash;
            PgnCharacterClassTable['='] = PgnSymbolStateMachine.EqualitySign;
            PgnCharacterClassTable['+'] = PgnSymbolStateMachine.PlusOrOctothorpe;
            PgnCharacterClassTable['#'] = PgnSymbolStateMachine.PlusOrOctothorpe;
            PgnCharacterClassTable['!'] = PgnSymbolStateMachine.ExclamationOrQuestionMark;
            PgnCharacterClassTable['?'] = PgnSymbolStateMachine.ExclamationOrQuestionMark;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCharacterClass(char pgnCharacter)
            => pgnCharacter <= 0xff ? PgnCharacterClassTable[pgnCharacter] : IllegalCharacter;
    }
}
