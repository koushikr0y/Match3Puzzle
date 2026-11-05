// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;

using GameVanilla.Game.Common;
using GameVanilla.Game.Popups;
using GameVanilla.Game.Scenes;
#endif


namespace GameVanilla.Game.UI
{
	/// <summary>
	/// The rewarded advertisement button that is present in the level scene.
	/// </summary>
	public class RewardedAdButton : MonoBehaviour
	{
		/// <summary>
		/// Shows the rewarded advertisement.
		/// </summary>
		public void ShowRewardedAd()
		{
			#if UNITY_ADS
			if (Advertisement.IsReady("rewardedVideo"))
			{
				var options = new ShowOptions { resultCallback = HandleShowResult };
				Advertisement.Show("rewardedVideo", options);
			}
			#endif
		}

		#if UNITY_ADS
		/// <summary>
		/// Handles the result of showing the rewarded advertisement.
		/// </summary>
		/// <param name="result">The result of showing the rewarded advertisement.</param>
		private void HandleShowResult(ShowResult result)
		{
			switch (result)
			{
				case ShowResult.Finished:
					var gameManager = PuzzleMatchManager.instance;
					var rewardCoins = gameManager.gameConfig.rewardedAdCoins;
            		gameManager.coinsSystem.BuyCoins(rewardCoins);
					var levelScene = GameObject.Find("LevelScene");
					if (levelScene != null)
					{
						levelScene.GetComponent<LevelScene>().OpenPopup<AlertPopup>("Popups/AlertPopup", popup =>
						{
                			popup.SetTitle("Reward");
                			popup.SetText(string.Format("You earned {0} coins!", rewardCoins));
            			}, false);
					}
					break;

				default:
					break;
			}
		}
		#endif
	}
}
