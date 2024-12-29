using JammerDash.Tech;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
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
        public Text info;
        public Image bg;
        public UnityEvent onEdit;
        public GameObject cubePrefab;
        public GameObject sawPrefab;
        public SceneData sceneData;
        int ID;
        private List<GameObject> cubes = new List<GameObject>();
        private List<GameObject> saws = new List<GameObject>();

        // Set the level data for this script
        public void SetSceneData(SceneData data)
        {
            sceneData = data;
            info.text = $"Data: ID - {data.ID}; HP - {data.playerHP}; CS - {data.boxSize}; BPM - {data.bpm};";
            bg.color = data.defBGColor;
            ID = data.ID;
        }
        

        public void SetLevelName(string levelName)
        {
            if (levelNameText != null)
            {
                levelNameText.text = levelName;
            }
            else
            {
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
                songNameText.text = "No song assigned";
            }
        }


        public void DeleteLevel()
        {
            string levelPath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.ID + " - " + sceneData.levelName);

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
                difficultyText.text = "N/A";
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


       

        public void EditLevel()
        {
            string json = File.ReadAllText(Application.persistentDataPath + $"/scenes/{ID} - {levelNameText.text}/" + $"{levelNameText.text}.json");
            SceneData sceneData = SceneData.FromJson(json);
            if (sceneData != null)
            {

                SceneManager.LoadSceneAsync("SampleScene").completed += operation =>
                {
                    CustomLevelDataManager data = CustomLevelDataManager.Instance;
                    data.levelName = levelNameText.text;
                    CustomLevelDataManager.Instance.LoadEditLevelData(ID, levelNameText.text);
                };


            }
            else
            {
                Notifications.instance.Notify("Error: This level does not exist.", null);
            }
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