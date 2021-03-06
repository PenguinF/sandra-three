﻿#region License
/*********************************************************************************
 * PgnErrorInfo.cs
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
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Reports an error at a certain location in a source PGN.
    /// </summary>
    public class PgnErrorInfo : ISpan
    {
        private static PgnErrorLevel GetErrorLevel(PgnErrorCode errorCode)
        {
            switch (errorCode)
            {
                case PgnErrorCode.IllegalCharacter:
                case PgnErrorCode.UnterminatedTagValue:
                case PgnErrorCode.UnrecognizedEscapeSequence:
                case PgnErrorCode.IllegalControlCharacterInTagValue:
                case PgnErrorCode.UnrecognizedMove:
                case PgnErrorCode.EmptyTag:
                case PgnErrorCode.MissingTagBracketOpen:
                case PgnErrorCode.MissingTagName:
                case PgnErrorCode.MissingTagValue:
                case PgnErrorCode.MultipleTagValues:
                case PgnErrorCode.MissingTagBracketClose:
                case PgnErrorCode.MissingMove:
                case PgnErrorCode.OrphanParenthesisClose:
                case PgnErrorCode.OrphanBracketOpen:
                case PgnErrorCode.OrphanTagValue:
                case PgnErrorCode.OrphanBracketClose:
                case PgnErrorCode.MissingParenthesisClose:
                    return PgnErrorLevel.Error;
                case PgnErrorCode.UnterminatedMultiLineComment:
                case PgnErrorCode.EmptyVariation:
                case PgnErrorCode.MissingTagSection:
                case PgnErrorCode.MissingGameTerminationMarker:
                    return PgnErrorLevel.Warning;
                case PgnErrorCode.EmptyNag:
                case PgnErrorCode.OverflowNag:
                case PgnErrorCode.MissingMoveNumber:
                case PgnErrorCode.OrphanPeriod:
                case PgnErrorCode.VariationBeforeNAG:
                    return PgnErrorLevel.Message;
                default:
                    // Treat unknown error codes as just messages.
                    return PgnErrorLevel.Message;
            }
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public PgnErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets the severity of the error.
        /// </summary>
        public PgnErrorLevel ErrorLevel { get; }

        /// <summary>
        /// Gets the start position of the text span where the error occurred.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the length of the text span where the error occurred.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the list of parameters.
        /// </summary>
        public string[] Parameters { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="PgnErrorInfo"/>.
        /// </summary>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="start">
        /// The start position of the text span where the error occurred.
        /// </param>
        /// <param name="length">
        /// The length of the text span where the error occurred.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either <paramref name="start"/> or <paramref name="length"/>, or both are negative.
        /// </exception>
        public PgnErrorInfo(PgnErrorCode errorCode, int start, int length)
            : this(errorCode, start, length, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PgnErrorInfo"/>.
        /// </summary>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="start">
        /// The start position of the text span where the error occurred.
        /// </param>
        /// <param name="length">
        /// The length of the text span where the error occurred.
        /// </param>
        /// <param name="parameters">
        /// Parameters of the error.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either <paramref name="start"/> or <paramref name="length"/>, or both are negative.
        /// </exception>
        public PgnErrorInfo(PgnErrorCode errorCode, int start, int length, string[] parameters)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            ErrorCode = errorCode;
            ErrorLevel = GetErrorLevel(errorCode);
            Start = start;
            Length = length;
            Parameters = parameters;
        }
    }
}
