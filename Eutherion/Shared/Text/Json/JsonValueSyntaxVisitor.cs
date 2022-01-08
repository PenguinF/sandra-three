#region License
/*********************************************************************************
 * JsonValueSyntaxVisitor.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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

using System;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenJsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenJsonValueSyntaxVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(GreenJsonValueSyntax node, T arg) => default;
        public virtual TResult Visit(GreenJsonValueSyntax node, T arg) => node == null ? default : node.Accept(this, arg);

        public virtual TResult VisitBooleanLiteralSyntax(GreenJsonBooleanLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitErrorStringSyntax(GreenJsonErrorStringSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitIntegerLiteralSyntax(GreenJsonIntegerLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitListSyntax(GreenJsonListSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMapSyntax(GreenJsonMapSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMissingValueSyntax(GreenJsonMissingValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitStringLiteralSyntax(GreenJsonStringLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUndefinedValueSyntax(GreenJsonUndefinedValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax node, T arg) => DefaultVisit(node, arg);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenJsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenJsonValueSyntaxVisitor<TResult> : GreenJsonValueSyntaxVisitor<_void, TResult>
    {
        public virtual TResult Visit(GreenJsonValueSyntax node) => Visit(node, _void._);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenJsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenJsonValueSyntaxVisitor : GreenJsonValueSyntaxVisitor<_void, _void>
    {
        public virtual void Visit(GreenJsonValueSyntax node) => Visit(node, _void._);
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
        public virtual TResult VisitErrorStringSyntax(JsonErrorStringSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitListSyntax(JsonListSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMapSyntax(JsonMapSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitMissingValueSyntax(JsonMissingValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitStringLiteralSyntax(JsonStringLiteralSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUnknownSymbolSyntax(JsonUnknownSymbolSyntax node, T arg) => DefaultVisit(node, arg);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="JsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonValueSyntaxVisitor<TResult> : JsonValueSyntaxVisitor<_void, TResult>
    {
        public virtual TResult Visit(JsonValueSyntax node) => Visit(node, _void._);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="JsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonValueSyntaxVisitor : JsonValueSyntaxVisitor<_void, _void>
    {
        public virtual void Visit(JsonValueSyntax node) => Visit(node, _void._);
    }
}
