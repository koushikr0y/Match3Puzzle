// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.Scenes;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the settings popup.
    /// </summary>
    public class SettingsPopup : Popup
    {
        [SerializeField]
        private ToggleGroup avatarToggleGroup;

        [SerializeField]
        private Slider soundSlider;

        [SerializeField]
        private Slider musicSlider;

        [SerializeField]
        private AnimatedButton resetProgressButton;

        [SerializeField]
        private Image resetProgressImage;

        [SerializeField]
        private Sprite resetProgressDisabledSprite;

        private int currentAvatar;
        private int currentSound;
        private int currentMusic;
        private int currentNotifications;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(avatarToggleGroup);
            Assert.IsNotNull(soundSlider);
            Assert.IsNotNull(musicSlider);
            Assert.IsNotNull(resetProgressButton);
            Assert.IsNotNull(resetProgressImage);
            Assert.IsNotNull(resetProgressDisabledSprite);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            var avatarSelected = PlayerPrefs.GetInt("avatar_selected");
            var toggles = avatarToggleGroup.GetComponentsInChildren<Toggle>();
            for (var i = 0; i < toggles.Length; i++)
            {
                toggles[i].isOn = i == avatarSelected;
            }

            soundSlider.value = PlayerPrefs.GetInt("sound_enabled");
            musicSlider.value = PlayerPrefs.GetInt("music_enabled");
        }

        /// <summary>
        /// Called when the close button is pressed.
        /// </summary>
        public void OnCloseButtonPressed()
        {
            Close();
        }

        /// <summary>
        /// Called when the save button is pressed.
        /// </summary>
        public void OnSaveButtonPressed()
        {
            PlayerPrefs.SetInt("avatar_selected", currentAvatar);
            SoundManager.instance.SetSoundEnabled(currentSound == 1);
            SoundManager.instance.SetMusicEnabled(currentMusic == 1);
            var homeScene = parentScene as HomeScene;
            if (homeScene != null)
            {
                homeScene.UpdateButtons();
            }
            Close();
        }

        /// <summary>
        /// Called when the reset progress button is pressed.
        /// </summary>
        public void OnResetProgressButtonPressed()
        {
            PuzzleMatchManager.instance.lastSelectedLevel = 0;
            PlayerPrefs.SetInt("next_level", 0);
            for (var i = 1; i <= 30; i++)
            {
                PlayerPrefs.DeleteKey(string.Format("level_stars_{0}", i));
            }
            resetProgressImage.sprite = resetProgressDisabledSprite;
            resetProgressButton.interactable = false;
        }

        /// <summary>
        /// Called when the help button is pressed.
        /// </summary>
        public void OnHelpButtonPressed()
        {
            parentScene.OpenPopup<AlertPopup>("Popups/AlertPopup", popup =>
            {
                popup.SetTitle("Help");
                popup.SetText("Do you need help?");
            }, false);
        }

        /// <summary>
        /// Called when the info button is pressed.
        /// </summary>
        public void OnInfoButtonPressed()
        {
            parentScene.OpenPopup<AlertPopup>("Popups/AlertPopup", popup =>
            {
                popup.SetTitle("About");
                popup.SetText("Created by gamevanilla.\n Copyright (C) 2018.");
            }, false);
        }

        /// <summary>
        /// Called when the girl avatar is selected.
        /// </summary>
        public void OnGirlAvatarSelected()
        {
            currentAvatar = 0;
        }

        /// <summary>
        /// Called when the boy avatar is selected.
        /// </summary>
        public void OnBoyAvatarSelected()
        {
            currentAvatar = 1;
        }

        /// <summary>
        /// Called when the sound slider value is changed.
        /// </summary>
        public void OnSoundSliderValueChanged()
        {
            currentSound = (int) soundSlider.value;
        }

        /// <summary>
        /// Called when the music slider value is changed.
        /// </summary>
        public void OnMusicSliderValueChanged()
        {
            currentMusic = (int) musicSlider.value;
        }
    }
}
