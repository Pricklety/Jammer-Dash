
using JammerDash.Audio;
using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JammerDash.Tech
{

    public class GameInfo : MonoBehaviour
    {
        public Text gameInfoText;
        public string musicFolderPath = "music"; // Now it's PersistentDataPath/music

        private AudioSource musicAudioSource;

        void Start()
        {
            // Try to find an AudioSource component on this GameObject
            musicAudioSource = AudioManager.Instance.source;

            // If there isn't an AudioSource on this GameObject, try to find one in the scene
            if (musicAudioSource == null)
            {
                musicAudioSource = FindObjectOfType<AudioSource>();
            }

            InvokeRepeating("UpdateGameInfo", 0f, 0.25f);
        }

        void UpdateGameInfo()
        {
            // Clear the existing text before updating
            gameInfoText.text = "";

            DisplayTimeInfo();
            DisplayList();
        }



        void DisplayList()
        {
            // Get all music files in the specified folder
            string musicFolderPathFull = Path.Combine(Main.gamePath, musicFolderPath);
            string[] musicFiles = Directory.GetFiles(musicFolderPathFull);


            // Display total music count
            string totalMusicCount = "Playlist length: " + musicFiles.Length.ToString("n0") + " songs\n\n" +
                "Player level: " + Account.Instance.level + "\n" +
                "Total score: " + Account.Instance.totalXP.ToString("N0") + "\n";

            gameInfoText.text += totalMusicCount + "----------------\n\n";
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
}
