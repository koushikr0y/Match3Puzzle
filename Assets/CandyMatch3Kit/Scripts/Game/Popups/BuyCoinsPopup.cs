// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup for buying coins.
    /// </summary>
    public class BuyCoinsPopup : Popup
    {
        [SerializeField]
        private GameObject iapItemsParent;

        [SerializeField]
        private GameObject iapRowPrefab;

        [SerializeField]
        private Text numCoinsText;

        [SerializeField]
        private ParticleSystem coinsParticles;

        private Popup loadingPopup;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(iapItemsParent);
            Assert.IsNotNull(iapRowPrefab);
            Assert.IsNotNull(numCoinsText);
            Assert.IsNotNull(coinsParticles);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            var coins = PlayerPrefs.GetInt("num_coins");
            numCoinsText.text = coins.ToString("n0");
            PuzzleMatchManager.instance.coinsSystem.Subscribe(OnCoinsChanged);

            foreach (var item in PuzzleMatchManager.instance.gameConfig.iapItems)
            {
                var row = Instantiate(iapRowPrefab);
                row.transform.SetParent(iapItemsParent.transform, false);
                row.GetComponent<IapRow>().Fill(item);
                row.GetComponent<IapRow>().buyCoinsPopup = this;
            }
        }

        /// <summary>
        /// Unity's OnDestroy method.
        /// </summary>
        private void OnDestroy()
        {
            PuzzleMatchManager.instance.coinsSystem.Unsubscribe(OnCoinsChanged);
        }

        /// <summary>
        /// Called when the buy button is pressed.
        /// </summary>
        /// <param name="numCoins">The number of coins to buy.</param>
        public void OnBuyButtonPressed(int numCoins)
        {
            PuzzleMatchManager.instance.coinsSystem.BuyCoins(numCoins);
        }

        /// <summary>
        /// Called when the close button is pressed.
        /// </summary>
        public void OnCloseButtonPressed()
        {
            Close();
        }

        /// <summary>
        /// Called when the number of coins changes.
        /// </summary>
        /// <param name="numCoins">The current number of coins.</param>
        private void OnCoinsChanged(int numCoins)
        {
            numCoinsText.text = numCoins.ToString("n0");
            coinsParticles.Play();
            GetComponent<PlaySound>().Play("CoinsPopButton");
        }

        /// <summary>
        /// Opens a loading popup.
        /// </summary>
        public void OpenLoadingPopup()
        {
            #if UNITY_IOS
            parentScene.OpenPopup<LoadingPopup>("Popups/LoadingPopup",
                popup =>
                {
                    loadingPopup = popup;
                }, false);
            #endif
        }

        /// <summary>
        /// Closes the loading popup.
        /// </summary>
        public void CloseLoadingPopup()
        {
            #if UNITY_IOS
            if (loadingPopup != null)
            {
                loadingPopup.Close();
            }
            #endif
        }
    }
}
