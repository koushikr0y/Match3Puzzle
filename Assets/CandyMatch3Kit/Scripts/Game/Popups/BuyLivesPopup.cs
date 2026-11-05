// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup for buying lives.
    /// </summary>
    public class BuyLivesPopup : Popup
    {
        [SerializeField]
        private Sprite lifeSprite;

        [SerializeField]
        private List<Image> lifeImages;

        [SerializeField]
        private Text refillCostText;

        [SerializeField]
        private Text timeToNextLifeText;

        [SerializeField]
        private ParticleSystem lifeParticles;

        [SerializeField]
        private AnimatedButton refillButton;

        [SerializeField]
        private Image refillButtonImage;

        [SerializeField]
        private Sprite refillButtonDisabledSprite;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(lifeSprite);
            Assert.IsNotNull(refillCostText);
            Assert.IsNotNull(timeToNextLifeText);
            Assert.IsNotNull(lifeParticles);
            Assert.IsNotNull(refillButton);
            Assert.IsNotNull(refillButtonImage);
            Assert.IsNotNull(refillButtonDisabledSprite);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            PuzzleMatchManager.instance.livesSystem.Subscribe(OnLivesCountdownUpdated, OnLivesCountdownFinished);
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            var numLives = PlayerPrefs.GetInt("num_lives");
            if (numLives >= maxLives)
            {
                DisableRefillButton();
            }
            UpdateLifeSprites(numLives);
            refillCostText.text = PuzzleMatchManager.instance.gameConfig.livesRefillCost.ToString();
        }

        /// <summary>
        /// Unity's OnDestroy method.
        /// </summary>
        private void OnDestroy()
        {
            PuzzleMatchManager.instance.livesSystem.Unsubscribe(OnLivesCountdownUpdated, OnLivesCountdownFinished);
        }

        /// <summary>
        /// Called when the refill button is pressed.
        /// </summary>
        public void OnRefillButtonPressed()
        {
            var numCoins = PlayerPrefs.GetInt("num_coins");
            if (numCoins >= PuzzleMatchManager.instance.gameConfig.livesRefillCost)
            {
                PuzzleMatchManager.instance.livesSystem.RefillLives();
                lifeParticles.Play();
                SoundManager.instance.PlaySound("BuyPopButton");
                DisableRefillButton();
            }
            else
            {
                var scene = parentScene;
                if (scene != null)
                {
                    scene.CloseCurrentPopup();
                    SoundManager.instance.PlaySound("Button");
                    scene.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup",
                        popup =>
                        {
                            popup.onClose.AddListener(
                                () =>
                                {
                                    scene.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
                                });
                        });
                }
            }
        }

        /// <summary>
        /// Called when the close button is pressed.
        /// </summary>
        public void OnCloseButtonPressed()
        {
            Close();
        }

        /// <summary>
        /// Called when the lives countdown is updated.
        /// </summary>
        /// <param name="timeSpan">The time left for a free life.</param>
        /// <param name="lives">The current number of lives.</param>
        private void OnLivesCountdownUpdated(TimeSpan timeSpan, int lives)
        {
            timeToNextLifeText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            UpdateLifeSprites(lives);
        }

        /// <summary>
        /// Called when the lives countdown finishes.
        /// </summary>
        /// <param name="lives">The current number of lives.</param>
        private void OnLivesCountdownFinished(int lives)
        {
            timeToNextLifeText.text = "Full";
            UpdateLifeSprites(lives);
        }

        /// <summary>
        /// Updates the life sprites.
        /// </summary>
        /// <param name="lives">The current number of lives.</param>
        private void UpdateLifeSprites(int lives)
        {
            if (lives > 5)
            {
                lives = 5;
            }
            for (var i = 0; i < lives; i++)
            {
                lifeImages[i].sprite = lifeSprite;
            }

            if (lives == PuzzleMatchManager.instance.gameConfig.maxLives)
            {
                DisableRefillButton();
            }
        }

        /// <summary>
        /// Disables the refill button.
        /// </summary>
        private void DisableRefillButton()
        {
            refillButtonImage.sprite = refillButtonDisabledSprite;
            refillButton.interactable = false;
        }
    }
}
