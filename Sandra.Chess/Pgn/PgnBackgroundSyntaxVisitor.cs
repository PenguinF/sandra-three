#region License
/*********************************************************************************
 * PgnBackgroundSyntaxVisitor.cs
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
    /// Represents a visitor that visits a single <see cref="PgnBackgroundSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnBackgroundSyntaxVisitor
    {
        public virtual void DefaultVisit(PgnBackgroundSyntax node) { }
        public virtual void Visit(PgnBackgroundSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitEscapeSyntax(PgnEscapeSyntax node) => DefaultVisit(node);
        public virtual void VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node) => DefaultVisit(node);
        public virtual void VisitWhitespaceSyntax(PgnWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="PgnBackgroundSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnBackgroundSyntaxVisitor<TResult>
    {
        public virtual TResult DefaultVisit(PgnBackgroundSyntax node) => default;
        public virtual TResult Visit(PgnBackgroundSyntax node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitEscapeSyntax(PgnEscapeSyntax node) => DefaultVisit(node);
        public virtual TResult VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node) => DefaultVisit(node);
        public virtual TResult VisitWhitespaceSyntax(PgnWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="PgnBackgroundSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnBackgroundSyntaxVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(PgnBackgroundSyntax node, T arg) => default;
        public virtual TResult Visit(PgnBackgroundSyntax node, T arg) => node == null ? default : node.Accept(this, arg);
        public virtual TResult VisitEscapeSyntax(PgnEscapeSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitWhitespaceSyntax(PgnWhitespaceSyntax node, T arg) => DefaultVisit(node, arg);
    }
}
