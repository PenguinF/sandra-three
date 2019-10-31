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
    }

    /// <summary>
    /// Represents a visitor that visits a <see cref="JsonSyntax"/> which has no child <see cref="JsonSyntax"/> nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonTerminalSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(JsonSyntax node) => default;
        public virtual TResult Visit(JsonSyntax node) => node == null ? default : node.Accept(this);
    }

    /// <summary>
    /// Represents a visitor that visits a <see cref="JsonSyntax"/> which has no child <see cref="JsonSyntax"/> nodes.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class JsonTerminalSymbolVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(JsonSyntax node, T arg) => default;
        public virtual TResult Visit(JsonSyntax node, T arg) => node == null ? default : node.Accept(this, arg);
    }
}
