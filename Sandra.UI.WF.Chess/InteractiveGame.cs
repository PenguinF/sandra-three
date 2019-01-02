#region License
/*********************************************************************************
 * InteractiveGame.cs
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

using SysExtensions;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Encapsulates a chess game which publishes a set of shared <see cref="UIAction"/>s.
    /// </summary>
    public partial class InteractiveGame
    {
        // For accessing global settings and displaying windows.
        public readonly MdiContainerForm OwnerForm;
        public readonly Chess.Game Game;

        public InteractiveGame(MdiContainerForm ownerForm, Chess.Position initialPosition)
        {
            OwnerForm = ownerForm;
            Game = new Chess.Game(initialPosition);
        }

        public void ActiveMoveTreeUpdated()
        {
            // Like this because this InteractiveGame has a longer lifetime than chessBoardForm and movesForm.
            // It will go out of scope automatically when both chessBoardForm and movesForm are closed.
            if (chessBoardForm != null) chessBoardForm.GameUpdated();
            MovesTextBox movesTextBox = getMovesTextBox();
            if (movesTextBox != null) movesTextBox.GameUpdated();
        }

        public void HandleMouseWheelEvent(int delta)
        {
            if (delta > 0)
            {
                (delta / 120).Times(Game.Backward);
                ActiveMoveTreeUpdated();
            }
            else if (delta < 0)
            {
                (-delta / 120).Times(Game.Forward);
                ActiveMoveTreeUpdated();
            }
        }

        // Keep track of which types of forms are opened.
        StandardChessBoardForm chessBoardForm;
        SnappingMdiChildForm movesForm;

        MovesTextBox getMovesTextBox()
        {
            if (movesForm == null) return null;
            return (MovesTextBox)movesForm.Controls[0];
        }

        private static Chess.Variation getFirstMove(Chess.Variation variation)
        {
            Chess.Variation firstMoveInVariation = variation;
            while (firstMoveInVariation != null && firstMoveInVariation.VariationIndex == 0)
            {
                firstMoveInVariation = firstMoveInVariation.ParentTree.ParentVariation;
            }
            return firstMoveInVariation;
        }
    }
}
