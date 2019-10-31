#region License
/*********************************************************************************
 * JsonTerminalSymbolVisitor.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a visitor that visits a <see cref="JsonSyntax"/> which has no child <see cref="JsonSyntax"/> nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonTerminalSymbolVisitor
    {
        public virtual void DefaultVisit(JsonSyntax node) { }
        public virtual void Visit(JsonSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitBackgroundSyntax(RedJsonBackgroundSyntax node) => DefaultVisit(node);
        public virtual void VisitBooleanLiteralSyntax(RedJsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitColon(RedJsonColon node) => DefaultVisit(node);
        public virtual void VisitComma(RedJsonComma node) => DefaultVisit(node);
        public virtual void VisitCurlyClose(RedJsonCurlyClose node) => DefaultVisit(node);
        public virtual void VisitCurlyOpen(RedJsonCurlyOpen node) => DefaultVisit(node);
        public virtual void VisitIntegerLiteralSyntax(RedJsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitMissingValueSyntax(RedJsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual void VisitStringLiteralSyntax(RedJsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitSquareBracketClose(RedJsonSquareBracketClose node) => DefaultVisit(node);
        public virtual void VisitSquareBracketOpen(RedJsonSquareBracketOpen node) => DefaultVisit(node);
        public virtual void VisitUndefinedValueSyntax(RedJsonUndefinedValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a <see cref="JsonSyntax"/> which has no child <see cref="JsonSyntax"/> nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonTerminalSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(JsonSyntax node) => default;
        public virtual TResult Visit(JsonSyntax node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitBackgroundSyntax(RedJsonBackgroundSyntax node) => DefaultVisit(node);
        public virtual TResult VisitBooleanLiteralSyntax(RedJsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitColon(RedJsonColon node) => DefaultVisit(node);
        public virtual TResult VisitComma(RedJsonComma node) => DefaultVisit(node);
        public virtual TResult VisitCurlyClose(RedJsonCurlyClose node) => DefaultVisit(node);
        public virtual TResult VisitCurlyOpen(RedJsonCurlyOpen node) => DefaultVisit(node);
        public virtual TResult VisitIntegerLiteralSyntax(RedJsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMissingValueSyntax(RedJsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitStringLiteralSyntax(RedJsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitSquareBracketClose(RedJsonSquareBracketClose node) => DefaultVisit(node);
        public virtual TResult VisitSquareBracketOpen(RedJsonSquareBracketOpen node) => DefaultVisit(node);
        public virtual TResult VisitUndefinedValueSyntax(RedJsonUndefinedValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a <see cref="JsonSyntax"/> which has no child <see cref="JsonSyntax"/> nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonTerminalSymbolVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(JsonSyntax node, T arg) => default;
        public virtual TResult Visit(JsonSyntax node, T arg) => node == null ? default : node.Accept(this, arg);
        public virtual TResult VisitBackgroundSyntax(RedJsonBackgroundSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitBooleanLiteralSyntax(RedJsonBooleanLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitColon(RedJsonColon node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitComma(RedJsonComma node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitCurlyClose(RedJsonCurlyClose node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitCurlyOpen(RedJsonCurlyOpen node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitIntegerLiteralSyntax(RedJsonIntegerLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMissingValueSyntax(RedJsonMissingValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitStringLiteralSyntax(RedJsonStringLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitSquareBracketClose(RedJsonSquareBracketClose node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitSquareBracketOpen(RedJsonSquareBracketOpen node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUndefinedValueSyntax(RedJsonUndefinedValueSyntax node, T arg) => DefaultVisit(node, arg);
    }
}
