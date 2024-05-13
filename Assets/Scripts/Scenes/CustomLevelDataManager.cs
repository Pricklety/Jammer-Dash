using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity;
using JammerDash.Editor.Basics;

namespace JammerDash.Tech
{

    public class CustomLevelDataManager : MonoBehaviour
    {
        // Singleton instance
        public static CustomLevelDataManager Instance;
        public string levelName;
        public string creator;
        public int diff;
        public int ID;
        bool loaded;

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
            levelName = sceneName;
            ID = id;
            string filePath = Path.Combine(Application.persistentDataPath, "levels", sceneName + ".jdl");

            if (File.Exists(filePath))
            {
                filePath = Path.Combine(Application.persistentDataPath, "levels", "extracted", levelName, $"{levelName}.json");
                string json = File.ReadAllText(filePath);
                Debug.LogWarning(filePath);
                SceneData sceneData = SceneData.FromJson(json);
                levelName = sceneData.levelName;
                creator = sceneData.creator;
                diff = (int)sceneData.calculatedDifficulty;
                ID = sceneData.ID;
                loaded = false;
                SceneManager.sceneLoaded += OnSceneLoaded;
                UnityEngine.AddressableAssets.Addressables.LoadSceneAsync("Assets/" + SceneManager.GetActiveScene().name + ".unity", LoadSceneMode.Single, true, 1000);

            }
            else
            {
                Debug.LogWarning("JDL file not found: " + filePath);
            }

            return null;
        }


        IEnumerator LoadImage(string url)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    // Enable the GameObject with the RawImage component
                    RawImage rawImage = FindInactiveObjectOfType<RawImage>();
                    if (rawImage != null)
                    {
                        // Enable the GameObject with the RawImage component
                        rawImage.gameObject.SetActive(true);
                    }

                    // Create a Sprite from the downloaded texture and set it to the Image component
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    if (rawImage != null)
                    {
                        // Enable the GameObject with the RawImage component
                        rawImage.texture = texture;
                    }
                }
                else
                {
                    Debug.Log("Failed to load image: " + www.error);
                }
            }
        }
        // Function to find an inactive object of a specific type
        private T FindInactiveObjectOfType<T>() where T : Component
        {
            // Find all objects of type T in the scene
            T[] components = Resources.FindObjectsOfTypeAll<T>();

            // Iterate through the found components
            foreach (T component in components)
            {
                // Check if the GameObject of the component is inactive
                if (!component.gameObject.activeSelf)
                {
                    // Return the component if it's inactive
                    return component;
                }
            }

            // If no inactive object of the specified type is found, return null
            return null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "LevelDefault")
            {
                StartCoroutine(ProcessAfterSceneLoaded());

                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private IEnumerator ProcessAfterSceneLoaded()
        {
            new WaitForEndOfFrame();
            string filePath = Path.Combine(Application.persistentDataPath, "levels", "extracted", levelName, $"{levelName}.json");
            filePath = filePath.Replace("/", "\\");
            Debug.Log(filePath);
            string json = File.ReadAllText(filePath);
            SceneData sceneData = SceneData.FromJson(json);
            Debug.Log(sceneData.levelName);

            Debug.Log(sceneData.cubePositions.Count);

            itemUnused[] obj = FindObjectsOfType<itemUnused>();
            foreach (itemUnused gobject in obj)
            {
                Destroy(gobject.gameObject);

            }
            foreach (Vector3 cubePos in sceneData.cubePositions)
            {
                GameObject cubeObject = Instantiate(Resources.Load<GameObject>("hitter01"), cubePos, Quaternion.identity);
                Debug.Log("Cube instantiated: " + cubeObject);

            }

            foreach (Vector3 sawPos in sceneData.sawPositions)
            {
                Instantiate(Resources.Load<GameObject>("Saws and Spikes/rotateSaw01"), sawPos, Quaternion.identity);
            }
            for (int i = 0; i < sceneData.longCubePositions.Count; i++)
            {
                // Get the current position and width for this long cube
                Vector3 longCubePos = sceneData.longCubePositions[i];
                float width = sceneData.longCubeWidth[i];

                // Instantiate the "hitter02" prefab at the current longCubePos
                GameObject longCubeObject = Instantiate(Resources.Load<GameObject>("hitter02"), longCubePos, Quaternion.identity);

                // Get the SpriteRenderer component of the instantiated object
                SpriteRenderer longCubeRenderer = longCubeObject.GetComponent<SpriteRenderer>();
                BoxCollider2D collider = longCubeObject.GetComponent<BoxCollider2D>();

                // Set the width of the SpriteRenderer
                longCubeRenderer.size = new Vector2(width, 1);
                collider.size = new Vector2(width + 0.5f, 0.75f);
                collider.offset = new Vector2(width / 1.965f, 0f);
                Debug.Log("Instantiated long cube");
            }
            if (sceneData.picLocation != null)
            {
                StartCoroutine(LoadImage(sceneData.picLocation));
            }

            Camera[] cams = FindObjectsOfType<Camera>();

            foreach (Camera cam in cams)
            {
                cam.backgroundColor = sceneData.defBGColor;
            }



            levelName = sceneData.levelName;
            creator = sceneData.creator;
            diff = (int)sceneData.calculatedDifficulty;
            ID = sceneData.ID;
            GameObject.Find("Cube").SetActive(sceneData.ground);

            FindObjectOfType<Camera>().backgroundColor = sceneData.defBGColor;

            yield return null;
        }
    }

}