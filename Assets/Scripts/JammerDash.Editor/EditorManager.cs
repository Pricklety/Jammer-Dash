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
using JammerDash.Difficulty;
using UnityEngine.UI.Extensions.Tweens;
using JammerDash.Audio;
using System.Threading.Tasks;
using JammerDash.Editor.Screens;
using UnityEngine.Localization.Settings;
using System.Diagnostics;
using UnityEngine.Video;
using System.Threading;
using TMPro;
using System.Text.RegularExpressions;

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
        public Text expandText;
        public AudioClip[] hitSounds;
        [Header("UI and Stuff")]
        public LoadingScreen loadingPanel;
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
        public GameObject bgSelector;
        public RawImage bgPreview;
        public RawImage camBG;
        public GameObject camBG1;
        public VideoPlayer videoPlayer;
        public GameObject bgSel;
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
        public Slider lengthSlider;
        public Text lengthText;
        public InputField romaji;
        public InputField romajiArtist;
        public InputField source;
        
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
        public GameObject[] cubePrefab;
        public GameObject sawPrefab;
        public GameObject longCubePrefab;
        public Transform parentTransform;
        public List<Vector2> cubePositions = new();
        public List<Vector2> sawPositions = new();
        public List<Vector2> longCubePositions = new();
        public List<float> longCubeWidth = new();
        public string imageTexturePath;
        public string imageParentPath;
        public Color backgroundColor;
        public string nameInBundle;
        public List<GameObject> cubes; // List of cube game objects
        public List<GameObject> saws; // List of saw game objects
        public List<GameObject> longCubes;
        public InputField bpm;
        public int ID;

        [Header("Length Calculator")]
        public string[] targetTags = { "Cube", "Saw" }; // Set the desired tags
        public float speed = 7f; // Speed in units per second
        public float additionalDistance = 5f;
        public Text levelLength;

        [Header("Other")]
        private string selectedImagePath;
        public float delay;
        public float delayLimit = 0.25f;
        public Animator autoSave;

        [Header("Long Cube")]
        public float minWidth = 1f;
        public float maxWidth = 999f;
        public float expansionSpeed = 1f;


        // Start is called before the first frame update
        void Start()
        {
            TexturePack.Instance.UpdateTexture();
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
                Notifications.instance.Notify("Client error: Shaders are not initialized.", null);
            }
            MeasureTimeToReachDistance();
            Time.timeScale = playback.value;
            cubes.AddRange(GameObject.FindGameObjectsWithTag("Cubes"));
            saws.AddRange(GameObject.FindGameObjectsWithTag("Saw"));
            StartCoroutine(AutoSave());
        }

        public void OnMultiplierChange()
        {
            // Check if the audio clip is assigned
            if (audio.clip == null)
            {
                return;
            }

            // Parse BPM value from text
            if (!int.TryParse(bpm.text, out int parsedBPM))
            {
                Notifications.instance.Notify("Failed to parse BPM value. Try again.", null);
                return;
            }
            parsedBPM *= (int)bpmMultiplier.value;
            bpmMultiplier.GetComponentInChildren<Text>().text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Marker multiplier")} (" + bpmMultiplier.value + "x)";

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
                Vector3 position = new(x, -1f, 0f);
                GameObject newLine = Instantiate(linePrefab, position, Quaternion.identity);

                // Set original position if newLine has "Beat" tag
                if (newLine.CompareTag("Beat"))
                {
                    originalPositions[i] = position;
                }
            }


        }
        private async void MeasureTimeToReachDistance()
        {
            GameObject[] objectsWithTag = await Difficulty.Object.FindObjectsWithTags(targetTags);
            Transform targetObject = Calculator.FindFarthestObjectInX(objectsWithTag);
            if (targetObject != null)
            {
                float distance = targetObject.position.x + additionalDistance;

                // Check if the target is already behind the starting point
                if (distance <= 0)
                {
                    levelLength.text = "Invalid distance.";
                    return;
                }

                float timeToReachDistance = distance / speed;

                // Convert time to minutes:seconds format
                int minutes = Mathf.FloorToInt(timeToReachDistance / 60);
                float seconds = timeToReachDistance % 60;


                // Display the result on the Text component
                levelLength.text = $"";

            }
            else
            {
                levelLength.text = "";
            }
        }



        public IEnumerator LoadAudioClip(string filePath)
        {
            Resources.UnloadUnusedAssets();
            filePath = filePath.Replace("\\", "/");
            // Encode the file path to ensure proper URL encoding
            string encodedPath = EncodeFilePath(filePath);
            string fileUri = "file://" + encodedPath;

            // Use UnityWebRequest to load the audio clip
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.UNKNOWN))
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

        Dictionary<string, int> cubeTypeMapping = new Dictionary<string, int>
{
    { "hitter01", 1 },
    { "hitter03", 3 },
    { "hitter04", 4 },
    { "hitter05", 5 },
    { "hitter06", 6 },
};

        public void LoadSceneData(SceneData scene)
        {
            cubes = new List<GameObject>();
            saws = new List<GameObject>();
            longCubes = new();
            string name = customSongName.text;
            string filePath = GetSceneDataPath(scene.ID, scene.name);
            ID = scene.ID;
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
                for (int i = 0; i < sceneData.cubePositions.Count; i++)
                {
                    Vector3 cubePos = sceneData.cubePositions[i];
                    // Get the original integer index from sceneData.
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
                        if (mappedIndex >= 0 && mappedIndex < cubePrefab.Length)
                        {
                            GameObject cube = Instantiate(cubePrefab[mappedIndex], cubePos, Quaternion.identity, parentTransform);
                            cubes.Add(cube);
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
                StartCoroutine(LoadImageCoroutine(Path.Combine(Main.gamePath, "scenes", $"{scene.ID} - {scene.name}", "bgImage.png")));
                bpm.text = sceneData.bpm.ToString();
                color1.startingColor = sceneData.defBGColor;
                StartCoroutine(LoadAudioClip(Path.Combine(Main.gamePath, "scenes", $"{scene.ID} - {scene.name}", $"{sceneData.artist} - {sceneData.songName}.mp3")));
                creator.text = sceneData.creator;
                romaji.text = sceneData.romanizedName;
                romajiArtist.text = sceneData.romanizedArtist;
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
            string extension = Path.GetExtension(url).ToLower();

             if (extension == ".mp4")
    {
        // Check if the VideoPlayer component is already attached, otherwise add it
        if (videoPlayer == null)
        {
            videoPlayer = camBG.gameObject.AddComponent<VideoPlayer>();
        }

        // Set the video URL
        videoPlayer.url = url;

        // Set VideoPlayer properties
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        
        // Prepare the video before playing
        videoPlayer.prepareCompleted += OnVideoPrepared;

        // Prepare the video (loads the video into memory asynchronously)
        videoPlayer.Prepare();

        // Show loading notification or loading animation here if needed
        Notifications.instance.Notify("Loading video...", null);

        // Wait until video preparation is complete
        while (!videoPlayer.isPrepared)
        {
            yield return null; // Yield until preparation is complete
        }

        // Once the video is prepared, we can assign it to the texture and play it
        bgPreview.gameObject.SetActive(true); // Show RawImage
        bgImage.isOn = true;
        bgPreview.texture = videoPlayer.targetTexture;
        camBG.texture = videoPlayer.targetTexture;

        // Play the video after it's prepared
        videoPlayer.Play();
    }
            else
            {
            // Handle image loading
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
                camBG.texture = texture;
                }
                else
                {
                UnityEngine.Debug.Log("Failed to load image: " + www.error);
                }
            }
            }
        
         if (extension == ".mp4")
    {
        // Check if the VideoPlayer component is already attached, otherwise add it
        if (videoPlayer == null)
        {
            videoPlayer = camBG.gameObject.AddComponent<VideoPlayer>();
        }

        // Set the video URL
        videoPlayer.url = url;

        // Set VideoPlayer properties
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        
        // Prepare the video before playing
        videoPlayer.prepareCompleted += OnVideoPrepared;

        // Prepare the video (loads the video into memory asynchronously)
        videoPlayer.Prepare();

        Notifications.instance.Notify("Loading video...", null);

        // Wait until video preparation is complete
        while (!videoPlayer.isPrepared)
        {
            yield return null; // Yield until preparation is complete
        }

        // Once the video is prepared, we can assign it to the texture and play it
        bgPreview.gameObject.SetActive(true); // Show RawImage
        bgImage.isOn = true;
        bgPreview.texture = videoPlayer.targetTexture;
        camBG.texture = videoPlayer.targetTexture;

        // Play the video after it's prepared
        videoPlayer.Play();
    }
    else
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
                    camBG.texture = texture;
                }
                else
                {
                    UnityEngine.Debug.Log("Failed to load image: " + www.error);
                }
            }
    }
        }
private void OnVideoPrepared(VideoPlayer source)
{
    Debug.Log("Video successfully prepared and loaded.");

    // You can notify the user that the video is ready
    Notifications.instance.Notify("Video loaded and ready to play!", null);
}
        public async Task SaveLevelData()
        {
            try
            {
                // Create a new SceneData instance and populate it with current objects' positions
                SceneData sceneData = await CreateLevelSceneData(loadingPanel);


                Notifications.instance.Notify($"{sceneData.name} successfully exported with ID {sceneData.ID}.", null);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error saving scene: " + e);
                Notifications.instance.Notify($"Couldn't save scene: {e.Message}.\nTry again later.", null);
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
                Notifications.instance.Notify("Failed to parse BPM value. Try again.", null);
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
                Vector3 position = new(x, -1f, 0f);
                GameObject newLine = Instantiate(linePrefab, position, Quaternion.identity);

                // Set original position if newLine has "Beat" tag
                if (newLine.CompareTag("Beat"))
                {
                    originalPositions[i] = position;
                }
            }



        }

        private Vector3[] originalPositions;


       
        public string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            // Calculate milliseconds from the fractional part of the original time
            int ms = Mathf.FloorToInt((time - Mathf.Floor(time)) * 1000);

            // Ensure seconds don't go beyond 59
            seconds = Mathf.Clamp(seconds, 0, 59);

            return string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, ms);
        }
        public void LengthPos()
        {
            cam.transform.position = new Vector3(lengthSlider.value * 7, 1, -10);
        }

        // Update is called once per frame
        void Update()
        {
            expandText.text = $"{KeybindingManager.changeLongCubeSize} + {KeybindingManager.place} {LocalizationSettings.StringDatabase.GetLocalizedString("lang", "to expand")}";
            lengthText.text = FormatTime(lengthSlider.value / 7);
            if (Camera.main.transform.position.x > lengthSlider.value / 7)
            {
                lengthText.text = FormatTime(Camera.main.transform.position.x / 7);
            }
            lengthSlider.maxValue = AudioManager.Instance.source.clip.length;
            lengthSlider.value = cam.transform.position.x / 7;
            GameObject[] beats = GameObject.FindGameObjectsWithTag("Beat");

            bgPic.SetActive(bgImage.isOn);
            size.gameObject.GetComponentInChildren<Text>().text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Cube size")}: {size.value}x";
            hp.gameObject.GetComponentInChildren<Text>().text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Player HP")}: {hp.value}";

            if (Input.GetKey(KeybindingManager.changeLongCubeSize) && Input.GetKeyDown(KeybindingManager.place) && selectedObject != null && selectedObject.gameObject.name.Contains("hitter02"))
            {
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));

                Vector3 objectPosition = selectedObject.position;

                float mouseToObjectDistanceX = mouseWorldPosition.x - objectPosition.x;

                if (mouseToObjectDistanceX > 0)
                {
                    float newSizeX = Mathf.Abs(mouseToObjectDistanceX);

                    newSizeX = Mathf.Clamp(newSizeX, 1f, Mathf.Infinity);

                    SpriteRenderer spriteRenderer = selectedObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.size = new Vector2(newSizeX, spriteRenderer.size.y);
                    }
                }
            }
            if (offsetmarker.text.Length > 0)
            {
                float value = float.Parse(offsetmarker.text);
                for (int i = 0; i < beats.Length; i++)
                {
                    beats[i].transform.position = new Vector3(originalPositions[i].x + (value / 700), -1.5f, 5);
                }
            }
            else
            {
                float value = 0;
                for (int i = 0; i < beats.Length; i++)
                {
                    beats[i].transform.position = new Vector3(originalPositions[i].x + (value / 700), -1.5f, 5);
                }
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
            playbackText.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Playback")} ({Time.timeScale:0.00}x)";
            audio = AudioManager.Instance.source;
            audio.pitch = Time.timeScale;
            videoPlayer.playbackSpeed = Time.timeScale;
            PlayerEditorMovement player = GameObject.FindObjectOfType<PlayerEditorMovement>();




            if (Input.GetKey(KeybindingManager.deleteBPM) && Input.GetKey(KeybindingManager.delete))
            {
                foreach (GameObject beat in beats)
                {
                    Destroy(beat);
                }
            }
            delay += Time.unscaledDeltaTime;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition.normalized);

            Camera.main.backgroundColor = color1.GetColor();
            bgColorSel.color = color1.GetColor();

            Vector2 screenPos = new(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            bool pointerOverUI = EventSystem.current.IsPointerOverGameObject();


            int accurateCount = cubes.Count + saws.Count + longCubes.Count;

            objectCount.text = LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Objects") + ": " + accurateCount.ToString() + "/100000";




            if (!Input.GetKey(KeybindingManager.changeLongCubeSize) && Input.GetKeyDown(KeybindingManager.place) && delay >= delayLimit && itemButtons[currentButtonPressed].clicked && !pointerOverUI && !player.enabled)
            {
                if (cubes.Count > 0 && accurateCount <= 100000)
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
                    switch (item.name)
                    {
                        case "hitter01(Clone)":
                            audio.PlayOneShot(hitSounds[0]);
                            break;
                        case "hitter02(Clone)":
                            audio.PlayOneShot(hitSounds[1]);
                            break;
                        case "hitter03(Clone)":
                            audio.PlayOneShot(hitSounds[2]);
                            break;
                        case "hitter04(Clone)":
                            audio.PlayOneShot(hitSounds[3]);
                            break;
                        case "hitter05(Clone)":
                            audio.PlayOneShot(hitSounds[4]);
                            break;
                        case "hitter06(Clone)":
                            audio.PlayOneShot(hitSounds[5]);
                            break;
                        default:
                            break;
                    }

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
               
            }

            if ((Input.GetKeyDown(KeybindingManager.playMode) && !Input.GetKey(KeybindingManager.songMode)) && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)
            {
                Play();

            }
            if ((Input.GetKey(KeybindingManager.songMode) && Input.GetKeyDown(KeybindingManager.playMode)) && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)
            {
                PlayAudio();
            }

            if (selectedObject != null && !player.enabled && selectedObject.transform.position.x >= 1)
            {
                float moveX = 0f;
                float moveY = 0f;
                float moveSpeed = 1f;


                if (Input.GetKey(KeybindingManager.moveObjectLeft))
                    moveX -= 1f;
                if (Input.GetKey(KeybindingManager.moveObjectRight))
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

                if (Input.GetKeyDown(KeybindingManager.moveObjectUp) && selectedObject.transform.position.y < 4)
                {
                    selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y + 1, 0);
                }

                if (Input.GetKeyDown(KeybindingManager.moveObjectDown) && selectedObject.transform.position.y > -1)
                {
                    selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y - 1, 0);
                }

                // Apply movement directly to transform.position
                selectedObject.transform.position += moveSpeed * 0.01f * new Vector3(moveX, moveY, 0);

                if (Input.GetKey(KeybindingManager.delete))
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


            }

            if (Camera.main.transform.position.x < 7)
            {
                Camera.main.transform.position = new Vector3(7, 0.7f, -10);
            }
            if (player.transform.position != new Vector3(0, -1, player.transform.position.z) && !player.enabled)
            {
                player.transform.position = new Vector2(0, -1);
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
                videoPlayer.Play();
            }
            else if (audio.isPlaying && player.enabled)
            {
                audio.Stop();
                audio.time = 0f;
                videoPlayer.Stop();
                videoPlayer.time = 0f;
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
        public void OpenSongFolder()
        {
            string path = @Path.Combine(Main.gamePath, "scenes", $"{ID} - {customSongName.text}");
            path = path.Replace("\\", "/");
            string normalizedPath = Path.GetFullPath(path);
            Process.Start("explorer.exe", normalizedPath);
        }
        public async void MainMenu()
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {

                await SaveLevelData().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        UnityEngine.Debug.LogError($"Error saving scene: {task.Exception?.Flatten().Message}");
                    }
                });

            }
            SceneManager.LoadScene(1);
            Time.timeScale = 1f;
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


            // Clean up resources
            RenderTexture.active = null;
            Destroy(tempRT);
            Destroy(bgTexture2D);
        }



        private string GetSceneDataPath(int ID, string name)
        {
            string directoryPath = Path.Combine(Main.gamePath, "scenes", $"{ID} - {name}");

            string filePath = Path.Combine(directoryPath, name + ".json");
            return filePath;
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
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Background", ".png", ".jpg", ".jpeg", ".mp4"));
            FileBrowser.SetDefaultFilter(".png");
            FileBrowser.ShowLoadDialog(OnFileSelected, null, FileBrowser.PickMode.Files);
        }
        private IEnumerator AutoSave()
        {
            while (true)
            {
                // Trigger the save operation
                yield return SaveSceneData();

                // Wait for 3 minutes before the next save
                yield return new WaitForSeconds(5 * 60);
            }
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
            camBG.texture = null;
            bgPreview.texture = null;
        }

        IEnumerator LoadImage(string path)
        {
            if (File.Exists(path))
            {
            string extension = Path.GetExtension(path).ToLower();

            if (extension == ".mp4")
            {videoPlayer.Prepare();
                // Handle video loading
                if (videoPlayer == null)
                {
                videoPlayer = camBG.gameObject.AddComponent<VideoPlayer>();
                }

                videoPlayer.url = path;
                videoPlayer.isLooping = true;

                // Enable the GameObject with the RawImage component
                bgPreview.gameObject.SetActive(true);
                bgImage.isOn = true;
                bgPreview.texture = videoPlayer.targetTexture;
                camBG.texture = videoPlayer.targetTexture;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            }
            else
            {
                // Handle image loading
                byte[] fileData = File.ReadAllBytes(path);
                Texture2D texture = new(2, 2);

                if (texture.LoadImage(fileData))
                {
                bgPreview.texture = texture;
                yield return null;

                camBG.texture = texture;
                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to load the image: " + path);
                }
            }
            }
            else
            {
            UnityEngine.Debug.LogError("File does not exist: " + path);
            }
        }
        public void SaveSceneButton()
        {

            loadingPanel.ShowLoadingScreen(); // Show loading screen
            // Call the async method without awaiting it; logs the exception if it fails
            _ = SaveSceneData().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    UnityEngine.Debug.LogError($"Error saving scene: {task.Exception?.Flatten().Message}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        

        public void SaveLevel()
        {
            // Similar to SaveSceneButton, call SaveLevelData asynchronously and log errors
            _ = SaveLevelData().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    UnityEngine.Debug.LogError($"Error saving level: {task.Exception?.Flatten().Message}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public async Task SaveSceneData()
        {
            try
            {
                autoSave.Play("autoSave");
                // Create a new SceneData instance and populate it with current objects' positions
                SceneData sceneData = await CreateSceneData(loadingPanel);

                // Serialize the SceneData instance to formatted JSON
                string json = JsonUtility.ToJson(sceneData, true);

                // Get the file path based on the scene name
                string filePath = GetSceneDataPath(sceneData.ID, sceneData.name);

                // Write the JSON data to the file
                File.WriteAllText(filePath, json);

                Notifications.instance.Notify($"{sceneData.name} successfully saved.", null);
            }
            catch (Exception e)
            {
                Notifications.instance.Notify($"Oops, something wrong happened!\nTry again later.", null);
                Debug.LogError("Error while saving/exporting level: " + e);
                FindAnyObjectByType<LoadingScreen>().HideLoadingScreen();
            }
        }


        public async void SaveScene()
        {
            try
            {
                // Create a new SceneData instance and populate it with current objects' positions
                SceneData sceneData = await CreateSceneData(loadingPanel);

                // Serialize the SceneData instance to formatted JSON
                string json = JsonUtility.ToJson(sceneData, true);

                // Get the file path based on the scene name
                string filePath = GetSceneDataPath(sceneData.ID, sceneData.name);

                // Write the JSON data to the file
                File.WriteAllText(filePath, json);


                UnityEngine.Debug.Log("Scene saved successfully: " + sceneNameInput.text.Trim());
            }
            catch (Exception e)
            {
                // Log errors that occur during saving
                UnityEngine.Debug.LogError($"Failed to save scene: {e.Message}");
            }
        }

        private async Task<GameObject[]> GetCubesOnMainThread(LoadingScreen loadingScreen)
        {
            string[] tags = { "Cubes", "Saw", "LongCube" };
            List<GameObject> foundCubes = new List<GameObject>();

            // Total tags to search
            int totalTags = tags.Length;

            for (int i = 0; i < totalTags; i++)
            {
                string tag = tags[i];

                // Find objects with the current tag
                GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
                foundCubes.AddRange(objectsWithTag);

                // Update the loading screen with the current count
                loadingScreen.UpdateLoading($"Retrieving cubes... Found {foundCubes.Count} so far.", (i + 1) / (float)totalTags);

                // Simulate delay for demonstration purposes (Remove this in production)
                await Task.Delay(100); // Optional: simulate processing time
            }

            return foundCubes.ToArray();
        }




        private async Task<SceneData> LoadSceneDataFromFile()
        {
            string path = Path.Combine(Main.gamePath, "scenes", ID.ToString() + " - " + sceneNameInput.text, sceneNameInput.text + ".json");
            SceneData data = null;

            if (File.Exists(path))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(path);
                    data = SceneData.FromJson(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error reading JSON file: {e.Message}");
                    Notifications.instance.Notify("An error occured. Try restarting the game or editor.", null);
                    data = new SceneData(); // Create a default instance if needed
                }
            }
            else
            {
                Debug.LogWarning("Scene file not found. Creating new SceneData.");
                data = new SceneData();
            }

            return data;
        }
        private void UpdateLoadingText(string message)
        {
            loadingPanel.loadingText.text = message; // Assuming loadingTextUI is a reference to your UI Text component
        }

    private async Task<SceneData> CreateSceneData(LoadingScreen loadingScreen)
    {
        loadingScreen.UpdateLoading("Retrieving cubes...", 0f);
        GameObject[] cubes = await GetCubesOnMainThread(loadingPanel);
        if (cubes == null)
        {
            loadingScreen.HideLoadingScreen(); // Hide loading screen
            return null;
        }
        await Task.Delay(500);
        // Update loading screen with the count of cubes retrieved
        int cubeCount = cubes.Length;
        loadingScreen.UpdateLoading($"Gathering cube positions... Retrieved {cubeCount} cubes", 0.1f);
        List<Vector2> cubePositions = cubes.Select(cube => (Vector2)cube.transform.position).ToList();
        await Task.Delay(500);
        loadingScreen.UpdateLoading("Calculating farthest object...", 0.2f);
        float distance1 = 0f;
        GetFarthestObjectOnMainThread(cubes, (targetObject) =>
        {
            // This block will run once the farthest object has been found
            float distance = targetObject != null ? targetObject.position.x + additionalDistance : 0;
            distance1 = distance;
        });

        await Task.Delay(500);
            float calculateDifficulty = await Calculator.CalculateDifficultyAsync(
            this.cubes,
            saws,
            longCubes,
            hp,
            size,
            cubePositions.ToArray(),
            float.Parse(bpmInput.text),
            updateLoadingText: (text) =>
            {
                // Ensure updateLoadingText is executed on the main thread
                MainThreadDispatcher.Enqueue(() =>
                {
                    loadingPanel.UpdateLoading(text, 0.45f);
                });
            }
        );
        await Task.Delay(3000);
        // Load existing scene data if available, with fallback
        SceneData data = await LoadSceneDataFromFile() ?? new SceneData();

        loadingScreen.UpdateLoading("Creating Scene Data object...", 0.5f);
        SceneData sceneData = new SceneData // This is the sceneData for edit levels
        {
            version = 2,
            name = customSongName.text,
            calculatedDifficulty = calculateDifficulty,
            bpm = int.TryParse(bpmInput.text, out int bpmValue) ? bpmValue : 0,
            saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            songLength = audio.clip != null ? audio.clip.length : 0,
            creator = creator != null ? creator.text : "Unknown",
            romanizedName = romaji != null ? romaji.text : "Unknown",
            romanizedArtist = romajiArtist != null ? romajiArtist.text : "Unknown",
            playerHP = hp != null ? (int)hp.value : 300,
            boxSize = size != null ? size.value : 1,
            artist = songArtist != null ? songArtist.text : "Unknown",
            songName = customSongName.text,
            offset = float.TryParse(offsetmarker != null ? offsetmarker.text : "0", out float offsetValue) ? offsetValue : 0,
            ID = (data.ID == 0) ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : data.ID,
            defBGColor = Camera.main != null ? Camera.main.backgroundColor : Color.black,
            source = source.text,
            description = input.text,
        };
        loadingScreen.UpdateLoading("Saving background image...", 0.6f);

        if (bgPreview != null && bgPreview.texture != null && bgImage != null && bgImage.isOn)
        {
            string sceneFolderPath = Path.Combine(Main.gamePath, "scenes", sceneData.ID + " - " + sceneData.name);
            Directory.CreateDirectory(sceneFolderPath);

            if (camBG != null)
            {
                // Save the video file
                string videoFilePath = Path.Combine(sceneFolderPath, "backgroundVideo.mp4");
                if (videoPlayer.url != Path.Combine(sceneFolderPath, "backgroundVideo.mp4") && !string.IsNullOrEmpty(videoPlayer.url) && File.Exists(videoPlayer.url)) {
                    File.Copy(videoPlayer.url, videoFilePath, true);

                // Extract the first frame of the video and save it as bgImage.png
                string bgImagePath = Path.Combine(sceneFolderPath, "bgImage.png");
                if (!File.Exists(bgImagePath)) {
                    loadingScreen.UpdateLoading("Generating thumbnail...", 0.65f);
                await SaveFrameAsImage(videoPlayer.url, bgImagePath);
                }
                
                }
                
            }
            else
            {
                // Save the background image as bgImage.png
                string bgImagePath = Path.Combine(sceneFolderPath, "bgImage.png");
                SaveBackgroundImageTexture(bgImagePath);
            }
        }
        loadingScreen.UpdateLoading("Processing positions for cubes, saws, and long cubes...", 0.7f);
        sceneData.cubePositions = this.cubes?.Select(cube => (Vector2)cube.transform.position).ToList() ?? new List<Vector2>();
        sceneData.cubeType = this.cubes
            ?.Select(cube =>
            {
                // Get the base name without "(Clone)"
                string name = cube.name.Replace("(Clone)", "").Trim();

                // Look up the mapped integer value for the name
                if (cubeTypeMapping.TryGetValue(name, out int mappedIndex))
                {
                    return mappedIndex;
                }

                return 1;
            })
            .ToList(); // Convert to a list if sceneData.cubeType expects one

        sceneData.sawPositions = saws?.Select(saw => (Vector2)saw.transform.position).ToList() ?? new List<Vector2>();
        if (longCubes != null)
        {
            sceneData.longCubePositions = longCubes.Select(cube => (Vector2)cube.transform.position).ToList();
            sceneData.longCubeWidth = longCubes.Select(cube => cube.GetComponent<SpriteRenderer>()?.size.x ?? 0).ToList();
        }

        loadingScreen.UpdateLoading("Scene data created successfully.", 1f);
        await Task.Delay(100);
        loadingScreen.HideLoadingScreen(); // Hide loading screen
        return sceneData;
    }

   private async Task SaveFrameAsImage(string videoFilePath, string imagePath)
{
    // Create VideoPlayer object and set it up
    VideoPlayer videoPlayer = new GameObject("VideoPlayer").AddComponent<VideoPlayer>();
    videoPlayer.url = videoFilePath;
    


    videoPlayer.renderMode = VideoRenderMode.RenderTexture;
    
    RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
    videoPlayer.targetTexture = renderTexture;

    videoPlayer.Prepare();
    while (!videoPlayer.isPrepared)
    {
        await Task.Yield(); // Wait until video is prepared
    }

    // Calculate the middle time of the video
    double videoDuration = videoPlayer.length;
    double middleTime = videoDuration / 2.0;

    // Seek to the middle of the video and play
    videoPlayer.time = middleTime;
    videoPlayer.Play();

    // Wait for the frame to be rendered (make sure it's fully ready)
    await WaitForFrameToRender(videoPlayer);

    // Pause the video after rendering the frame
    videoPlayer.Pause();

    // Create a texture to read the frame
    Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
    RenderTexture.active = renderTexture;
    texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    texture.Apply();
    RenderTexture.active = null;

    // Encode texture as PNG and save it
    byte[] bytes = texture.EncodeToPNG();
    File.WriteAllBytes(imagePath, bytes);

    // Clean up
    Destroy(videoPlayer.gameObject);
    Destroy(renderTexture);
    Destroy(texture);
}
private async Task WaitForFrameToRender(VideoPlayer videoPlayer)
{
    while (!videoPlayer.isPlaying)
    {
        await Task.Yield();
    }

    await Task.Delay(1500); 
}


    private async Task<SceneData> CreateLevelSceneData(LoadingScreen loadingScreen)
    {
        loadingScreen.ShowLoadingScreen(); // Show loading screen

        loadingScreen.UpdateLoading("Retrieving cubes...", 0f);
        GameObject[] cubes = await GetCubesOnMainThread(loadingPanel);
        if (cubes == null || cubes.Count() == 0)
        {
            loadingScreen.HideLoadingScreen(); // Hide loading screen
            Notifications.instance.Notify("You can't export an empty level.", null);
            return null;
        }
        await Task.Delay(500);
        // Update loading screen with the count of cubes retrieved
        int cubeCount = cubes.Length;
        loadingScreen.UpdateLoading($"Gathering cube positions... Retrieved {cubeCount} cubes", 0.1f);
        List<Vector2> cubePositions = cubes.Select(cube => (Vector2)cube.transform.position).ToList();
        await Task.Delay(100);
        loadingScreen.UpdateLoading("Calculating farthest object...", 0.2f);
        float distance1 = 0f;
        GetFarthestObjectOnMainThread(cubes, (targetObject) =>
        {
            // This block will run once the farthest object has been found
            float distance = targetObject != null ? targetObject.position.x + additionalDistance : 0;
            distance1 = distance;
        });

        await Task.Delay(500);

        loadingScreen.UpdateLoading("Calculating difficulty...", 0.4f);
        float calculateDifficulty = await Calculator.CalculateDifficultyAsync(
            this.cubes,
            saws,
            longCubes,
            hp,
            size,
            cubePositions.ToArray(),
            float.Parse(bpmInput.text),
            updateLoadingText: (text) =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    loadingPanel.UpdateLoading(text, 0.45f);
                });
            }
        );
        await Task.Delay(5000);
        SceneData data = await LoadSceneDataFromFile() ?? new SceneData();
        
        loadingScreen.UpdateLoading("Creating Level info...", 0.5f);
        int levelLength = (int)(distance1 / 7);
        float sectionLength = 70f;
int totalSections = Mathf.CeilToInt((float)cubes.Length / sectionLength);

Section[] sections = new Section[totalSections];

for (int sectionIndex = 0; sectionIndex < totalSections; sectionIndex++)
{
    float sectionStartX = sectionIndex * sectionLength; // X-axis start position
    float sectionEndX = (sectionIndex + 1) * sectionLength; // X-axis end position

    Section section = new Section()
    {
        sectionBegin = sectionStartX, // Convert to milliseconds
        sectionEnd = sectionEndX, // Convert to milliseconds
        bpm = int.TryParse(bpmInput.text, out int bpmValue2) ? bpmValue2 : 0,
        difficulty = Calculator.CalculateSectionDifficulty(
            cubes.ToList(),
            saws,
            longCubes,
            cubePositions.ToArray(),
            bpmValue2,
            sectionIndex,
            10
        )
    };

    sections[sectionIndex] = section;
    Debug.Log($"Section {sectionIndex}: Begin = {section.sectionBegin}ms, End = {section.sectionEnd}ms");
}

        SceneData sceneData = new SceneData
        {
            version = 2,
            name = customSongName.text,
            calculatedDifficulty = calculateDifficulty,
            gameVersion = Application.version,
            saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            songLength = audio.clip != null ? audio.clip.length : 0,
            artist = songArtist != null ? songArtist.text : "Unknown",
            songName = customSongName.text,
            levelLength = (int)(distance1 / 7),
            creator = creator != null ? creator.text : "Unknown",
            romanizedName = romaji != null ? romaji.text : "Unknown",
            romanizedArtist = romajiArtist != null ? romajiArtist.text : "Unknown",
            offset = float.TryParse(offsetmarker?.text, out float offsetValue) ? offsetValue : 0,
            bpm = int.TryParse(bpmInput.text, out int bpmValue) ? bpmValue : 0,
            ID = (data.ID == 0) ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : data.ID,
            source = source.text,
            sections = sections,
            description = input.text
        };
        await Task.Delay(7000);
        loadingScreen.UpdateLoading("Populating objects...", 0.6f);

        sceneData.cubePositions = this.cubes?.Select(cube => (Vector2)cube.transform.position).ToList() ?? new List<Vector2>();
        sceneData.cubeType = this.cubes
            ?.Select(cube =>
            {
                // Get the base name without "(Clone)"
                string name = cube.gameObject.name.Replace("(Clone)", "").Trim();

                // Look up the mapped integer value for the name
                if (cubeTypeMapping.TryGetValue(name, out int mappedIndex))
                {
                    return mappedIndex;
                }

                return 1;
            })
            .ToList();
        sceneData.sawPositions = saws?.Select(saw => (Vector2)saw.transform.position).ToList() ?? new List<Vector2>();
        if (longCubes != null)
        {
            sceneData.longCubePositions = longCubes.Select(cube => (Vector2)cube.transform.position).ToList();
            sceneData.longCubeWidth = longCubes.Select(cube => cube.GetComponent<SpriteRenderer>()?.size.x ?? 0).ToList();
        }

        // Set source and destination paths for the scene
        string sourceFolderPath = Path.Combine(Main.gamePath, "scenes", sceneData.ID + " - " + sceneData.name);
        string destinationFolderPath = Path.Combine(Main.gamePath, "levels", sceneData.ID + " - " + sceneData.name);
        await Task.Delay(500);
        loadingScreen.UpdateLoading("Copying files...", 0.7f);
        try
        {
            await CopyFilesAsync(sourceFolderPath, destinationFolderPath);
        }
        catch (Exception ex)
        {
            loadingScreen.HideLoadingScreen(); // Hide loading screen
            Debug.LogWarning("Something wrong happened: " + ex);
            Notifications.instance.Notify("Something happened and exporting failed.\nPlease report this issue by sending the player logs.", () => Process.Start(JammerDash.Main.gamePath + "/Player.log"));
            return null;
        }
        await Task.Delay(500);
        loadingScreen.UpdateLoading("Creating ZIP file...", 0.8f);
        string exportFolder = Path.Combine(Main.gamePath, "exports");
        if (!Directory.Exists(exportFolder))
            Directory.CreateDirectory(exportFolder);
        string zipExport = Path.Combine(Main.gamePath, "exports", $"{sceneData.ID} - {sceneData.name}.jdl");
        string zipFilePath = Path.Combine(Main.gamePath, "levels", $"{sceneData.ID} - {sceneData.name}.jdl");

        try
        {
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
            if (File.Exists(zipExport))
            {
                File.Delete(zipExport);
            }
            ZipFile.CreateFromDirectory(destinationFolderPath, zipExport, System.IO.Compression.CompressionLevel.NoCompression, false, null);
            ZipFile.CreateFromDirectory(destinationFolderPath, zipFilePath, System.IO.Compression.CompressionLevel.NoCompression, false, null);
            // Delete the original copied folder
            Directory.Delete(destinationFolderPath, true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating ZIP file: {ex.Message}");
            loadingScreen.HideLoadingScreen(); // Hide loading screen
            return null;
        }
        await Task.Delay(250);
        loadingScreen.UpdateLoading("Level scene data created successfully.", 1f);
        loadingScreen.HideLoadingScreen(); // Hide loading screen
        return sceneData;
    }

    private void GetFarthestObjectOnMainThread(GameObject[] cubes, Action<Transform> callback)
    {
        // Check if cubes is null or empty
        if (cubes == null || cubes.Length == 0)
        {
            callback(null); // Call the callback with null
            return; // Early return if no cubes are provided
        }

        // Enqueue the action to run on the main thread
        MainThreadDispatcher.Enqueue(() =>
        {
            // Run the FindFarthestObjectInX method on the main thread
            Transform result = Calculator.FindFarthestObjectInX(cubes);

            // Call the callback with the result
            callback(result);
        });
    }

    private Task CopyFilesAsync(string sourceFolderPath, string destinationFolderPath)
    {
        return Task.Run(() =>
        {
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
        });
    }
    public TextMeshProUGUI outputText;

    public GameObject description;
    public InputField input;

    public void ToggleDescription() {
        description.SetActive(!description.activeSelf);
    }

public void ParseBBCode(string bbCodeText)
    {
        string parsedText = bbCodeText;

      // Basic Formatting
        parsedText = Regex.Replace(parsedText, @"\[b\](.*?)\[/b\]", "<b>$1</b>");
        parsedText = Regex.Replace(parsedText, @"\[i\](.*?)\[/i\]", "<i>$1</i>");
        parsedText = Regex.Replace(parsedText, @"\[u\](.*?)\[/u\]", "<u>$1</u>");
        parsedText = Regex.Replace(parsedText, @"\[s\](.*?)\[/s\]", "<s>$1</s>"); // Strikethrough

        // Text Color & Size
        parsedText = Regex.Replace(parsedText, @"\[color=(.*?)\](.*?)\[/color\]", "<color=$1>$2</color>");
        parsedText = Regex.Replace(parsedText, @"\[size=(.*?)\](.*?)\[/size\]", "<size=$1>$2</size>");

        // Alignment
        parsedText = Regex.Replace(parsedText, @"\[align=(left|center|right|justified)\](.*?)\[/align\]", "<align=$1>$2</align>");

        // Hyperlinks
        parsedText = Regex.Replace(parsedText, @"\[url=(.*?)\](.*?)\[/url\]", "<link=\"$1\">$2</link>");

        // Superscript & Subscript
        parsedText = Regex.Replace(parsedText, @"\[sup\](.*?)\[/sup\]", "<sup>$1</sup>");
        parsedText = Regex.Replace(parsedText, @"\[sub\](.*?)\[/sub\]", "<sub>$1</sub>");

        // Quotes & Indent
        parsedText = Regex.Replace(parsedText, @"\[quote\](.*?)\[/quote\]", "<indent=10%><i>$1</i></indent>");
        parsedText = Regex.Replace(parsedText, @"\[indent=(\d+)\](.*?)\[/indent\]", "<indent=$1>$2</indent>");

        // Image Support (TextMeshPro Sprites)
        parsedText = Regex.Replace(parsedText, @"\[img=(.*?)\]", "<sprite name=\"$1\">");

        // Line Height
        parsedText = Regex.Replace(parsedText, @"\[line-height=(\d+)\](.*?)\[/line-height\]", "<line-height=$1>$2</line-height>");

        // Centering (Custom workaround since TMP does not support <center>)
        parsedText = Regex.Replace(parsedText, @"\[center\](.*?)\[/center\]", "<align=center>$1</align>");


        outputText.text = parsedText;
    }

    }
}