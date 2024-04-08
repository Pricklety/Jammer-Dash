using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameInfo : MonoBehaviour
{
    public Text gameInfoText;
    public string musicFolderPath = "music"; // Now it's PersistentDataPath/music

    private AudioSource musicAudioSource; 
    private float highestRecordedAmplitude = 0f;

    void Start()
    {
        // Try to find an AudioSource component on this GameObject
        musicAudioSource = GetComponent<AudioSource>();

        // If there isn't an AudioSource on this GameObject, try to find one in the scene
        if (musicAudioSource == null)
        {
            musicAudioSource = FindObjectOfType<AudioSource>();
        }

        InvokeRepeating("UpdateGameInfo", 0f, 0.05f);
    }

    void UpdateGameInfo()
    {
        // Clear the existing text before updating
        gameInfoText.text = "";

        DisplayTimeInfo();
        DisplayMusicList();
        DisplayAudioInfo();
    }

   

    void DisplayMusicList()
    {
        // Get all music files in the specified folder
        string musicFolderPathFull = Path.Combine(Application.persistentDataPath, musicFolderPath);
        string[] musicFiles = Directory.GetFiles(musicFolderPathFull, "*.mp3");


        // Display total music count
        string totalMusicCount = "Total Music Count: " + musicFiles.Length + "\n\n";

        gameInfoText.text += totalMusicCount + "----------------\n\n";
    }

    void DisplayAudioInfo()
    {
        // Display information about the dynamically found AudioSource
        if (musicAudioSource != null)
        {
            float volume;
            bool result = musicAudioSource.outputAudioMixerGroup.audioMixer.GetFloat("Master", out volume);
            
            string audioInfo = "Audio Source Information:\n";
            audioInfo += "Is Playing: " + musicAudioSource.isPlaying + "\n";
            if (result)
            {
                float mappedVolume = Mathf.InverseLerp(-80f, 0f, volume);
                audioInfo += "Volume: " + mappedVolume + "\n";
            }
            else
            {
                audioInfo += "Volume: Can't access.\n";
            }
           
            audioInfo += "Time: " + FormatTime(musicAudioSource.time) + "\n";
            audioInfo += "Length: " + FormatTime(musicAudioSource.clip.length) + "\n";
            

            gameInfoText.text += audioInfo + "----------------\n\n";
        }
        else
        {
            gameInfoText.text += "No AudioSource found.\n\n" + "----------------\n\n";
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        // Ensure seconds don't go beyond 59
        seconds = Mathf.Clamp(seconds, 0, 59);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void DisplayTimeInfo()
    {
        string timeInfo = "Time: " + DateTime.Now.ToString("ddd, dd MMM, yy / hh:mm:ss tt") + "\n";
        if (FindObjectOfType<GameTimer>() != null)
        {
            timeInfo += "Running Time: " + FormatElapsedTime(GameTimer.GetRunningTime()) + "\n\n";
        }

        gameInfoText.text += timeInfo + "----------------\n\n";
    }

    string FormatElapsedTime(float elapsedTime)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }
}
