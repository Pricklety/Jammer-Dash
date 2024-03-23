using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System;
using UnityEngine.Audio;

public class menuMusicControl : MonoBehaviour
{
    private AudioSource audioSource;
    private float fadeDuration = 0.5f; // Duration of the fade-out in seconds
    private bool fadingOut = false;
    public AudioClip christmasClip;
    public AudioClip normalClip;
    public int normalClipIndex = 0; // Index of the normal music clip
    public int christmasClipIndex = 1; // Index of the Christmas music clip
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
        if (DateTime.Now.Month == 12)
        {
            GetComponent<AudioSource>().clip = christmasClip;
        }
        else
        {
            GetComponent<AudioSource>().clip = normalClip;  
        }
    }
    private void Start()
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            int desiredIndex = GetDesiredSongIndex(audioManager);
            audioManager.Play(desiredIndex);
        }
        else
        {
            Debug.LogError("AudioManager instance not found!");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public int GetDesiredSongIndex(AudioManager audioManager)
    {
        // Check if it's December
        if (System.DateTime.Now.Month == 12)
        {
            return GetActualIndex(audioManager, christmasClipIndex); // Play Christmas music
        }
        else
        {
            return GetActualIndex(audioManager, normalClipIndex); // Play normal music
        }
    }

    private int GetActualIndex(AudioManager audioManager, int desiredIndex)
    {
        if (audioManager != null && audioManager.songPathsList != null && audioManager.songPathsList.Count > 0)
        {
            // Ensure desiredIndex is within the valid range
            desiredIndex = Mathf.Clamp(desiredIndex, 0, audioManager.songPathsList.Count - 1);

            // Find the actual index of the desired song in the shuffled playlist
            for (int i = 0; i < audioManager.songPathsList.Count; i++)
            {
                if (audioManager.songPathsList[i] == audioManager.songPathsList[desiredIndex])
                {
                    return i;
                }
            }
        }

        Debug.LogWarning("Desired song not found in the playlist. Playing the first song.");
        return 0; // Return the first song if the desired one is not found or if AudioManager is not properly initialized
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        fadingOut = false;
    }

    private void Update()
    {

        if (SceneManager.GetActiveScene().buildIndex > 1)
        {
            fadingOut = true;
        }
        else if (SceneManager.GetActiveScene().name == "LevelDefault")
        {
            fadingOut = true;
        }
        else
        {
            fadingOut = false;
            if (audioSource.volume < 1 && audioSource.pitch < 1)
            {
                audioSource.volume++;

                audioSource.outputAudioMixerGroup.audioMixer.SetFloat("Lowpass", 22000);
            }
            audioSource.volume = 1;
            audioSource.pitch = 1;


        }
        if (fadingOut)
        {
            // Continue the fade-out process
            StartCoroutine(FadeOutAndPause());
        }
    }

    private IEnumerator FadeOutAndPause()
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0.0f;


        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            audioSource.volume = Mathf.Lerp(startVolume, 0.0f, t);

            yield return null;
        }

        // Lower the pitch after the fade out
        audioSource.pitch = 0f;


        fadingOut = false;
    }



}
