#region License
/*********************************************************************************
 * JsonSymbolEnumerator.cs
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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    public class JsonSymbolEnumerator : IEnumerable<JsonSymbol>
    {
        // Originally I tried to use yield return expressions but it turns out that the compiler
        // does not generate an entirely correct state machine from it in this case. It has
        // trouble with JsonComma.Value and refuses to yield return it more than once inside
        // the loop over the elements of an object or array, causing syntax highlighting to skew.
        // So yeah, this is an implementation by hand.

        private class EmptyJsonSymbolEnumerator : IEnumerator<JsonSymbol>
        {
            public static readonly EmptyJsonSymbolEnumerator Instance = new EmptyJsonSymbolEnumerator();

            private EmptyJsonSymbolEnumerator() { }

            void IEnumerator.Reset() { }
            bool IEnumerator.MoveNext() => false;
            JsonSymbol IEnumerator<JsonSymbol>.Current => default;
            object IEnumerator.Current => default;
            void IDisposable.Dispose() { }
        }

        private class SingleJsonSymbolEnumerator : IEnumerator<JsonSymbol>
        {
            private readonly JsonSymbol symbol;
            private bool yielded;

            public SingleJsonSymbolEnumerator(JsonSymbol symbol) => this.symbol = symbol;

            // When called from a foreach construct, it goes:
            // MoveNext() Current MoveNext() Dispose()
            void IEnumerator.Reset() { }
            bool IEnumerator.MoveNext() { if (yielded) return false; yielded = true; return true; }
            JsonSymbol IEnumerator<JsonSymbol>.Current => symbol;
            object IEnumerator.Current => symbol;
            void IDisposable.Dispose() { }
        }

        private class JsonSyntaxNodeWithBackgroundBeforeEnumerator : IEnumerator<JsonSymbol>
        {
            private readonly JsonValueWithBackgroundSyntax jsonSyntaxNodeWithBackgroundBefore;

            public JsonSyntaxNodeWithBackgroundBeforeEnumerator(JsonValueWithBackgroundSyntax jsonSyntaxNodeWithBackgroundBefore)
            {
                this.jsonSyntaxNodeWithBackgroundBefore = jsonSyntaxNodeWithBackgroundBefore;
                currentEnumerator = jsonSyntaxNodeWithBackgroundBefore.BackgroundBefore.BackgroundSymbols.GetEnumerator();
                currentMoveNext = BackgroundBeforeMoveNext;
            }

            private JsonSymbol current;
            private IEnumerator<JsonSymbol> currentEnumerator;
            private Func<bool> currentMoveNext;

            private bool BackgroundBeforeMoveNext()
            {
                if (currentEnumerator.MoveNext())
                {
                    current = currentEnumerator.Current;
                    return true;
                }

                currentEnumerator = EnumeratorSelector.Instance.Visit(jsonSyntaxNodeWithBackgroundBefore.ContentNode);
                currentMoveNext = ValueNodeMoveNext;
                return ValueNodeMoveNext();
            }

            private bool ValueNodeMoveNext()
            {
                if (currentEnumerator.MoveNext())
                {
                    current = currentEnumerator.Current;
                    return true;
                }

                return false;
            }

            void IEnumerator.Reset() { }
            bool IEnumerator.MoveNext() => currentMoveNext();
            JsonSymbol IEnumerator<JsonSymbol>.Current => current;
            object IEnumerator.Current => current;
            void IDisposable.Dispose() { }
        }

        private class JsonMultiValueSyntaxNodeEnumerator : IEnumerator<JsonSymbol>
        {
            private readonly JsonMultiValueSyntax jsonMultiValueSyntaxNode;

            public JsonMultiValueSyntaxNodeEnumerator(JsonMultiValueSyntax jsonMultiValueSyntaxNode)
            {
                this.jsonMultiValueSyntaxNode = jsonMultiValueSyntaxNode;
                currentEnumerator = new JsonSyntaxNodeWithBackgroundBeforeEnumerator(jsonMultiValueSyntaxNode.ValueNode);
                currentMoveNext = NodeWithBackgroundMoveNext;
            }

            private JsonSymbol current;
            private IEnumerator<JsonValueWithBackgroundSyntax> ignoredNodesEnumerator;
            private IEnumerator<JsonSymbol> currentEnumerator;
            private Func<bool> currentMoveNext;

            private bool NodeWithBackgroundMoveNext()
            {
                if (currentEnumerator.MoveNext())
                {
                    current = currentEnumerator.Current;
                    return true;
                }

                ignoredNodesEnumerator = jsonMultiValueSyntaxNode.IgnoredNodes.GetEnumerator();
                return IgnoredNodesMoveNext();
            }

            private bool IgnoredNodesMoveNext()
            {
                if (ignoredNodesEnumerator.MoveNext())
                {
                    currentEnumerator = new JsonSyntaxNodeWithBackgroundBeforeEnumerator(ignoredNodesEnumerator.Current);
                    currentMoveNext = IgnoredNodeWithBackgroundMoveNext;
                    return IgnoredNodeWithBackgroundMoveNext();
                }

                currentEnumerator = jsonMultiValueSyntaxNode.BackgroundAfter.BackgroundSymbols.GetEnumerator();
                currentMoveNext = BackgroundAfterMoveNext;
                return BackgroundAfterMoveNext();
            }

            private bool IgnoredNodeWithBackgroundMoveNext()
            {
                if (currentEnumerator.MoveNext())
                {
                    current = currentEnumerator.Current;
                    return true;
                }

                return IgnoredNodesMoveNext();
            }

            private bool BackgroundAfterMoveNext()
            {
                if (currentEnumerator.MoveNext())
                {
                    current = currentEnumerator.Current;
                    return true;
                }

                return false;
            }

            void IEnumerator.Reset() { }
            bool IEnumerator.MoveNext() => currentMoveNext();
            JsonSymbol IEnumerator<JsonSymbol>.Current => current;
            object IEnumerator.Current => current;
            void IDisposable.Dispose() { }
        }

        private class JsonListSyntaxEnumerator : IEnumerator<JsonSymbol>
        {
            private readonly JsonListSyntax jsonListSyntax;

            public JsonListSyntaxEnumerator(JsonListSyntax jsonListSyntax)
            {
                this.jsonListSyntax = jsonListSyntax;
                currentMoveNext = SquareBracketOpenMoveNext;
            }

            private JsonSymbol current;
            private bool comma;
            private IEnumerator<JsonMultiValueSyntax> elementEnumerator;
            private IEnumerator<JsonSymbol> elementValueEnumerator;
            private Func<bool> currentMoveNext;

            private bool SquareBracketOpenMoveNext()
            {
                current = JsonSquareBracketOpen.Value;
                elementEnumerator = jsonListSyntax.ElementNodes.GetEnumerator();
                currentMoveNext = ElementsMoveNext;
                return true;
            }

            private bool ElementsMoveNext()
            {
                if (elementEnumerator.MoveNext())
                {
                    elementValueEnumerator = new JsonMultiValueSyntaxNodeEnumerator(elementEnumerator.Current);
                    currentMoveNext = ElementNodeValueMoveNext;

                    if (comma)
                    {
                        current = JsonComma.Value;
                        return true;
                    }
                    else
                    {
                        comma = true;
                    }

                    return ElementNodeValueMoveNext();
                }

                if (!jsonListSyntax.MissingSquareBracketClose)
                {
                    currentMoveNext = NoMoveNext;
                    current = JsonSquareBracketClose.Value;
                    return true;
                }

                return false;
            }

            private bool ElementNodeValueMoveNext()
            {
                if (elementValueEnumerator.MoveNext())
                {
                    current = elementValueEnumerator.Current;
                    return true;
                }

                return ElementsMoveNext();
            }

            private bool NoMoveNext() => false;

            void IEnumerator.Reset() { }
            bool IEnumerator.MoveNext() => currentMoveNext();
            JsonSymbol IEnumerator<JsonSymbol>.Current => current;
            object IEnumerator.Current => current;
            void IDisposable.Dispose() { }
        }

        private class JsonMapSyntaxEnumerator : IEnumerator<JsonSymbol>
        {
            private readonly JsonMapSyntax jsonMapSyntax;

            public JsonMapSyntaxEnumerator(JsonMapSyntax jsonMapSyntax)
            {
                this.jsonMapSyntax = jsonMapSyntax;
                currentMoveNext = CurlyOpenMoveNext;
            }

            private JsonSymbol current;
            private bool comma;
            private JsonKeyValueSyntax keyValueNode;
            private IEnumerator<JsonKeyValueSyntax> keyValueNodeEnumerator;
            private IEnumerator<JsonMultiValueSyntax> valueNodeEnumerator;
            private IEnumerator<JsonSymbol> multiValueEnumerator;
            private Func<bool> currentMoveNext;

            private bool CurlyOpenMoveNext()
            {
                current = JsonCurlyOpen.Value;
                keyValueNodeEnumerator = jsonMapSyntax.KeyValueNodes.GetEnumerator();
                currentMoveNext = KeyValueMoveNext;
                return true;
            }

            private bool KeyValueMoveNext()
            {
                if (keyValueNodeEnumerator.MoveNext())
                {
                    keyValueNode = keyValueNodeEnumerator.Current;
                    multiValueEnumerator = new JsonMultiValueSyntaxNodeEnumerator(keyValueNode.KeyNode);
                    currentMoveNext = KeyNodeMoveNext;

                    if (comma)
                    {
                        current = JsonComma.Value;
                        return true;
                    }

                    comma = true;
                    return KeyNodeMoveNext();
                }

                if (!jsonMapSyntax.MissingCurlyClose)
                {
                    currentMoveNext = NoMoveNext;
                    current = JsonCurlyClose.Value;
                    return true;
                }

                return false;
            }

            private bool KeyNodeMoveNext()
            {
                if (multiValueEnumerator.MoveNext())
                {
                    current = multiValueEnumerator.Current;
                    return true;
                }

                valueNodeEnumerator = keyValueNode.ValueNodes.GetEnumerator();
                currentMoveNext = ValueNodesMoveNext;
                return ValueNodesMoveNext();
            }

            private bool ValueNodesMoveNext()
            {
                if (valueNodeEnumerator.MoveNext())
                {
                    current = JsonColon.Value;
                    multiValueEnumerator = new JsonMultiValueSyntaxNodeEnumerator(valueNodeEnumerator.Current);
                    currentMoveNext = ValueNodeMoveNext;
                    return true;
                }

                return KeyValueMoveNext();
            }

            private bool ValueNodeMoveNext()
            {
                if (multiValueEnumerator.MoveNext())
                {
                    current = multiValueEnumerator.Current;
                    return true;
                }

                return ValueNodesMoveNext();
            }

            private bool NoMoveNext() => false;

            void IEnumerator.Reset() { }
            bool IEnumerator.MoveNext() => currentMoveNext();
            JsonSymbol IEnumerator<JsonSymbol>.Current => current;
            object IEnumerator.Current => current;
            void IDisposable.Dispose() { }
        }

        private class EnumeratorSelector : JsonValueSyntaxVisitor<IEnumerator<JsonSymbol>>
        {
            public static readonly EnumeratorSelector Instance = new EnumeratorSelector();

            private EnumeratorSelector() { }

            public override IEnumerator<JsonSymbol> VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node)
                => new SingleJsonSymbolEnumerator(node.BooleanToken);

            public override IEnumerator<JsonSymbol> VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node)
                => new SingleJsonSymbolEnumerator(node.IntegerToken);

            public override IEnumerator<JsonSymbol> VisitListSyntax(JsonListSyntax node)
                => new JsonListSyntaxEnumerator(node);

            public override IEnumerator<JsonSymbol> VisitMapSyntax(JsonMapSyntax node)
                => new JsonMapSyntaxEnumerator(node);

            public override IEnumerator<JsonSymbol> VisitMissingValueSyntax(JsonMissingValueSyntax node)
                => EmptyJsonSymbolEnumerator.Instance;

            public override IEnumerator<JsonSymbol> VisitStringLiteralSyntax(JsonStringLiteralSyntax node)
                => new SingleJsonSymbolEnumerator(node.StringToken);

            public override IEnumerator<JsonSymbol> VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node)
                => new SingleJsonSymbolEnumerator(node.UndefinedToken);
        }

        private readonly JsonMultiValueSyntax rootNode;

        public JsonSymbolEnumerator(JsonMultiValueSyntax rootNode)
            => this.rootNode = rootNode ?? throw new ArgumentNullException(nameof(rootNode));

        public IEnumerator<JsonSymbol> GetEnumerator()
            => new JsonMultiValueSyntaxNodeEnumerator(rootNode);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
