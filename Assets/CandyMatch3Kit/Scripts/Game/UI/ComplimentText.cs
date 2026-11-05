// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

using GameVanilla.Core;

namespace GameVanilla.Game.UI
{
	/// <summary>
	/// The available compliment types.
	/// </summary>
	public enum ComplimentType
	{
		Good,
		Super,
		Yummy
	}

	/// <summary>
	/// This class manages the compliment text that is displayed when several consecutive cascades
	/// take place during a game.
	/// </summary>
	public class ComplimentText : MonoBehaviour
	{
		public Image complimentImage;

		public Sprite goodSprite;
		public Sprite superSprite;
		public Sprite yummySprite;

		/// <summary>
		/// Sets the compliment type.
		/// </summary>
		/// <param name="type">The compliment type.</param>
		public void SetComplimentType(ComplimentType type)
		{
			switch (type)
			{
				case ComplimentType.Good:
					complimentImage.sprite = goodSprite;
					break;

				case ComplimentType.Super:
					complimentImage.sprite = superSprite;
					break;

				case ComplimentType.Yummy:
					complimentImage.sprite = yummySprite;
					break;
			}

			complimentImage.SetNativeSize();
			SoundManager.instance.PlaySound("ComplimentText");
		}
	}
}
