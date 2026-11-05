// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

using GameVanilla.Game.Common;

namespace GameVanilla.Game.UI
{
	/// <summary>
	/// This class loads the booster data into the in-game booster buttons when the game starts.
	/// </summary>
	public class BoosterBar : MonoBehaviour
	{
		[SerializeField]
		private BuyBoosterButton button1;

		[SerializeField]
		private BuyBoosterButton button2;

		[SerializeField]
		private BuyBoosterButton button3;

		[SerializeField]
		private BuyBoosterButton button4;

		/// <summary>
		/// Sets the data of the in-game booster buttons.
		/// </summary>
		/// <param name="level">The current level.</param>
		public void SetData(Level level)
		{
			if (level.availableBoosters[BoosterType.Lollipop])
			{
				button1.UpdateAmount(PlayerPrefs.GetInt("num_boosters_0"));
			}
			else
			{
				button1.gameObject.SetActive(false);
			}

			if (level.availableBoosters[BoosterType.Bomb])
			{
				button2.UpdateAmount(PlayerPrefs.GetInt("num_boosters_1"));
			}
			else
			{
				button2.gameObject.SetActive(false);
			}

			if (level.availableBoosters[BoosterType.Switch])
			{
				button3.UpdateAmount(PlayerPrefs.GetInt("num_boosters_2"));
			}
			else
			{
				button3.gameObject.SetActive(false);
			}

			if (level.availableBoosters[BoosterType.ColorBomb])
			{
				button4.UpdateAmount(PlayerPrefs.GetInt("num_boosters_3"));
			}
			else
			{
				button4.gameObject.SetActive(false);
			}
		}
	}
}
