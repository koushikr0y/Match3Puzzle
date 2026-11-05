// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The base class of the combos.
    /// </summary>
    public abstract class Combo
    {
        public Tile tileA;
        public Tile tileB;

        /// <summary>
        /// Resolves this combo.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="tiles">The tiles destroyed by the combo.</param>
        /// <param name="fxPool">The pool to use for the visual effects.</param>
        public abstract void Resolve(GameBoard board, List<GameObject> tiles, FxPool fxPool);
    }
}
