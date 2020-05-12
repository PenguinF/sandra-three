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
using System.Collections.Generic;

namespace Sandra.Chess.Tests
{
    public static partial class ParseTrees
    {
        internal static List<(string, ParseTree)> TagSectionParseTrees() => new List<(string, ParseTree)>
        {
        };

        internal static List<(string, ParseTree, PgnErrorCode[])> TagSectionParseTreesWithErrors() => new List<(string, ParseTree, PgnErrorCode[])>
        {
            ("[", TagSectionOnly(TagPair(BracketOpen)),
                new[] { PgnErrorCode.EmptyTag, PgnErrorCode.MissingTagBracketClose }),
            ("]", TagSectionOnly(TagPair(BracketClose)),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.EmptyTag }),
            ("[]", TagSectionOnly(TagPair(BracketOpen, BracketClose)),
                new[] { PgnErrorCode.EmptyTag }),
            ("[[]", TagSectionOnly(TagPair(BracketOpen), TagPair(BracketOpen, BracketClose)),
                new[] { PgnErrorCode.EmptyTag, PgnErrorCode.MissingTagBracketClose, PgnErrorCode.EmptyTag }),
            ("[]]", TagSectionOnly(TagPair(BracketOpen, BracketClose), TagPair(BracketClose)),
                new[] { PgnErrorCode.EmptyTag, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.EmptyTag }),

            // Missing brackets at 4 places.
            // Last one doesn't miss brackets but still has a duplicate tag.
            ("Event \"?\"\nEvent \"?\"", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(WS_TagName, WS_TagValue)),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose }),
            ("Event \"?\"\nEvent \"?\"]", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(WS_TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingTagBracketOpen }),
            ("Event \"?\"\n[Event \"?\"", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(WS_BracketOpen, TagName, WS_TagValue)),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingTagBracketClose }),
            ("Event \"?\"\n[Event \"?\"]", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose }),
            ("Event \"?\"]\nEvent \"?\"", TagSectionOnly(TagPair(TagName, WS_TagValue, BracketClose), TagPair(WS_TagName, WS_TagValue)),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose }),
            ("Event \"?\"]\nEvent \"?\"]", TagSectionOnly(TagPair(TagName, WS_TagValue, BracketClose), TagPair(WS_TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketOpen }),
            ("Event \"?\"]\n[Event \"?\"", TagSectionOnly(TagPair(TagName, WS_TagValue, BracketClose), TagPair(WS_BracketOpen, TagName, WS_TagValue)),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose }),
            ("Event \"?\"]\n[Event \"?\"]", TagSectionOnly(TagPair(TagName, WS_TagValue, BracketClose), TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.MissingTagBracketOpen }),
            ("[Event \"?\"\nEvent \"?\"", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue), TagPair(WS_TagName, WS_TagValue)),
                new[] { PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose }),
            ("[Event \"?\"\nEvent \"?\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue), TagPair(WS_TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingTagBracketOpen }),
            ("[Event \"?\"\n[Event \"?\"", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue), TagPair(WS_BracketOpen, TagName, WS_TagValue)),
                new[] { PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingTagBracketClose }),
            ("[Event \"?\"\n[Event \"?\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue), TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.MissingTagBracketClose }),
            ("[Event \"?\"]\nEvent \"?\"", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose), TagPair(WS_TagName, WS_TagValue)),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose }),
            ("[Event \"?\"]\nEvent \"?\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose), TagPair(WS_TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.MissingTagBracketOpen }),
            ("[Event \"?\"]\n[Event \"?\"", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose), TagPair(WS_BracketOpen, TagName, WS_TagValue)),
                new[] { PgnErrorCode.MissingTagBracketClose }),
            ("[Event \"?\"]\n[Event \"?\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose), TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose)),
                new PgnErrorCode[] { }),

            // Missing tag values.
            ("Event", TagSectionOnly(TagPair(TagName)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose
                }),
            ("Event\nEvent", TagSectionOnly(TagPair(TagName), TagPair(WS_TagName)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"?\"Event\nEvent", TagSectionOnly(TagPair(TagValue), TagPair(TagName), TagPair(WS_TagName)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose
                }),
            ("Event\"?\"\nEvent", TagSectionOnly(TagPair(TagName, TagValue), TagPair(WS_TagName)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose
                }),
            ("Event\n\"?\"Event", TagSectionOnly(TagPair(TagName, WS_TagValue), TagPair(TagName)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose
                }),
            ("Event\nEvent\"?\"", TagSectionOnly(TagPair(TagName), TagPair(WS_TagName, TagValue)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose
                }),

            // Duplicate values, missing tag names.
            ("\"?\"\n\"?\"", TagSectionOnly(TagPair(TagValue, WS_TagValue)),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"?\"\n]\"?\"", TagSectionOnly(TagPair(TagValue, WS_BracketClose), TagPair(TagValue)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"?\"[\n\"?\"", TagSectionOnly(TagPair(TagValue), TagPair(BracketOpen, WS_TagValue)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"?\"]\n[\"?\"", TagSectionOnly(TagPair(TagValue, BracketClose), TagPair(WS_BracketOpen, TagValue)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName,
                    PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"?\"[\n]\"?\"", TagSectionOnly(TagPair(TagValue), TagPair(BracketOpen, WS_BracketClose), TagPair(TagValue)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.EmptyTag,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("Event\"?\"\n\"?\"", TagSectionOnly(TagPair(TagName, TagValue, WS_TagValue)),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"?\"\nEvent\"?\"", TagSectionOnly(TagPair(TagValue), TagPair(WS_TagName, TagValue)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"?\"\n\"?\"Event", TagSectionOnly(TagPair(TagValue, WS_TagValue), TagPair(TagName)),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose
                }),

            ("\"", TagSectionOnly(TagPair(TagValue)),
                new[]
                {
                    PgnErrorCode.UnterminatedTagValue, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"\n", TagSectionOnly(TagPair(TagValue)),
                new[]
                {
                    PgnErrorCode.IllegalControlCharacterInTagValue, PgnErrorCode.UnterminatedTagValue, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"\"", TagSectionOnly(TagPair(TagValue)),
                new[]
                {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"\"\"", TagSectionOnly(TagPair(TagValue, TagValue)),
                new[]
                {
                    PgnErrorCode.UnterminatedTagValue, PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"\"\"\"", TagSectionOnly(TagPair(TagValue, TagValue)),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"\"\"\"\"", TagSectionOnly(TagPair(TagValue, TagValue, TagValue)),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.UnterminatedTagValue, PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"\"\"\"\"\"", TagSectionOnly(TagPair(TagValue, TagValue, TagValue)),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"\"\"\"\"\"\"", TagSectionOnly(TagPair(TagValue, TagValue, TagValue, TagValue)),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MultipleTagValues, PgnErrorCode.UnterminatedTagValue, PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"\"\"\"\"\"\"\"", TagSectionOnly(TagPair(TagValue, TagValue, TagValue, TagValue)),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MultipleTagValues, PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),

            // Whitespace between consecutive tag values.
            ("\n\"?\"\n\"?\"", TagSectionOnly(TagPair(WS_TagValue, WS_TagValue)),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\"?\"\n\"?\"\n", OneGameTrailingTrivia(TagSection(TagPair(TagValue, WS_TagValue)), NoPlies, WhitespaceTrivia),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),
            ("\n\"?\"\n\"?\"\n", OneGameTrailingTrivia(TagSection(TagPair(WS_TagValue, WS_TagValue)), NoPlies, WhitespaceTrivia),
                new[]
                {
                    PgnErrorCode.MultipleTagValues, PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose
                }),

            // Error tag values must behave like regular tag values, i.e. not generate extra errors.
            ("[Event \"\\u\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.UnrecognizedEscapeSequence }),
            ("[Event \"\n\"]", TagSectionOnly(TagPair(BracketOpen, TagName, WS_TagValue, BracketClose)),
                new[] { PgnErrorCode.IllegalControlCharacterInTagValue }),
        };
    }
}
