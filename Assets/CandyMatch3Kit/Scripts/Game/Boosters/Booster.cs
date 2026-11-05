// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The base class of the in-game boosters.
    /// </summary>
    public abstract class Booster
    {
        /// <summary>
        /// Resolves this booster.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="tile">The tile in which to apply the booster.</param>
        public abstract void Resolve(GameBoard board, GameObject tile);
    }
}
