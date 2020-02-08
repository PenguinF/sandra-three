#region License
/*********************************************************************************
 * JsonValueSyntaxVisitor.cs
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenJsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenJsonValueSyntaxVisitor
    {
        public virtual void DefaultVisit(GreenJsonValueSyntax node) { }
        public virtual void Visit(GreenJsonValueSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitBooleanLiteralSyntax(GreenJsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitErrorStringSyntax(GreenJsonErrorStringSyntax node) => DefaultVisit(node);
        public virtual void VisitIntegerLiteralSyntax(GreenJsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitListSyntax(GreenJsonListSyntax node) => DefaultVisit(node);
        public virtual void VisitMapSyntax(GreenJsonMapSyntax node) => DefaultVisit(node);
        public virtual void VisitMissingValueSyntax(GreenJsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual void VisitStringLiteralSyntax(GreenJsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitUndefinedValueSyntax(GreenJsonUndefinedValueSyntax node) => DefaultVisit(node);
        public virtual void VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenJsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenJsonValueSyntaxVisitor<TResult>
    {
        public virtual TResult DefaultVisit(GreenJsonValueSyntax node) => default;
        public virtual TResult Visit(GreenJsonValueSyntax node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitBooleanLiteralSyntax(GreenJsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitErrorStringSyntax(GreenJsonErrorStringSyntax node) => DefaultVisit(node);
        public virtual TResult VisitIntegerLiteralSyntax(GreenJsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitListSyntax(GreenJsonListSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMapSyntax(GreenJsonMapSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMissingValueSyntax(GreenJsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitStringLiteralSyntax(GreenJsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUndefinedValueSyntax(GreenJsonUndefinedValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax node) => DefaultVisit(node);
    }

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
    /// Represents a visitor that visits a single <see cref="JsonValueSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonValueSyntaxVisitor
    {
        public virtual void DefaultVisit(JsonValueSyntax node) { }
        public virtual void Visit(JsonValueSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitErrorStringSyntax(JsonErrorStringSyntax node) => DefaultVisit(node);
        public virtual void VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitListSyntax(JsonListSyntax node) => DefaultVisit(node);
        public virtual void VisitMapSyntax(JsonMapSyntax node) => DefaultVisit(node);
        public virtual void VisitMissingValueSyntax(JsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual void VisitStringLiteralSyntax(JsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual void VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node) => DefaultVisit(node);
        public virtual void VisitUnknownSymbolSyntax(JsonUnknownSymbolSyntax node) => DefaultVisit(node);
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
        public virtual TResult VisitErrorStringSyntax(JsonErrorStringSyntax node) => DefaultVisit(node);
        public virtual TResult VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitListSyntax(JsonListSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMapSyntax(JsonMapSyntax node) => DefaultVisit(node);
        public virtual TResult VisitMissingValueSyntax(JsonMissingValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitStringLiteralSyntax(JsonStringLiteralSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUnknownSymbolSyntax(JsonUnknownSymbolSyntax node) => DefaultVisit(node);
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
}
