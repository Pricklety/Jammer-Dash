using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.Networking;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using JammerDash.Tech;

namespace JammerDash.Editor
{
    public class EditorManager : MonoBehaviour
    {
        [Header("UI - Buttons")]
        public ItemController[] itemButtons;
        public GameObject[] objects;
        public Button loadMusicButton;
        public int currentButtonPressed;
        public Ray ray;
        public RaycastHit hit;
        public int finishCount = 1;

        [Header("UI and Stuff")]
        public GameObject[] editorPages;
        public GameObject optionsMenu;
        public GameObject mainMenu;
        public GameObject colorPickerMenuBG;
        public Text objectCount;
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public AudioSource audio;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        public GameObject songSel;
        public GameObject bg;
        public Image bgColorSel;
        public GameObject grSelector;
        public Toggle bgImage;
        public GameObject bgimgblur;
        public GameObject bgSelector;
        public RawImage bgPreview;
        public Image camBG;
        public GameObject camBG1;
        public GameObject bgSel;
        public Toggle groundToggle;
        public InputField sceneNameInput;
        public GameObject bgPic; // Reference to the bgPic game object
        public GameObject musicPanel;
        public string musicFolderPath;
        public Dropdown defMusic;
        public LineController lineControllerPrefab;
        public Text artistText;
        public Text nameText;
        public Slider playback;
        public Text playbackText;
        public GameObject linePrefab;
        public Slider sliderOffset2;
        public InputField bpmInput;
        public Slider bpmMultiplier;
        public InputField creator;
        public InputField customSongName;
        public Text levelID;
        public Text difficulty;

        public Slider hp;
        public Slider size;

        [Header("Post Processing")]
        public PostProcessVolume vol;
        public Vignette vignette;
        public LensDistortion len;
        public ChromaticAberration chr;
        public Bloom blo;
        public DepthOfField dep;
        public AutoExposure exp;

        [Header("Camera")]
        public Camera cam;
        public GameObject planBPreview;
        public FlexibleColorPicker color1;

        public LineController lineController;


        public Transform selectedObject;
        public GameObject selectionHighlight;
        public GameObject ground;
        public GameObject elev;

        [Header("Scene Management")]
        public GameObject backgroundImagePrefab;
        public GameObject cubePrefab; // Replace with your cube prefab
        public GameObject sawPrefab; // Replace with your saw prefab
        public GameObject longCubePrefab;
        public Transform parentTransform; // Replace with the parent transform for instantiated objects
        public List<Vector3> cubePositions = new();
        public List<Vector3> sawPositions = new();
        public List<Vector3> longCubePositions = new();
        public List<float> longCubeWidth = new();
        public string imageTexturePath;
        public string imageParentPath;
        public Color backgroundColor;
        public string sceneNameInBundle;
        public List<GameObject> cubes; // List of cube game objects
        public List<GameObject> saws; // List of saw game objects
        public List<GameObject> longCubes;
        public InputField bpm;

        [Header("Length Calculator")]
        public string[] targetTags = { "Cube", "Saw" }; // Set the desired tags
        public float speed = 7f; // Speed in units per second
        public float additionalDistance = 5f;
        public Text levelLength;

        [Header("Other")]
        private string selectedImagePath;
        private string selectedSongPath;
        public float delay;
        public float delayLimit = 0.25f;
        public float timer = 0f;

        [Header("Long Cube")]
        public float minWidth = 1f;
        public float maxWidth = 999f; // Change this value as needed
        public float expansionSpeed = 1f; // Adjust as needed

        private Vector3 initialMousePosition;
        private Vector3 initialScale;
        private bool isCursorOnRightSide;


        // Start is called before the first frame update
        void Start()
        {
            cubes = new List<GameObject>();
            saws = new List<GameObject>();
            if (vol != null && vol.profile != null)
            {
                vol.profile.TryGetSettings(out vignette);
                vol.profile.TryGetSettings(out len);
                vol.profile.TryGetSettings(out chr);
                vol.profile.TryGetSettings(out blo);
                vol.profile.TryGetSettings(out dep);
                vol.profile.TryGetSettings(out exp);
            }
            else
            {
                UnityEngine.Debug.LogError("Post-Processing Volume or its profile is not properly set up.");
            }
            MeasureTimeToReachDistance();
            Time.timeScale = playback.value;
            cubes.AddRange(GameObject.FindGameObjectsWithTag("Cubes"));
            saws.AddRange(GameObject.FindGameObjectsWithTag("Saw"));
        }

        public void OnMultiplierChange()
        {
            // Check if the audio clip is assigned
            if (audio.clip == null)
            {
                UnityEngine.Debug.LogError("Audio clip is not assigned.");
                return;
            }

            // Parse BPM value from text
            if (!int.TryParse(bpm.text, out int parsedBPM))
            {
                UnityEngine.Debug.LogError("Failed to parse BPM value.");
                return;
            }
            parsedBPM *= (int)bpmMultiplier.value;
            bpmMultiplier.GetComponentInChildren<Text>().text = "Marker Multiplier (" + bpmMultiplier.value + ")";

            // Calculate the length of each line
            float lineLength = audio.clip.length * 7f;

            // Calculate the spacing between each line based on BPM
            float spacing = 7f / (parsedBPM / 60f);

            // Determine the number of lines to spawn based on the entire length of the audio clip
            int numLines = Mathf.FloorToInt(lineLength / spacing);

            // Clear existing beat objects
            foreach (GameObject gobject in GameObject.FindGameObjectsWithTag("Beat"))
            {
                Destroy(gobject);
            }

            originalPositions = new Vector3[numLines]; // Create originalPositions array

            for (int i = 0; i < numLines; i++)
            {
                float x = i * spacing;
                Vector3 position = new(x, 0f, 0f);
                GameObject newLine = Instantiate(linePrefab, position, Quaternion.identity);

                // Set original position if newLine has "Beat" tag
                if (newLine.CompareTag("Beat"))
                {
                    originalPositions[i] = position;
                }
            }

            sliderOffset2.value = 0f;

        }
        private void MeasureTimeToReachDistance()
        {
            GameObject[] objectsWithTag = FindObjectsWithTags(targetTags);
            Transform targetObject = FindFarthestObjectInX(objectsWithTag);
            if (targetObject != null)
            {
                float distance = targetObject.position.x + additionalDistance;

                // Check if the target is already behind the starting point
                if (distance <= 0)
                {
                    UnityEngine.Debug.LogWarning("The target is already behind the starting point.");
                    levelLength.text = "Invalid distance.";
                    return;
                }

                float timeToReachDistance = distance / speed;

                // Convert time to minutes:seconds format
                int minutes = Mathf.FloorToInt(timeToReachDistance / 60);
                int seconds = Mathf.FloorToInt(timeToReachDistance % 60);

                // Display the result on the Text component
                levelLength.text = $"Length: {minutes:D2}:{seconds:D2}";
            }
            else
            {
                levelLength.text = "Length: 00:00";
            }
        }

        private GameObject[] FindObjectsWithTags(string[] tags)
        {
            GameObject[] objects = new GameObject[0];

            foreach (string tag in tags)
            {
                objects = objects.Concat(GameObject.FindGameObjectsWithTag(tag)).ToArray();
            }

            return objects;
        }

        private Transform FindFarthestObjectInX(GameObject[] objects)
        {
            Transform farthestObject = null;
            float maxX = float.MinValue;

            foreach (GameObject obj in objects)
            {
                float x = obj.transform.position.x;
                if (x > maxX)
                {
                    maxX = x;
                    farthestObject = obj.transform;
                }
            }

            return farthestObject;
        }
        private void SetupMusicDropdown(Dropdown dropdown, Func<string[]> getMusicFiles)
        {
            string[] customMusicFiles = getMusicFiles();

            List<string> names = new();
            foreach (string clip in customMusicFiles)
            {
                names.Add(clip);
            }

            // Clear existing options and add new ones
            dropdown.ClearOptions();

            List<Dropdown.OptionData> options = new();
            foreach (string fileName in names)
            {
                options.Add(new Dropdown.OptionData(fileName));
            }

            dropdown.AddOptions(options);

        }



        public void OnSongDropdownValueChanged(int index, Func<object[]> getMusicFiles)
        {
            UnityEngine.Debug.Log("Dropdown value changed: " + index);

            object[] customMusicFiles = getMusicFiles();

            // Ensure the selected index is valid
            if (index >= 0 && index < customMusicFiles.Length)
            {
                // Update LineController with the selected custom song
                if (customMusicFiles[index] is string selectedAudioClip)
                {
                    UnityEngine.Debug.Log("Custom song selected: " + selectedAudioClip);

                }
                else
                {
                    UnityEngine.Debug.LogError("Invalid type in customMusicFiles array.");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Invalid index or customMusicFiles array is not properly initialized.");
            }
        }


        public void LoadCustomMusic()
        {
            FileBrowser.m_instance = Instantiate(Resources.Load<GameObject>("SimpleFileBrowserCanvas")).GetComponent<FileBrowser>();
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Music", ".mp3", ".wav", ".ogg"));
            FileBrowser.SetDefaultFilter("Music");
            FileBrowser.ShowLoadDialog(SongSelected, null, FileBrowser.PickMode.Files, false, null, null, "Load Local song...", "Choose");
        }
        void SongSelected(string[] paths)
        {
            if (paths.Length >= 0)
            {
                selectedSongPath = paths[0];
                StartCoroutine(LoadCustomClip(selectedSongPath));
                musicFolderPath = selectedSongPath;
            }
        }

        public void OpenDefaultMusic()
        {
            FileBrowser.m_instance = Instantiate(Resources.Load<GameObject>("SimpleFileBrowserLibraryCanvas")).GetComponent<FileBrowser>();
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Music", ".mp3", ".wav", ".ogg"));
            FileBrowser.SetDefaultFilter("Music");
            FileBrowser.ShowLoadDialog(DefaultSongSelected, null, FileBrowser.PickMode.Files, false, Application.streamingAssetsPath + "/music", null, "Choose From Library...", "Choose");
        }

        void DefaultSongSelected(string[] paths)
        {
            if (paths.Length >= 0)
            {
                selectedSongPath = paths[0];
                StartCoroutine(LoadClip(Path.GetFileName(selectedSongPath)));
            }
        }

        public IEnumerator LoadCustomClip(string filePath)
        {
            string path = Path.Combine(Application.persistentDataPath, "scenes", sceneNameInput.text, sceneNameInput.text + ".json");
            string levelPath = Path.Combine(Application.persistentDataPath, "scenes", sceneNameInput.text);
            string json = File.ReadAllText(path);
            SceneData data = SceneData.FromJson(json);

            // Load the audio clip directly without the need for asynchronous loading
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN);
            yield return www.SendWebRequest();

            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                LevelDataManager.Instance.LoadLevelData(LevelDataManager.Instance.levelName);
                UnityEngine.Debug.LogError($"Failed to load audio clip: {www.error}");
                yield break;
            }

            // Get the downloaded audio clip
            AudioClip downloadedAudioClip = DownloadHandlerAudioClip.GetContent(www);

            // Set the audio source clip
            downloadedAudioClip.name = Path.GetFileName(filePath);
            UnityEngine.Debug.Log(filePath);
            nameText.text = downloadedAudioClip.name;
            audio.clip = downloadedAudioClip;
            lineController.audioClip = downloadedAudioClip;
            customSongName.text = downloadedAudioClip.name;
            musicFolderPath = filePath;
            string[] audioFiles = Directory.GetFiles(levelPath, "*.wav");
            audioFiles = audioFiles.Concat(Directory.GetFiles(levelPath, "*.ogg")).ToArray();
            audioFiles = audioFiles.Concat(Directory.GetFiles(levelPath, "*.mp3")).ToArray();
            UnityEngine.Debug.Log(audioFiles.Length);
            // Delete all existing audio files
            foreach (string audioFile in audioFiles)
            {
                File.Delete(audioFile);
                UnityEngine.Debug.Log($"Deleted audio file: {audioFile}");
            }

            // Copy the downloaded audio file to the level path
            string destinationFilePath = Path.Combine(levelPath, Path.GetFileName(filePath));
            CopyFileDirectly(filePath, destinationFilePath);
            // Yield return the loaded audio clip
            yield return downloadedAudioClip;
        }

        private IEnumerator LoadCustomAudioClip(string fileName)
        {
            string filePath = Path.Combine(Application.persistentDataPath, "scenes", sceneNameInput.text, fileName);
            string formattedPath = "file://" + filePath.Replace("\\", "/");

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(formattedPath, AudioType.UNKNOWN);

            // Start the request asynchronously
            var operation = www.SendWebRequest();

            // Keep updating loading progress until the request is done
            while (!operation.isDone)
            {
                UnityEngine.Debug.Log(fileName);
                yield return null;
            }

            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError($"Failed to load custom audio clip: {www.error}");
                UnityEngine.Debug.Log(fileName);
                yield break;
            }

            // Get the downloaded audio clip
            AudioClip downloadedAudioClip = DownloadHandlerAudioClip.GetContent(www);

            // Set the audio source clip
            downloadedAudioClip.name = Path.GetFileNameWithoutExtension(fileName);
            nameText.text = downloadedAudioClip.name;
            audio.clip = downloadedAudioClip;
            lineController.audioClip = downloadedAudioClip;
            musicFolderPath = filePath;
            // List all audio files in the directory
            string[] audioFiles = Directory.GetFiles(Path.GetDirectoryName(filePath), "*.wav");
            audioFiles = audioFiles.Concat(Directory.GetFiles(Path.GetDirectoryName(filePath), "*.ogg")).ToArray();
            audioFiles = audioFiles.Concat(Directory.GetFiles(Path.GetDirectoryName(filePath), "*.mp3")).ToArray();

            // Delete audio files that don't match the downloaded file name
            foreach (string audioFile in audioFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(audioFile);
                if (fileNameWithoutExtension != downloadedAudioClip.name)
                {
                    File.Delete(audioFile);
                    UnityEngine.Debug.Log($"Deleted audio file: {audioFile}");
                }
            }

            yield return downloadedAudioClip;
        }

        public IEnumerator LoadClip(string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "music", fileName);
            string formattedPath = "file://" + filePath.Replace("\\", "/");

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(formattedPath, AudioType.UNKNOWN);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError($"Failed to load audio clip: {www.error}");
                yield break;
            }

            // Get the downloaded audio clip
            AudioClip downloadedAudioClip = DownloadHandlerAudioClip.GetContent(www);

            // Set the audio source clip
            downloadedAudioClip.name = Path.GetFileNameWithoutExtension(fileName);
            nameText.text = downloadedAudioClip.name;
            audio.clip = downloadedAudioClip;
            lineController.audioClip = downloadedAudioClip;
            musicFolderPath = filePath;

            customSongName.text = downloadedAudioClip.name;

            string[] audioFiles = Directory.GetFiles(Path.GetDirectoryName(filePath), "*.wav");
            audioFiles = audioFiles.Concat(Directory.GetFiles(Path.GetDirectoryName(filePath), "*.ogg")).ToArray();
            audioFiles = audioFiles.Concat(Directory.GetFiles(Path.GetDirectoryName(filePath), "*.mp3")).ToArray();

            // Delete audio files that don't match the downloaded file name
            foreach (string audioFile in audioFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(audioFile);
                if (fileNameWithoutExtension != downloadedAudioClip.name)
                {
                    File.Delete(audioFile);
                    UnityEngine.Debug.Log($"Deleted audio file: {audioFile}");
                }
            }

            // Yield return the loaded audio clip
            yield return downloadedAudioClip;
        }


        public void LoadSongBPM()
        {
            int bpm = UniBpmAnalyzer.AnalyzeBpm(lineController.audioClip);
            bpmInput.text = $"{bpm / 2}";

        }
        public void OpenMusic()
        {
            musicPanel.SetActive(true);
        }
        public void CloseMusic()
        {
            musicPanel.SetActive(false);
        }

        public void LoadSceneData(SceneData scene)
        {
            cubes = new List<GameObject>();
            saws = new List<GameObject>();
            longCubes = new();
            string sceneName = sceneNameInput.text.Trim();
            string filePath = GetSceneDataPath(sceneName);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                SceneData sceneData = scene;
                // Clear existing objects
                foreach (GameObject cube in cubes)
                {
                    Destroy(cube);
                }
                cubes.Clear();

                foreach (Vector3 cubePos in sceneData.cubePositions)
                {
                    GameObject cube = Instantiate(cubePrefab, cubePos, Quaternion.identity, parentTransform);
                    cubes.Add(cube);
                }

                // Clear existing saws
                foreach (GameObject saw in saws)
                {
                    Destroy(saw);
                }
                saws.Clear();

                foreach (Vector3 sawPos in sceneData.sawPositions)
                {
                    GameObject saw = Instantiate(sawPrefab, sawPos, Quaternion.identity, parentTransform);
                    saws.Add(saw);
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
                    collider.size = new Vector2(width + 0.5f, 1.05f);
                    collider.offset = new Vector2(width / 2, 0f);
                    longCubes.Add(longCubeObject);
                }
                StartCoroutine(LoadImageCoroutine(sceneData.picLocation));
                UnityEngine.Debug.Log("File Path: " + sceneData.songName);
                UnityEngine.Debug.Log("Objects and UI loaded successfully");
                bpm.text = sceneData.bpm.ToString();
                color1.startingColor = sceneData.defBGColor;
                StartCoroutine(LoadCustomAudioClip(sceneData.songName));
                creator.text = sceneData.creator;
                customSongName.text = sceneData.songName;
                hp.value = sceneData.playerHP;
                size.value = sceneData.boxSize;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Scene data file not found: " + filePath);
            }
        }
        private IEnumerator LoadImageCoroutine(string url)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    // Enable the GameObject with the RawImage component
                    bgPreview.gameObject.SetActive(true);
                    bgImage.isOn = true;

                    // Set the texture to the RawImage component
                    bgPreview.texture = DownloadHandlerTexture.GetContent(www);

                    // Create a Sprite from the downloaded texture and set it to the Image component
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    camBG.sprite = sprite;
                }
                else
                {
                    UnityEngine.Debug.Log("Failed to load image: " + www.error);
                }
            }
        }

        public void SaveLevelData()
        {
            try
            {
                // Create a new SceneData instance and populate it with current objects' positions
                SceneData sceneData = CreateLevelSceneData();

                Text text = GameObject.Find("errorText").GetComponent<Text>();

                // Get the directory path based on the scene name
                string directoryPath = GetLevelDataPath(sceneData.levelName);
                UnityEngine.Debug.Log(directoryPath);

                // Check if the directory exists before proceeding
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath); // Create the directory if it doesn't exist
                }

                // Serialize the SceneData instance to formatted JSON
                string json = JsonUtility.ToJson(sceneData, true);


                // Write the encrypted JSON data to a file inside the directory
                string encryptedJsonFilePath = Path.Combine(directoryPath, $"{sceneData.levelName}.json");
                encryptedJsonFilePath = encryptedJsonFilePath.Replace("/", "\\");
                if (File.Exists(encryptedJsonFilePath))
                {
                    File.Delete(encryptedJsonFilePath);
                    File.WriteAllText(encryptedJsonFilePath, json);
                }
                else
                    File.WriteAllText(encryptedJsonFilePath, json);

                // Zip the directory and keep only the .jdl file
                string zipFilePath = Path.Combine(Application.persistentDataPath, "levels", $"{sceneData.levelName}.zip");
                ZipFile.CreateFromDirectory(directoryPath, zipFilePath, System.IO.Compression.CompressionLevel.Optimal, false);

                // Delete the directory
                Directory.Delete(directoryPath, true);
                UnityEngine.Debug.Log(directoryPath);
                text.text = $"{sceneData.levelName} exported successfully.";
                File.Delete(zipFilePath);
                UnityEngine.Debug.Log($"Level data for {sceneData.levelName} saved in folder: {directoryPath}");
            }
            catch (Exception e)
            {
                Text text = GameObject.Find("errorText").GetComponent<Text>();
                UnityEngine.Debug.LogError("Error saving scene: " + e);
                text.text = $"Couldn't save scene: {e.Message}.\nTry again later.\nError happened on {DateTime.Now}\n\nFull error: {e}";
            }
        }

        // Function to encrypt a string using AES encryption
        private string Encrypt(string plainText, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] iv = new byte[16];
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msEncrypt = new();
                using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
        private string GetLevelDataPath(string sceneName)
        {
            // Combine the application's persistent data path with "levels" folder and the scene name
            string directoryPath = Path.Combine(Application.persistentDataPath, "levels", sceneName);

            // Create the directory if it doesn't exist
            Directory.CreateDirectory(directoryPath);

            // Combine the directory path with the scene name and ".jdl" extension for the file
            string jdlFilePath = Path.Combine(directoryPath, $"{sceneName}.jdl");

            // Convert the folder to a .jdl file if it's not already converted
            if (!File.Exists(jdlFilePath))
            {
                ConvertFolderToJDL(directoryPath);
            }

            return directoryPath;
        }

        private void ConvertFolderToJDL(string folderPath)
        {
            try
            {
                // Check if the .jdl file already exists
                string jdlFilePath = $"{folderPath}.jdl";
                if (File.Exists(jdlFilePath))
                {
                    UnityEngine.Debug.LogWarning($"Folder '{folderPath}' is already converted to .jdl file: '{jdlFilePath}'");
                    return;
                }

                // Create a zip archive of the folder
                string zipFilePath = $"{folderPath}.zip";
                ZipFile.CreateFromDirectory(folderPath, zipFilePath);

                // Rename the zip file to have a .jdl extension
                File.Move(zipFilePath, jdlFilePath);

                UnityEngine.Debug.Log($"Folder '{folderPath}' converted to .jdl file: '{jdlFilePath}'");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error converting folder to .jdl: {e.Message}");
            }
        }

        public void SaveSceneData()
        {
            try
            {
                // Create a new SceneData instance and populate it with current objects' positions
                SceneData sceneData = CreateSceneData();

                Text text = GameObject.Find("errorText").GetComponent<Text>();
                // Serialize the SceneData instance to formatted JSON
                string json = JsonUtility.ToJson(sceneData, true);

                // Get the file path based on the scene name
                string filePath = GetSceneDataPath(sceneData.levelName);

                // Write the JSON data to the file
                File.WriteAllText(filePath, json);

                text.text = $"{sceneData.levelName} successfully saved." +
                $"\nSong: {sceneData.songName} at ID: {sceneData.clip.GetInstanceID()}" +
                $"\nSN: {sceneData.calculatedDifficulty}sn" +
                $"\nLength: {sceneData.levelLength} seconds" +
                $"\nLast saved on: {DateTime.Now}" +
                $"\nObjects: {cubes.Count + saws.Count + longCubes.Count} ({cubes.Count}c, {saws.Count}s, {longCubes.Count}l)" +
                $"\nBPM: {sceneData.bpm}" +
                $"\nVersion: {Application.version}" +
                $"\nID: {sceneData.ID:0000000000}" +
                $"\nHP: {sceneData.playerHP}" +
                $"\nCS: {sceneData.boxSize}";
            }
            catch (Exception e)
            {
                Text text = GameObject.Find("errorText").GetComponent<Text>();
                UnityEngine.Debug.LogError("Error saving scene: " + e);
                text.text = $"Couldn't save scene: {e.Message}.\nTry again later.\nError happened on {DateTime.Now}\n\nFull error: {e}";
            }
        }

        public float CalculateAverageCubeDistance(List<GameObject> cubes)
        {
            if (cubes.Count < 2)
            {
                UnityEngine.Debug.Log("Not enough cubes in the list.");
                return 0;
            }

            float totalDistance = 0f;
            int numDistances = 0;

            for (int i = 0; i < cubes.Count; i++)
            {
                for (int j = i + 1; j < cubes.Count; j++)
                {
                    Vector3 positionA = cubes[i].transform.position;
                    Vector3 positionB = cubes[j].transform.position;

                    // Calculate distance between cubes
                    float distance = Vector3.Distance(positionA, positionB);
                    totalDistance += distance;
                    numDistances++;
                }
            }

            // Calculate average distance
            float averageDistance = totalDistance / numDistances;

            return averageDistance;
        }
        public void LoadSceneData()
        {
            try
            {
                string sceneName = sceneNameInput.text.Trim();

                if (string.IsNullOrEmpty(sceneName))
                {
                    UnityEngine.Debug.LogError("Scene name is empty. Please enter a valid scene name.");
                    return;
                }

                string filePath = GetSceneDataPath(sceneName);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    SceneData sceneData = JsonUtility.FromJson<SceneData>(json);

                    LoadSceneWithData(sceneData);

                    UnityEngine.Debug.Log("Scene loaded successfully: " + sceneName);
                }
                else
                {
                    UnityEngine.Debug.LogError("Scene file not found: " + filePath);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error loading scene: " + e.Message);
            }
        }
        public void InstantiateLines()
        {
            // Check if the audio clip is assigned
            if (audio.clip == null)
            {
                UnityEngine.Debug.LogError("Audio clip is not assigned.");
                return;
            }

            // Parse BPM value from text
            if (!int.TryParse(bpm.text, out int parsedBPM))
            {
                UnityEngine.Debug.LogError("Failed to parse BPM value.");
                return;
            }

            // Calculate the length of each line
            float lineLength = audio.clip.length * 7f;

            // Calculate the spacing between each line based on BPM
            float spacing = 7f / (parsedBPM / 60f);

            // Determine the number of lines to spawn based on the entire length of the audio clip
            int numLines = Mathf.FloorToInt(lineLength / spacing);

            // Clear existing beat objects
            foreach (GameObject gobject in GameObject.FindGameObjectsWithTag("Beat"))
            {
                Destroy(gobject);
            }

            originalPositions = new Vector3[numLines]; // Create originalPositions array

            for (int i = 0; i < numLines; i++)
            {
                float x = i * spacing;
                Vector3 position = new(x, 0f, 0f);
                GameObject newLine = Instantiate(linePrefab, position, Quaternion.identity);

                // Set original position if newLine has "Beat" tag
                if (newLine.CompareTag("Beat"))
                {
                    originalPositions[i] = position;
                }
            }

            sliderOffset2.value = 0f;


        }
        private bool slider1Selected = false;
        private Vector3[] originalPositions;
        public void SetSlider1Selected(bool isSelected)
        {
            slider1Selected = isSelected;
        }
        public void ChangeBPMOffset()
        {
            float value = 0;
            value = sliderOffset2.value;
            GameObject[] beats = GameObject.FindGameObjectsWithTag("Beat");

            for (int i = 0; i < beats.Length; i++)
            {
                beats[i].transform.position = new Vector3(originalPositions[i].x + value, 0, 0); // Move the GameObjects by adding the offset to the original position
            }

        }
        bool IsCursorOnRightSide()
        {
            UnityEngine.Debug.Log("test");
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 objectPosition = selectedObject.transform.position;
            return mousePosition.x >= objectPosition.x;
        }

        // Update is called once per frame
        void Update()
        {

            size.gameObject.GetComponentInChildren<Text>().text = $"Cube size: {size.value}x";
            hp.gameObject.GetComponentInChildren<Text>().text = $"Player health: {hp.value}";
            if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.name.Contains("hitter02"))
                    {
                        selectedObject = hit.collider.transform;
                        initialMousePosition = Input.mousePosition;
                        isCursorOnRightSide = IsCursorOnRightSide();
                    }
                }
            }

            if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift) && selectedObject != null && selectedObject.gameObject.name.Contains("hitter02"))
            {
                float mouseSpeed = Input.GetAxis("Mouse X");
                // Calculate the change in size based on the mouse movement direction
                float newSizeX = selectedObject.GetComponent<SpriteRenderer>().size.x + (mouseSpeed * Time.unscaledDeltaTime * (isCursorOnRightSide ? 1 : -1));
                // Clamp the size within the specified limits
                newSizeX = Mathf.Clamp(newSizeX, 1f, 100);

                // Update the size of the SpriteRenderer
                SpriteRenderer spriteRenderer = selectedObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.size = new Vector2(newSizeX, spriteRenderer.size.y);
                }
            }
            else if (selectedObject != null && DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
            { // Capture initial mouse position only once
                if (Input.GetMouseButtonDown(0))
                {
                    initialMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                UnityEngine.Debug.Log("IT'S " + DateTime.Now.Day + "/" + DateTime.Now.Month);

                Vector3 currentMousePosition = Input.mousePosition;
                float delta = currentMousePosition.x - initialMousePosition.x;

                // Calculate new width and position
                float newWidth = Mathf.Clamp(initialScale.x + delta * expansionSpeed, 1, 100);
                float widthChange = newWidth - initialScale.x;
                Vector3 newPosition = selectedObject.transform.position + selectedObject.transform.right * (widthChange / 2f);

                // Update scale and position
                selectedObject.transform.localScale = new Vector3(newWidth, selectedObject.transform.localScale.y, selectedObject.transform.localScale.z);
                selectedObject.transform.position = newPosition;

                UnityEngine.Debug.Log("fools");
            }
            sliderOffset2.GetComponentInChildren<Text>().text = "BPM Offset (" + (sliderOffset2.value / 7).ToString("F2") + "s)";
            if (sliderOffset2.value < 0.15f && sliderOffset2.value > -0.15f)
            {
                sliderOffset2.value = 0f;
            }

            if (!Input.GetKey(KeyCode.R))
            {

                if (playback.value < 0.275)
                {
                    playback.value = 0.25f;
                    Time.timeScale = playback.value;
                }
                else if (playback.value >= 0.275f && playback.value < 0.4f)
                {
                    playback.value = 0.3f;
                    Time.timeScale = playback.value;
                }
                else if (playback.value >= 0.4f && playback.value < 0.55f)
                {
                    playback.value = 0.5f;
                    Time.timeScale = playback.value;
                }
                else if (playback.value >= 0.55f && playback.value < 0.675f)
                {
                    playback.value = 0.6f;
                    Time.timeScale = playback.value;
                }
                else if (playback.value >= 0.675f && playback.value < 0.875f)
                {
                    playback.value = 0.75f;
                    Time.timeScale = playback.value;
                }
                else if (playback.value >= 0.875f)
                {
                    playback.value = 1;
                    Time.timeScale = playback.value;
                }
            }
            playbackText.text = $"Playback ({Time.timeScale:0.00}x)";
            audio = GameObject.Find("Music").GetComponent<AudioSource>();
            audio.pitch = Time.timeScale;
            PlayerEditorMovement player = GameObject.FindObjectOfType<PlayerEditorMovement>();


            if (cubes.Count > 0 || saws.Count > 0 || longCubes.Count > 0)
            {

                FindFarthestObjectInX(cubes.ToArray());
                FindFarthestObjectInX(saws.ToArray());
                FindFarthestObjectInX(longCubes.ToArray());
                MeasureTimeToReachDistance();
            }
            bgColorSel.color = cam.backgroundColor;
            float xPosNormalized = transform.position.x / 1;
            audio.panStereo = Mathf.Lerp(-1f, 1f, xPosNormalized);

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Delete))
            {
                GameObject[] beats = GameObject.FindGameObjectsWithTag("Beat");
                foreach (GameObject beat in beats)
                {
                    Destroy(beat);
                }
            }
            delay += Time.unscaledDeltaTime;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition.normalized);

            Camera.main.backgroundColor = color1.GetColor();

            Vector2 screenPos = new(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            bool pointerOverUI = EventSystem.current.IsPointerOverGameObject();


            int accurateCount = cubes.Count + saws.Count + longCubes.Count;

            objectCount.text = "Objects: " + accurateCount.ToString() + "/6000";
            string file = Path.Combine(Application.persistentDataPath, "scenes", sceneNameInput.text, sceneNameInput.text + ".json");
            string json = File.ReadAllText(file);
            SceneData data = SceneData.FromJson(json);
            levelID.text = "Level ID: " + data.ID;

           

            if (Input.GetMouseButton(0))
            {
                timer += Time.unscaledDeltaTime;
               
            }
            if (Input.GetMouseButtonUp(0))
            {

                // Find the farthest object in X direction
                Transform targetObject = FindFarthestObjectInX(FindObjectsWithTags(targetTags));

                // Calculate the distance based on the position of the farthest object and additional distance
                if (targetObject != null)
                { 
                    float distance = targetObject.position.x + additionalDistance;
                List<Vector2> cubePositions = new List<Vector2>(); // Declare cubePositions as a List<Vector2>

                foreach (GameObject cube in cubes)
                {
                    Vector2 cubePos = cube.transform.position;
                    cubePositions.Add(cubePos); // Add cube position to the list
                }
                // Calculate the number of cubes per Y level
                int[] cubesPerY = CalculateCubesPerY(cubePositions.ToArray());

                // Calculate the difficulty based on cubes per Y level and other parameters
                float calculatedDifficulty = CalculateDifficulty(cubesPerY, cubePositions.ToArray(), distance / 7);
                    difficulty.text = "Level Difficulty: " + calculatedDifficulty.ToString("f2");
                }
            }

            if (Input.GetMouseButtonUp(0) && timer < 0.125f && delay >= delayLimit && itemButtons[currentButtonPressed].clicked && !pointerOverUI && !player.enabled)
            {
                if (cubes.Count > 0)
                {

                    List<Vector2> cubePositions = new List<Vector2>();

                    for (int i = 0; i < cubes.Count && i < longCubes.Count; i++)
                    {
                        // Get the position of the cube and add it to the list
                        Vector2 cubePos = cubes[i].transform.position;
                        cubePositions.Add(cubePos);

                        // Get the position of the long cube and add it to the list
                        Vector2 longCubePos = longCubes[i].transform.position;
                        cubePositions.Add(longCubePos);
                    }


                    
                }
                timer = 0f;
                delay = 0;
                UnityEngine.Debug.Log(itemButtons[currentButtonPressed]);
                if (worldPos.y < 4.5f && worldPos.x > 1 && worldPos.y > -1.5f && worldPos.x < 20000 && accurateCount < 6000)
                {
                    GameObject[] beatObjects = GameObject.FindGameObjectsWithTag("Beat");
                    GameObject previousBeatObject = null;
                    GameObject nextBeatObject = null;
                    float shortestPreviousDistance = Mathf.Infinity;
                    float shortestNextDistance = Mathf.Infinity;
                    Vector3 clickPosition = new(Mathf.RoundToInt(worldPos.x * 2) / 2, Mathf.Round(worldPos.y), 0);

                    foreach (GameObject beatObject in beatObjects)
                    {
                        float distance = Vector3.Distance(beatObject.transform.position, clickPosition);

                        // Check if the object is closer to the click position than the previously found objects
                        if (beatObject.transform.position.x < clickPosition.x && distance < shortestPreviousDistance)
                        {
                            shortestPreviousDistance = distance;
                            previousBeatObject = beatObject;
                        }
                        else if (beatObject.transform.position.x >= clickPosition.x && distance < shortestNextDistance)
                        {
                            shortestNextDistance = distance;
                            nextBeatObject = beatObject;
                        }
                    }

                    float sqrDistanceToPrevious = (previousBeatObject != null) ? (Input.mousePosition - Camera.main.WorldToScreenPoint(previousBeatObject.transform.position)).sqrMagnitude : float.MaxValue;
                    float sqrDistanceToNext = (nextBeatObject != null) ? (Input.mousePosition - Camera.main.WorldToScreenPoint(nextBeatObject.transform.position)).sqrMagnitude : float.MaxValue;

                    // Determine the nearest beat object
                    GameObject nearestBeatObject = null;
                    if (sqrDistanceToPrevious < sqrDistanceToNext)
                    {
                        nearestBeatObject = previousBeatObject;
                    }
                    else
                    {
                        nearestBeatObject = nextBeatObject;
                    }


                    if (nearestBeatObject != null)
                    {
                        GameObject item = Instantiate(objects[currentButtonPressed], new Vector2(nearestBeatObject.transform.position.x, Mathf.Round(worldPos.y)), Quaternion.identity);

                        if (item.CompareTag("Cubes"))
                        {
                            cubes.Add(item);
                            SelectObject(item.transform);
                        }

                        if (item.CompareTag("Saw"))
                        {
                            saws.Add(item);

                            // Select the newly instantiated saw
                            SelectObject(item.transform);
                        }

                        if (item.CompareTag("LongCube"))
                        {
                            longCubes.Add(item);

                            SelectObject(item.transform);
                        }

                    }
                    else
                    {
                        GameObject item = Instantiate(objects[currentButtonPressed], new Vector3(worldPos.x, Mathf.Round(worldPos.y), 0), Quaternion.identity);
                        if (item.CompareTag("Cubes"))
                        {
                            cubes.Add(item);
                            SelectObject(item.transform);
                        }

                        if (item.CompareTag("Saw"))
                        {
                            saws.Add(item);

                            // Select the newly instantiated saw
                            SelectObject(item.transform);
                        }
                        if (item.CompareTag("LongCube"))
                        {
                            longCubes.Add(item);
                            SelectObject(item.transform);
                        }
                    }

                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                timer = 0f;
            }
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

                if (selectedObject != null)
                {
                    if (!selectedObject.CompareTag("Beat"))
                    {
                        selectedObject.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                    else if (selectedObject.CompareTag("Beat"))
                    {
                        selectedObject.GetComponent<SpriteRenderer>().color = Color.red;
                    }

                    selectedObject = null;
                }

                if (hit.collider != null && hit.collider.transform.CompareTag("Cubes") || hit.collider != null && hit.collider.transform.CompareTag("Saw") || hit.collider != null && hit.collider.transform.CompareTag("Beat") || hit.collider != null && hit.collider.transform.CompareTag("LongCube"))
                {
                    // Get the GameObject associated with the hit
                    GameObject selected = hit.collider.gameObject;

                    // Assign the selected object to the selectedObject variable
                    selectedObject = selected.transform;

                    // Assuming the object has a SpriteRenderer component

                    if (selected.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                    {
                        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.7f);
                    }
                }
                else
                {
                    // Handle the case where the hit.collider is null
                    UnityEngine.Debug.LogWarning("Raycast hit nothing with a collider.");
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) && !Input.GetKey(KeyCode.LeftShift))
            {
                Play();

            }
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Return))
            {
                PlayAudio();
            }

            if (selectedObject != null && !player.enabled && selectedObject.transform.position.x >= 1 && selectedObject.transform.position.x <= 20000)
            {
                float moveX = 0f;
                float moveY = 0f;
                float moveSpeed = 1f;


                if (Input.GetKey(KeyCode.A))
                    moveX -= 1f;
                if (Input.GetKey(KeyCode.D))
                    moveX += 1f;

                // Apply speed modifiers
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    moveX *= 0.25f;
                    moveY *= 0.25f;
                }
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    moveX *= 2f;
                    moveY *= 2f;
                }

                if (Input.GetKeyDown(KeyCode.W) && selectedObject.transform.position.y < 4)
                {
                    selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y + 1, 0);
                }

                if (Input.GetKeyDown(KeyCode.S) && selectedObject.transform.position.y > -1)
                {
                    selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y - 1, 0);
                }

                // Apply movement directly to transform.position
                selectedObject.transform.position += moveSpeed * 0.01f * new Vector3(moveX, moveY, 0);

                if (Input.GetKey(KeyCode.Delete))
                {
                    cubes.Remove(selectedObject.gameObject);
                    saws.Remove(selectedObject.gameObject);
                    longCubes.Remove(selectedObject.gameObject);
                    Destroy(selectedObject.gameObject);

                }
                if (selectedObject.transform.position.x < 1)
                {
                    selectedObject.transform.position = new Vector3(1, selectedObject.transform.position.y, selectedObject.transform.position.z);
                }

                if (selectedObject.transform.position.x > 20000)
                {
                    selectedObject.transform.position = new Vector3(20000, selectedObject.transform.position.y, selectedObject.transform.position.z);
                }
            }

            if (Camera.main.transform.position.x < 7)
            {
                Camera.main.transform.position = new Vector3(7, 0.7f, -10);
            }
            if (player.transform.position != new Vector3(0, -1, player.transform.position.z) && !player.enabled)
            {
                player.transform.position = new Vector2(0, -1);
            }

            if (bgImage.isOn)
            {
                bgimgblur.SetActive(false);
                camBG1.SetActive(true);
            }
            else
            {
                bgimgblur.SetActive(true);
                camBG1.SetActive(false);
            }

            if (!groundToggle.isOn)
            {
                ground.SetActive(false);
                elev.SetActive(false);
            }
            else
            {
                ground.SetActive(true);
                elev.SetActive(true);
            }

            if (player.enabled)
            {
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z))
                {
                    lineController.SaveLinePosition();
                }
            }

        }

        void SelectObject(Transform objTransform)
        {
            if (selectedObject != null)
            {
                selectedObject.GetComponent<SpriteRenderer>().color = Color.white;
            }

            selectedObject = objTransform;

            if (selectedObject.CompareTag("Player"))
            {
                selectedObject = null;
            }

            // Assuming the object has a SpriteRenderer component

            if (objTransform.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.7f);
            }
        }

        private void ClearSelection()
        {
            if (selectedObject != null)
            {
                Destroy(selectedObject.gameObject);
                selectedObject = null;
            }

            if (selectionHighlight != null)
            {
                Destroy(selectionHighlight);
                selectionHighlight = null; // Add this line to reset the reference
            }
        }



        public void DeleteSelectedObject()
        {
            if (selectedObject != null)
            {
                Destroy(selectedObject.gameObject);
                ClearSelection();
            }
        }

        public void ChangePagePrev()
        {
            editorPages[^1].SetActive(true);
        }

        public void ChangePageNext()
        {
            editorPages[editorPages.Length + 1].SetActive(true);

        }

        public void OpenOptions()
        {
            optionsMenu.SetActive(true);
        }

        public void CloseOptions()
        {
            optionsMenu.SetActive(false);
        }

        public void Play()
        {

            PlayerEditorMovement player = GameObject.FindObjectOfType<PlayerEditorMovement>();
            if (!audio.isPlaying && !player.enabled)
            {
                audio.Play();
            }
            else if (audio.isPlaying && player.enabled)
            {
                audio.Stop();
                audio.time = 0f;
            }
            player.enabled = !player.enabled;
            CameraController cam = GameObject.FindObjectOfType<CameraController>();
            cam.enabled = !cam.enabled;
        }

        public void PlayAudio()
        {
            lineController.ToggleLine();
        }

        public void PlaySong()
        {
            if (!audio.isPlaying)
            {
                audio.Play();
            }
            else
            {
                audio.Stop();
            }

        }

        public void OpenPicker01()
        {
            colorPickerMenuBG.SetActive(true);
        }

        public void ClosePicker01()
        {
            colorPickerMenuBG.SetActive(false);
        }

        public void OpenMenu()
        {
            mainMenu.SetActive(true);
        }

        public void CloseMenu()
        {
            mainMenu.SetActive(false);
        }

        public void OpenSelector()
        {
            songSel.SetActive(true);
        }

        public void CloseSelector()
        {
            songSel.SetActive(false);
        }
        public void OpenBG()
        {
            bg.SetActive(true);
        }
        public void CloseBG()
        {
            bg.SetActive(false);
        }

        public void OpenGR()
        {
            grSelector.SetActive(true);
        }
        public void CloseGR()
        {
            grSelector.SetActive(false);
        }

        public void MainMenu()
        {
            SceneManager.LoadSceneAsync(1);
            Time.timeScale = 1f;
        }

        private void LoadSceneWithData(SceneData sceneData)
        {
            LoadSceneAddressable("Assets/LevelDefault.unity", () =>
            {

                SceneManager.UnloadSceneAsync("SampleScene");

                LevelDataManager.Instance.LoadLevelData(sceneData.levelName);


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
                    UnityEngine.Debug.LogError($"Failed to load scene '{sceneKey}': {operation.OperationException}");
                }
            };
        }

        public void SaveScene()
        {
            // Create a new SceneData instance and populate it with current objects' positions
            SceneData sceneData = CreateSceneData();

            // Serialize the SceneData instance to formatted JSON
            string json = JsonUtility.ToJson(sceneData, true);

            // Get the file path based on the scene name
            string filePath = GetSceneDataPath(sceneNameInput.text.Trim());

            // Write the JSON data to the file
            File.WriteAllText(filePath, json);


            UnityEngine.Debug.Log("Scene saved successfully: " + sceneNameInput.text.Trim());
        }
        private void CopyFileDirectly(string sourcePath, string destinationPath)
        {
            try
            {
                File.Copy(sourcePath, destinationPath, true);
                UnityEngine.Debug.Log("Copied file to " + destinationPath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Failed to copy file: " + e.Message);
            }
        }
        private void SaveBackgroundImageTexture(string sceneDataPath)
        {
            // Ensure that the directory for the scene data exists
            Directory.CreateDirectory(Path.GetDirectoryName(sceneDataPath));

            // Create a temporary render texture
            RenderTexture tempRT = new(bgPreview.mainTexture.width, bgPreview.mainTexture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(bgPreview.mainTexture, tempRT);

            // Activate the render texture
            RenderTexture.active = tempRT;

            // Create a new texture and read the pixels from the render texture
            Texture2D bgTexture2D = new(tempRT.width, tempRT.height);
            bgTexture2D.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            bgTexture2D.Apply();

            // Encode the Texture2D to PNG format
            byte[] bytes = bgTexture2D.EncodeToPNG();

            // Specify the full file path for the background image file
            string bgImagePath = Path.Combine(Path.GetDirectoryName(sceneDataPath), "bgImage.png");

            // Write the encoded bytes to the image file
            File.WriteAllBytes(bgImagePath, bytes);

            UnityEngine.Debug.Log("Background image saved successfully: " + bgImagePath);

            // Clean up resources
            RenderTexture.active = null;
            Destroy(tempRT);
            Destroy(bgTexture2D);
        }

        public void LoadScene()
        {
            string sceneName = sceneNameInput.text.Trim();

            if (string.IsNullOrEmpty(sceneName))
            {
                UnityEngine.Debug.LogError("Scene name is empty. Please enter a valid scene name.");
                return;
            }

            string filePath = GetSceneDataPath(sceneName);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                SceneData sceneData = JsonUtility.FromJson<SceneData>(json);

                LoadSceneWithData(sceneData);

                UnityEngine.Debug.Log("Scene loaded successfully: " + sceneName);
            }
            else
            {
                UnityEngine.Debug.LogError("Scene file not found: " + filePath);
            }
        }

        private string GetSceneDataPath(string sceneName)
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, "scenes", sceneName);
            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, sceneName + ".json");
            filePath = filePath.Replace("\\", "/");
            return filePath;
        }


        private SceneData CreateSceneData()
        {
            // Find the farthest object in X direction
            Transform targetObject = FindFarthestObjectInX(FindObjectsWithTags(targetTags));

            // Calculate the distance based on the position of the farthest object and additional distance
            float distance = targetObject.position.x + additionalDistance;
            List<Vector2> cubePositions = new List<Vector2>(); // Declare cubePositions as a List<Vector2>

            foreach (GameObject cube in cubes)
            {
                Vector2 cubePos = cube.transform.position;
                cubePositions.Add(cubePos); // Add cube position to the list
            }
            // Calculate the number of cubes per Y level
            int[] cubesPerY = CalculateCubesPerY(cubePositions.ToArray());

            // Calculate the difficulty based on cubes per Y level and other parameters
            float calculatedDifficulty = CalculateDifficulty(cubesPerY, cubePositions.ToArray(), distance / 7);
            string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, "scenes", sceneNameInput.text, sceneNameInput.text + ".json"));
            SceneData data = SceneData.FromJson(json);
            // Create a SceneData object and populate its properties
            SceneData sceneData = new SceneData()
            {
                levelName = sceneNameInput.text.Trim(),
                sceneName = sceneNameInput.text.Trim(),
                songName = (audio.clip != null) ? customSongName.text : "Pricklety - Fall'd",
                bpm = int.Parse(bpmInput.text),
                calculatedDifficulty = calculatedDifficulty % 150,
                gameVersion = Application.version,
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                clip = audio.clip,
                ground = groundToggle.isOn,
                levelLength = (int)(distance / 7),
                creator = creator.text,
                playerHP = (int)hp.value,
                boxSize = size.value
            };
            if (data.ID == 0)
            {
                sceneData.ID = Random.Range(int.MinValue, int.MaxValue);
            }
            else
            {
                sceneData.ID = data.ID;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                sceneData.defBGColor = mainCamera.backgroundColor;
            }
            else
            {
                UnityEngine.Debug.LogWarning("No main camera found. Setting default background color to black.");
                sceneData.defBGColor = Color.black; // Or any other default color you want to use
            }

            string directoryPath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName);

            string newPath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName, sceneData.songName);
            newPath = newPath.Replace("/", "\\");
            CopyFileDirectly(musicFolderPath, newPath);
            UnityEngine.Debug.LogError(newPath);

            sceneData.clipPath = newPath;
            if (bgPreview.texture != null && bgImage.isOn)
            {
                SaveBackgroundImageTexture(Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName, "bgImage.png"));
                sceneData.picLocation = Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName, "bgImage.png");
            }
            UnityEngine.Debug.Log(musicFolderPath);

            if (cubes != null)
            {
                sceneData.cubePositions = new List<Vector3>();

                foreach (GameObject cube in cubes)
                {
                    Vector3 cubePos = cube.transform.position;
                    sceneData.cubePositions.Add(cubePos);
                    UnityEngine.Debug.Log($"Added cube position: {cubePos}");
                }
            }

            if (saws != null)
            {
                sceneData.sawPositions = new List<Vector3>();

                foreach (GameObject saw in saws)
                {
                    Vector3 sawPos = saw.transform.position;
                    sceneData.sawPositions.Add(sawPos);
                    UnityEngine.Debug.Log($"Added saw position: {sawPos}");
                }
            }
            if (longCubes != null)
            {
                sceneData.longCubePositions = new List<Vector3>();
                sceneData.longCubeWidth = new();

                foreach (GameObject cube in longCubes)
                {
                    Vector3 cubePos = cube.transform.position;
                    sceneData.longCubePositions.Add(cubePos);
                    float width = cube.GetComponent<SpriteRenderer>().size.x;
                    sceneData.longCubeWidth.Add(width);
                    UnityEngine.Debug.Log($"Added long cube position: {cubePos}");
                    UnityEngine.Debug.Log($"Added long cube width: {width}");
                }

            }
            return sceneData;
        }

        private SceneData CreateLevelSceneData()
        {
            SaveSceneData();
            Transform targetObject = FindFarthestObjectInX(FindObjectsWithTags(targetTags));

            // Calculate the distance based on the position of the farthest object and additional distance
            float distance = targetObject.position.x + additionalDistance;
            List<Vector2> cubePositions = new List<Vector2>(); // Declare cubePositions as a List<Vector2>

            foreach (GameObject cube in cubes)
            {
                Vector2 cubePos = cube.transform.position;
                cubePositions.Add(cubePos); // Add cube position to the list
            }
            // Calculate the number of cubes per Y level
            int[] cubesPerY = CalculateCubesPerY(cubePositions.ToArray());

            // Calculate the difficulty based on cubes per Y level and other parameters
            float calculatedDifficulty = CalculateDifficulty(cubesPerY, cubePositions.ToArray(), distance / 7);

            // Create a SceneData object and populate its properties
            SceneData sceneData = new SceneData()
            {
                levelName = sceneNameInput.text.Trim(),
                sceneName = sceneNameInput.text.Trim(),
                songName = (audio.clip != null) ? customSongName.text : "Pricklety - Fall'd",
                calculatedDifficulty = calculatedDifficulty,
                gameVersion = Application.version,
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                clip = audio.clip,
                ground = groundToggle.isOn,
                levelLength = (int)(distance / 7),
                creator = creator.text,

            };
            string newPath = Path.Combine(Application.persistentDataPath, "levels", "extracted", sceneData.levelName, sceneData.songName);
            newPath = newPath.Replace("\\", "/");
            if (sceneData.ID == 0)
                sceneData.ID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            // Get the source and destination paths
            string sourceFolderPath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName);
            string destinationFolderPath = Path.Combine(Application.persistentDataPath, "levels", sceneData.levelName);

            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationFolderPath);

            // Copy files and directories from source to destination
            foreach (string sourcePath in Directory.GetFiles(sourceFolderPath, "*", SearchOption.AllDirectories))
            {
                string relativePath = sourcePath.Substring(sourceFolderPath.Length + 1);
                string destinationPath = Path.Combine(destinationFolderPath, relativePath);

                string destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                File.Copy(sourcePath, destinationPath, true);
            }



            sceneData.clipPath = newPath;
            // Create a ZIP file with the .jdl extension
            string zipFilePath = Path.Combine(Application.persistentDataPath, "levels", $"{sceneData.levelName}.jdl");
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);

            ZipFile.CreateFromDirectory(destinationFolderPath, zipFilePath, System.IO.Compression.CompressionLevel.NoCompression, false);

            // Delete the original copied folder
            Directory.Delete(destinationFolderPath, true);


            return sceneData;
        }

        public float CalculateDifficulty(int[] cubeCountsPerY, Vector2[] cubePositions, float clickTimingWindow)
        {
            float difficulty = 0f;
            Transform targetObject = FindFarthestObjectInX(FindObjectsWithTags(targetTags));

            // Calculate the distance based on the position of the farthest object and additional distance
            float distance = targetObject.position.x + additionalDistance;

            // Iterate over each Y level
            for (int y = -1; y <= 4; y++)
            {
                // Get the number of cubes on this Y level
                int cubeCount = cubeCountsPerY[y + 1];

                // If there are no cubes on this level, continue
                if (cubeCount == 0)
                    continue;

                // Iterate over each pair of consecutive cubes on this Y level
                for (int i = 0; i < cubeCount - 1; i++)
                {
                    // Calculate timing window based on Y level difference and player movement speed
                    float timingWindow = CalculateTimingWindow(cubePositions[i], cubePositions[i + 1], y);

                    // Consider X position variation for precision calculation
                    float xPositionVariation = Mathf.Abs(cubePositions[i].x - cubePositions[i + 1].x);

                    // Factor in precision required for clicking correctly
                    float precisionFactor = CalculatePrecisionFactor(xPositionVariation);

                    // Avoid division by zero by ensuring clickTimingWindow + distance is not zero
                    float divisor = clickTimingWindow + distance != 0 ? clickTimingWindow + distance : float.Epsilon;
                    // Add difficulty contribution for this pair of cubes
                    float contribution = timingWindow / 1.04f
                     + cubeCount / 20f
                     * (cubes.Count / 80f)
                     * (saws.Count / 20f)
                     * (longCubes.Count / 100f)
                     * (precisionFactor / divisor * 100f)
                     + 1 / (hp.value + 1) * 15f
                     +Mathf.Exp(0.1f * (0.5f - size.value) * 70f);

                    // Round the contribution to avoid approximate results
                    contribution = Mathf.Round(contribution * 1000000f) / 1000000f; // Round to 6 decimal places

                    difficulty += contribution;
                }
            }
            // Round the final difficulty value
            difficulty = Mathf.Round(difficulty * 1000000f) / 1000000f;
            return difficulty;
        }
        private float CalculateTimingWindow(Vector2 position1, Vector2 position2, int yLevelDifference)
        {
            // Calculate the Y-axis distance between the cubes
            float yDistance = Mathf.Abs(position2.y - position1.y);

            // Calculate timing window based on Y-axis distance and player movement speed
            float timeWindow = yDistance * 0.1f;

            return timeWindow;
        }


        private float CalculatePrecisionFactor(float xPositionVariation)
        {
            float precisionFactor = 1 - xPositionVariation / 20f;

            return Mathf.Clamp01(precisionFactor);
        }
        public int[] CalculateCubesPerY(Vector2[] cubePositions)
        {
            // Initialize an array to store the number of cubes per Y level
            int[] cubesPerY = new int[6]; // Y levels range from -1 to 4, so 6 elements are needed

            // Iterate over each cube position and count the number of cubes per Y level
            foreach (Vector2 position in cubePositions)
            {
                int yLevel = Mathf.RoundToInt(position.y); // Round Y position to the nearest integer
                yLevel = Mathf.Clamp(yLevel, -1, 4); // Ensure Y level is within valid range

                // Increment the count for the corresponding Y level
                cubesPerY[yLevel + 1]++;
            }

            return cubesPerY;
        }


        public void OpenBGSel()
        {
            bgSel.SetActive(true);
        }

        public void CloseBGSel()
        {
            bgSel.SetActive(false);
        }

        public void OpenImageDialog()
        {
            FileBrowser.m_instance = Instantiate(Resources.Load<GameObject>("SimpleFileBrowserCanvas")).GetComponent<FileBrowser>();
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png", ".jpg", ".jpeg"));
            FileBrowser.SetDefaultFilter(".png");
            FileBrowser.ShowLoadDialog(OnFileSelected, null, FileBrowser.PickMode.Files);
        }

        void OnFileSelected(string[] paths)
        {
            if (paths.Length >= 0)
            {
                selectedImagePath = paths[0];
                StartCoroutine(LoadImage(selectedImagePath));
            }
        }

        public void RemoveIMG()
        {
            camBG.sprite = null;
            bgPreview.texture = null;
        }

        IEnumerator LoadImage(string path)
        {
            if (File.Exists(path))
            {
                byte[] fileData = File.ReadAllBytes(path);
                Texture2D texture = new(2, 2);

                if (texture.LoadImage(fileData))
                {
                    bgPreview.texture = texture;
                    yield return null;

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    camBG.sprite = sprite;
                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to load the image: " + path);
                }
            }
            else
            {
                UnityEngine.Debug.LogError("File does not exist: " + path);
            }
        }

    }
}
