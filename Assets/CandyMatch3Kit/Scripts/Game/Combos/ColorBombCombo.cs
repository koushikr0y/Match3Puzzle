// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

using GameVanilla.Core;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The base class of the combos that involve a color bomb.
    /// </summary>
    public class ColorBombCombo : Combo
    {
        /// <summary>
        /// Resolves this combo.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="tiles">The tiles destroyed by the combo.</param>
        /// <param name="fxPool">The pool to use for the visual effects.</param>
        public override void Resolve(GameBoard board, List<GameObject> tiles, FxPool fxPool)
        {
            var bomb = tileA.GetComponent<ColorBomb>() != null ? tileA : tileB;
            board.ExplodeTileNonRecursive(bomb.gameObject);

            var explosion = fxPool.colorBombExplosion.GetObject();
            explosion.transform.position = tileB.transform.position;

            SoundManager.instance.PlaySound("ColorBomb");
        }
    }
}
