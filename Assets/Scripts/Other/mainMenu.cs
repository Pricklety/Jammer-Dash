using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Audio;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.IO.Compression;
using Lachee.Discord;
using Button = UnityEngine.UI.Button;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class mainMenu : MonoBehaviour, IPointerClickHandler
{
    public GameObject musicAsset;
    public Image bg;
    public Sprite[] sprite;
    private bool quittingAllowed = false;
    public SettingsData data;
    public float quitTimer = 3f;
    public float quitTime = 0f;
    public Image quitPanel2;
    PlayerData playerData;

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject playPanel;
    public GameObject creditsPanel;
    public GameObject settingsPanel;
    public GameObject additionalPanel;
    public GameObject levelInfo;
    public GameObject quitPanel;
    public GameObject community;
    public GameObject musicPanel;
    public GameObject changelogs;
    public GameObject funMode;

    [Header("LevelInfo")]
    public GameObject levelInfoPanelPrefab;
    public Transform levelInfoParent;
    public GameObject levelCreatePanel;
    public InputField newLevelNameInput;
    public GameObject cubePrefab;
    public GameObject sawPrefab;
    public GameObject playPrefab;
    public Transform playlevelInfoParent;

    [Header("Internet Check")]
    public Text internetStatusText;
    public GameObject checkInternet;
    public Button refreshButton;
    public Image ccIMG;

    [Header("Video Background")]
    public GameObject videoPlayerObject;
    public VideoPlayer videoPlayer;
    public VideoClip[] videoClips;
    private List<string> videoUrls;
    private int currentVideoIndex = 0;

    [Header("Idle")]
    public Animator animator;
    private float idleTimer = 0f;
    public float idleTimeThreshold = 10f;

    [Header("Music")]
    public AudioMixer audioMixer;
    AudioSource source;
    private float lowpassTargetValue; 
    private float fadeDuration = 0.25f; 
    private float currentLerpTime = 0f;
    public bool focus = true;

    [Header("Profile")]
    public Slider levelSlider;
    public Text levelText;
    public RawImage discordpfp;
    public Text discordName;

    [Header("Fun Mode")]
    public GameObject vis2;

    [Header("Visualizer V2")]
    public Text musicText;
    public Slider musicSlider;

    [Header("Parallax")]
    public Transform[] parallaxLayers; 
    public float[] parallaxSpeeds;     
    public Transform background;        
    public float backgroundParallaxSpeed; 
    public float maxMovementOffset;   
    public float scaleMultiplier;      
    public float edgeMargin;           

    private Vector3[] initialPositions; 
    private Vector3 lastMousePosition;  

    private Camera mainCamera;
    private RectTransform canvasRect;

    void Start()
    {
        playerData = LevelSystem.Instance.LoadTotalXP();
        data = SettingsFileHandler.LoadSettingsFromFile();
        LoadRandomBackground();
        RefreshInternetConnection();
        LoadLevelsFromFiles();
        LoadLevelFromLevels();
        levelSlider.maxValue = LevelSystem.Instance.xpRequiredPerLevel[LevelSystem.Instance.level];
        levelSlider.value = LevelSystem.Instance.currentXP;

        discordpfp.texture = DiscordManager.current.CurrentUser.avatar;
        discordName.text = DiscordManager.current.CurrentUser.username + "\n\nID: " +
        DiscordManager.current.CurrentUser.ID;
        if (data.parallax)
        {
            initialPositions = new Vector3[parallaxLayers.Length];
            for (int i = 0; i < parallaxLayers.Length; i++)
            {
                initialPositions[i] = parallaxLayers[i].position;
            }

            // Set the initial mouse position
            lastMousePosition = Input.mousePosition;

            // Set the initial scale of the background
            background.localScale *= scaleMultiplier;

            mainCamera = Camera.main;
            canvasRect = GetComponentInChildren<Canvas>().GetComponent<RectTransform>();

        }
    }
    public void LoadLevelFromLevels()
    {
        foreach (Transform child in levelInfoParent)
        {
            if (child.GetComponent<CustomLevelScript>())
            {
                Destroy(child.gameObject);
            }
        }

        string levelsPath = Path.Combine(Application.persistentDataPath, "levels");

        if (!Directory.Exists(levelsPath))
        {
            Debug.LogError("The 'levels' folder does not exist in persistentDataPath.");
            Directory.CreateDirectory(levelsPath);
            return;
        }

        string[] levelFiles = Directory.GetFiles(levelsPath, "*.jdl", SearchOption.AllDirectories);

        foreach (string filePath in levelFiles)
        {
            if (Path.GetFileName(filePath).Equals("LevelDefault.jdl"))
            {
                continue; // Skip LevelDefault.jdl
            }

            // Create a temporary folder
            string tempFolder = Path.Combine(Application.temporaryCachePath, "tempExtractedJson");
            Directory.CreateDirectory(tempFolder);

            try
            {
                // Extract JSON data from JDL file to the temporary folder
                string jsonFilePath = ExtractJSONFromJDL(filePath);

                if (jsonFilePath == null)
                {
                    Debug.LogError("Failed to extract JSON from JDL: " + filePath);
                    continue;
                }

                // Read JSON content from the extracted JSON file
                string json = File.ReadAllText(jsonFilePath);

                // Deserialize JSON data into SceneData object
                SceneData sceneData = SceneData.FromJson(json);

                if (sceneData == null)
                {
                    Debug.LogError("Failed to deserialize JSON from file: " + jsonFilePath);
                    continue;
                }

                // Log the level name to verify if sceneData is successfully deserialized
                Debug.LogWarning(sceneData.levelName);

                // Create a directory with the level name
                string extractedPath = Path.Combine(levelsPath, "extracted", sceneData.sceneName);
                Directory.CreateDirectory(extractedPath);

                // Move the JSON file to the new directory
                string jsonDestinationPath = Path.Combine(extractedPath, sceneData.sceneName + ".json");
                if (File.Exists(jsonDestinationPath))
                {
                    File.Delete(jsonDestinationPath);
                }
                File.Move(jsonFilePath, jsonDestinationPath);
                ExtractMP3FromJDL(filePath, extractedPath);
                GameObject levelInfoPanel = Instantiate(playPrefab, playlevelInfoParent);

                // Display level information on UI
                DisplayCustomLevelInfo(sceneData, levelInfoPanel.GetComponent<CustomLevelScript>());
                levelInfoPanel.GetComponent<CustomLevelScript>().SetSceneData(sceneData);
            }
            finally
            {
                // Clean up the temporary folder
                Directory.Delete(tempFolder, true);
            }
        }
    }


    public static string ExtractJSONFromJDL(string jdlFilePath)
    {
        try
        {
            // Create a temporary folder
            string tempFolder = Path.Combine(Application.temporaryCachePath, "tempExtractedJson");
            Directory.CreateDirectory(tempFolder);

            using (ZipArchive archive = ZipFile.OpenRead(jdlFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!entry.FullName.EndsWith(".json"))
                    {
                        continue; // Skip non-JSON files
                    }

                    // Generate a unique filename for the extracted JSON file
                    string extractedFilePath = Path.Combine(tempFolder, Path.GetFileName(entry.FullName));

                    // Extract the JSON file to the temporary folder
                    entry.ExtractToFile(extractedFilePath, true); // Overwrite if file exists

                    // Return the path of the extracted JSON file
                    return extractedFilePath;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error extracting JSON from JDL: " + e.Message);
        }

        return null;
    }

    public static void ExtractMP3FromJDL(string jdlFilePath, string destinationFilePath)
    {
        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(jdlFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string entryFileName = entry.FullName;

                    if (entryFileName.EndsWith(".mp3"))
                    {
                        // Combine the destination directory path with the MP3 filename
                        string destinationFullPath = Path.Combine(destinationFilePath, Path.GetFileName(entryFileName));

                        // Extract the MP3 file to the specified destination file path
                        entry.ExtractToFile(destinationFullPath, overwrite: true);
                        return; // Exit the method after extracting the MP3 file
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error extracting MP3 from JDL: " + e.Message);
        }
    }



    public void URL(string url)
    {
        Application.OpenURL(url);
    }
    public void LoadRandomBackground()
    {
        Debug.Log(data);
        if (DateTime.Now.Month == 12 && data.artBG)
        {
            sprite = Resources.LoadAll<Sprite>("backgrounds/christmas");
            if (videoPlayerObject != null)
                videoPlayerObject.SetActive(false);
        }
        else if (DateTime.Now.Month == 2 && DateTime.Now.Day == 14 && data.artBG)
        {
            sprite = Resources.LoadAll<Sprite>("backgrounds/valentine");
            if (videoPlayerObject != null)
                videoPlayerObject.SetActive(false);
        }
        else if (data.artBG)
        {
            sprite = Resources.LoadAll<Sprite>("backgrounds/default");
            if (videoPlayerObject != null)
                videoPlayerObject.SetActive(false);
        }
        else if (data.customBG)
        {
            string[] files = Directory.GetFiles(Application.persistentDataPath + "/backgrounds", "*.png");
            List<Sprite> sprites = new List<Sprite>();
            foreach (string file in files)
            {
                byte[] fileData = File.ReadAllBytes(file);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData); // LoadImage overwrites texture dimensions
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                sprites.Add(sprite);
            }
            if (videoPlayerObject != null)
                videoPlayerObject.SetActive(false);

            sprite = sprites.ToArray();
        }
        else if (data.vidBG)
        {
            string[] files = Directory.GetFiles(Application.persistentDataPath + "/backgrounds", "*.mp4");
            List<VideoClip> clips = new List<VideoClip>();

            foreach (string file in files)
            {
                // Check the size of each file before adding it to the list
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Length <= 5 * 1024 * 1024) // 5MB in bytes
                {
                    // Create a video clip from the file path
                    LoadVideoClipFromFile(file);
                    
                }
                else
                {
                    Debug.LogError("Video file size exceeds the limit (5MB): " + file);
                }
            }
            videoPlayer.loopPointReached += OnVideoLoopPointReached;
            // Assign the video clips array
            videoClips = clips.ToArray();
        }
        else
        {
            sprite = Resources.LoadAll<Sprite>("backgrounds/basic"); 
            if (videoPlayerObject != null)
                videoPlayerObject.SetActive(false);
        }

        if (sprite.Length > 0 && data.artBG || sprite.Length > 0 && data.customBG)
        {
            bg.color = Color.white;
            int randomIndex = Random.Range(0, sprite.Length);
            bg.sprite = sprite[randomIndex];
        }
        else
        {
            ChangeBasicCol();
        }
    }
    private void LoadVideoClipFromFile(string filePath)
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath + "/backgrounds", "*.mp4");
        List<string> urls = new List<string>();

        foreach (string file in files)
        {
            // Add file paths as URLs
            urls.Add("file://" + file);
        }
        videoUrls = urls;
        // Assign the video URLs to the VideoPlayer
        videoPlayer.clip = null;
        videoPlayer.url = urls[Random.Range(0, urls.Count)];
        videoPlayer.Prepare();

        // Handle PrepareCompleted event to start playing the video
        videoPlayer.prepareCompleted += OnVideoPrepareCompleted;

    }

    void OnVideoLoopPointReached(VideoPlayer vp)
    {
        // Video finished playing, play the next video
        currentVideoIndex = (currentVideoIndex + 1) % videoUrls.Count;
        if (currentVideoIndex == videoUrls.Count)
        {
            currentVideoIndex = 0;
        }
        PlayVideo(videoUrls[currentVideoIndex]);
    }
    void PlayVideo(string url)
    {
        videoPlayer.clip = null;
        videoPlayer.url = url;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepareCompleted;
        videoPlayer.loopPointReached += OnVideoLoopPointReached;
    }
    void OnVideoPrepareCompleted(VideoPlayer vp)
    {
        // Video preparation is complete, start playing the video
        vp.Play();
    }
    public void ChangeBasicCol()
    {
        StartCoroutine(SmoothColorChange());
    }
        
    public float elapsedTime;

    private IEnumerator SmoothColorChange()
    {
        if (sprite != null && sprite.Length > 0)
        {
            bg.sprite = sprite[0];
            Color targetColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);

            float duration = 5f;  // Adjust the duration as needed

            while (elapsedTime < duration)
            {
                bg.color = Color.Lerp(bg.color, targetColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            elapsedTime = 0f;

            // Ensure the final color is exactly the target color
            bg.color = targetColor;
        }
        else
        {
            bg.color = Color.white;
        }
    }


    void CheckInternetConnection()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            internetStatusText.text = "Not connected to the internet";
            refreshButton.gameObject.SetActive(true);
        }
        else
        {
            internetStatusText.text = "";
            refreshButton.gameObject.SetActive(false);
        }
    }

    public void RefreshInternetConnection()
    {
        StartCoroutine(RefreshInternetStatus());
    }

    IEnumerator RefreshInternetStatus()
    {
        checkInternet.SetActive(true);
        refreshButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f); // Simulating a delay for the check
        checkInternet.SetActive(false);
        CheckInternetConnection();
    }
    public void CreatePanel()
    {
        levelCreatePanel.SetActive(true);
    }

    public void CreatePanelClose()
    {
        levelCreatePanel.SetActive(false);
    }
    public void CreateNewLevel()
    {
        // Get input values for the new level
        string newLevelName = newLevelNameInput.text;
        float newDifficulty = 0;
        SceneData newLevelData = new SceneData
        {
            sceneName = newLevelName, // You may want to customize the scene name
            levelName = newLevelName,
            calculatedDifficulty = newDifficulty
        };

        // Save the new level data to a JSON file
        SaveLevelToFile(newLevelData);

        
        // Reload levels to update the UI with the new level
        LoadLevelsFromFiles();
    }

    [System.Serializable]
    public class CountryInfo
    {
        public string country;
        // Add other fields from the ipinfo.io response if needed
    }

    void SaveLevelToFile(SceneData sceneData)
    {
        // Convert SceneData to JSON
        string json = sceneData.ToJson();

        // Save JSON to a new file in the persistentDataPath + scenes & levels folder
        string path = Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName);
        string filePath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName, $"{sceneData.levelName}.json");
        string musicPath = Path.Combine(Application.persistentDataPath, "scenes", sceneData.levelName, $"{sceneData.songName}.mp3");
        string defSongPath = Path.Combine(Application.streamingAssetsPath, "music", "Pricklety - Fall'd.mp3");
        defSongPath.Replace("\\", "/");
        sceneData.clipPath = defSongPath;
        if (Directory.Exists(path))
        {
            File.WriteAllText(filePath, json);
            File.Delete(musicPath);
            File.Copy(defSongPath, musicPath);
        }
        else
        {
            Directory.CreateDirectory(path);
            File.WriteAllText(filePath, json);
            File.Copy(defSongPath, musicPath);
        }
        
    }

    public void LoadLevelsFromFiles()
    {
        // Clear existing level information panels
        foreach (Transform child in levelInfoParent)
        {
            Destroy(child.gameObject);
        }
        string levelsPath = Path.Combine(Application.persistentDataPath, "scenes");

        if (!Directory.Exists(levelsPath))
        {
            Debug.LogError("The 'scenes' folder does not exist in persistentDataPath.");
            Directory.CreateDirectory(levelsPath);
            return;
        }

        string[] levelFiles = Directory.GetFiles(levelsPath, "*.json", SearchOption.AllDirectories);

        foreach (string filePath in levelFiles)
        {
            string json = File.ReadAllText(filePath);
            SceneData sceneData = SceneData.FromJson(json);

            // Instantiate the level information panel prefab
            GameObject levelInfoPanel = Instantiate(levelInfoPanelPrefab, levelInfoParent);

            // Display level information on UI
            DisplayLevelInfo(sceneData, levelInfoPanel.GetComponent<LevelScript>());
            levelInfoPanel.GetComponent<LevelScript>().SetSceneData(sceneData);
        }
    }

    private SceneData LoadSceneData(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SceneData sceneData = SceneData.FromJson(json);
            return sceneData;
        }
        else
        {
            Debug.LogWarning("Scene data file not found: " + filePath);
            return null;
        }
    }

    private void InstantiateObjects(SceneData sceneData)
    {
        // Instantiate cubes and saws based on sceneData
        foreach (Vector3 cubePos in sceneData.cubePositions)
        {
            Instantiate(cubePrefab, cubePos, Quaternion.identity);
        }

        foreach (Vector3 sawPos in sceneData.sawPositions)
        {
            Instantiate(sawPrefab, sawPos, Quaternion.identity);
        }

        // Other initialization logic for objects
    }

    // Display level information on UI
    void DisplayLevelInfo(SceneData sceneData, LevelScript level)
    {
        
        // Check if LevelScript component is not null
        if (level != null)
        {
            Debug.Log(sceneData);
            // Assuming YourPanelScript has methods to set text or image properties
            level.SetLevelName($"{sceneData.levelName}");
            level.SetSongName($"♫ {sceneData.songName}");
            level.SetDifficulty($"{sceneData.calculatedDifficulty:0.00} sn");

        }
        else
        {
            // Logging for debugging purposes
            Debug.LogError("Failed to load level " + sceneData.ID);
        }

    }

    void DisplayCustomLevelInfo(SceneData sceneData, CustomLevelScript level)
    {

        // Check if LevelScript component is not null
        if (level != null)
        {
            // Assuming YourPanelScript has methods to set text or image properties
            level.SetLevelName($"{sceneData.levelName}");
            level.SetSongName($"♫ {sceneData.songName}");
            level.SetDifficulty($"{Mathf.RoundToInt(sceneData.calculatedDifficulty):0}");
            level.SetCreator($"mapped by {sceneData.creator}");
        }
        else
        {
            // Logging for debugging purposes
            Debug.LogError("Failed to load level " + sceneData.ID);
        }

        // Method to toggle menu panels
    }

    // Method to toggle menu panels
    private void ToggleMenuPanel(GameObject panel)
    {
        // Toggle the specified panel directly if it's changelogs or creditsPanel
        if (panel == changelogs || panel == creditsPanel || panel == funMode)
        {
            panel.SetActive(!panel.activeSelf);

            if (panel.active)
            {
                // Turn off all panels
                mainPanel.SetActive(false);
                playPanel.SetActive(false);
                creditsPanel.SetActive(false);
                settingsPanel.SetActive(false);
                musicPanel.SetActive(false);
                levelInfo.SetActive(false);
                community.SetActive(false);
                changelogs.SetActive(false);
                additionalPanel.SetActive(false);
                funMode.SetActive(false);
                vis2.SetActive(false);
            }
            if (!panel.active)
            {
                panel.SetActive(true);
            }

        }
        else
        {
            // Turn off all panels
            mainPanel.SetActive(false);
            playPanel.SetActive(false);
            creditsPanel.SetActive(false);
            settingsPanel.SetActive(false);
            musicPanel.SetActive(false);
            levelInfo.SetActive(false);
            community.SetActive(false);
            changelogs.SetActive(false);
            additionalPanel.SetActive(false);
            funMode.SetActive(false);
            vis2.SetActive(false);

            // Enable the specified panel if it's not null
            if (panel != null)
            {
                panel.SetActive(true);
            }
            // Enable mainPanel if none of the specific panels are active
            else if (!playPanel.activeSelf && !creditsPanel.activeSelf && !settingsPanel.activeSelf)
            {
                mainPanel.SetActive(true);
            }
        }
    }

    // Menu buttons
    public void Home()
    {
        ToggleMenuPanel(mainPanel);
    }
    public void Play()
    {
        ToggleMenuPanel(playPanel);
        InputSystem.pollingFrequency = 1000;
        InputSystem.settings.maxQueuedEventsPerUpdate = 1000;
    }

    public void PlayRandomSFX()
    {
        UnityEngine.Object[] clips = Resources.LoadAll("Audio/SFX");
        foreach (var obj in clips)
        {
            Debug.Log("Loaded object: " + obj.name);
        }
        FindObjectOfType<AudioSource>().PlayOneShot((AudioClip)clips[Random.Range(0, clips.Length)]);

    }
    public void Credits()
    {
        ToggleMenuPanel(creditsPanel); 
    }

    public void Settings()
    {
        ToggleMenuPanel(settingsPanel);
    }

    public void OpenMusic()
    {
        musicPanel.SetActive(!musicPanel.activeSelf);
    }

    public void OpenChangelogs()
    {
        ToggleMenuPanel(changelogs);
    }

    public void AdditionalOpen()
    {
        additionalPanel.SetActive(true);
    }

    public void AdditionalClose()
    {
        additionalPanel.SetActive(false);
    }

    public void FunMode()
    {
        ToggleMenuPanel(funMode);
    }

    // Fun mode buttons

    public void VisualizerToggle()
    {
        vis2.SetActive(true);
    }
    public void Quit()
    {
        StartCoroutine(QuitGame());
    }

    IEnumerator QuitGame()
    {
        ShowQuit();
        yield return null;
    }


    public void ShowQuit()
    {
        quitPanel.SetActive(true);

        // Set target value for Lowpass filter
        lowpassTargetValue = 500;  // Adjust the cutoff frequency as needed

        // Start coroutine for fading
        StartCoroutine(FadeLowpass());

        Application.wantsToQuit += QuitHandler;

    }

    // Call this method when hiding the quit panel
    public void HideQuit()
    {
        quitPanel.SetActive(false);

        // Set target value for Lowpass filter
        lowpassTargetValue = 22000;  // Adjust the cutoff frequency as needed

        // Start coroutine for fading
        StartCoroutine(FadeLowpass());

        Application.wantsToQuit -= QuitHandler;
        PostProcessVolume volume = FindObjectOfType<PostProcessVolume>();
        if (volume != null)
        {
            PostProcessProfile profile = volume.profile;
            Vignette vignette;
            if (profile.TryGetSettings(out vignette))
            {
                vignette.intensity.value = 0;
            }
            else
            {
                // Settings couldn't be retrieved
                Debug.LogWarning("Vignette settings not found in the Post Process Profile.");
            }
        }
        else
        {
            // PostProcessVolume not found in the scene
            Debug.LogWarning("Post Process Volume not found in the scene.");
        }
    }

    // Coroutine for fading the Lowpass filter
    private IEnumerator FadeLowpass()
    {
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        float timer = 0f;
        float startValue = lowpassTargetValue;  // Initial value based on fade in or out

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            // Lerp between start and target values
            float currentValue = Mathf.Lerp(startValue, lowpassTargetValue, t);

            // Set the Lowpass filter parameter on the Master AudioMixer
            audioMixer.SetFloat("Lowpass", currentValue);
            
            yield return null;
        }

        // Ensure the target value is set after the fade completes
        audioMixer.SetFloat("Lowpass", lowpassTargetValue); 
    }


    public void OnApplicationFocus(bool focus)
    {
        
        source = GameObject.Find("mainmenu").GetComponent<AudioSource>();
        if (source == null)
        {
            return;
        }

        Options options = GetComponent<Options>();
        if (options == null)
        {
            return;
        }
        focus = options.focusVol.isOn;
        float ogvol = options.masterVolumeSlider.value;
        float focVol = options.unfocusVol.value;

        if (!focus && this.focus)
        {
            source.outputAudioMixerGroup.audioMixer.SetFloat("Master", focVol);
        }
        else if (focus && this.focus)
        {
            source.outputAudioMixerGroup.audioMixer.SetFloat("Master", ogvol);
        }

    }



    private bool QuitHandler()
    {
        if (quittingAllowed)
        {
          
            return true; // Return true to allow quitting
        }
        else
        {
           
            return false; // Return false to prevent quitting
        }
    }

    public void AllowQuitting()
    {
        quittingAllowed = true;
        Application.Quit();
        Debug.Log("Quitting the application");
    }

    public void CancelQuitting()
    {
        quittingAllowed = false;
        HideQuit();
        Debug.Log("User canceled quitting");
        // Set the Lowpass filter parameter on the Master AudioMixer
        audioMixer.SetFloat("Lowpass", 22000);
    }


    
    public void Menu()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        playPanel.SetActive(false);
        community.SetActive(false);
        mainPanel.SetActive(true);
    }


    // Play section
   
    public void CommunityOpen()
    {
        community.SetActive(true);
        mainPanel.SetActive(false);
    }


    public void OpenInfo()
    {
        levelInfo.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void CloseInfo()
    {
        mainPanel.SetActive(true);
        levelInfo.SetActive(false);
    }

    public void OpenEditor()
    {
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    public void OpenLevel(int index)
    {
        SceneManager.LoadScene(index);
    }

    #region SOCIAL_MEDIA
    public void Discord()
    {
        Application.OpenURL("https://discord.gg/dJU8X2kDpn");
    }

    public void RegionDiscord(string ID)
    {
        Application.OpenURL($"https://discord.gg/{ID}");
    }

    public void Twitter()
    {
        Application.OpenURL("https://twitter.com/JammerDash");
    }

    public void YouTube()
    {
        Application.OpenURL("https://youtube.com/@JammerDashOfficial");
    }

    public void Newgrounds()
    {
        Application.OpenURL("https://www.newgrounds.com/bbs/topic/1530441");
    }

    public void Twitch()
    {
        Application.OpenURL("https://twitch.tv/prickletylive");
    }

    public void TikTok()
    {
        Application.OpenURL("https://tiktok.com/@pricklety");
    }

    public void Instagram()
    {
        Application.OpenURL("https://www.instagram.com/pricklety/");
    }


    #endregion

    public void CrashReports()
    {
        // File path of the player log
        string logFilePath = Application.persistentDataPath + "/Player.log";

        // Open the player log file using the default application associated with its file type
        Process.Start(logFilePath);
    }

    public void SettingsFile()
    {
        // File path of the player log
        string logFilePath = Application.persistentDataPath + "/settings.json";

        // Open the player log file using the default application associated with its file type
        Process.Start(logFilePath);
    }


    bool IsPointerOverUIButNotButton()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (RaycastResult result in results)
        {
            // Check if the object under the pointer is a UI button
            if (result.gameObject.GetComponent<Button>() != null || result.gameObject.GetComponent<Dropdown>() != null || result.gameObject.GetComponent<Slider>() != null)
            {
                return true;
            }
        }
        return false;
    }
    public void FixedUpdate()
    {
        LevelSystem.Instance.initialXPRequirement = LevelSystem.Instance.xpRequiredPerLevel[LevelSystem.Instance.level];
        levelSlider.maxValue = LevelSystem.Instance.initialXPRequirement;
        levelSlider.value = LevelSystem.Instance.currentXP;
        if (LevelSystem.Instance.currentXP > 0)
        {
            levelText.text = "Level: " + LevelSystem.Instance.level.ToString() + $" (XP: {LevelSystem.Instance.currentXP:N0}; Total: {playerData.totalXP:N0})";

        }
        bool hasInput = Input.GetMouseButtonDown(0);
        idleTimer += Time.fixedDeltaTime; 
        if (!hasInput)
        {
            // Player is idle, start the idle animation immediately if it's not already playing
            if (idleTimer > idleTimeThreshold || Input.GetKeyDown(KeyCode.F1))
            {
                animator.SetTrigger("StartIdle");
                animator.ResetTrigger("StopIdle");
            }
            
        }
        else if (hasInput && !IsPointerOverUIButNotButton())
        {
            idleTimer = 0;  // Resetting idleTimer when there is input
            animator.SetTrigger("StopIdle");
            animator.ResetTrigger("StartIdle");
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            additionalPanel.SetActive(!additionalPanel.active);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            FunMode();
        }

        if (quitPanel.activeSelf)
        {
            // Set the Lowpass filter parameter on the Master AudioMixer
            audioMixer.SetFloat("Lowpass", data.lowpassValue);
        }
        else
        {
            // Set the Lowpass filter parameter on the Master AudioMixer
            audioMixer.SetFloat("Lowpass", 22000);
        }
        data = SettingsFileHandler.LoadSettingsFromFile();
        if (data.parallax)
        {
            const float backgroundScaleFactor = 1.1f;
            const float cameraMovementFactor = 3f;
            const float maxMovementOffset = 2f;
            const float edgeMargin = 0.1f;
            const float maxLayerMovement = 25f;

            // Background parallax effect
            background.localScale = new Vector3(backgroundScaleFactor, backgroundScaleFactor, 1f);

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 mouseDelta = new Vector3(mouseWorldPos.x * cameraMovementFactor, mouseWorldPos.y / 15, 0);

            float cameraMovement = Mathf.Clamp(mouseDelta.x, -maxMovementOffset, maxMovementOffset) * backgroundParallaxSpeed * Time.unscaledDeltaTime;
            Vector3 backgroundOffset = new Vector3(cameraMovement, 0, 0);
            background.position = backgroundOffset + new Vector3(mouseDelta.x / 100, mouseDelta.y, mouseDelta.z);

            // Ensure background stays within viewport bounds
            Vector3 backgroundEdgePosition = background.position + new Vector3(background.lossyScale.x / 2f, 0, 0);
            Vector3 backgroundEdgeViewport = mainCamera.WorldToViewportPoint(backgroundEdgePosition);
            if (backgroundEdgeViewport.x < 1 - edgeMargin)
            {
                background.position += new Vector3(edgeMargin, 0, 0);
            }
            else if (backgroundEdgeViewport.x > edgeMargin)
            {
                background.position -= new Vector3(edgeMargin, 0, 0);
            }

            // Parallax effect on UI layers
            float layerMovement = Mathf.Clamp(mouseDelta.x, -maxMovementOffset, maxMovementOffset) * backgroundParallaxSpeed * Time.unscaledDeltaTime;
            Vector3 layerMouseDelta = new Vector3(Mathf.Clamp(mouseDelta.x / 3, -25, 25), mouseDelta.y, mouseDelta.z);

            for (int i = 0; i < parallaxLayers.Length; i++)
            {
                parallaxLayers[i].position = new Vector3(layerMovement - layerMouseDelta.x + 960f, 0, 0);
            }
        }
        else
        {
            background.localScale = Vector3.one;
            background.position = Vector3.zero;
        }
        musicText.text = $"{AudioManager.Instance.GetComponent<AudioSource>().clip.name} - {AudioManager.Instance.GetComponent<AudioSource>().time}/{AudioManager.Instance.GetComponent<AudioSource>().clip.length}";

        if (Input.GetKey(KeyCode.Escape))
        {
            quitTime += Time.unscaledDeltaTime;

            // Check if quitTime exceeds quitTimer
            if (quitTime >= quitTimer)
            {
                quitPanel2.color = new Color(quitPanel2.color.r, quitPanel2.color.g, quitPanel2.color.b, 1.0f); // Set color to fully opaque
                Application.Quit(); // Quit application
            }
            else
            {
                // If quitTime is still less than quitTimer, lerp the color gradually
                quitPanel2.color = Color.Lerp(quitPanel2.color, new Color(quitPanel2.color.r, quitPanel2.color.g, quitPanel2.color.b, 1.0f), quitTime / quitTimer);
            }
        }
        else
        {
            quitTime = 0f;
            quitPanel2.color = new Color(quitPanel2.color.r, quitPanel2.color.g, quitPanel2.color.b, 0f); // Set color to fully transparent
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if the clicked position is outside the panel
        RectTransform rectTransform = musicPanel.GetComponent<RectTransform>();
        if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
        {
            // Click is outside the panel, disable it
            musicPanel.SetActive(false);
        }
    }
}
