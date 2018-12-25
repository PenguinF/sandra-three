#region License
/*********************************************************************************
 * JsonSyntaxNodeVisitor.cs
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
#endregion

namespace SysExtensions.Text.Json
{
    /// <summary>
    /// Represents a visitor that visits a single <see cref="JsonSyntaxNode"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonSyntaxNodeVisitor
    {
        public virtual void DefaultVisit(JsonSyntaxNode node) { }
        public virtual void Visit(JsonSyntaxNode node) { if (node != null) node.Accept(this); }
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="JsonSyntaxNode"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonSyntaxNodeVisitor<TResult>
    {
        public virtual TResult DefaultVisit(JsonSyntaxNode node) => default(TResult);
        public virtual TResult Visit(JsonSyntaxNode node) => node == null ? default(TResult) : node.Accept(this);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="JsonSyntaxNode"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonSyntaxNodeVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(JsonSyntaxNode node, T arg) => default(TResult);
        public virtual TResult Visit(JsonSyntaxNode node, T arg) => node == null ? default(TResult) : node.Accept(this, arg);
    }
}
