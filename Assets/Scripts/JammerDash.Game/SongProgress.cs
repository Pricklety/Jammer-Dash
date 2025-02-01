using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JammerDash.Game.Player;
using JammerDash.Tech;
using JammerDash.Audio;

namespace JammerDash.Game
{
    public class SongProgress : MonoBehaviour
    {
        public AudioSource audioSource;
        public Slider progressSlider;
        public Text progressText;
        public GameObject canvas;
        public FinishLine finish;
        public PlayerMovement player;
        private void Start()
        {
            audioSource = AudioManager.Instance.source;
            audioSource.loop = false;
            if (SceneManager.GetActiveScene().name == "LevelDefault")
            {
                string levelName = CustomLevelDataManager.Instance.ID + " - " + CustomLevelDataManager.Instance.levelName;
                string jsonName = CustomLevelDataManager.Instance.levelName;

                string levelsFolderPath = Path.Combine(Main.gamePath, "levels", "extracted", levelName);
                string levelJsonFilePath = Path.Combine(levelsFolderPath, $"{jsonName}.json");

                if (File.Exists(levelJsonFilePath) && !string.IsNullOrEmpty(CustomLevelDataManager.Instance.levelName))
                {
                    // Load level data from "levels" folder
                    string json = File.ReadAllText(levelJsonFilePath);
                    SceneData sceneData = SceneData.FromJson(json);
                    StartCoroutine(LoadAudioClip(Path.Combine(levelsFolderPath, sceneData.artist + " - " + sceneData.songName + ".mp3")));
                }
                else
                {
                    Debug.LogError("The 'levels' folder doesn't contain the required level data.");
                }
            }

            if (progressSlider == null)
            {
                progressSlider = GameObject.Find("11").GetComponent<Slider>();
                progressText = GameObject.Find("progressText").GetComponent<Text>();
            }
            
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (data.canvasOff) {
                Notifications.instance.Notify("Go back to the menu, and click Left Shift + " + KeybindingManager.toggleUI.ToString() + " to re-toggle the gameplay UI", null);
            }
        }
        private void Update()
        {
            if (progressSlider == null)
            {
                progressSlider = GameObject.Find("11").GetComponent<Slider>();
                progressText = GameObject.Find("progressText").GetComponent<Text>();
            }
            // Update the current progress
            float currentProgress = (player.transform.position.x / finish.transform.position.x) * 100;

            float progressPercentage = (player.transform.position.x / finish.transform.position.x) * 100;

            // Update the text value with the progress percentage
            progressText.text = progressPercentage.ToString("0") + "%";

            // Update the slider value with the current progress
            progressSlider.value = currentProgress;
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            
            canvas.SetActive(!data.canvasOff);
        }

        private IEnumerator LoadAudioClip(string filePath)
        {
            filePath = filePath.Replace("+", "%2B");
            filePath = filePath.Replace("\\", "/");
            Debug.LogWarning(filePath);
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN);

            // Start the request asynchronously
            var operation = www.SendWebRequest();

            // Keep updating loading progress until the request is done
            while (!operation.isDone)
            {
                // Calculate loading progress
                float progress = operation.progress;
                Time.timeScale = 0f;


                yield return null;
            }

            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load audio clip: {www.error}");
                Notifications.instance.Notify("This level failed to load due to invalid/missing audio file.", null);
                SceneManager.LoadSceneAsync(1);
                Time.timeScale = 1f;
                CustomLevelDataManager.Instance.sceneLoaded = false;
                yield break;
            }

            // Get the loaded audio clip
            AudioClip loadedAudioClip = DownloadHandlerAudioClip.GetContent(www);
            // Set the audio source clip
            loadedAudioClip.name = Path.GetFileName(filePath);
            audioSource.clip = loadedAudioClip;
            Time.timeScale = 1f;

            // Update loading text to indicate completion
            GameObject.Find("Canvas/loadingText").GetComponent<Text>().text = $"";

            // Yield return the loaded audio clip
            yield return loadedAudioClip;

            StopAllCoroutines();
        }

    }
}