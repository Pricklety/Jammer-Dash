using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomLevelScript : MonoBehaviour
{
    public Text levelNameText;
    public Text songNameText;
    public Text difficultyText;
    public Text creatorText;
    public UnityEvent onPlay;
    public GameObject cubePrefab;
    public GameObject sawPrefab;
    public SceneData sceneData;
    private List<GameObject> cubes = new List<GameObject>();
    private List<GameObject> saws = new List<GameObject>();
    // Set the level data for this script
    public void SetSceneData(SceneData data)
    {
        sceneData = data;
        sceneData.clipPath = sceneData.clipPath.Replace("scenes", "levels\\extracted");
    }

    public void SetLevelName(string levelName)
    {
        if (levelNameText != null)
        {
            levelNameText.text = levelName;
        }
        else
        {
            Debug.LogError("levelNameText is not assigned in the inspector.");
            levelNameText.text = "Unknown";
        }
    }

    public void SetSongName(string songName)
    {
        if (songNameText != null)
        {
            if (songName.Contains(".mp3"))
            {
                songName = songName.Replace(".mp3", "");    
            }
            songNameText.text = songName;
        }
        else
        {
            Debug.LogError("songNameText is not assigned in the inspector.");
            songNameText.text = "No song assigned";
        }
    }

    public void SetCreator(string creatorName)
    {
        if (creatorText != null)
        {
            creatorText.text = creatorName;
        }
    }

    
    public void SetDifficulty(string difficulty)
    {
        if (difficultyText != null)
        {
            difficultyText.text = difficulty;
        }
        else
        {
            Debug.LogError("difficultyText is not assigned in the inspector.");
        }
    }
    public static string ExtractJSONFromJDL(string jdlFilePath)
    {
        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(jdlFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!entry.FullName.EndsWith(".json"))
                    {
                        continue; // Skip non-JSON files
                    }

                    using (StreamReader reader = new StreamReader(entry.Open()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error extracting JSON from JDL: " + e.Message);
        }

        return null;
    }


    public void PlayLevel()
    {
        LevelDataManager.Instance.levelName = null;
        LevelDataManager.Instance.creator = null;
        LevelDataManager.Instance.diff = 0;
        LevelDataManager.Instance.ID = 0;
        string json = File.ReadAllText(Application.persistentDataPath + $"/levels/extracted/{levelNameText.text}/" + $"{levelNameText.text}.json");
        SceneData sceneData = SceneData.FromJson(json);
        CustomLevelDataManager data = CustomLevelDataManager.Instance;
        data.levelName = levelNameText.text;
        LoadSceneAddressable("Assets/LevelDefault.unity", () =>
        {

            SceneManager.UnloadSceneAsync("MainMenu");
            CustomLevelDataManager data = CustomLevelDataManager.Instance;
            data.levelName = levelNameText.text;
            CustomLevelDataManager.Instance.LoadLevelData(levelNameText.text, data.ID);
        });
    }



    private void LoadSceneAddressable(string sceneKey, System.Action onComplete)
    {
        AsyncOperationHandle<SceneInstance> loadOperation = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive);
        loadOperation.Completed += operation =>
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to load scene '{sceneKey}': {operation.OperationException}");
            }
        };
    }
}
