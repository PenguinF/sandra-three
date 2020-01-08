#region License
/*********************************************************************************
 * MovesTextBox.UIActions.cs
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

using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.AppTemplate;

namespace Sandra.UI
{
    public partial class MovesTextBox
    {
        public const string MovesTextBoxUIActionPrefix = nameof(MovesTextBox) + ".";

        public static readonly DefaultUIActionBinding UsePgnPieceSymbols = new DefaultUIActionBinding(
            new UIAction(MovesTextBoxUIActionPrefix + nameof(UsePgnPieceSymbols)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.UsePgnPieceSymbols.ToTextProvider(),
                },
            });

        public UIActionState TryUsePgnPieceSymbols(bool perform)
        {
            if (IsDisposed || Disposing) return UIActionVisibility.Hidden;

            if (perform)
            {
                moveFormattingOption
                    = moveFormattingOption == MoveFormattingOption.UsePgn
                    ? MoveFormattingOption.UseLocalizedShortAlgebraic
                    : MoveFormattingOption.UsePgn;

                UpdateMoveFormatter();
            }

            return new UIActionState(UIActionVisibility.Enabled, moveFormattingOption == MoveFormattingOption.UsePgn);
        }

        public static readonly DefaultUIActionBinding UseLongAlgebraicNotation = new DefaultUIActionBinding(
            new UIAction(MovesTextBoxUIActionPrefix + nameof(UseLongAlgebraicNotation)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = LocalizedStringKeys.UseLongAlgebraicNotation.ToTextProvider(),
                },
            });

        public UIActionState TryUseLongAlgebraicNotation(bool perform)
        {
            if (IsDisposed || Disposing) return UIActionVisibility.Hidden;

            if (perform)
            {
                moveFormattingOption
                    = moveFormattingOption == MoveFormattingOption.UseLocalizedLongAlgebraic
                    ? MoveFormattingOption.UseLocalizedShortAlgebraic
                    : MoveFormattingOption.UseLocalizedLongAlgebraic;

                UpdateMoveFormatter();
            }

            return new UIActionState(UIActionVisibility.Enabled, moveFormattingOption == MoveFormattingOption.UseLocalizedLongAlgebraic);
        }
    }
}
