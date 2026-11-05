// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

namespace GameVanilla.Game.Common
{
	/// <summary>
	/// The base class used for the tiles in the game.
	/// </summary>
	public abstract class Tile : MonoBehaviour
	{
		[HideInInspector] public GameBoard board;
		[HideInInspector] public int x;
		[HideInInspector] public int y;

		public bool destructable;

		public abstract List<GameObject> Explode();
		public abstract void ShowExplosionFx(FxPool pool);
		public abstract void UpdateGameState(GameState state);

		/// <summary>
		/// Unity's OnEnable method.
		/// </summary>
		public virtual void OnEnable()
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer != null)
			{
				var newColor = spriteRenderer.color;
				newColor.a = 1.0f;
				spriteRenderer.color = newColor;
			}

			transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			transform.localRotation = Quaternion.identity;
		}

		/// <summary>
		/// Unity's OnDisable method.
		/// </summary>
		public virtual void OnDisable()
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer != null)
			{
				var newColor = spriteRenderer.color;
				newColor.a = 1.0f;
				spriteRenderer.color = newColor;
			}

			transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			transform.localRotation = Quaternion.identity;
		}
	}
}
