// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

namespace GameVanilla.Core
{
    /// <summary>
    /// Utility class for swapping the sprite of a UI Image between two predefined values.
    /// </summary>
    public class SpriteSwapper : MonoBehaviour
    {
        [SerializeField]
        private Sprite enabledSprite;

        [SerializeField]
        private Sprite disabledSprite;

        private Image image;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        public void Awake()
        {
            image = GetComponent<Image>();
        }

        /// <summary>
        /// Swaps the sprite.
        /// </summary>
        public void SwapSprite()
        {
            image.sprite = image.sprite == enabledSprite ? disabledSprite : enabledSprite;
        }

        /// <summary>
        /// Sets the sprite as enabled/disabled.
        /// </summary>
        /// <param name="spriteEnabled">True if the sprite should be enabled; false otherwise.</param>
        public void SetEnabled(bool spriteEnabled)
        {
            image.sprite = spriteEnabled ? enabledSprite : disabledSprite;
        }
    }
}
