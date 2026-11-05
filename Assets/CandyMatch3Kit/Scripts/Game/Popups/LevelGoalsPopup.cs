// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.Scenes;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup that shows the goals of a level.
    /// </summary>
    public class LevelGoalsPopup : Popup
    {
        [SerializeField]
        private GameObject goalGroup;

        [SerializeField]
        private GameObject goalPrefab;

        [SerializeField]
        private GameObject scoreGoalGroup;

        [SerializeField]
        private Text scoreGoalAmountText;

        [SerializeField]
        private GameObject scoreGoalOnlyItem;

        [SerializeField]
        private Text scoreGoalOnlyItemText;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(goalPrefab);
            Assert.IsNotNull(scoreGoalGroup);
            Assert.IsNotNull(scoreGoalAmountText);
            Assert.IsNotNull(scoreGoalOnlyItem);
            Assert.IsNotNull(scoreGoalOnlyItemText);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            StartCoroutine(AutoKill());
        }

        /// <summary>
        /// This coroutine automatically closes the popup after its animation has finished.
        /// </summary>
        /// <returns>The coroutine.</returns>
        private IEnumerator AutoKill()
        {
            yield return new WaitForSeconds(2.4f);
            Close();
            var gameScene = parentScene as GameScene;
            if (gameScene != null)
            {
                gameScene.StartGame();
            }
        }

        /// <summary>
        /// Sets the goals of this popup.
        /// </summary>
        /// <param name="goals">The goals to show on this popup.</param>
        public void SetGoals(List<Goal> goals)
        {
            foreach (var goal in goals)
            {
                if (!(goal is ReachScoreGoal))
                {
                    var goalObject = Instantiate(goalPrefab);
                    goalObject.transform.SetParent(goalGroup.transform, false);
                    goalObject.GetComponent<GoalUiElement>().Fill(goal);
                }
            }
            var reachScoreGoal = goals.Find(x => x is ReachScoreGoal);
            if (reachScoreGoal != null)
            {
                if (goals.Count == 1)
                {
                    scoreGoalGroup.SetActive(false);
                    scoreGoalOnlyItem.SetActive(true);
                    scoreGoalOnlyItemText.text = ((ReachScoreGoal)reachScoreGoal).score.ToString();
                }
                else
                {
                    scoreGoalGroup.SetActive(true);
                    scoreGoalOnlyItem.SetActive(false);
                    scoreGoalAmountText.text = ((ReachScoreGoal)reachScoreGoal).score.ToString();
                }
            }
            else
            {
                scoreGoalGroup.SetActive(false);
                scoreGoalOnlyItem.SetActive(false);
            }
        }
    }
}
