// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using GameVanilla.Core;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the in-game settings popup.
    /// </summary>
    public class InGameSettingsPopup : Popup
    {
        [SerializeField]
        private AnimatedButton soundButton;

        [SerializeField]
        private AnimatedButton musicButton;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(soundButton);
            Assert.IsNotNull(musicButton);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            UpdateButtons();

            var settingsButton = GameObject.Find("SettingsButton");
            if (settingsButton != null)
            {
                settingsButton.transform.SetParent(GameObject.Find("Canvas").transform, false);
                settingsButton.GetComponent<RectTransform>().SetAsLastSibling();
            }
        }

        /// <summary>
        /// Called when the exit button is pressed.
        /// </summary>
        public void OnExitButtonPressed()
        {
            var settingsButton = GameObject.Find("SettingsButton");
            if (settingsButton != null)
            {
                settingsButton.transform.SetParent(GameObject.Find("BackgroundCanvas").transform);
                settingsButton.GetComponent<RectTransform>().SetAsLastSibling();
            }

            parentScene.CloseCurrentPopup();
            parentScene.OpenPopup<ExitGamePopup>("Popups/ExitGamePopup");
        }

        /// <summary>
        /// Called when the sound button is pressed.
        /// </summary>
        public void OnSoundButtonPressed()
        {
            SoundManager.instance.ToggleSound();
        }

        /// <summary>
        /// Called when the music button is pressed.
        /// </summary>
        public void OnMusicButtonPressed()
        {
            SoundManager.instance.ToggleMusic();
        }

        /// <summary>
        /// Updates the state of the sound and music buttons based on the appropriate PlayerPrefs values.
        /// </summary>
        public void UpdateButtons()
        {
            var sound = PlayerPrefs.GetInt("sound_enabled");
            soundButton.GetComponent<SpriteSwapper>().SetEnabled(sound == 1);
            var music = PlayerPrefs.GetInt("music_enabled");
            musicButton.GetComponent<SpriteSwapper>().SetEnabled(music == 1);
        }
    }
}
