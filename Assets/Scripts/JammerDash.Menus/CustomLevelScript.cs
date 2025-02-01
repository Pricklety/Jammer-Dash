using JammerDash.Tech;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JammerDash.Menus.Play
{
    public class CustomLevelScript : MonoBehaviour
    {
        public Text levelNameText;
        public Text songNameText;
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

        public void SetInfo(string songName)
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
            CustomLevelDataManager data = CustomLevelDataManager.Instance;

            if (data.sceneLoaded)
            {
                Debug.LogWarning("Scene already loaded, skipping loading.");
                return;
            }

            CustomLevelDataManager.Instance.LoadLevelData(sceneData.name, sceneData.ID);
           
        }

        private void LoadSceneAddressable(string sceneKey, Action onComplete)
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
}
