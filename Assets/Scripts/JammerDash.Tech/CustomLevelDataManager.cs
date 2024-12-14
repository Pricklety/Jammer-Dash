using JammerDash.Audio;
using JammerDash.Editor;
using JammerDash.Editor.Basics;
using JammerDash.Menus.Play;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JammerDash.Tech
{
    public class CustomLevelDataManager : MonoBehaviour
    {
        // Singleton instance
        public static CustomLevelDataManager Instance;
        public string levelName;
        public string artist;
        public string creator;
        public int diff;
        public int ID;
        public int playerhp;
        public float cubesize;
        public bool sceneLoaded = false;
        public SceneData data;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Function to load SceneData based on the scene name
        public SceneData LoadLevelData(string sceneName, int id)
        {
            if (sceneLoaded)
            {
                Debug.LogWarning("Scene already loaded.");
                return null;
            }

            levelName = sceneName;
            ID = id;
            string filePath = Path.Combine(Application.persistentDataPath, "levels", "extracted", $"{id} - {levelName}", $"{levelName}.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Debug.LogWarning(filePath);
                SceneData sceneData = SceneData.FromJson(json);
                levelName = sceneData.sceneName;
                creator = sceneData.creator;
                artist = sceneData.artist;
                diff = (int)sceneData.calculatedDifficulty;
                ID = sceneData.ID;
                playerhp = sceneData.playerHP != 0 ? sceneData.playerHP : 300;
                cubesize = sceneData.boxSize != 0 ? sceneData.boxSize : 1;
                sceneLoaded = true; // Set the flag to true
                SceneManager.sceneLoaded += OnSceneLoaded;
                Addressables.LoadSceneAsync("Assets/LevelDefault.unity", LoadSceneMode.Single);
            }
            else
            {
                Notifications.instance.Notify("Error opening level. This level does not exist.", null);
                Debug.LogWarning("Error opening level. This level does not exist: " + filePath);
            }

            return null;
        }
        public SceneData LoadEditLevelData(int ID, string sceneName)
        {
            string filePath = Path.Combine(Application.persistentDataPath, "scenes", ID.ToString() + " - " + sceneName, $"{sceneName}.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                SceneData sceneData = SceneData.FromJson(json);

                // Load the audio clip
                string audioFilePath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.ID + " - " + sceneName, $"{sceneData.songName}");

                if (SceneManager.GetActiveScene().name == "SampleScene")
                {
                    OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
                    EditorManager manager = FindObjectOfType<EditorManager>();
                    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(Path.Combine(Application.persistentDataPath, "scenes", sceneData.ID + " - " + sceneName, "bgImage.png")))
                    {
                        if (www.isDone)
                        {
                            FindObjectOfType<RawImage>().gameObject.SetActive(true);
                            manager.bgImage.isOn = true;
                            manager.bgPreview.texture = DownloadHandlerTexture.GetContent(www);
                        }
                    }
                    manager.sceneNameInput.text = sceneData.sceneName;
                    manager.songArtist.text = sceneData.artist;
                    manager.customSongName.text = sceneData.songName;
                    manager.ground.SetActive(sceneData.ground);
                    manager.groundToggle.isOn = sceneData.ground;
                    manager.lineController.audioClip = sceneData.clip;
                    manager.LoadSceneData(sceneData);
                    manager.ID = sceneData.ID;
                    levelName = sceneData.sceneName;
                    creator = sceneData.creator;
                    diff = (int)sceneData.calculatedDifficulty;
                    this.ID = sceneData.ID;
                    SceneManager.sceneLoaded += OnSceneLoaded;
                    return sceneData;
                }

            }
            else
            {
                UnityEngine.Debug.LogWarning("Scene data file not found: " + filePath);
            }

            return null;
        }
        public IEnumerator LoadImage(string url, RawImage rawImage)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (rawImage == null)
                {
                    rawImage = FindInactiveObjectOfType<RawImage>();
                }
                if (www.result == UnityWebRequest.Result.Success)
                {
                    if (rawImage != null)
                    {
                        rawImage.gameObject.SetActive(true);
                        rawImage.texture = DownloadHandlerTexture.GetContent(www);
                    }
                }
                else
                {
                    Debug.Log("Failed to load image: " + www.error);
                }
            }
        }

        private T FindInactiveObjectOfType<T>() where T : Component
        { 
            T[] components = Resources.FindObjectsOfTypeAll<T>();
            foreach (T component in components)
            {
                if (!component.gameObject.activeSelf)
                {
                    return component;
                }
            }
            return null;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "LevelDefault")
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                StartCoroutine(ProcessAfterSceneLoaded());
            }
        }
        Dictionary<string, int> cubeTypeMapping = new Dictionary<string, int>
{
    { "hitter01", 1 },
    { "hitter03", 3 },
    { "hitter04", 4 },
    { "hitter05", 5 },
    { "hitter06", 6 },
};
        private IEnumerator ProcessAfterSceneLoaded()
        {
            yield return new WaitForEndOfFrame();
            string filePath = Path.Combine(Application.persistentDataPath, "levels", "extracted", $"{ID} - {levelName}", $"{levelName}.json");
            Debug.Log(filePath);
            string json = File.ReadAllText(filePath);
            SceneData sceneData = SceneData.FromJson(json);
            Debug.Log(sceneData.levelName);
            data = sceneData;
            Debug.Log(sceneData.cubePositions.Count);

            itemUnused[] obj = FindObjectsOfType<itemUnused>();
            foreach (itemUnused gobject in obj)
            {
                Destroy(gobject.gameObject);
            }
            for (int i = 0; i < sceneData.cubePositions.Count; i++)
                {
                    Vector3 cubePos = sceneData.cubePositions[i];
                int originalCubeType;
                if (sceneData.cubeType == null || sceneData.cubeType.Count <= i)
                {
                    originalCubeType = 1;
                }
                else
                {
                    originalCubeType = sceneData.cubeType[i];
                }

                // Format the name of the cube type based on the index
                string cubeTypeName = $"hitter{originalCubeType:D2}"; // Format: hitter01, hitter03, etc.

                    // Look up the mapped index
                    if (cubeTypeMapping.TryGetValue(cubeTypeName, out int mappedIndex))
                    {
                        // Validate mappedIndex within cubePrefab array bounds
                        if (mappedIndex >= 0 && mappedIndex <= 6)
                        {
                            Instantiate(Resources.Load<GameObject>(cubeTypeName), cubePos, Quaternion.identity);
                        }
                        else
                        {
                            Debug.LogWarning($"Mapped index {mappedIndex} is out of bounds for cubePrefab.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Cube type name '{cubeTypeName}' not found in mapping. Skipping instantiation.");
                    }
                }

            foreach (Vector3 sawPos in sceneData.sawPositions)
            {
                Instantiate(Resources.Load<GameObject>("Saws and Spikes/rotateSaw01"), sawPos, Quaternion.identity);
            }

            for (int i = 0; i < sceneData.longCubePositions.Count; i++)
            {
                Vector3 longCubePos = sceneData.longCubePositions[i];
                float width = sceneData.longCubeWidth[i];
                GameObject longCubeObject = Instantiate(Resources.Load<GameObject>("hitter02"), longCubePos, Quaternion.identity);
                SpriteRenderer longCubeRenderer = longCubeObject.GetComponent<SpriteRenderer>();
                BoxCollider2D collider = longCubeObject.GetComponent<BoxCollider2D>();

                longCubeRenderer.size = new Vector2(width, 1);
                collider.size = new Vector2(width + 0.5f, 0.75f);
                collider.offset = new Vector2(width / 1.965f, 0f);
                Debug.Log("Instantiated long cube");
            }

                StartCoroutine(LoadImage(Path.Combine(Application.persistentDataPath, "levels", "extracted", $"{ID} - {levelName}", "bgImage.png"), null));
            

            Camera[] cams = FindObjectsOfType<Camera>();
            foreach (Camera cam in cams)
            {
                cam.backgroundColor = sceneData.defBGColor;
            }

            levelName = sceneData.sceneName;
            artist = sceneData.artist;
            creator = sceneData.creator;
            diff = (int)sceneData.calculatedDifficulty;
            ID = sceneData.ID;
            GameObject.Find("Cube").SetActive(sceneData.ground);

            FindObjectOfType<Camera>().backgroundColor = sceneData.defBGColor;
            yield return null;
        }
    }
}
