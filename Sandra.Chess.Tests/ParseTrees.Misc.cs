#region License
/*********************************************************************************
 * ParseTrees.Misc.cs
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
        internal static List<(string, ParseTree, PgnErrorCode[])> MiscParseTrees() => new List<(string, ParseTree, PgnErrorCode[])>
        {
            // Number of games sanity.
            ("* [Event \"?\"]", Games(
                Game(ResultNoTrivia),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose)), NoPlies)),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("* [Event \"?\"] *", Games(
                Game(ResultNoTrivia),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose)), NoPlies, WS_Result)),
                new[] { PgnErrorCode.MissingTagSection }),
            ("* [Event \"?\"] * [Event \"?\"]", Games(
                Game(ResultNoTrivia),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose)), NoPlies, WS_Result),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, WS_TagValue, BracketClose)), NoPlies)),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            // Entering a result should turn "[Ra1" into a genuine tag pair.
            ("[Event \"?\"] 1.a8=N! [Ra1", Games(
                Game(SmallestTagSectionWithWhitespace, Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia)),
                    Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia))))),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.MissingGameTerminationMarker }),
            ("[Event \"?\"] 1.a8=N! 1[Ra1", Games(
                Game(SmallestTagSectionWithWhitespace, Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia)),
                    Ply(WS_MoveNumberNoFloats, WithFloats(Floats(OrphanTagElementNoTrivia), MoveNoTrivia))))),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.MissingGameTerminationMarker }),
            ("[Event \"?\"] 1.a8=N! 1-[Ra1", Games(
                Game(SmallestTagSectionWithWhitespace, Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia)),
                    Ply(WS_MoveNoFloats),
                    Ply(WithFloats(Floats(OrphanTagElementNoTrivia), MoveNoTrivia))))),
                new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.OrphanBracketOpen, PgnErrorCode.MissingGameTerminationMarker }),
            ("[Event \"?\"] 1.a8=N! 1-0[Ra1", Games(
                Game(SmallestTagSectionWithWhitespace, Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia))),
                    WS_Result),
                Game(TagSection(TagPair(BracketOpen, TagName)), NoPlies)),
                new[] { PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),

            // Deleting a game by using the backspace key should never merge both games into one, except at the very last test case.
            // Going from:
            // "[A"?"] 1. e4 (1. d4) * [A"?""
            // to:
            // "[A"?"] [A"?""
            // Starting with bonus test case "[A"?"] 1. e4 (1. d4 * [A"?"" where one closing parenthesis is missing.
            ("[A\"?\"] 1. e4 (1. d4 * [A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move), VariationNoFloats(WS_Open,
                        Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)))))),
                    WS_Result),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.MissingParenthesisClose,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] 1. e4 (1. d4) * [A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move), VariationNoFloats(WS_Open,
                        Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))), CloseNoTrivia))),
                    WS_Result),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, TagValue)), NoPlies)),
                new[] { PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] 1. e4 (1. d4) [A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move), VariationNoFloats(WS_Open,
                        Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))), CloseNoTrivia)))),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] 1. e4 (1. d4[A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move), VariationNoFloats(WS_Open,
                        Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))))))),
                Game(TagSection(TagPair(BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] 1. e4 (1. [A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move), VariationNoFloats(WS_Open,
                        Plies(Ply(MoveNumberNoFloats), OnePeriod))))),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.MissingMove, PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] 1. e4 (1[A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move), VariationNoFloats(WS_Open,
                        Plies(Ply(MoveNumberNoFloats)))))),
                Game(TagSection(TagPair(BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.MissingMove, PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] 1. e4 ([A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move), VariationNoFloats(WS_Open,
                        Plies())))),
                Game(TagSection(TagPair(BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.EmptyVariation, PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] 1. e4 [A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)))),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] 1. [A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats), OnePeriod)),
                Game(TagSection(TagPair(WS_BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.MissingMove, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] 1[A\"?\"", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats))),
                Game(TagSection(TagPair(BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.MissingMove, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"] [A\"?\"", Games(
                Game(
                    TagSection(
                        TagPair(BracketOpen, TagName, TagValue, BracketClose),
                        TagPair(WS_BracketOpen, TagName, TagValue)),
                    Plies())),
                new[] { PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),

            // Accidentally creating a tag pair inside a move tree shouldn't split the game into two.
            // This contrasts with the scenario above where a game is deleted using the backspace key,
            // where the trailing tag pair should not be regarded as a typo but a genuine tag pair.

            // Scenario 1: User wants to starts annotation between 'd4' and 'd5', presses '{' but forgets SHIFT key.
            //             Some editors add the matching ']' as well, so test both.
            ("[A\"?\"] 1. d4 [ d5 *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WithFloats(Floats(WS_OrphanTagElement), WS_Move))),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen }),
            ("[A\"?\"] 1. d4 [] d5 *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WithFloats(Floats(WS_OrphanTagElement, OrphanTagElementNoTrivia), WS_Move))),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.OrphanBracketClose }),

            // Scenario 2: user forgets SHIFT key, surrounds 'annotation' with [] rather than {}.
            ("[A\"?\"] 1. d4 annotation *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WS_MoveNoFloats)),
                WS_Result),
                new[] { PgnErrorCode.UnrecognizedMove }),
            ("[A\"?\"] 1. d4 [annotation] *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia)),
                    Floats(OrphanTagElementNoTrivia)),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.UnrecognizedMove, PgnErrorCode.OrphanBracketClose }),

            // Scenario 3: user keeps on typing after typo.
            ("[A\"?\"] 1. d4 [e4 was Fischer's choice.} *", OneGame(
                SmallestCorrectTagSection,
                PliesTrailingFloatItems(OnePeriod,  // This is actually the '.' after 'choice'.
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia)),
                    Ply(WS_MoveNoFloats),   //was
                    Ply(WS_MoveNoFloats),   //Fischer
                    Ply(NoFloats(new ParseTree<PgnMoveWithTriviaSyntax> { IllegalCharacterTrivia, Move })),   //s
                    Ply(WS_MoveNoFloats)),  //choice
                ResultWithTrivia(new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { IllegalCharacter, WhitespaceElement } })),
                new[] {
                    PgnErrorCode.OrphanBracketOpen, PgnErrorCode.UnrecognizedMove, PgnErrorCode.IllegalCharacter,
                    PgnErrorCode.UnrecognizedMove, PgnErrorCode.UnrecognizedMove, PgnErrorCode.UnrecognizedMove,
                    PgnErrorCode.OrphanPeriod, PgnErrorCode.IllegalCharacter }),

            // Progression of states to decide whether or not to start a new tag section.
            // First section is cases that should not trigger a new tag section, second section is cases that should.
            ("[A\"?\"] 1. d4 Bf9 *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WS_MoveNoFloats)),
                WS_Result),
                new[] { PgnErrorCode.UnrecognizedMove }),
            ("[A\"?\"] 1. d4 Event *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WS_MoveNoFloats)),
                WS_Result),
                new[] { PgnErrorCode.UnrecognizedMove }),
            ("[A\"?\"] 1. d4 Bf1+ *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WS_MoveNoFloats)),
                WS_Result),
                new PgnErrorCode[0]),
            ("[A\"?\"] 1. d4 \"?\" *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Floats(WS_OrphanTagElement)),
                WS_Result),
                new[] { PgnErrorCode.OrphanTagValue }),
            ("[A\"?\"] 1. d4 [ $1 *", OneGame(
                SmallestCorrectTagSection,
                Plies(Ply(
                    WS_MoveNumberNoFloats,
                    WithFloats(OnePeriod, WS_Move),
                    WithFloats(Floats(WS_OrphanTagElement), WS_NAG))),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen }),
            ("[A\"?\"] 1. d4 [[ $1 *", OneGame(
                SmallestCorrectTagSection,
                Plies(Ply(
                    WS_MoveNumberNoFloats,
                    WithFloats(OnePeriod, WS_Move),
                    WithFloats(Floats(WS_OrphanTagElement, OrphanTagElementNoTrivia), WS_NAG))),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.OrphanBracketOpen }),
            ("[A\"?\"] 1. d4 [[[ $1 *", OneGame(
                SmallestCorrectTagSection,
                Plies(Ply(
                    WS_MoveNumberNoFloats,
                    WithFloats(OnePeriod, WS_Move),
                    WithFloats(Floats(WS_OrphanTagElement, OrphanTagElementNoTrivia, OrphanTagElementNoTrivia), WS_NAG))),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.OrphanBracketOpen, PgnErrorCode.OrphanBracketOpen }),
            ("[A\"?\"] 1. d4 [\"?\"[\"?\"[ $1 *", OneGame(
                SmallestCorrectTagSection,
                Plies(Ply(
                    WS_MoveNumberNoFloats,
                    WithFloats(OnePeriod, WS_Move),
                    WithFloats(Floats(WS_OrphanTagElement, OrphanTagElementNoTrivia, OrphanTagElementNoTrivia, OrphanTagElementNoTrivia, OrphanTagElementNoTrivia), WS_NAG))),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketOpen, PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketOpen }),
            ("[A\"?\"] 1. d4 [Event[\"?\"[ $1 *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia),
                        WithFloats(Floats(OrphanTagElementNoTrivia, OrphanTagElementNoTrivia, OrphanTagElementNoTrivia), WS_NAG))),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.UnrecognizedMove, PgnErrorCode.OrphanBracketOpen, PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketOpen }),
            ("[A\"?\"] 1. d4 [Event Event \"?\"] $1 *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia)),
                    Ply(WS_MoveNoFloats, WithFloats(Floats(WS_OrphanTagElement, OrphanTagElementNoTrivia), WS_NAG))),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.UnrecognizedMove, PgnErrorCode.UnrecognizedMove, PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketClose }),
            ("[A\"?\"] 1. d4 [Ne1+ \"?\"] $1 *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia),
                        WithFloats(Floats(WS_OrphanTagElement, OrphanTagElementNoTrivia), WS_NAG))),
                WS_Result),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketClose }),
            ("[A\"?\"] 1. d4 [Ne9+ \"?\"] $1 *", OneGame(
                SmallestCorrectTagSection,
                Plies(
                    Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                    Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia),
                        WithFloats(Floats(WS_OrphanTagElement, OrphanTagElementNoTrivia), WS_NAG))),
                WS_Result),
                new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.OrphanBracketOpen, PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketClose }),

            ("[A\"?\"] 1. d4 [A\"?\"] $1 *", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)))),
                Game(
                    TagSection(TagPair(WS_BracketOpen, TagName, TagValue, BracketClose)),
                    Plies(Ply(WS_NAGNoFloats)),
                    WS_Result)),
                new[] {
                    PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("[A\"?\"] 1. d4 [[A\"?\"] $1 *", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(
                        Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                        Floats(WS_OrphanTagElement))),
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_NAGNoFloats)),
                    WS_Result)),
                new[] {
                    PgnErrorCode.OrphanBracketOpen, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("[A\"?\"] 1. d4 [A[A\"?\"] $1 *", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(
                        Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                        Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia)))),
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_NAGNoFloats)),
                    WS_Result)),
                new[] {
                    PgnErrorCode.OrphanBracketOpen, PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("[A\"?\"] 1. d4 [A[[A\"?\"] $1 *", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(
                        Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                        Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia)),
                        Floats(OrphanTagElementNoTrivia))),
                Game(
                    SmallestCorrectTagSection,
                    Plies(Ply(WS_NAGNoFloats)),
                    WS_Result)),
                new[] {
                    PgnErrorCode.OrphanBracketOpen, PgnErrorCode.UnrecognizedMove, PgnErrorCode.OrphanBracketOpen, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("[A\"?\"] 1. d4 [ *[A\"?\"]*", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(
                        Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                        Floats(WS_OrphanTagElement)),
                    WS_Result),
                Game(
                    SmallestCorrectTagSection,
                    NoPlies,
                    ResultNoTrivia)),
                new[] { PgnErrorCode.OrphanBracketOpen }),
            ("[A\"?\"] 1. d4 [A *[A\"?\"]*", Games(
                Game(
                    SmallestCorrectTagSection,
                    Plies(
                        Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)),
                        Ply(WithFloats(Floats(WS_OrphanTagElement), MoveNoTrivia))),
                    WS_Result),
                Game(
                    SmallestCorrectTagSection,
                    NoPlies,
                    ResultNoTrivia)),
                new[] { PgnErrorCode.OrphanBracketOpen, PgnErrorCode.UnrecognizedMove }),

            // Error tag values should also trigger.
            ("[A\"?\"]1[A\"\\?\"]", Games(
                Game(SmallestCorrectTagSection, Plies(Ply(MoveNumberNoFloats))),
                Game(SmallestCorrectTagSection, NoPlies)),
                new[] {
                    PgnErrorCode.UnrecognizedEscapeSequence, PgnErrorCode.MissingMove, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingGameTerminationMarker }),
            ("[A\"?\"]1[A\"", Games(
                Game(SmallestCorrectTagSection, Plies(Ply(MoveNumberNoFloats))),
                Game(TagSection(TagPair(BracketOpen, TagName, TagValue)), NoPlies)),
                new[] {
                    PgnErrorCode.UnterminatedTagValue, PgnErrorCode.MissingMove, PgnErrorCode.MissingGameTerminationMarker,
                    PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),

            // Cases in which a move text or unrecognized move is seen while in a tag section.
            // Only inside a tag pair can a move text become a tag name, but only of course if they are valid tag names too.
            // Vary on the second tag name and the presence of its preceding '['.

            // Valid tag name, not a valid move: always stay in tag section.
            ("[Qf0\"?\"][Qf0\"?\"] *", OneGame(
                TagSection(
                    TagPair(BracketOpen, TagName, TagValue, BracketClose),
                    TagPair(BracketOpen, TagName, TagValue, BracketClose)),
                NoPlies,
                WS_Result),
                new PgnErrorCode[0] { }),
            ("[Qf0\"?\"]Qf0\"?\"] *", OneGame(
                TagSection(
                    TagPair(BracketOpen, TagName, TagValue, BracketClose),
                    TagPair(TagName, TagValue, BracketClose)),
                NoPlies,
                WS_Result),
                new [] { PgnErrorCode.MissingTagBracketOpen }),

            // Not a valid tag name, valid move: always trigger move section.
            ("[A\"?\"][a3+\"?\"] *", OneGame(
                TagSection(
                    TagPair(BracketOpen, TagName, TagValue, BracketClose),
                    TagPair(BracketOpen)),
                Plies(Ply(MoveNoFloats), Floats(OrphanTagElementNoTrivia, OrphanTagElementNoTrivia)),
                WS_Result),
                new[] {
                    PgnErrorCode.EmptyTag, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketClose, PgnErrorCode.MissingMoveNumber }),
            ("[A\"?\"]a3+\"?\"] *", OneGame(
                TagSection(TagPair(BracketOpen, TagName, TagValue, BracketClose)),
                Plies(Ply(MoveNoFloats), Floats(OrphanTagElementNoTrivia, OrphanTagElementNoTrivia)),
                WS_Result),
                new[] { PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketClose, PgnErrorCode.MissingMoveNumber }),

            // Both a valid tag name and a valid move: depends on being in a tag pair.
            ("[A\"?\"][Qa1a2\"?\"] *", OneGame(
                TagSection(
                    TagPair(BracketOpen, TagName, TagValue, BracketClose),
                    TagPair(BracketOpen, TagName, TagValue, BracketClose)),
                NoPlies,
                WS_Result),
                new PgnErrorCode[0] { }),
            ("[A\"?\"]Qa1a2\"?\"] *", OneGame(
                TagSection(TagPair(BracketOpen, TagName, TagValue, BracketClose)),
                Plies(Ply(MoveNoFloats), Floats(OrphanTagElementNoTrivia, OrphanTagElementNoTrivia)),
                WS_Result),
                new[] { PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketClose, PgnErrorCode.MissingMoveNumber }),

            // Only a '[' should trigger reinterpretation of a move text as a tag name.
            ("\"?\"[Qa1a2\"?\"] *", OneGame(
                TagSection(
                    TagPair(TagValue),
                    TagPair(BracketOpen, TagName, TagValue, BracketClose)),
                NoPlies,
                WS_Result),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose }),
            ("\"?\"Qa1a2\"?\"] *", OneGame(
                TagSection(TagPair(TagValue)),
                Plies(Ply(MoveNoFloats), Floats(OrphanTagElementNoTrivia, OrphanTagElementNoTrivia)),
                WS_Result),
                new[] {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagName, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketClose, PgnErrorCode.MissingMoveNumber }),
            ("A\"?\"[Qa1a2\"?\"] *", OneGame(
                TagSection(
                    TagPair(TagName, TagValue),
                    TagPair(BracketOpen, TagName, TagValue, BracketClose)),
                NoPlies,
                WS_Result),
                new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose }),
            ("A\"?\"Qa1a2\"?\"] *", OneGame(
                TagSection(TagPair(TagName, TagValue)),
                Plies(Ply(MoveNoFloats), Floats(OrphanTagElementNoTrivia, OrphanTagElementNoTrivia)),
                WS_Result),
                new[] {
                    PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagBracketClose,
                    PgnErrorCode.OrphanTagValue, PgnErrorCode.OrphanBracketClose, PgnErrorCode.MissingMoveNumber }),

            // Special case. Someone's going to try this someday.
            ("½-½", OneGameTrailingTrivia(
                EmptyTagSection,
                Plies(Ply(NoFloats(new ParseTree<PgnMoveWithTriviaSyntax> { IllegalCharacterTrivia, Move }))), IllegalCharacterTrivia),
                new[] {
                    PgnErrorCode.IllegalCharacter, PgnErrorCode.IllegalCharacter, PgnErrorCode.UnrecognizedMove,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
        };
    }
}
