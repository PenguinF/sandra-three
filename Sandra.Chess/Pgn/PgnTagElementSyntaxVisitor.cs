#region License
/*********************************************************************************
 * PgnTagElementSyntaxVisitor.cs
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
    /// Represents a visitor that visits a single <see cref="GreenPgnTagElementSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnTagElementSyntaxVisitor
    {
        public virtual void DefaultVisit(PgnTagElementSyntax node) { }
        public virtual void Visit(PgnTagElementSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitBracketCloseSyntax(PgnBracketCloseSyntax node) => DefaultVisit(node);
        public virtual void VisitBracketOpenSyntax(PgnBracketOpenSyntax node) => DefaultVisit(node);
        public virtual void VisitTagNameSyntax(PgnTagNameSyntax node) => DefaultVisit(node);
        public virtual void VisitTagValueSyntax(PgnTagValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenPgnTagElementSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnTagElementSyntaxVisitor<TResult>
    {
        public virtual TResult DefaultVisit(PgnTagElementSyntax node) => default;
        public virtual TResult Visit(PgnTagElementSyntax node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitBracketCloseSyntax(PgnBracketCloseSyntax node) => DefaultVisit(node);
        public virtual TResult VisitBracketOpenSyntax(PgnBracketOpenSyntax node) => DefaultVisit(node);
        public virtual TResult VisitTagNameSyntax(PgnTagNameSyntax node) => DefaultVisit(node);
        public virtual TResult VisitTagValueSyntax(PgnTagValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenPgnTagElementSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnTagElementSyntaxVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(PgnTagElementSyntax node, T arg) => default;
        public virtual TResult Visit(PgnTagElementSyntax node, T arg) => node == null ? default : node.Accept(this, arg);
        public virtual TResult VisitBracketCloseSyntax(PgnBracketCloseSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitBracketOpenSyntax(PgnBracketOpenSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitTagNameSyntax(PgnTagNameSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitTagValueSyntax(PgnTagValueSyntax node, T arg) => DefaultVisit(node, arg);
    }
}
