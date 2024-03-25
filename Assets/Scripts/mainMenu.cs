using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.Linq;
using System.Net.NetworkInformation;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Networking;
using UnityEngine.Audio;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class mainMenu : MonoBehaviour
{
    public GameObject musicAsset;
    public Image bg;
    public Sprite[] sprite;
    private bool quittingAllowed = false;
    public SettingsData data;

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject playPanel;
    public GameObject creditsPanel;
    public GameObject settingsPanel;
    public GameObject multiplayerPanel;
    public GameObject additionalPanel;
    public GameObject onlinePanel;
    public GameObject levelInfo;
    public GameObject quitPanel;
    public GameObject leaderboard;
    public GameObject community;
    public GameObject legacy;
    public GameObject crashReport;

    [Header("Crash Reports")]
    public Text text;

    [Header("LevelInfo")]
    public GameObject levelInfoPanelPrefab;
    public Transform levelInfoParent;
    public GameObject levelCreatePanel;
    public InputField newLevelNameInput;
    public GameObject cubePrefab;
    public GameObject sawPrefab;

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

    void Start()
    {
        data = SettingsFileHandler.LoadSettingsFromFile();
        LoadRandomBackground();
        LoadLevelsFromFiles();
        RefreshInternetConnection();

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


    private void GetIPAddress(System.Action<string> callback)
    {
        StartCoroutine(IpService.GetPublicIpAddress(callback));
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
        string defSongPath = Path.Combine(Application.streamingAssetsPath, "music", "NikoN1nja - Slowly Going Insane.mp3");
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
    void LoadCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SceneData sceneData = LevelDataManager.Instance.LoadLevelData(sceneName);
        LevelScript level = EventSystem.current.lastSelectedGameObject.GetComponentInParent<LevelScript>();

        if (sceneData != null)
        {
            DisplayLevelInfo(sceneData, level); // Example method to display information
            // Other logic to handle the loaded data
        }
        else
        {
            Debug.LogWarning("Scene data not found for scene: " + sceneName);
        }
    }


    // Display level information on UI
    void DisplayLevelInfo(SceneData sceneData, LevelScript level)
    {
        // Logging for debugging purposes
        Debug.Log("DisplayLevelInfo - Start");

        // Check if LevelScript component is not null
        if (level != null)
        {
            // Assuming YourPanelScript has methods to set text or image properties
            level.SetLevelName($"{sceneData.levelName}");
            level.SetSongName($"♫ {sceneData.songName}");
            level.SetDifficulty($"{sceneData.calculatedDifficulty:0.00} sn");

            // Logging for debugging purposes
            Debug.Log("DisplayLevelInfo - Level information displayed successfully");
        }
        else
        {
            // Logging for debugging purposes
            Debug.LogError("DisplayLevelInfo - LevelScript component is null");
        }

        // Logging for debugging purposes
        Debug.Log("DisplayLevelInfo - End");
    }


    // Menu buttons
    public void Play()
    {
        mainPanel.SetActive(false);
        playPanel.SetActive(true);
    }

    public void Credits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void Settings()
    {
        settingsPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void AdditionalOpen()
    {
        additionalPanel.SetActive(true);
    }

    public void AdditionalClose()
    {
        additionalPanel.SetActive(false);
    }

    public void LegacyOpen()
    {
        legacy.SetActive(true);
        playPanel.SetActive(false);
    }

    public void LegacyClose()
    {
        playPanel.SetActive(true);
        legacy.SetActive(false);
    }

    public void LBOpen()
    {
        multiplayerPanel.SetActive(false);
        leaderboard.SetActive(true);
    }

    public void LBClose()
    {
        multiplayerPanel.SetActive(true);
        leaderboard.SetActive(false);
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

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
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
        Debug.Log("Finding AudioSource...");
        source = GameObject.Find("mainmenu").GetComponent<AudioSource>();
        if (source == null)
        {
            Debug.LogError("AudioSource not found or not attached to 'mainmenu' GameObject.");
            return;
        }

        Debug.Log("Getting Options component...");
        Options options = GetComponent<Options>();
        if (options == null)
        {
            Debug.LogError("Options component not found or not attached to this GameObject.");
            return;
        }
        focus = options.focusVol.isOn;
        Debug.Log("Setting volume based on focus state...");
        float ogvol = options.masterVolumeSlider.value;
        float focVol = options.unfocusVol.value;

        if (!focus && this.focus)
        {
            Debug.Log("Application lost focus. Setting volume to unfocused volume: " + focVol);
            source.outputAudioMixerGroup.audioMixer.SetFloat("Master", focVol);
        }
        else if (focus && this.focus)
        {
            Debug.Log("Application regained focus. Setting volume to original volume: " + ogvol);
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
    public void Editor()
    {
        playPanel.SetActive(false);
        multiplayerPanel.SetActive(true);
    }

    public void Story()
    {
        if (PlayerPrefs.HasKey("Beginning"))
        {
            PlayerPrefs.GetString("Beginning", "itbegan");
            SceneManager.LoadScene("storySelector");
        }
        else
        {
            SceneManager.LoadScene("cutscene01");
        }
    }

    public void CommunityOpen()
    {
        community.SetActive(true);
        mainPanel.SetActive(false);
    }


    public void OpenOnline()
    {
        onlinePanel.SetActive(true);
        playPanel.SetActive(false);
    }

    public void CloseOnline()
    {
        onlinePanel.SetActive(false);
        playPanel.SetActive(true);
    }

    public void OpenInfo()
    {
        levelInfo.SetActive(true);
        onlinePanel.SetActive(false);
    }

    public void CloseInfo()
    {
        onlinePanel.SetActive(true);
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


    public void FixedUpdate()
    {
        // Check for any input (keyboard or mouse)
        bool hasInput = Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || Input.GetMouseButtonDown(0);
        idleTimer += Time.fixedDeltaTime;
        if (!hasInput)
        {
            // Player is idle, start the idle animation immediately if it's not already playing
            if (idleTimer > idleTimeThreshold)
            {
                animator.SetTrigger("StartIdle");
                animator.ResetTrigger("StopIdle");
                Cursor.visible = false;
            }
            
        }
        else
        {
            idleTimer = 0;  // Resetting idleTimer when there is input
            animator.SetTrigger("StopIdle");
            animator.ResetTrigger("StartIdle");
            Cursor.visible = true;
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
        
    }
}
