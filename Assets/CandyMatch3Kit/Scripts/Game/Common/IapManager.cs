// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

#if CANDY_MATCH_ENABLE_IAP
using UnityEngine;
using UnityEngine.Purchasing;

using GameVanilla.Game.Popups;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// This class manages the in-app purchases of the game. It is based on the official Unity IAP
    /// documentation available here: https://docs.unity3d.com/Manual/UnityIAPInitialization.html
    /// </summary>
    public class IapManager : IStoreListener
    {
        public IStoreController controller { get; private set; }
        public IExtensionProvider extensions { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public IapManager()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var item in PuzzleMatchManager.instance.gameConfig.iapItems)
            {
                builder.AddProduct(item.storeId, ProductType.Consumable);
            }
            UnityPurchasing.Initialize(this, builder);
        }

        /// <summary>
        /// Called when Unity IAP is ready to make purchases.
        /// </summary>
        /// <param name="storeController">The store controller.</param>
        /// <param name="extensionProvider">The extension provider.</param>
        public void OnInitialized(IStoreController storeController, IExtensionProvider extensionProvider)
        {
            controller = storeController;
            extensions = extensionProvider;
        }

        /// <summary>
        /// Called when Unity IAP encounters an unrecoverable initialization error.
        ///
        /// Note that this will not be called if Internet is unavailable; Unity IAP
        /// will attempt initialization until it becomes available.
        /// </summary>
        /// <param name="error">The error received.</param>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
        }

        /// <summary>
        /// Called when a purchase completes.
        ///
        /// May be called at any time after OnInitialized().
        /// </summary>
        /// <param name="e">The purchase event arguments.</param>
        /// <returns>The processing result of the purchase.</returns>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            var purchasedProductId = e.purchasedProduct.definition.id;
            var catalogProduct =
                PuzzleMatchManager.instance.gameConfig.iapItems.Find(x => x.storeId == purchasedProductId);
            if (catalogProduct != null)
            {
                PuzzleMatchManager.instance.coinsSystem.BuyCoins(catalogProduct.numCoins);
			    var shopPopup = Object.FindObjectOfType<BuyCoinsPopup>();
                if (shopPopup != null)
                {
                    shopPopup.GetComponent<BuyCoinsPopup>().CloseLoadingPopup();
                    shopPopup.GetComponent<BuyCoinsPopup>().parentScene.OpenPopup<AlertPopup>("Popups/AlertPopup",
                        popup =>
                        {
                            popup.SetTitle("Purchase");
                            popup.SetText(string.Format("You purchased {0} coins!", catalogProduct.numCoins));
                        }, false);
                }
            }
            return PurchaseProcessingResult.Complete;
        }

        /// <summary>
        /// Called when a purchase fails.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="reason">The failure reason.</param>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            var shopPopup = Object.FindObjectOfType<BuyCoinsPopup>();
            if (shopPopup != null)
            {
                shopPopup.GetComponent<BuyCoinsPopup>().CloseLoadingPopup();
                shopPopup.GetComponent<BuyCoinsPopup>().parentScene.OpenPopup<AlertPopup>("Popups/AlertPopup",
                    popup =>
                    {
                        popup.SetTitle("Error");
                        popup.SetText(string.Format("There was an error when purchasing {0}: {1}",
                            product.metadata.localizedTitle, GetPurchaseFailureReasonString(reason)));
                    }, false);
            }
        }

        /// <summary>
        /// Returns a readable string of the specified purchase failure reason.
        /// </summary>
        /// <param name="reason">The purchase failure reason.</param>
        /// <returns>A readable string of the specified purchase failure reason.</returns>
        private string GetPurchaseFailureReasonString(PurchaseFailureReason reason)
        {
            switch (reason)
            {
                case PurchaseFailureReason.DuplicateTransaction:
                    return "Duplicate transaction.";

                case PurchaseFailureReason.ExistingPurchasePending:
                    return "Existing purchase pending.";

                case PurchaseFailureReason.PaymentDeclined:
                    return "Payment was declined.";

                case PurchaseFailureReason.ProductUnavailable:
                    return "Product is not available.";

                case PurchaseFailureReason.PurchasingUnavailable:
                    return "Purchasing is not available.";

                case PurchaseFailureReason.SignatureInvalid:
                    return "Invalid signature.";

                case PurchaseFailureReason.Unknown:
                    return "Unknown error.";

                case PurchaseFailureReason.UserCancelled:
                    return "User cancelled.";

            }

            return "Unknown error.";
        }
    }
}
#endif
