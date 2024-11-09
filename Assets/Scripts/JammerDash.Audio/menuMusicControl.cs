using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using JammerDash.Audio;

namespace JammerDash.Audio
{
    public class menuMusicControl : MonoBehaviour
    {
        private AudioSource audioSource;
        private float fadeDuration = 0.5f; // Duration of the fade-out in seconds
        public bool fadingOut = false;
        public AudioClip christmasClip;
        public AudioClip normalClip;
        public int normalClipIndex = 1; // Index of the normal music clip
        public int christmasClipIndex = 2; // Index of the Christmas music clip

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetDesiredMusic();
        }

        private void SetDesiredMusic()
        {
            // Check if it's December
            if (DateTime.Now.Month == 12)
            {
                audioSource.clip = christmasClip;
            }
            else
            {
                audioSource.clip = normalClip;
            }

        }

        public int GetDesiredSongIndex(AudioManager audioManager)
        {
            // Check if it's December
            if (System.DateTime.Now.Month == 12)
            {
                int index = GetActualIndex(audioManager, christmasClipIndex); // Get index based on Christmas clip
                audioManager.currentClipIndex = index; // Set AudioManager's currentClipIndex
                return index;
            }
            else
            {
                int index = GetActualIndex(audioManager, normalClipIndex); // Get index based on normal clip
                audioManager.currentClipIndex = index; // Set AudioManager's currentClipIndex
                return index;
            }
        }


        private int GetActualIndex(AudioManager audioManager, int desiredIndex)
        {
            if (audioManager != null && audioManager.songPathsList != null && audioManager.songPathsList.Count > 0)
            {
                // Ensure desiredIndex is within the valid range
                desiredIndex = Mathf.Clamp(desiredIndex, 0, audioManager.songPathsList.Count - 1);

                // Find the actual index of the desired song in the playlist
                string clipName = System.DateTime.Now.Month == 12 ? christmasClip.name : normalClip.name; // Get the name of the appropriate clip
                for (int i = 0; i < audioManager.songPathsList.Count; i++)
                {
                    string songPath = audioManager.songPathsList[i];
                    string fileName = Path.GetFileNameWithoutExtension(songPath);
                    if (fileName.Contains(clipName))
                    {
                        return i; // Return the index if the file name contains the clip name
                    }
                }
            }

            UnityEngine.Debug.LogWarning("Desired song not found in the playlist. Playing the first song.");
            return desiredIndex;
        }
    }
}
