#region License
/*********************************************************************************
 * ParseTrees.TagSection.cs
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

using Sandra.Chess.Pgn;
using Sandra.Chess.Pgn.Temp;
using System.Collections.Generic;

namespace Sandra.Chess.Tests
{
    public static partial class ParseTrees
    {
        private static ParseTree<PgnSyntaxNodes> TagSectionOnly(ParseTree<PgnTagPairSyntax> firstTagPair, params ParseTree<PgnTagPairSyntax>[] otherTagPairs)
            => TagSectionTrailingTrivia(EmptyTrivia, firstTagPair, otherTagPairs);

        internal static List<(string, ParseTree)> TagSectionParseTrees() => new List<(string, ParseTree)>
        {
            ("[", TagSectionOnly(TagPair(BracketOpen))),
            ("]", TagSectionOnly(TagPair(BracketClose))),
            ("[]", TagSectionOnly(TagPair(BracketOpen, BracketClose))),
            ("[[]", TagSectionOnly(TagPair(BracketOpen), TagPair(BracketOpen, BracketClose))),
            ("[]]", TagSectionOnly(TagPair(BracketOpen, BracketClose), TagPair(BracketClose))),

            // Missing brackets at 4 places.
            // Last one doesn't miss brackets but still has a duplicate tag.
            ("Event \"?\"\nEvent \"?\"", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(WS_TagName, WS_TagValue))),
            ("Event \"?\"\nEvent \"?\"]", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(WS_TagName, WS_TagValue, BracketClose))),
            ("Event \"?\"\n[Event \"?\"", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(WS_BracketOpen, TagName, WS_TagValue))),
            ("Event \"?\"\n[Event \"?\"]", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose))),
            ("Event \"?\"]\nEvent \"?\"", TagSectionOnly(TagPair(TagName, WS_TagValue, BracketClose), TagPair(WS_TagName, WS_TagValue))),
            ("Event \"?\"]\nEvent \"?\"]", TagSectionOnly(TagPair(TagName, WS_TagValue, BracketClose), TagPair(WS_TagName, WS_TagValue, BracketClose))),
            ("Event \"?\"]\n[Event \"?\"", TagSectionOnly(TagPair(TagName, WS_TagValue, BracketClose), TagPair(WS_BracketOpen, TagName, WS_TagValue))),
            ("Event \"?\"]\n[Event \"?\"]", TagSectionOnly(TagPair(TagName, WS_TagValue, BracketClose), TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose))),
            ("[Event \"?\"\nEvent \"?\"", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue), TagPair(WS_TagName, WS_TagValue))),
            ("[Event \"?\"\nEvent \"?\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue), TagPair(WS_TagName, WS_TagValue, BracketClose))),
            ("[Event \"?\"\n[Event \"?\"", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue), TagPair(WS_BracketOpen, TagName, WS_TagValue))),
            ("[Event \"?\"\n[Event \"?\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue), TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose))),
            ("[Event \"?\"]\nEvent \"?\"", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose), TagPair(WS_TagName, WS_TagValue))),
            ("[Event \"?\"]\nEvent \"?\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose), TagPair(WS_TagName, WS_TagValue, BracketClose))),
            ("[Event \"?\"]\n[Event \"?\"", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose), TagPair(WS_BracketOpen, TagName, WS_TagValue))),
            ("[Event \"?\"]\n[Event \"?\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose), TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose))),

            // Missing tag values.
            ("Event\nEvent", TagSectionOnly(TagPair(TagName), TagPair(WS_TagName))),
            ("\"?\"Event\nEvent", TagSectionOnly(TagPair(TagValue), TagPair(TagName), TagPair(WS_TagName))),
            ("Event\"?\"\nEvent", TagSectionOnly(TagPair(TagName, TagValue), TagPair(WS_TagName))),
            ("Event\n\"?\"Event", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(TagName))),
            ("Event\nEvent\"?\"", TagSectionOnly(TagPair(TagName), TagPair(WS_TagName, TagValue))),

            // Duplicate values, missing tag names.
            ("\"?\"\n\"?\"", TagSectionOnly(TagPair(TagValue, WS_TagValue))),
            ("\"?\"\n]\"?\"", TagSectionOnly(TagPair(TagValue, WS_BracketClose), TagPair(TagValue))),
            ("\"?\"[\n\"?\"", TagSectionOnly(TagPair(TagValue), TagPair(BracketOpen, WS_TagValue))),
            ("\"?\"]\n[\"?\"", TagSectionOnly(TagPair(TagValue, BracketClose), TagPair(WS_BracketOpen, TagValue))),
            ("\"?\"[\n]\"?\"", TagSectionOnly(TagPair(TagValue), TagPair(BracketOpen, WS_BracketClose), TagPair(TagValue))),
            ("Event\"?\"\n\"?\"", TagSectionOnly(TagPair(TagName, TagValue, WS_TagValue))),
            ("\"?\"\nEvent\"?\"", TagSectionOnly(TagPair(TagValue), TagPair(WS_TagName, TagValue))),
            ("\"?\"\n\"?\"Event", TagSectionOnly(TagPair(TagValue, WS_TagValue), TagPair(TagName))),

            // Whitespace between consecutive tag values.
            ("\n\"?\"\n\"?\"", TagSectionOnly(TagPair(WS_TagValue, WS_TagValue))),
            ("\"?\"\n\"?\"\n", TagSectionTrailingTrivia(WhitespaceTrivia, TagPair(TagValue, WS_TagValue))),
            ("\n\"?\"\n\"?\"\n", TagSectionTrailingTrivia(WhitespaceTrivia, TagPair(WS_TagValue, WS_TagValue))),
        };

        internal static List<(string, ParseTree, PgnErrorCode[])> TagSectionParseTreesWithErrors() => new List<(string, ParseTree, PgnErrorCode[])>
        {
            ("\"", TagSectionOnly(TagPair(TagValue)),
                new[] { PgnErrorCode.UnterminatedTagValue }),
            ("\"\n", TagSectionOnly(TagPair(TagValue)),
                new[] { PgnErrorCode.IllegalControlCharacterInTagValue, PgnErrorCode.UnterminatedTagValue }),

            // Error tag values must behave like regular tag values, i.e. not generate extra errors.
            ("[Event \"\\u\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.UnrecognizedEscapeSequence }),
            ("[Event \"\n\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.IllegalControlCharacterInTagValue }),
        };
    }
}
