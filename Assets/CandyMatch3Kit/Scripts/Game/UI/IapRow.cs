// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.Popups;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// An in-app purchasable item in the shop popup.
    /// </summary>
    public class IapRow : MonoBehaviour
    {
        [HideInInspector]
        public BuyCoinsPopup buyCoinsPopup;

        [SerializeField]
        private GameObject mostPopular;
        [SerializeField]
        private GameObject bestValue;
        [SerializeField]
        private GameObject discount;
        [SerializeField]
        private Text discountText;
        [SerializeField]
        private Text numCoinsText;
        [SerializeField]
        private Text priceText;
        [SerializeField]
        private Image coinsImage;
        [SerializeField]
        private List<Sprite> coinIcons;

        private IapItem cachedItem;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(mostPopular);
            Assert.IsNotNull(bestValue);
            Assert.IsNotNull(discount);
            Assert.IsNotNull(discountText);
            Assert.IsNotNull(numCoinsText);
            Assert.IsNotNull(priceText);
            Assert.IsNotNull(coinsImage);
        }

        /// <summary>
        /// Fills this widget with the specified IAP item's information.
        /// </summary>
        /// <param name="item">The IAP item with which to fill this widget.</param>
        public void Fill(IapItem item)
        {
            cachedItem = item;

            numCoinsText.text = item.numCoins.ToString("n0");
            if (item.discount > 0)
            {
                discountText.text = string.Format("{0}%", item.discount);
            }
            else
            {
                discount.SetActive(false);
            }

            if (item.mostPopular)
            {
                bestValue.SetActive(false);
            }
            else if (item.bestValue)
            {
                mostPopular.SetActive(false);
            }
            else
            {
                mostPopular.SetActive(false);
                bestValue.SetActive(false);
            }

            coinsImage.sprite = coinIcons[(int)item.coinIcon];
            coinsImage.SetNativeSize();

            #if CANDY_MATCH_ENABLE_IAP
            var storeController = PuzzleMatchManager.instance.iapManager.controller;
            if (storeController != null)
            {
                var product = storeController.products.WithID(item.storeId);
                if (product != null)
                {
                    priceText.text = product.metadata.localizedPriceString;
                }
            }
            #else
            priceText.text = "$5,99";
            #endif
        }

        /// <summary>
        /// Called when the purchase button is pressed.
        /// </summary>
        public void OnPurchaseButtonPressed()
        {
            #if CANDY_MATCH_ENABLE_IAP
            var storeController = PuzzleMatchManager.instance.iapManager.controller;
            if (storeController != null)
            {
                storeController.InitiatePurchase(cachedItem.storeId);
                buyCoinsPopup.OpenLoadingPopup();
            }
            #else
            PuzzleMatchManager.instance.coinsSystem.BuyCoins(cachedItem.numCoins);
            #endif
            GetComponent<PlaySound>().Play("Button");
        }
    }
}
