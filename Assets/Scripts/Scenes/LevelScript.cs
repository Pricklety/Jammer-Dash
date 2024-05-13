using JammerDash.Tech;
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

namespace JammerDash.Menus.Play
{

    public class LevelScript : MonoBehaviour
    {
        public Text levelNameText;
        public Text songNameText;
        public Text difficultyText;
        public UnityEvent onPlay;
        public UnityEvent onEdit;
        public GameObject cubePrefab;
        public GameObject sawPrefab;
        public SceneData sceneData;
        private List<GameObject> cubes = new List<GameObject>();
        private List<GameObject> saws = new List<GameObject>();
        // Set the level data for this script
        public void SetSceneData(SceneData data)
        {
            sceneData = data;
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
                songNameText.text = songName;
            }
            else
            {
                Debug.LogError("songNameText is not assigned in the inspector.");
                songNameText.text = "No song assigned";
            }
        }


        public void DeleteLevel()
        {
            string levelPath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName);

            // Check if the level path exists
            if (Directory.Exists(levelPath))
            {
                // Delete the directory and its contents recursively
                Directory.Delete(levelPath, true);

                Debug.Log("Level deleted successfully: " + sceneData.levelName);
            }
            else
            {
                Debug.LogWarning("Level does not exist: " + sceneData.levelName);
            }

            // Clear existing level information panels
            mainMenu menu = FindObjectOfType<mainMenu>();
            foreach (Transform child in menu.levelInfoParent)
            {
                Destroy(child.gameObject);
            }

            // Reload the UI to remove the deleted level
            menu.LoadLevelsFromFiles();
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
        public void SetCustomDifficulty(string difficulty)
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
            CustomLevelDataManager.Instance.levelName = null;
            CustomLevelDataManager.Instance.creator = null;
            CustomLevelDataManager.Instance.diff = 0;
            CustomLevelDataManager.Instance.ID = 0;
            string json = File.ReadAllText(Application.persistentDataPath + $"/scenes/{levelNameText.text}/" + $"{levelNameText.text}.json");
            SceneData sceneData = SceneData.FromJson(json);
            LevelDataManager data = LevelDataManager.Instance;
            data.levelName = levelNameText.text;
            LoadSceneAddressable("Assets/LevelDefault.unity", () =>
            {

                SceneManager.UnloadSceneAsync("MainMenu");
                LevelDataManager data = LevelDataManager.Instance;
                data.levelName = levelNameText.text;
                LevelDataManager.Instance.LoadLevelData(levelNameText.text);
            });
        }


        public void EditLevel()
        {
            string json = File.ReadAllText(Application.persistentDataPath + $"/scenes/{levelNameText.text}/" + $"{levelNameText.text}.json");
            SceneData sceneData = SceneData.FromJson(json);
            if (sceneData != null)
            {

                PlayerPrefs.SetString("CurrentLevelData", sceneData.ToJson());
                SceneManager.LoadSceneAsync("SampleScene").completed += operation =>
                {
                    LevelDataManager data = LevelDataManager.Instance;
                    data.levelName = levelNameText.text;
                    LevelDataManager.Instance.LoadLevelData(levelNameText.text);
                };


            }
            else
            {
                Debug.LogError("SceneData is not set for this level.");
            }
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

        public void OpenEditor()
        {
            if (sceneData != null)
            {
                PlayerPrefs.SetString("CurrentLevelData", sceneData.ToJson());
                SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("SceneData is not set for this level.");
            }
        }
    }

}