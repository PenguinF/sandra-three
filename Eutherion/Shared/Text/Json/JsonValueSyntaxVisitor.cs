#region License
/*********************************************************************************
 * JsonValueSyntaxVisitor.cs
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
    /// Represents a visitor that visits a single <see cref="JsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonValueSyntaxVisitor
    {
        public virtual void DefaultVisit(JsonValueSyntax node) { }
        public virtual void Visit(JsonValueSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitListSyntax(JsonListSyntax node) => DefaultVisit(node);
        public virtual void VisitMapSyntax(JsonMapSyntax node) => DefaultVisit(node);
        public virtual void VisitMissingValueSyntax(JsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual void VisitStringLiteralSyntax(JsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="JsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonValueSyntaxVisitor<TResult>
    {
        public virtual TResult DefaultVisit(JsonValueSyntax node) => default;
        public virtual TResult Visit(JsonValueSyntax node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitListSyntax(JsonListSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMapSyntax(JsonMapSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMissingValueSyntax(JsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitStringLiteralSyntax(JsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="JsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonValueSyntaxVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(JsonValueSyntax node, T arg) => default;
        public virtual TResult Visit(JsonValueSyntax node, T arg) => node == null ? default : node.Accept(this, arg);
        public virtual TResult VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitListSyntax(JsonListSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMapSyntax(JsonMapSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMissingValueSyntax(JsonMissingValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitStringLiteralSyntax(JsonStringLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node, T arg) => DefaultVisit(node, arg);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="RedJsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class RedJsonValueSyntaxVisitor
    {
        public virtual void DefaultVisit(RedJsonValueSyntax node) { }
        public virtual void Visit(RedJsonValueSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitBooleanLiteralSyntax(RedJsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitIntegerLiteralSyntax(RedJsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitListSyntax(RedJsonListSyntax node) => DefaultVisit(node);
        public virtual void VisitMissingValueSyntax(RedJsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual void VisitStringLiteralSyntax(RedJsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitUndefinedValueSyntax(RedJsonUndefinedValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="RedJsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class RedJsonValueSyntaxVisitor<TResult>
    {
        public virtual TResult DefaultVisit(RedJsonValueSyntax node) => default;
        public virtual TResult Visit(RedJsonValueSyntax node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitBooleanLiteralSyntax(RedJsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitIntegerLiteralSyntax(RedJsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitListSyntax(RedJsonListSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMissingValueSyntax(RedJsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitStringLiteralSyntax(RedJsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUndefinedValueSyntax(RedJsonUndefinedValueSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="RedJsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class RedJsonValueSyntaxVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(RedJsonValueSyntax node, T arg) => default;
        public virtual TResult Visit(RedJsonValueSyntax node, T arg) => node == null ? default : node.Accept(this, arg);
        public virtual TResult VisitBooleanLiteralSyntax(RedJsonBooleanLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitIntegerLiteralSyntax(RedJsonIntegerLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitListSyntax(RedJsonListSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMissingValueSyntax(RedJsonMissingValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitStringLiteralSyntax(RedJsonStringLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUndefinedValueSyntax(RedJsonUndefinedValueSyntax node, T arg) => DefaultVisit(node, arg);
    }
}
