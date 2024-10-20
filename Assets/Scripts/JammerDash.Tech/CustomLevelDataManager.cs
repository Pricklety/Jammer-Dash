using JammerDash.Editor;
using JammerDash.Editor.Basics;
using JammerDash.Menus.Play;
using System;
using System.Collections;
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
            string filePath = Path.Combine(Application.persistentDataPath, "levels", sceneName + ".jdl");

            if (File.Exists(filePath))
            {
                filePath = Path.Combine(Application.persistentDataPath, "levels", "extracted", levelName, $"{levelName}.json");
                string json = File.ReadAllText(filePath);
                Debug.LogWarning(filePath);
                SceneData sceneData = SceneData.FromJson(json);
                levelName = sceneData.sceneName;
                creator = sceneData.creator;
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
                Debug.LogWarning("JDL file not found: " + filePath);
            }

            return null;
        }
        public SceneData LoadEditLevelData(string sceneName)
        {
            string filePath = Path.Combine(Application.persistentDataPath, "scenes", sceneName, $"{sceneName}.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                SceneData sceneData = SceneData.FromJson(json);

                // Load the audio clip
                string audioFilePath = Path.Combine(Application.persistentDataPath, "scenes", sceneName, $"{sceneData.songName}");

                if (SceneManager.GetActiveScene().name == "SampleScene")
                {
                    OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
                    EditorManager manager = FindObjectOfType<EditorManager>();
                    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(Path.Combine(Application.persistentDataPath, "scenes", sceneName, "bgImage.png")))
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
                    levelName = sceneData.sceneName;
                    creator = sceneData.creator;
                    diff = (int)sceneData.calculatedDifficulty;
                    ID = sceneData.ID;
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
        IEnumerator LoadImage(string url)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    RawImage rawImage = FindInactiveObjectOfType<RawImage>();
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

        private IEnumerator ProcessAfterSceneLoaded()
        {
            yield return new WaitForEndOfFrame();
            string filePath = Path.Combine(Application.persistentDataPath, "levels", "extracted", levelName, $"{levelName}.json");
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

                StartCoroutine(LoadImage(Path.Combine(Application.persistentDataPath, "scenes", levelName, "bgImage.png")));
            

            Camera[] cams = FindObjectsOfType<Camera>();
            foreach (Camera cam in cams)
            {
                cam.backgroundColor = sceneData.defBGColor;
            }

            levelName = sceneData.sceneName;
            creator = sceneData.creator;
            diff = (int)sceneData.calculatedDifficulty;
            ID = sceneData.ID;
            GameObject.Find("Cube").SetActive(sceneData.ground);

            FindObjectOfType<Camera>().backgroundColor = sceneData.defBGColor;
            yield return null;
        }
    }
}
