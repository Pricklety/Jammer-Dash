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

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(SetDesiredMusic());
        }

        

        private AudioClip LoadAudioClip(string path, out int clipIndex)
        {
            clipIndex = -1;
            if (File.Exists(path))
            {
            WWW www = new WWW("file://" + path);
            while (!www.isDone) { }
            if (string.IsNullOrEmpty(www.error))
            {
                AudioClip clip = www.GetAudioClip(false, true);
                if (clip != null)
                {
                clip.name = Path.GetFileNameWithoutExtension(path);
                clipIndex = AudioManager.Instance.songPathsList.IndexOf(path);
                }
                return clip;
            }
            }
            return null;
        }

        private IEnumerator SetDesiredMusic()
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (!data.randomSong) 
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
            else 
            {
            var songPathsList = AudioManager.Instance.songPathsList;
            if (songPathsList != null && songPathsList.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, songPathsList.Count);
                string randomSongPath = songPathsList[randomIndex];
                AudioClip randomClip = LoadAudioClip(randomSongPath, out int clipIndex);
                if (randomClip != null)
                {
                audioSource.clip = randomClip;
                AudioManager.Instance.currentClipIndex = clipIndex;
                }
            }
            }
            yield return new WaitForEndOfFrame();
            audioSource.Play();
        }


    }
}
