/*********************************************************************************
 * MovesTextBox.UIActions.cs
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
using System;

namespace Sandra.UI.WF
{
    public partial class MovesTextBox
    {
        public const string MovesTextBoxUIActionPrefix = nameof(MovesTextBox) + ".";

        public static readonly DefaultUIActionBinding UsePGNPieceSymbols = new DefaultUIActionBinding(
            new UIAction(MovesTextBoxUIActionPrefix + nameof(UsePGNPieceSymbols)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.UsePGNPieceSymbols,
            });

        public UIActionState TryUsePGNPieceSymbols(bool perform)
        {
            if (perform)
            {
                moveFormattingOption
                    = moveFormattingOption == MoveFormattingOption.UsePGN
                    ? MoveFormattingOption.UseLocalizedShortAlgebraic
                    : MoveFormattingOption.UsePGN;

                updateMoveFormatter();
            }

            return new UIActionState(UIActionVisibility.Enabled, moveFormattingOption == MoveFormattingOption.UsePGN);
        }

        public static readonly DefaultUIActionBinding UseLongAlgebraicNotation = new DefaultUIActionBinding(
            new UIAction(MovesTextBoxUIActionPrefix + nameof(UseLongAlgebraicNotation)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.UseLongAlgebraicNotation,
            });

        public UIActionState TryUseLongAlgebraicNotation(bool perform)
        {
            if (perform)
            {
                moveFormattingOption
                    = moveFormattingOption == MoveFormattingOption.UseLocalizedLongAlgebraic
                    ? MoveFormattingOption.UseLocalizedShortAlgebraic
                    : MoveFormattingOption.UseLocalizedLongAlgebraic;

                updateMoveFormatter();
            }

            return new UIActionState(UIActionVisibility.Enabled, moveFormattingOption == MoveFormattingOption.UseLocalizedLongAlgebraic);
        }

        public static readonly DefaultUIActionBinding CopySelectionToClipBoard = new DefaultUIActionBinding(
            new UIAction(MovesTextBoxUIActionPrefix + nameof(CopySelectionToClipBoard)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.Copy,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.C), },
            });

        public UIActionState TryCopySelectionToClipBoard(bool perform)
        {
            if (SelectionLength == 0) return UIActionVisibility.Disabled;
            if (perform) Copy();
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding SelectAllText = new DefaultUIActionBinding(
            new UIAction(MovesTextBoxUIActionPrefix + nameof(SelectAllText)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.SelectAll,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.A), },
            });

        public UIActionState TrySelectAllText(bool perform)
        {
            if (TextLength == 0) return UIActionVisibility.Disabled;
            if (perform) SelectAll();
            return UIActionVisibility.Enabled;
        }
    }
}
