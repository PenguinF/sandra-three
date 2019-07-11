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

using Eutherion.Utils;
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
                valueNodesEnumerator = jsonMultiValueSyntaxNode.ValueNodes.GetEnumerator();
                currentMoveNext = ValueNodesMoveNext;
            }

            private JsonSymbol current;
            private readonly IEnumerator<JsonValueWithBackgroundSyntax> valueNodesEnumerator;
            private IEnumerator<JsonSymbol> currentEnumerator;
            private Func<bool> currentMoveNext;

            private bool ValueNodesMoveNext()
            {
                if (valueNodesEnumerator.MoveNext())
                {
                    currentEnumerator = new JsonSyntaxNodeWithBackgroundBeforeEnumerator(valueNodesEnumerator.Current);
                    currentMoveNext = ValueNodeWithBackgroundMoveNext;
                    return ValueNodeWithBackgroundMoveNext();
                }

                currentEnumerator = jsonMultiValueSyntaxNode.BackgroundAfter.BackgroundSymbols.GetEnumerator();
                currentMoveNext = BackgroundAfterMoveNext;
                return BackgroundAfterMoveNext();
            }

            private bool ValueNodeWithBackgroundMoveNext()
            {
                if (currentEnumerator.MoveNext())
                {
                    current = currentEnumerator.Current;
                    return true;
                }

                return ValueNodesMoveNext();
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
            private IEnumerator<JsonMultiValueSyntax> listItemEnumerator;
            private IEnumerator<JsonSymbol> listItemValueEnumerator;
            private Func<bool> currentMoveNext;

            private bool SquareBracketOpenMoveNext()
            {
                current = JsonSquareBracketOpen.Value;
                listItemEnumerator = jsonListSyntax.ListItemNodes.GetEnumerator();
                currentMoveNext = ElementsMoveNext;
                return true;
            }

            private bool ElementsMoveNext()
            {
                if (listItemEnumerator.MoveNext())
                {
                    listItemValueEnumerator = new JsonMultiValueSyntaxNodeEnumerator(listItemEnumerator.Current);
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
                if (listItemValueEnumerator.MoveNext())
                {
                    current = listItemValueEnumerator.Current;
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
            private IEnumerator<JsonMultiValueSyntax> valueSectionNodesEnumerator;
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

                valueSectionNodesEnumerator = keyValueNode.ValueSectionNodes.GetEnumerator();
                valueSectionNodesEnumerator.MoveNext(); // Skip the key node.
                currentMoveNext = ValueNodesMoveNext;
                return ValueNodesMoveNext();
            }

            private bool ValueNodesMoveNext()
            {
                if (valueSectionNodesEnumerator.MoveNext())
                {
                    current = JsonColon.Value;
                    multiValueEnumerator = new JsonMultiValueSyntaxNodeEnumerator(valueSectionNodesEnumerator.Current);
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
                => new SingleElementEnumerator<JsonSymbol>(node.BooleanToken);

            public override IEnumerator<JsonSymbol> VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node)
                => new SingleElementEnumerator<JsonSymbol>(node.IntegerToken);

            public override IEnumerator<JsonSymbol> VisitListSyntax(JsonListSyntax node)
                => new JsonListSyntaxEnumerator(node);

            public override IEnumerator<JsonSymbol> VisitMapSyntax(JsonMapSyntax node)
                => new JsonMapSyntaxEnumerator(node);

            public override IEnumerator<JsonSymbol> VisitMissingValueSyntax(JsonMissingValueSyntax node)
                => EmptyEnumerator<JsonSymbol>.Instance;

            public override IEnumerator<JsonSymbol> VisitStringLiteralSyntax(JsonStringLiteralSyntax node)
                => new SingleElementEnumerator<JsonSymbol>(node.StringToken);

            public override IEnumerator<JsonSymbol> VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node)
                => new SingleElementEnumerator<JsonSymbol>(node.UndefinedToken);
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
