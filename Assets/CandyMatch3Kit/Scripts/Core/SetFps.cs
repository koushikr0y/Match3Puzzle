// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace GameVanilla.Core
{
    /// <summary>
    /// Utility class to set the desired FPS of the game.
    /// </summary>
    public class SetFps : MonoBehaviour
    {
        public int fps = 60;

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            Application.targetFrameRate = fps;
        }
    }
}
