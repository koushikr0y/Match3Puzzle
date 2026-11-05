// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// This class is used to detect the available combos in the game.
    /// </summary>
    public class ComboDetector
    {
        /// <summary>
        /// Returns the combo that applies to the two specified tiles and null if there is none.
        /// </summary>
        /// <param name="tileA">The first tile.</param>
        /// <param name="tileB">The second tile.</param>
        /// <returns>The combo that applies to the two specified tiles; null if there is none.</returns>
        public Combo GetCombo(Tile tileA, Tile tileB)
        {
            // Color bomb + Color bomb.
            if (tileA.GetComponent<ColorBomb>() != null &&
                tileB.GetComponent<ColorBomb>() != null)
            {
                return new TwoColorBombCombo {tileA = tileA, tileB = tileB};
            }
            
            // Color bomb + Wrapped candy.
            if ((tileA.GetComponent<ColorBomb>() != null &&
                 tileB.GetComponent<WrappedCandy>() != null) ||
                (tileA.GetComponent<WrappedCandy>() != null &&
                 tileB.GetComponent<ColorBomb>() != null))
            {
                return new ColorBombWithWrappedCandyCombo {tileA = tileA, tileB = tileB};
            }
            
            // Color bomb + Striped candy.
            if ((tileA.GetComponent<ColorBomb>() != null &&
                 tileB.GetComponent<StripedCandy>() != null) ||
                (tileA.GetComponent<StripedCandy>() != null &&
                 tileB.GetComponent<ColorBomb>() != null))
            {
                return new ColorBombWithStripedCandyCombo {tileA = tileA, tileB = tileB};
            }
            
            // Color bomb + Normal candy.
            if ((tileA.GetComponent<ColorBomb>() != null &&
                 tileB.GetComponent<Candy>() != null) ||
                (tileA.GetComponent<Candy>() != null &&
                 tileB.GetComponent<ColorBomb>() != null))
            {
                return new ColorBombWithCandyCombo {tileA = tileA, tileB = tileB};
            }
            
            // Wrapped candy + Wrapped candy.
            if ((tileA.GetComponent<WrappedCandy>() != null &&
                 tileB.GetComponent<WrappedCandy>() != null))
            {
                return new TwoWrappedCandyCombo {tileA = tileA, tileB = tileB};
            }
            
            // Wrapped candy + Striped candy.
            if ((tileA.GetComponent<WrappedCandy>() != null &&
                 tileB.GetComponent<StripedCandy>() != null) ||
                (tileA.GetComponent<StripedCandy>() != null &&
                 tileB.GetComponent<WrappedCandy>() != null))
            {
                return new WrappedWithStripedCandyCombo {tileA = tileA, tileB = tileB};
            }
            
            // Striped candy + Striped candy.
            if (tileA.GetComponent<StripedCandy>() != null &&
                tileB.GetComponent<StripedCandy>() != null)
            {
                return new TwoStripedCandyCombo {tileA = tileA, tileB = tileB};
            }

            return null;
        }
    }
}
