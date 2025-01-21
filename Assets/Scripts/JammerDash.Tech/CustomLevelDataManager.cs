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
using JammerDash.Game;
using UnityEngine.Video;

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
        public float scoreMultiplier = 1f;
        public int seed;
        public Dictionary<ModType, bool> modStates;
        public float speed;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                modStates = new Dictionary<ModType, bool>();
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
        public IEnumerator LoadImage(string url, RawImage rawImage, bool isVideo = false)
        {
            if (isVideo)
            {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                VideoPlayer videoPlayer = rawImage.gameObject.AddComponent<VideoPlayer>();
                videoPlayer.url = url;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = new RenderTexture((int)rawImage.rectTransform.rect.width, (int)rawImage.rectTransform.rect.height, 0);
                rawImage.texture = videoPlayer.targetTexture;
                videoPlayer.Play();
                }
                else
                {
                Debug.Log("Failed to load video: " + www.error);
                }
            }
            }
            else
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

        System.Random random = new System.Random(); // Seed with scene ID for reproducibility

        // Define allowed y positions
        float[] allowedYPositions = { -1, 0, 1, 2, 3 };

        // Group cube positions by x value
        var cubeGroups = GroupByX(sceneData.cubePositions);

        foreach (var group in cubeGroups)
        {
            float yPosition = allowedYPositions[random.Next(allowedYPositions.Length)];

            for (int i = 0; i < group.Value.Count; i++)
            {
                Vector3 modifiedCubePos = group.Value[i];

                if (modStates.ContainsKey(ModType.yMirror) && modStates[ModType.yMirror])
                {
                    modifiedCubePos.y = 3 - modifiedCubePos.y;
                }
                if (modStates.ContainsKey(ModType.oneLine) && modStates[ModType.oneLine])
                {
                    modifiedCubePos.y = -1;
                }
                if (modStates.ContainsKey(ModType.random) && modStates[ModType.random])
                {
                    modifiedCubePos.y = yPosition;
                }
                if (modStates.ContainsKey(ModType.easy) && modStates[ModType.easy])
                {
                    modifiedCubePos.x = modifiedCubePos.x / 7 * 5;
                }
               

                int originalCubeType;
                if (sceneData.cubeType == null || sceneData.cubeType.Count <= sceneData.cubePositions.IndexOf(group.Value[i]))
                {
                    originalCubeType = 1;
                }
                else
                {
                    originalCubeType = sceneData.cubeType[sceneData.cubePositions.IndexOf(group.Value[i])];
                }

                // Format the name of the cube type based on the index
                string cubeTypeName = $"hitter{originalCubeType:D2}"; // Format: hitter01, hitter03, etc.

                // Look up the mapped index
                if (cubeTypeMapping.TryGetValue(cubeTypeName, out int mappedIndex))
                {
                    // Validate mappedIndex within cubePrefab array bounds
                    if (mappedIndex >= 0 && mappedIndex <= 6)
                    {
                        Instantiate(Resources.Load<GameObject>(cubeTypeName), modifiedCubePos, Quaternion.identity);
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
        }

        // Group saw positions by x value
        var sawGroups = GroupByX(sceneData.sawPositions);

        foreach (var group in sawGroups)
        {
            float yPosition = allowedYPositions[random.Next(allowedYPositions.Length)];

            foreach (var sawPos in group.Value)
            {
                Vector3 modifiedSawPos = sawPos;

                if (modStates.ContainsKey(ModType.noSpikes) && modStates[ModType.noSpikes] || modStates.ContainsKey(ModType.oneLine) && modStates[ModType.oneLine])
                {
                    // nothing
                }
                else
                {
                    if (modStates.ContainsKey(ModType.yMirror) && modStates[ModType.yMirror])
                    {
                        modifiedSawPos.y = 3 - modifiedSawPos.y;
                    }
                    if (modStates.ContainsKey(ModType.oneLine) && modStates[ModType.oneLine])
                    {
                        modifiedSawPos.y = -1;
                    }
                    if (modStates.ContainsKey(ModType.random) && modStates[ModType.random])
                    {
                        modifiedSawPos.y = yPosition;
                    }
                    if (modStates.ContainsKey(ModType.easy) && modStates[ModType.easy])
                    {
                        modifiedSawPos.x = modifiedSawPos.x / 7 * 5;
                    }
                    // Ensure saws are not too close to cubes or at the same y and x values
                    bool conflict = false;
                    foreach (var cubePos in sceneData.cubePositions)
                    {
                        if (Vector3.Distance(modifiedSawPos, cubePos) < 1.0f)
                        {
                            conflict = true;
                            break;
                        }
                    }

                    if (!conflict)
                    {
                        Instantiate(Resources.Load<GameObject>("Saws and Spikes/rotateSaw01"), modifiedSawPos, Quaternion.identity);
                    }
                }
            }
        }

        // Group long cube positions by x value
        var longCubeGroups = GroupByX(sceneData.longCubePositions);

        foreach (var group in longCubeGroups)
        {
            float yPosition = allowedYPositions[random.Next(allowedYPositions.Length)];

            foreach (var longCubePos in group.Value)
            {
                Vector3 modifiedLongCubePos = longCubePos;

                if (modStates.ContainsKey(ModType.yMirror) && modStates[ModType.yMirror])
                {
                    modifiedLongCubePos.y = 3 - modifiedLongCubePos.y;
                }
                if (modStates.ContainsKey(ModType.oneLine) && modStates[ModType.oneLine])
                {
                    modifiedLongCubePos.y = -1;
                }
                if (modStates.ContainsKey(ModType.random) && modStates[ModType.random])
                {
                    modifiedLongCubePos.y = yPosition;
                }
                if (modStates.ContainsKey(ModType.easy) && modStates[ModType.easy])
                {
                    modifiedLongCubePos.x = modifiedLongCubePos.x / 7 * 5;
                }
                float width = sceneData.longCubeWidth[sceneData.longCubePositions.IndexOf(longCubePos)];
                GameObject longCubeObject = Instantiate(Resources.Load<GameObject>("hitter02"), modifiedLongCubePos, Quaternion.identity);
                SpriteRenderer longCubeRenderer = longCubeObject.GetComponent<SpriteRenderer>();
                BoxCollider2D collider = longCubeObject.GetComponent<BoxCollider2D>();

                if (modStates.ContainsKey(ModType.easy) && modStates[ModType.easy])
                {
                    longCubeRenderer.size = new Vector2(width / 7 * 5, 1);
                    collider.size = new Vector2((width / 7 * 5) + 0.25f, 0.75f);
                    collider.offset = new Vector2(width / 1.965f / 7 * 5, 0f);
                    Debug.Log("Instantiated long cube");
                }
                else {
                longCubeRenderer.size = new Vector2(width, 1);
                collider.size = new Vector2(width + 0.5f, 0.75f);
                collider.offset = new Vector2(width / 1.965f, 0f);
                }
                
                Debug.Log("Instantiated long cube");
            }
        }

        StartCoroutine(LoadImage(Path.Combine(Application.persistentDataPath, "levels", "extracted", $"{ID} - {levelName}", "bgImage.png"), null));

        Camera[] cams = FindObjectsOfType<Camera>();
        foreach (Camera cam in cams)
        {
            cam.backgroundColor = sceneData.defBGColor;
        }
    }

    private Dictionary<float, List<Vector3>> GroupByX(List<Vector3> positions)
    {
        var groups = new Dictionary<float, List<Vector3>>();

        foreach (var pos in positions)
        {
            if (!groups.ContainsKey(pos.x))
            {
                groups[pos.x] = new List<Vector3>();
            }
            groups[pos.x].Add(pos);
        }

        return groups;
    }
    }
}
