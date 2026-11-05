// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.Scenes;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup that offers the player to buy more moves or time
    /// after he loses a game.
    /// </summary>
    public class NoMovesOrTimePopup : Popup
    {
        [SerializeField]
        private Text numCoinsText;

        [SerializeField]
        private GameObject movesGroup;

        [SerializeField]
        private GameObject timeGroup;

        [SerializeField]
        private Text title1Text;

        [SerializeField]
        private Text title2Text;

        [SerializeField]
        private Text numExtraMovesText;

        [SerializeField]
        private Text costText;

        [SerializeField]
        private ParticleSystem coinParticles;

        private GameScene gameScene;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(numCoinsText);
            Assert.IsNotNull(movesGroup);
            Assert.IsNotNull(timeGroup);
            Assert.IsNotNull(title1Text);
            Assert.IsNotNull(title2Text);
            Assert.IsNotNull(numExtraMovesText);
            Assert.IsNotNull(costText);
            Assert.IsNotNull(coinParticles);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            var coins = PlayerPrefs.GetInt("num_coins");
            numCoinsText.text = coins.ToString("n0");
        }

        /// <summary>
        /// Sets the game scene associated to this popup.
        /// </summary>
        /// <param name="scene">The associated game scene.</param>
        public void SetGameScene(GameScene scene)
        {
            gameScene = scene;
            var gameConfig = PuzzleMatchManager.instance.gameConfig;
            if (gameScene.level.limitType == LimitType.Moves)
            {
                timeGroup.SetActive(false);
                title1Text.text = "Out of moves!";
                title2Text.text = string.Format("Add +{0} extra moves to continue.", gameConfig.numExtraMoves);
                costText.text = gameConfig.extraMovesCost.ToString();
                numExtraMovesText.text = string.Format("+{0}", gameConfig.numExtraMoves);
            }
            else
            {
                movesGroup.SetActive(false);
                title1Text.text = "Out of time!";
                title2Text.text = string.Format("Add +{0} extra seconds to continue.",
                    PuzzleMatchManager.instance.gameConfig.numExtraTime);
                costText.text = gameConfig.extraTimeCost.ToString();
            }
        }

        /// <summary>
        /// Called when the play button is pressed.
        /// </summary>
        public void OnPlayButtonPressed()
        {
            if (gameScene.level.limitType == LimitType.Moves)
            {
                var numCoins = PlayerPrefs.GetInt("num_coins");
                if (numCoins >= PuzzleMatchManager.instance.gameConfig.extraMovesCost)
                {
                    PuzzleMatchManager.instance.coinsSystem.SpendCoins(PuzzleMatchManager.instance.gameConfig.extraMovesCost);
                    coinParticles.Play();
                    SoundManager.instance.PlaySound("CoinsPopButton");
                    Close();
                    gameScene.Continue();
                }
                else
                {
                    SoundManager.instance.PlaySound("Button");
                    OpenCoinsPopup();
                }
            }
            else if (gameScene.level.limitType == LimitType.Time)
            {
                var numCoins = PlayerPrefs.GetInt("num_coins");
                if (numCoins >= PuzzleMatchManager.instance.gameConfig.extraTimeCost)
                {
                    PuzzleMatchManager.instance.coinsSystem.SpendCoins(PuzzleMatchManager.instance.gameConfig.extraTimeCost);
                    coinParticles.Play();
                    SoundManager.instance.PlaySound("CoinsPopButton");
                    Close();
                    gameScene.Continue();
                }
                else
                {
                    SoundManager.instance.PlaySound("Button");
                    OpenCoinsPopup();
                }
            }
        }

        /// <summary>
        /// Called when the exit button is pressed.
        /// </summary>
        public void OnExitButtonPressed()
        {
            Close();
            gameScene.OpenLosePopup();
        }

        /// <summary>
        /// Opens the coins popup.
        /// </summary>
        private void OpenCoinsPopup()
        {
            var scene = parentScene as GameScene;
            if (scene != null)
            {
                scene.CloseCurrentPopup();
                scene.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup",
                    popup =>
                    {
                        popup.onClose.AddListener(() =>
                        {
                            scene.OpenPopup<NoMovesOrTimePopup>("Popups/NoMovesOrTimePopup",
                                extraPopup =>
                                {
                                    extraPopup.SetGameScene(scene);
                                });
                        });
                    });
            }
        }
    }
}
