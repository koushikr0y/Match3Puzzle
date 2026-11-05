// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Scenes
{
    /// <summary>
    /// This class contains the logic associated to the level scene.
    /// </summary>
    public class LevelScene : BaseScene
    {
        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private GameObject scrollView;

        [SerializeField]
        private GameObject avatarPrefab;

        [SerializeField]
        private GameObject rewardedAdButton;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(scrollRect);
            Assert.IsNotNull(scrollView);
            Assert.IsNotNull(avatarPrefab);
            Assert.IsNotNull(rewardedAdButton);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            scrollRect.vertical = false;

            #if UNITY_ADS
            rewardedAdButton.SetActive(true);
            #else
            rewardedAdButton.SetActive(false);
            #endif

            var avatar = Instantiate(avatarPrefab);
            avatar.transform.SetParent(scrollView.transform, false);

            var nextLevel = PlayerPrefs.GetInt("next_level");
            if (nextLevel == 0)
            {
                nextLevel = 1;
            }

            LevelButton currentButton = null;
            foreach (var button in scrollView.GetComponentsInChildren<LevelButton>())
            {
                if (button.numLevel != nextLevel)
                {
                    continue;
                }
                currentButton = button;
                break;
            }

            if (currentButton == null)
            {
                return;
            }

            var newPos = scrollView.GetComponent<RectTransform>().anchoredPosition;
            newPos.y =
                scrollRect.transform.InverseTransformPoint(scrollView.GetComponent<RectTransform>().position).y -
                scrollRect.transform.InverseTransformPoint(currentButton.transform.position).y;
            if (newPos.y < scrollView.GetComponent<RectTransform>().anchoredPosition.y)
            {
                scrollView.GetComponent<RectTransform>().anchoredPosition = newPos;
            }

            var targetPos = currentButton.transform.position + new Vector3(0, 1.0f, 0);

            LevelButton prevButton = null;
            if (PuzzleMatchManager.instance.unlockedNextLevel)
            {
                foreach (var button in scrollView.GetComponentsInChildren<LevelButton>())
                {
                    if (button.numLevel != PuzzleMatchManager.instance.lastSelectedLevel)
                    {
                        continue;
                    }
                    prevButton = button;
                    break;
                }
            }

            if (prevButton != null)
            {
                avatar.transform.position = prevButton.transform.position + new Vector3(0, 1.0f, 0);
                var sequence = LeanTween.sequence();
                sequence.append(0.5f);
                sequence.append(LeanTween.move(avatar, targetPos, 0.8f));
                sequence.append(() => avatar.GetComponent<LevelAvatar>().StartFloatingAnimation());
                sequence.append(() => scrollRect.vertical = true);
            }
            else
            {
                avatar.transform.position = targetPos;
                avatar.GetComponent<LevelAvatar>().StartFloatingAnimation();
                scrollRect.vertical = true;
            }
        }
    }
}
