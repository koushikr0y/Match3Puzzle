// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages the player avatar that is displayed on the level scene.
    /// </summary>
    public class LevelAvatar : MonoBehaviour
    {
        [SerializeField]
        private Sprite girlAvatarSprite;

        [SerializeField]
        private Sprite boyAvatarSprite;

        [SerializeField]
        private Image avatarImage;

        private bool floating;
        private float runningTime;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(girlAvatarSprite);
            Assert.IsNotNull(boyAvatarSprite);
            Assert.IsNotNull(avatarImage);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            var avatarSelected = PlayerPrefs.GetInt("avatar_selected");
            avatarImage.sprite = avatarSelected == 0 ? girlAvatarSprite : boyAvatarSprite;
        }

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        private void Update()
        {
            if (!floating)
            {
                return;
            }

            var deltaHeight = Mathf.Sin(runningTime + Time.deltaTime);
            var newPos = transform.position;
            newPos.y += deltaHeight * 0.002f;
            transform.position = newPos;
            runningTime += Time.deltaTime * 2;
        }

        /// <summary>
        /// Starts the idle floating animation of the avatar.
        /// </summary>
        public void StartFloatingAnimation()
        {
            floating = true;
        }
    }
}
