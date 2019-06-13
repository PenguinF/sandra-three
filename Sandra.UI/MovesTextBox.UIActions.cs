#region License
/*********************************************************************************
 * MovesTextBox.UIActions.cs
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

using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.AppTemplate;

namespace Sandra.UI
{
    public partial class MovesTextBox
    {
        public const string MovesTextBoxUIActionPrefix = nameof(MovesTextBox) + ".";

        public static readonly DefaultUIActionBinding UsePGNPieceSymbols = new DefaultUIActionBinding(
            new UIAction(MovesTextBoxUIActionPrefix + nameof(UsePGNPieceSymbols)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.UsePGNPieceSymbols.ToTextProvider(),
                },
            });

        public UIActionState TryUsePGNPieceSymbols(bool perform)
        {
            if (IsDisposed || Disposing) return UIActionVisibility.Hidden;

            if (perform)
            {
                moveFormattingOption
                    = moveFormattingOption == MoveFormattingOption.UsePGN
                    ? MoveFormattingOption.UseLocalizedShortAlgebraic
                    : MoveFormattingOption.UsePGN;

                UpdateMoveFormatter();
            }

            return new UIActionState(UIActionVisibility.Enabled, moveFormattingOption == MoveFormattingOption.UsePGN);
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
