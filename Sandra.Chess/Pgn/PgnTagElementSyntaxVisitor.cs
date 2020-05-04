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
    public abstract class GreenPgnTagElementSyntaxVisitor
    {
        public virtual void DefaultVisit(GreenPgnTagElementSyntax node) { }
        public virtual void Visit(GreenPgnTagElementSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitBracketCloseSyntax(GreenPgnBracketCloseSyntax node) => DefaultVisit(node);
        public virtual void VisitBracketOpenSyntax(GreenPgnBracketOpenSyntax node) => DefaultVisit(node);
        public virtual void VisitErrorTagValueSyntax(GreenPgnErrorTagValueSyntax node) => DefaultVisit(node);
        public virtual void VisitTagNameSyntax(GreenPgnTagNameSyntax node) => DefaultVisit(node);
        public virtual void VisitTagValueSyntax(GreenPgnTagValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenPgnTagElementSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenPgnTagElementSyntaxVisitor<TResult>
    {
        public virtual TResult DefaultVisit(GreenPgnTagElementSyntax node) => default;
        public virtual TResult Visit(GreenPgnTagElementSyntax node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitBracketCloseSyntax(GreenPgnBracketCloseSyntax node) => DefaultVisit(node);
        public virtual TResult VisitBracketOpenSyntax(GreenPgnBracketOpenSyntax node) => DefaultVisit(node);
        public virtual TResult VisitErrorTagValueSyntax(GreenPgnErrorTagValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitTagNameSyntax(GreenPgnTagNameSyntax node) => DefaultVisit(node);
        public virtual TResult VisitTagValueSyntax(GreenPgnTagValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenPgnTagElementSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenPgnTagElementSyntaxVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(GreenPgnTagElementSyntax node, T arg) => default;
        public virtual TResult Visit(GreenPgnTagElementSyntax node, T arg) => node == null ? default : node.Accept(this, arg);
        public virtual TResult VisitBracketCloseSyntax(GreenPgnBracketCloseSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitBracketOpenSyntax(GreenPgnBracketOpenSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitErrorTagValueSyntax(GreenPgnErrorTagValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitTagNameSyntax(GreenPgnTagNameSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitTagValueSyntax(GreenPgnTagValueSyntax node, T arg) => DefaultVisit(node, arg);
    }
}
