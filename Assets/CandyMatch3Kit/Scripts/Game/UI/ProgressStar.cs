// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages a star of the in-game progress bar.
    /// </summary>
    public class ProgressStar : MonoBehaviour
    {
        public Image image;
        public Sprite onSprite;
        public Sprite offSprite;

        private Animator animator;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            animator = GetComponent<Animator>();
            image.sprite = offSprite;
        }

        /// <summary>
        /// Activates the star (when the player achieves its associated score).
        /// </summary>
        public void Activate()
        {
            image.sprite = onSprite;
            animator.SetTrigger("Pop");
        }
    }
}
