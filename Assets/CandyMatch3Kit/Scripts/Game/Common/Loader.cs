// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// Utility class to initialize the game manager.
    /// </summary>
    public class Loader : MonoBehaviour
    {
        public PuzzleMatchManager gameManager;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            if (PuzzleMatchManager.instance == null)
            {
                Instantiate(gameManager);
            }
        }
    }
}
