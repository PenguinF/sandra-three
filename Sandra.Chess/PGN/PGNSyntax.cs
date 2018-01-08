/*********************************************************************************
 * PGNSyntax.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
using System;

namespace Sandra.PGN
{
    public interface PGNTerminalSymbol : IEquatable<PGNTerminalSymbol>
    {
        string GetText();
    }

    public sealed class SpaceSymbol : PGNTerminalSymbol
    {
        public const string SpaceText = " ";
        public bool Equals(PGNTerminalSymbol other) => other is SpaceSymbol;
        public string GetText() => SpaceText;
    }

    public sealed class SideLineStartSymbol : PGNTerminalSymbol
    {
        public const string SideLineStartText = "(";
        public bool Equals(PGNTerminalSymbol other) => other is SideLineStartSymbol;
        public string GetText() => SideLineStartText;
    }

    public sealed class SideLineEndSymbol : PGNTerminalSymbol
    {
        public const string SideLineEndText = ")";
        public bool Equals(PGNTerminalSymbol other) => other is SideLineEndSymbol;
        public string GetText() => SideLineEndText;
    }

    public sealed class BlackToMoveEllipsisSymbol : PGNTerminalSymbol
    {
        public const string EllipsisText = "..";
        public bool Equals(PGNTerminalSymbol other) => other is BlackToMoveEllipsisSymbol;
        public string GetText() => EllipsisText;
    }

    public sealed class MoveCounterSymbol : PGNTerminalSymbol
    {
        readonly int value;
        public bool Equals(PGNTerminalSymbol other) => other is MoveCounterSymbol && value == ((MoveCounterSymbol)other).value;
        public string GetText() => value + ".";
        public MoveCounterSymbol(int value) { this.value = value; }
    }

    public sealed class FormattedMoveSymbol : PGNTerminalSymbol
    {
        readonly string value;
        public bool Equals(PGNTerminalSymbol other) => other is FormattedMoveSymbol && value == ((FormattedMoveSymbol)other).value;
        public string GetText() => value;
        public readonly Chess.Variation Variation;
        public FormattedMoveSymbol(string value, Chess.Variation variation)
        {
            this.value = value;
            Variation = variation;
        }
    }
}
