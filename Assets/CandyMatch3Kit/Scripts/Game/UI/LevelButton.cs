// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Popups;
using GameVanilla.Game.Scenes;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages the level buttons that are displayed on the level scene.
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        public int numLevel;

        [SerializeField]
        private Sprite currentButtonSprite;

        [SerializeField]
        private Sprite playedButtonSprite;

        [SerializeField]
        private Sprite lockedButtonSprite;

        [SerializeField]
        private Sprite yellowStarSprite;

        [SerializeField]
        private Image buttonImage;

        [SerializeField]
        private Text numLevelTextBlue;

        [SerializeField]
        private Text numLevelTextPink;

        [SerializeField]
        private GameObject star1;

        [SerializeField]
        private GameObject star2;

        [SerializeField]
        private GameObject star3;

        [SerializeField]
        private GameObject shineAnimation;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(currentButtonSprite);
            Assert.IsNotNull(playedButtonSprite);
            Assert.IsNotNull(lockedButtonSprite);
            Assert.IsNotNull(yellowStarSprite);
            Assert.IsNotNull(buttonImage);
            Assert.IsNotNull(numLevelTextBlue);
            Assert.IsNotNull(numLevelTextPink);
            Assert.IsNotNull(star1);
            Assert.IsNotNull(star2);
            Assert.IsNotNull(star3);
            Assert.IsNotNull(shineAnimation);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            numLevelTextBlue.text = numLevel.ToString();
            numLevelTextPink.text = numLevel.ToString();
            var nextLevel = PlayerPrefs.GetInt("next_level");
            if (nextLevel == 0)
            {
                nextLevel = 1;
            }

            if (numLevel == nextLevel)
            {
                buttonImage.sprite = currentButtonSprite;
                star1.SetActive(false);
                star2.SetActive(false);
                star3.SetActive(false);
                shineAnimation.SetActive(true);
                numLevelTextPink.gameObject.SetActive(false);
            }
            else if (numLevel < nextLevel)
            {
                buttonImage.sprite = playedButtonSprite;
                numLevelTextBlue.gameObject.SetActive(false);
                var stars = PlayerPrefs.GetInt("level_stars_" + numLevel);
                switch (stars)
                {
                    case 1:
                        star1.GetComponent<Image>().sprite = yellowStarSprite;
                        break;

                    case 2:
                        star1.GetComponent<Image>().sprite = yellowStarSprite;
                        star2.GetComponent<Image>().sprite = yellowStarSprite;
                        break;

                    default:
                        star1.GetComponent<Image>().sprite = yellowStarSprite;
                        star2.GetComponent<Image>().sprite = yellowStarSprite;
                        star3.GetComponent<Image>().sprite = yellowStarSprite;
                        break;
                }
            }
            else
            {
                buttonImage.sprite = lockedButtonSprite;
                numLevelTextBlue.gameObject.SetActive(false);
                numLevelTextPink.gameObject.SetActive(false);
                star1.SetActive(false);
                star2.SetActive(false);
                star3.SetActive(false);
            }
        }

        /// <summary>
        /// Called when the button is pressed.
        /// </summary>
        public void OnButtonPressed()
        {
            if (buttonImage.sprite == lockedButtonSprite)
            {
                return;
            }

            var scene = GameObject.Find("LevelScene").GetComponent<LevelScene>();
            if (scene != null)
            {
                var numLives = PlayerPrefs.GetInt("num_lives");
                if (numLives > 0)
                {
                    if (!FileUtils.FileExists("Levels/" + numLevel))
                    {
                        scene.OpenPopup<AlertPopup>("Popups/AlertPopup",
                            popup => popup.SetText("This level does not exist."));
                    }
                    else
                    {
                        scene.OpenPopup<StartGamePopup>("Popups/StartGamePopup", popup =>
                        {
                            popup.LoadLevelData(numLevel);
                        });
                    }
                }
                else
                {
                    scene.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
                }
            }
        }
    }
}
