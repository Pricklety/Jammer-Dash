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
        DisplayGameData();
        DisplayProfileInfo();
        DisplayMusicList();
        DisplayAudioInfo();
    }

   

    void DisplayGameData()
    {
        // Add app version and Unity developer information
        string gameData = "Game Version: " + Application.version + "\n";
        gameData += "Unity Version: " + Application.unityVersion + "\n";
        gameData += "Genuine build?: " + Application.genuine + "\n";
        gameData += "Installed by " + Application.installMode + "\n";
        gameData += "Installer: " + Application.installerName + "\n";
        gameData += "Developed by " + Application.companyName + "\n\n";

        gameInfoText.text += gameData + "----------------\n\n";
    }

    void DisplayProfileInfo()
    {
        string profileInfo = "Rank: #N/A\n";
        profileInfo += "Shines: 0\n";
        profileInfo += "Jams: 0\n";
        profileInfo += "Friends: N/A\n";
        profileInfo += "Total play count: N/A\n\n";

        gameInfoText.text += profileInfo + "----------------\n\n";
    }


    void DisplayMusicList()
    {
        // Get all music files in the specified folder
        string musicFolderPathFull = Path.Combine(Application.persistentDataPath, musicFolderPath);
        string[] musicFiles = Directory.GetFiles(musicFolderPathFull, "*.mp3");

        // Display the list of music files in the UI Text
        string musicList = "Alphabetic Music List (First 40):\n";
        int maxLines = Mathf.Min(musicFiles.Length, 40); // Limit display to the first 30 lines

        for (int i = 0; i < maxLines; i++)
        {
            string fileName = Path.GetFileNameWithoutExtension(musicFiles[i]);
            musicList += $"{i + 1}. {fileName}\n";
        }

        // Check if there are more items to display
        if (musicFiles.Length > maxLines)
        {
            musicList += $"and {musicFiles.Length - maxLines} more...\n\n";
        }

        // Display total music count
        string totalMusicCount = "\nTotal Music Count: " + musicFiles.Length + "\n\n";

        gameInfoText.text += totalMusicCount + musicList + "----------------\n\n";
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
                float mappedVolume = Mathf.InverseLerp(-80f, 1f, volume);
                audioInfo += "Volume: " + mappedVolume + "\n";
            }
            else
            {
                audioInfo += "Volume: Can't access.\n";
            }
           
            audioInfo += "Time: " + FormatTime(musicAudioSource.time) + "\n";
            audioInfo += "Length: " + FormatTime(musicAudioSource.clip.length) + "\n";
            float[] samples = new float[4096];
            musicAudioSource.GetSpectrumData(samples, 0, FFTWindow.Rectangular);
            float sum = 0f;
            foreach (float sample in samples)
            {
                sum += sample * sample;
            }
            float averageAmplitude = Mathf.Sqrt(sum / samples.Length);

            // Update the highest recorded amplitude
            highestRecordedAmplitude = Mathf.Max(highestRecordedAmplitude, sum * 1000);

            audioInfo += "Amplitude: " + (sum * 1000).ToString("F2") + " / " + highestRecordedAmplitude.ToString("F2") + "\n\n";

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
