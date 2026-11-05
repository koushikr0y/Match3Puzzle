// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the level regeneration popup that appears when there are
    /// no possible matches on a level.
    /// </summary>
	public class RegenLevelPopup : Popup
    {
	    [SerializeField]
	    private Text text;

	    /// <summary>
	    /// Unity's Awake method.
	    /// </summary>
	    protected override void Awake()
	    {
		    base.Awake();
		    Assert.IsNotNull(text);
	    }

	    /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
	        StartCoroutine(AnimateText());
            StartCoroutine(AutoKill());
        }

	    /// <summary>
	    /// This coroutine animates the popup's text.
	    /// </summary>
	    /// <returns>The coroutine.</returns>
	    private IEnumerator AnimateText()
	    {
		    for (var i = 0; i < 100; i++)
		    {
			    text.text = "Regenerating level.";
			    yield return new WaitForSeconds(0.4f);
			    text.text = "Regenerating level..";
			    yield return new WaitForSeconds(0.4f);
			    text.text = "Regenerating level...";
			    yield return new WaitForSeconds(0.4f);
		    }
	    }

        /// <summary>
        /// This coroutine automatically closes the popup after some time has passed.
        /// </summary>
        /// <returns>The coroutine.</returns>
        private IEnumerator AutoKill()
        {
            yield return new WaitForSeconds(3.0f);
            Close();
        }
	}
}
