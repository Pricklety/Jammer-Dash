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
using JammerDash.Notifications;

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
        public GameObject colorPickerMenuBG;
        public Text objectCount;
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public AudioSource audio;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
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
        public InputField bpmInput;
        public Slider bpmMultiplier;
        public InputField creator;
        public InputField customSongName;
        public InputField songArtist;
        public Text levelID;
        public InputField offsetmarker;

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
        public float delay;
        public float delayLimit = 0.25f;

        [Header("Long Cube")]
        public float minWidth = 1f;
        public float maxWidth = 999f; 
        public float expansionSpeed = 1f; 

        private Vector3 initialMousePosition;
        private Vector3 initialScale;
        private bool isCursorOnRightSide;
        private GameObject[] beatObjects;
        private int lastBeatObjectCount = 0;

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


        }
        private void MeasureTimeToReachDistance()
        {
            GameObject[] objectsWithTag = Object.FindObjectsWithTags(targetTags);
            Transform targetObject = Difficulty.FindFarthestObjectInX(objectsWithTag);
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

      

        public IEnumerator LoadAudioClip(string filePath)
        {
            Resources.UnloadUnusedAssets();
            filePath = filePath.Replace("\\", "/");
            // Encode the file path to ensure proper URL encoding
            string encodedPath = EncodeFilePath(filePath);
            string fileUri = "file://" + encodedPath;
            UnityEngine.Debug.LogError(encodedPath);

            // Use UnityWebRequest to load the audio clip
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUri + ".mp3", AudioType.UNKNOWN))
            {

                var requestOperation = www.SendWebRequest();

                while (!requestOperation.isDone)
                {
                    yield return null; // Wait for the next frame
                }

                // Check if the request was successful
                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

                    if (audioClip != null)
                    {
                        // Set the audio source clip
                        audioClip.name = Path.GetFileName(filePath);
                        UnityEngine.Debug.Log(filePath);
                        nameText.text = audioClip.name;
                        audio.clip = audioClip;
                        lineController.audioClip = audioClip;
                        musicFolderPath = filePath;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Failed to extract audio clip from UnityWebRequest: " + filePath);
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to load audio clip: " + www.error);
                }
            }
        }

        private string EncodeFilePath(string filePath)
        {
            // Encode the file path to ensure proper URL encoding
            string encodedPath = Uri.EscapeUriString(filePath);
            // Replace "+" with "%2B"
            encodedPath = encodedPath.Replace("+", "%2B");
            return encodedPath;
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
                sceneData.clipPath = Path.Combine(Application.persistentDataPath, "scenes", sceneName, sceneData.artist + " - " + sceneData.songName);
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
                StartCoroutine(LoadAudioClip(sceneData.clipPath));
                creator.text = sceneData.creator; 
                hp.value = sceneData.playerHP;
                size.value = sceneData.boxSize;
                offsetmarker.text = sceneData.offset.ToString();
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

                // Get the directory path based on the scene name
                string directoryPath = GetLevelDataPath(sceneData.sceneName);
                UnityEngine.Debug.Log(directoryPath);

                // Check if the directory exists before proceeding
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath); // Create the directory if it doesn't exist
                }

                // Serialize the SceneData instance to formatted JSON
                string json = JsonUtility.ToJson(sceneData, true);


                // Write the encrypted JSON data to a file inside the directory
                string encryptedJsonFilePath = Path.Combine(directoryPath, $"{sceneData.sceneName}.json");
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
                Notifications.Notifications.instance.Notify($"{sceneData.levelName} exported successfully.", null);
                File.Delete(zipFilePath);
                UnityEngine.Debug.Log($"Level data for {sceneData.levelName} saved in folder: {directoryPath}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error saving scene: " + e);
                Notifications.Notifications.instance.Notify($"Couldn't save scene: {e.Message}.\nTry again later.", null);
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

                // Serialize the SceneData instance to formatted JSON
                string json = JsonUtility.ToJson(sceneData, true);

                // Get the file path based on the scene name
                string filePath = GetSceneDataPath(sceneData.sceneName);

                // Write the JSON data to the file
                File.WriteAllText(filePath, json);

                Notifications.Notifications.instance.Notify($"{ sceneData.sceneName} successfully saved." +
                $"\nDifficulty: {sceneData.calculatedDifficulty}sn" +
                $"\nLength: {sceneData.levelLength} seconds", null);
            }
            catch (Exception e)
            {
                Notifications.Notifications.instance.Notify($"Oops, something wrong happened!\nTry again later.", null);
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



        }
       
        private Vector3[] originalPositions;
       
       
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
            
            float value = float.Parse(offsetmarker.text);
            GameObject[] beats = GameObject.FindGameObjectsWithTag("Beat");

            for (int i = 0; i < beats.Length; i++)
            {
                beats[i].transform.position = new Vector3(originalPositions[i].x + (value / 700), 0, 0);
            }
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

                Difficulty.FindFarthestObjectInX(cubes.ToArray());
                Difficulty.FindFarthestObjectInX(saws.ToArray());
                Difficulty.FindFarthestObjectInX(longCubes.ToArray());
                MeasureTimeToReachDistance();
            }
            bgColorSel.color = cam.backgroundColor;
            float xPosNormalized = transform.position.x / 1;
            audio.panStereo = Mathf.Lerp(-1f, 1f, xPosNormalized);

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Delete))
            {
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

           

          
            if (Input.GetKeyDown(KeybindingManager.place) && delay >= delayLimit && itemButtons[currentButtonPressed].clicked && !pointerOverUI && !player.enabled)
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
                delay = 0;

                if (worldPos.y < 4.5f && worldPos.x > 1 && worldPos.y > -1.5f)
                {
                    // Cache beat objects and their positions
                    GameObject[] beatObjects = GameObject.FindGameObjectsWithTag("Beat");
                    Vector3[] beatPositions = new Vector3[beatObjects.Length];
                    for (int i = 0; i < beatObjects.Length; i++)
                    {
                        beatPositions[i] = beatObjects[i].transform.position;
                    }

                    GameObject previousBeatObject = null;
                    GameObject nextBeatObject = null;
                    float shortestPreviousDistance = Mathf.Infinity;
                    float shortestNextDistance = Mathf.Infinity;
                    Vector3 clickPosition = new(Mathf.RoundToInt(worldPos.x * 2) / 2, Mathf.Round(worldPos.y), 0);

                    for (int i = 0; i < beatPositions.Length; i++)
                    {
                        float distance = Vector3.Distance(beatPositions[i], clickPosition);

                        // Check if the object is closer to the click position than the previously found objects
                        if (beatPositions[i].x < clickPosition.x && distance < shortestPreviousDistance)
                        {
                            shortestPreviousDistance = distance;
                            previousBeatObject = beatObjects[i];
                        }
                        else if (beatPositions[i].x >= clickPosition.x && distance < shortestNextDistance)
                        {
                            shortestNextDistance = distance;
                            nextBeatObject = beatObjects[i];
                        }
                    }

                    float sqrDistanceToPrevious = (previousBeatObject != null) ? (Input.mousePosition - Camera.main.WorldToScreenPoint(previousBeatObject.transform.position)).sqrMagnitude : float.MaxValue;
                    float sqrDistanceToNext = (nextBeatObject != null) ? (Input.mousePosition - Camera.main.WorldToScreenPoint(nextBeatObject.transform.position)).sqrMagnitude : float.MaxValue;

                    // Determine the nearest beat object
                    GameObject nearestBeatObject = (sqrDistanceToPrevious < sqrDistanceToNext) ? previousBeatObject : nextBeatObject;

                    // Instantiate the item at the nearest beat object position
                    Vector2 instantiatePosition = (nearestBeatObject != null)
                        ? new Vector2(nearestBeatObject.transform.position.x, Mathf.Round(worldPos.y))
                        : new Vector3(worldPos.x, Mathf.Round(worldPos.y), 0);

                    GameObject item = Instantiate(objects[currentButtonPressed], instantiatePosition, Quaternion.identity);
                    if (item.CompareTag("Cubes"))
                    {
                        cubes.Add(item);
                        SelectObject(item.transform);
                    }
                    else if (item.CompareTag("Saw"))
                    {
                        saws.Add(item);
                        SelectObject(item.transform);
                    }
                    else if (item.CompareTag("LongCube"))
                    {
                        longCubes.Add(item);
                        SelectObject(item.transform);
                    }
                }
            }

            if (Input.GetKeyDown(KeybindingManager.selectObject))
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

       

        private string GetSceneDataPath(string sceneName)
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, "scenes", sceneName);
                 Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, sceneName + ".json");
            return filePath;
        }


        private SceneData CreateSceneData()
        {
            // Find the farthest object in X direction
            Transform targetObject = Difficulty.FindFarthestObjectInX(Object.FindObjectsWithTags(targetTags));

            // Calculate the distance based on the position of the farthest object and additional distance
            float distance = targetObject.position.x + additionalDistance;
            List<Vector2> cubePositions = new List<Vector2>(); // Declare cubePositions as a List<Vector2>

            foreach (GameObject cube in cubes)
            {
                Vector2 cubePos = cube.transform.position;
                cubePositions.Add(cubePos); // Add cube position to the list
            }
            // Calculate the number of cubes per Y level
            int[] cubesPerY = Difficulty.CalculateCubesPerY(cubePositions.ToArray());

            // Calculate the difficulty based on cubes per Y level and other parameters
            float calculatedDifficulty = Difficulty.CalculateDifficulty(cubes, saws, longCubes, hp, size, cubesPerY, cubePositions.ToArray(), distance / 7);
            string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, "scenes", sceneNameInput.text, sceneNameInput.text + ".json"));
            SceneData data = SceneData.FromJson(json);
            // Create a SceneData object and populate its properties
            SceneData sceneData = new SceneData()
            {

                sceneName = sceneNameInput.text.Trim(),
                bpm = int.Parse(bpmInput.text),
                calculatedDifficulty = calculatedDifficulty % 150,
                gameVersion = Application.version,
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                songLength = audio.clip.length,
                clipPath = Path.Combine(Application.persistentDataPath, "scenes", sceneNameInput.text, songArtist.text + " - " + customSongName.text + ".mp3"),
                ground = groundToggle.isOn,
                levelLength = (int)(distance / 7),
                creator = creator.text,
                playerHP = (int)hp.value,
                boxSize = size.value,
                artist = songArtist.text,
                songName = customSongName.text,
                offset = float.Parse(offsetmarker.text)
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


          
       
            if (bgPreview.texture != null && bgImage.isOn)
            {
                SaveBackgroundImageTexture(Path.Combine(Application.persistentDataPath, "scenes", sceneData.sceneName, "bgImage.png"));
                sceneData.picLocation = Path.Combine(Application.persistentDataPath, "scenes", sceneData.sceneName, "bgImage.png");
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
            Transform targetObject = Difficulty.FindFarthestObjectInX(Object.FindObjectsWithTags(targetTags));

            // Calculate the distance based on the position of the farthest object and additional distance
            float distance = targetObject.position.x + additionalDistance;
            List<Vector2> cubePositions = new List<Vector2>(); // Declare cubePositions as a List<Vector2>

            foreach (GameObject cube in cubes)
            {
                Vector2 cubePos = cube.transform.position;
                cubePositions.Add(cubePos); // Add cube position to the list
            }
            // Calculate the number of cubes per Y level
            int[] cubesPerY = Difficulty.CalculateCubesPerY(cubePositions.ToArray());

            // Calculate the difficulty based on cubes per Y level and other parameters
            float calculatedDifficulty = Difficulty.CalculateDifficulty(cubes, saws, longCubes, hp, size, cubesPerY, cubePositions.ToArray(), distance / 7);

            // Create a SceneData object and populate its properties
            SceneData sceneData = new SceneData()
            {
                levelName = sceneNameInput.text.Trim(),
                sceneName = sceneNameInput.text.Trim(),
                calculatedDifficulty = calculatedDifficulty,
                gameVersion = Application.version,
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),

                songLength = audio.clip.length,
                artist = songArtist.text,
                songName = customSongName.text,
                ground = groundToggle.isOn,
                levelLength = (int)(distance / 7),
                creator = creator.text,
                offset = float.Parse(offsetmarker.text)

            };
            sceneData.clipPath = Path.Combine(Application.persistentDataPath, "levels", "extracted", sceneNameInput.text, songArtist.text + " - " + customSongName.text + ".mp3");
            if (sceneData.ID == 0)
                sceneData.ID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            // Get the source and destination paths
            string sourceFolderPath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName);
            string destinationFolderPath = Path.Combine(Application.persistentDataPath, "levels", sceneData.levelName);


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



            // Create a ZIP file with the .jdl extension
            string zipFilePath = Path.Combine(Application.persistentDataPath, "levels", $"{sceneData.levelName}.jdl");
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);

            ZipFile.CreateFromDirectory(destinationFolderPath, zipFilePath, System.IO.Compression.CompressionLevel.NoCompression, false);

            // Delete the original copied folder
            Directory.Delete(destinationFolderPath, true);


            return sceneData;
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
