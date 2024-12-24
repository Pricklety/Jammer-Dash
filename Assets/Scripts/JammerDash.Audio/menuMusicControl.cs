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


    }
}
