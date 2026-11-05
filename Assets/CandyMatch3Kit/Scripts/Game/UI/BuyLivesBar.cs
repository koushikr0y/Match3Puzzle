// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Game.Common;
using GameVanilla.Game.Scenes;
using GameVanilla.Game.Popups;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class is used to manage the bar to buy lives that is located on the level scene.
    /// </summary>
    public class BuyLivesBar : MonoBehaviour
    {
        [SerializeField]
        private LevelScene levelScene;

        [SerializeField]
        private Sprite enabledLifeSprite;

        [SerializeField]
        private Sprite disabledLifeSprite;

        [SerializeField]
        private Image lifeImage;

        [SerializeField]
        private Text numLivesText;

        [SerializeField]
        private Text timeToNextLifeText;

        [SerializeField]
        private Image buttonImage;

        [SerializeField]
        private Sprite enabledButtonSprite;

        [SerializeField]
        private Sprite disabledButtonSprite;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(levelScene);
            Assert.IsNotNull(enabledLifeSprite);
            Assert.IsNotNull(disabledLifeSprite);
            Assert.IsNotNull(lifeImage);
            Assert.IsNotNull(numLivesText);
            Assert.IsNotNull(timeToNextLifeText);
            Assert.IsNotNull(buttonImage);
            Assert.IsNotNull(enabledButtonSprite);
            Assert.IsNotNull(disabledButtonSprite);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            var numLives = PlayerPrefs.GetInt("num_lives");
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            numLivesText.text = numLives.ToString();
            buttonImage.sprite = numLives == maxLives ? disabledButtonSprite : enabledButtonSprite;
            PuzzleMatchManager.instance.livesSystem.Subscribe(OnLivesCountdownUpdated, OnLivesCountdownFinished);
        }

        /// <summary>
        /// Unity's OnDestroy method.
        /// </summary>
        private void OnDestroy()
        {
            PuzzleMatchManager.instance.livesSystem.Unsubscribe(OnLivesCountdownUpdated, OnLivesCountdownFinished);
        }

        /// <summary>
        /// Called when the buy button is pressed.
        /// </summary>
        public void OnBuyButtonPressed()
        {
            var numLives = PlayerPrefs.GetInt("num_lives");
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            if (numLives < maxLives)
            {
                levelScene.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
            }
        }

        /// <summary>
        /// Called when the lives countdown is updated.
        /// </summary>
        /// <param name="timeSpan">The time left until the next free life.</param>
        /// <param name="lives">The current number of lives.</param>
        private void OnLivesCountdownUpdated(TimeSpan timeSpan, int lives)
        {
            timeToNextLifeText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            numLivesText.text = lives.ToString();
            lifeImage.sprite = lives == 0 ? disabledLifeSprite : enabledLifeSprite;
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            buttonImage.sprite = lives == maxLives ? disabledButtonSprite : enabledButtonSprite;
        }

        /// <summary>
        /// Called when the lives countdown finishes.
        /// </summary>
        /// <param name="lives">The current number of lives.</param>
        private void OnLivesCountdownFinished(int lives)
        {
            timeToNextLifeText.text = "Full";
            numLivesText.text = lives.ToString();
            lifeImage.sprite = lives == 0 ? disabledLifeSprite : enabledLifeSprite;
            buttonImage.sprite = disabledButtonSprite;
        }
    }
}
