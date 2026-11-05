// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

namespace GameVanilla.Core
{
    /// <summary>
    /// This class is the entry point to the sound management of the game.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public List<AudioClip> sounds;
        public static SoundManager instance;

        private ObjectPool soundPool;
        private readonly Dictionary<string, AudioClip> nameToSound = new Dictionary<string, AudioClip>();

        private BackgroundMusic bgMusic;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            soundPool = GetComponent<ObjectPool>();
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            foreach (var sound in sounds)
            {
                nameToSound.Add(sound.name, sound);
            }
            bgMusic = GameObject.Find("BackgroundMusic").GetComponent<BackgroundMusic>();
        }

        /// <summary>
        /// Adds the specified list of sounds to the system.
        /// </summary>
        /// <param name="soundsToAdd">The sounds to add to the system.</param>
        public void AddSounds(List<AudioClip> soundsToAdd)
        {
            foreach (var sound in soundsToAdd)
            {
                nameToSound.Add(sound.name, sound);
            }
        }

        /// <summary>
        /// Removes the specified list of sounds from the system.
        /// </summary>
        /// <param name="soundsToAdd">The sounds to remove from the system.</param>
        public void RemoveSounds(List<AudioClip> soundsToAdd)
        {
            foreach (var sound in soundsToAdd)
            {
                nameToSound.Remove(sound.name);
            }
        }

        /// <summary>
        /// Plays the specified audio clip.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="loop">True if the clip should be looped; false otherwise.</param>
        public void PlaySound(AudioClip clip, bool loop = false)
        {
            var sound = PlayerPrefs.GetInt("sound_enabled");
            if (sound != 1)
            {
                return;
            }

            if (clip != null)
            {
                soundPool.GetObject().GetComponent<SoundFx>().Play(clip, loop);
            }
        }

        /// <summary>
        /// Plays the sound with the specified name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        /// <param name="loop">True if the sound should be looped; false otherwise.</param>
        public void PlaySound(string soundName, bool loop = false)
        {
            var clip = nameToSound[soundName];
            if (clip != null)
            {
                PlaySound(clip, loop);
            }
        }

        /// <summary>
        /// Stop the sound with the specified name.
        /// </summary>
        /// <param name="soundName">The name of the sound to stop.</param>
        public void StopSound(string soundName)
        {
            foreach (var sound in soundPool.GetComponentsInChildren<SoundFx>())
            {
                if (sound.GetComponent<AudioSource>().clip == nameToSound[soundName])
                {
                    sound.GetComponent<PooledObject>().pool.ReturnObject(sound.gameObject);
                }
            }
        }

        /// <summary>
        /// Sets the sound as enabled/disabled.
        /// </summary>
        /// <param name="soundEnabled">True if the sound should be enabled; false otherwise.</param>
        public void SetSoundEnabled(bool soundEnabled)
        {
            PlayerPrefs.SetInt("sound_enabled", soundEnabled ? 1 : 0);
        }

        /// <summary>
        /// Sets the music as enabled/disabled.
        /// </summary>
        /// <param name="musicEnabled">True if the music should be enabled; false otherwise.</param>
        public void SetMusicEnabled(bool musicEnabled)
        {
            PlayerPrefs.SetInt("music_enabled", musicEnabled ? 1 : 0);
            bgMusic.GetComponent<AudioSource>().mute = !musicEnabled;
        }

        /// <summary>
        /// Toggles the sound.
        /// </summary>
        public void ToggleSound()
        {
            var sound = PlayerPrefs.GetInt("sound_enabled");
            PlayerPrefs.SetInt("sound_enabled", 1 - sound);
        }

        /// <summary>
        /// Toggles the music.
        /// </summary>
        public void ToggleMusic()
        {
            var music = PlayerPrefs.GetInt("music_enabled");
            PlayerPrefs.SetInt("music_enabled", 1 - music);
            bgMusic.GetComponent<AudioSource>().mute = (1 - music) == 0;
        }
    }
}
