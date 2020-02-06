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
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="IPgnSymbol"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(IPgnSymbol node) => default;
        public virtual TResult Visit(IPgnSymbol node) => node == null ? default : node.Accept(this);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="IPgnSymbol"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class PgnSymbolVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(IPgnSymbol node, T arg) => default;
        public virtual TResult Visit(IPgnSymbol node, T arg) => node == null ? default : node.Accept(this, arg);
    }
}
