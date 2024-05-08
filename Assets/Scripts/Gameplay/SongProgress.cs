using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SongProgress : MonoBehaviour
{
    public AudioSource audioSource;
    public Slider progressSlider;
    public Text progressText;

    public FinishLine finish;
    public PlayerMovement player;
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "LevelDefault")
        {
            string levelName = LevelDataManager.Instance.levelName;
            if (string.IsNullOrEmpty(levelName))
            {
                levelName = CustomLevelDataManager.Instance.levelName;
            }

            string levelsFolderPath = Path.Combine(Application.persistentDataPath, "levels", "extracted", levelName);
            string levelJsonFilePath = Path.Combine(levelsFolderPath, $"{levelName}.json");
            string sceneJsonFilePath = Path.Combine(Application.persistentDataPath, "scenes", levelName, $"{levelName}.json");

            if (File.Exists(levelJsonFilePath) && !string.IsNullOrEmpty(CustomLevelDataManager.Instance.levelName))
            {
                // Load level data from "levels" folder
                string json = File.ReadAllText(levelJsonFilePath);
                SceneData sceneData = SceneData.FromJson(json);
                StartCoroutine(LoadAudioClip(sceneData.songName, levelsFolderPath));
            }
            else if (File.Exists(sceneJsonFilePath) && string.IsNullOrEmpty(CustomLevelDataManager.Instance.levelName))
            {
                // Load level data from "scenes" folder if "levels" folder doesn't contain the file
                string json = File.ReadAllText(sceneJsonFilePath);
                SceneData sceneData = SceneData.FromJson(json);
                StartCoroutine(LoadAudioClip(sceneData.songName, Path.Combine(Application.persistentDataPath, "scenes", levelName)));
            }
            else
            {
                Debug.LogError("Neither 'levels' nor 'scenes' folder contains the required level data.");
            }
        }

        if (progressSlider == null)
        {
            progressSlider = GameObject.Find("11").GetComponent<Slider>();
            progressText = GameObject.Find("progressText").GetComponent<Text>();
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

    }

    private IEnumerator LoadAudioClip(string fileName, string folderPath)
    {
        string filePath = Path.Combine(folderPath, fileName);
        string formattedPath = "file://" + filePath.Replace("\\", "/");
        formattedPath = Uri.EscapeUriString(formattedPath);
        Debug.LogWarning(formattedPath);
        formattedPath = formattedPath.Replace("+", "%2B");
        Debug.Log(formattedPath);
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(formattedPath, AudioType.UNKNOWN);

        // Start the request asynchronously
        var operation = www.SendWebRequest();

        // Keep updating loading progress until the request is done
        while (!operation.isDone)
        {
            // Calculate loading progress
            float progress = operation.progress;
            Time.timeScale = 0f;
            // Update loading text
            GameObject.Find("Canvas/default/loadingText").GetComponent<Text>().text = $"Downloading Song: {fileName}: {www.downloadedBytes / 1024768} MB ({progress * 100}%)";


            yield return null;
        }

        // Check for errors
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load audio clip: {www.error}");
            GameObject.Find("Canvas/default/loadingText").GetComponent<Text>().text = "Failed to download the song. Restarting...";
            SceneManager.LoadSceneAsync(1);
            Time.timeScale = 1f;

            yield break;
        }

        // Get the loaded audio clip
        AudioClip loadedAudioClip = DownloadHandlerAudioClip.GetContent(www);
        // Set the audio source clip
        loadedAudioClip.name = Path.GetFileName(fileName);
        audioSource.clip = loadedAudioClip;
        Time.timeScale = 1f;

        // Update loading text to indicate completion
        GameObject.Find("Canvas/default/loadingText").GetComponent<Text>().text = $"";

        // Hide progress bar or loading UI elements if necessary

        // Yield return the loaded audio clip
        yield return loadedAudioClip;
    }

}
