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
    /// Represents a visitor that visits a single <see cref="GreenPgnBackgroundSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenPgnBackgroundSyntaxVisitor
    {
        public virtual void DefaultVisit(GreenPgnBackgroundSyntax node) { }
        public virtual void Visit(GreenPgnBackgroundSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitCommentSyntax(GreenPgnCommentSyntax node) => DefaultVisit(node);
        public virtual void VisitEscapeSyntax(GreenPgnEscapeSyntax node) => DefaultVisit(node);
        public virtual void VisitIllegalCharacterSyntax(GreenPgnIllegalCharacterSyntax node) => DefaultVisit(node);
        public virtual void VisitUnterminatedCommentSyntax(GreenPgnUnterminatedCommentSyntax node) => DefaultVisit(node);
        public virtual void VisitWhitespaceSyntax(GreenPgnWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenPgnBackgroundSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenPgnBackgroundSyntaxVisitor<TResult>
    {
        public virtual TResult DefaultVisit(GreenPgnBackgroundSyntax node) => default;
        public virtual TResult Visit(GreenPgnBackgroundSyntax node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitCommentSyntax(GreenPgnCommentSyntax node) => DefaultVisit(node);
        public virtual TResult VisitEscapeSyntax(GreenPgnEscapeSyntax node) => DefaultVisit(node);
        public virtual TResult VisitIllegalCharacterSyntax(GreenPgnIllegalCharacterSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUnterminatedCommentSyntax(GreenPgnUnterminatedCommentSyntax node) => DefaultVisit(node);
        public virtual TResult VisitWhitespaceSyntax(GreenPgnWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenPgnBackgroundSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenPgnBackgroundSyntaxVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(GreenPgnBackgroundSyntax node, T arg) => default;
        public virtual TResult Visit(GreenPgnBackgroundSyntax node, T arg) => node == null ? default : node.Accept(this, arg);
        public virtual TResult VisitCommentSyntax(GreenPgnCommentSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitEscapeSyntax(GreenPgnEscapeSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitIllegalCharacterSyntax(GreenPgnIllegalCharacterSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUnterminatedCommentSyntax(GreenPgnUnterminatedCommentSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitWhitespaceSyntax(GreenPgnWhitespaceSyntax node, T arg) => DefaultVisit(node, arg);
    }
}
