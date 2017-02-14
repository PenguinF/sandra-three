/*********************************************************************************
 * InteractiveGame.cs
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
    /// <summary>
    /// Encapsulates a chess game which publishes a set of shared <see cref="UIAction"/>s.
    /// </summary>
    public class InteractiveGame
    {
        public readonly Chess.Game Game;

        public InteractiveGame(Chess.Position initialPosition)
        {
            Game = new Chess.Game(initialPosition);
            Game.ActiveMoveIndexChanged += (_, e) => OnActiveMoveIndexChanged(e);
        }

        readonly WeakEvent event_ActiveMoveIndexChanged = new WeakEvent();

        /// <summary>
        /// <see cref="WeakEvent"/> which occurs when the active move index of the game was updated.
        /// </summary>
        public event EventHandler ActiveMoveIndexChanged
        {
            add { event_ActiveMoveIndexChanged.AddListener(value); }
            remove { event_ActiveMoveIndexChanged.RemoveListener(value); }
        }

        /// <summary>
        /// Raises the <see cref="ActiveMoveIndexChanged"/> event. 
        /// </summary>
        protected virtual void OnActiveMoveIndexChanged(EventArgs e)
        {
            event_ActiveMoveIndexChanged.Raise(this, e);
        }


        public const string InteractiveGameUIActionPrefix = nameof(InteractiveGame) + " ";

        public static readonly UIAction GotoPreviousMoveUIAction = new UIAction(InteractiveGameUIActionPrefix + nameof(GotoPreviousMoveUIAction));

        public UIActionState TryGotoPreviousMove(bool perform)
        {
            if (Game.ActiveMoveIndex == 0) return UIActionVisibility.Disabled;
            if (perform) Game.ActiveMoveIndex--;
            return UIActionVisibility.Enabled;
        }

        public static UIActionBinding DefaultGotoPreviousMoveBinding()
        {
            return new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaption = "Previous move",
                MainShortcut = new ShortcutKeys(ConsoleKey.LeftArrow),
                AlternativeShortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.LeftArrow),
                    new ShortcutKeys(ConsoleKey.Z),
                },
            };
        }

        public static readonly UIAction GotoNextMoveUIAction = new UIAction(InteractiveGameUIActionPrefix + nameof(GotoNextMoveUIAction));

        public UIActionState TryGotoNextMove(bool perform)
        {
            if (Game.ActiveMoveIndex == Game.MoveCount) return UIActionVisibility.Disabled;
            if (perform) Game.ActiveMoveIndex++;
            return UIActionVisibility.Enabled;
        }

        public static UIActionBinding DefaultGotoNextMoveBinding()
        {
            return new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaption = "Next move",
                MainShortcut = new ShortcutKeys(ConsoleKey.RightArrow),
                AlternativeShortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.RightArrow),
                    new ShortcutKeys(ConsoleKey.X),
                },
            };
        }
    }
}
