/*********************************************************************************
 * UIMenu.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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
using System;
using System.Collections.Generic;

namespace Sandra.UI.WF
{
    public abstract class UIMenuNode
    {
        public abstract TResult Accept<TResult>(IUIActionTreeVisitor<TResult> visitor);

        public sealed class Element : UIMenuNode
        {
            public readonly UIAction Key;
            public readonly string Caption;
            public readonly ShortcutKeys Shortcut;

            public Element(UIAction key, UIActionBinding binding)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                Key = key;
                Shortcut = binding.MainShortcut;
            }

            public override TResult Accept<TResult>(IUIActionTreeVisitor<TResult> visitor) => visitor.VisitElement(this);
        }

        public sealed class Container : UIMenuNode
        {
            public readonly List<UIMenuNode> Nodes = new List<UIMenuNode>();
            public string Caption = string.Empty;

            public override TResult Accept<TResult>(IUIActionTreeVisitor<TResult> visitor) => visitor.VisitContainer(this);
        }
    }

    public interface IUIActionTreeVisitor<TResult>
    {
        TResult VisitContainer(UIMenuNode.Container container);
        TResult VisitElement(UIMenuNode.Element element);
    }
}
