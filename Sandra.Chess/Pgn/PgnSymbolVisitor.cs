#region License
/*********************************************************************************
 * PgnSymbolVisitor.cs
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
    /// Represents a visitor that visits a single <see cref="IPgnSymbol"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnSymbolVisitor
    {
        public virtual void DefaultVisit(IPgnSymbol node) { }
        public virtual void Visit(IPgnSymbol node) { if (node != null) node.Accept(this); }
        public virtual void VisitBracketCloseSyntax(PgnBracketCloseSyntax node) => DefaultVisit(node);
        public virtual void VisitBracketOpenSyntax(PgnBracketOpenSyntax node) => DefaultVisit(node);
        public virtual void VisitCommentSyntax(PgnCommentSyntax node) => DefaultVisit(node);
        public virtual void VisitEscapeSyntax(PgnEscapeSyntax node) => DefaultVisit(node);
        public virtual void VisitGameResultSyntax(PgnGameResultSyntax node) => DefaultVisit(node);
        public virtual void VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node) => DefaultVisit(node);
        public virtual void VisitMoveNumberSyntax(PgnMoveNumberSyntax node) => DefaultVisit(node);
        public virtual void VisitMoveSyntax(PgnMoveSyntax node) => DefaultVisit(node);
        public virtual void VisitNagSyntax(PgnNagSyntax node) => DefaultVisit(node);
        public virtual void VisitParenthesisCloseSyntax(PgnParenthesisCloseSyntax node) => DefaultVisit(node);
        public virtual void VisitParenthesisOpenSyntax(PgnParenthesisOpenSyntax node) => DefaultVisit(node);
        public virtual void VisitPeriodSyntax(PgnPeriodSyntax node) => DefaultVisit(node);
        public virtual void VisitTagNameSyntax(PgnTagNameSyntax node) => DefaultVisit(node);
        public virtual void VisitTagValueSyntax(PgnTagValueSyntax node) => DefaultVisit(node);
        public virtual void VisitWhitespaceSyntax(PgnWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="IPgnSymbol"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(IPgnSymbol node) => default;
        public virtual TResult Visit(IPgnSymbol node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitBracketCloseSyntax(PgnBracketCloseSyntax node) => DefaultVisit(node);
        public virtual TResult VisitBracketOpenSyntax(PgnBracketOpenSyntax node) => DefaultVisit(node);
        public virtual TResult VisitCommentSyntax(PgnCommentSyntax node) => DefaultVisit(node);
        public virtual TResult VisitEscapeSyntax(PgnEscapeSyntax node) => DefaultVisit(node);
        public virtual TResult VisitGameResultSyntax(PgnGameResultSyntax node) => DefaultVisit(node);
        public virtual TResult VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMoveNumberSyntax(PgnMoveNumberSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMoveSyntax(PgnMoveSyntax node) => DefaultVisit(node);
        public virtual TResult VisitNagSyntax(PgnNagSyntax node) => DefaultVisit(node);
        public virtual TResult VisitParenthesisCloseSyntax(PgnParenthesisCloseSyntax node) => DefaultVisit(node);
        public virtual TResult VisitParenthesisOpenSyntax(PgnParenthesisOpenSyntax node) => DefaultVisit(node);
        public virtual TResult VisitPeriodSyntax(PgnPeriodSyntax node) => DefaultVisit(node);
        public virtual TResult VisitTagNameSyntax(PgnTagNameSyntax node) => DefaultVisit(node);
        public virtual TResult VisitTagValueSyntax(PgnTagValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitWhitespaceSyntax(PgnWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="IPgnSymbol"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnSymbolVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(IPgnSymbol node, T arg) => default;
        public virtual TResult Visit(IPgnSymbol node, T arg) => node == null ? default : node.Accept(this, arg);
        public virtual TResult VisitBracketCloseSyntax(PgnBracketCloseSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitBracketOpenSyntax(PgnBracketOpenSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitCommentSyntax(PgnCommentSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitEscapeSyntax(PgnEscapeSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitGameResultSyntax(PgnGameResultSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMoveNumberSyntax(PgnMoveNumberSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMoveSyntax(PgnMoveSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitNagSyntax(PgnNagSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitParenthesisCloseSyntax(PgnParenthesisCloseSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitParenthesisOpenSyntax(PgnParenthesisOpenSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitPeriodSyntax(PgnPeriodSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitTagNameSyntax(PgnTagNameSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitTagValueSyntax(PgnTagValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitWhitespaceSyntax(PgnWhitespaceSyntax node, T arg) => DefaultVisit(node, arg);
    }
}
