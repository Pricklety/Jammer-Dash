using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using UnityEngine.Rendering.PostProcessing;

using JammerDash.Menus;

namespace JammerDash.Audio
{
    public class AudioManager : MonoBehaviour
    {
        // Keybind function part thingy (Game.FunctionPanel)
        public Animator toggleAnim;
        public Text functionName;
        public Text functionKeybind;
        public Font font;
        bool isLogoSFX = false;
        public bool shuffle = false;
        bool songPlayed = false;
        private float masterVolume = 1.0f;
        public List<string> songPathsList;
        public int currentClipIndex = -1;
        float bgtimer = 0f;
        float spriteChangeInterval = 15f;
        public static AudioManager Instance { get; private set; }
        public int levelIndex = -1;
        public bool isMusicLoaded = false;
        private readonly int loadedSongsCount;
        private readonly float loadingProgress;

        public delegate void SongChanged();
        public static event SongChanged OnSongChanged;
        public AudioMixerGroup master;
        private Options options;
        public Slider masterS;
        public Slider musicSlider; 
    public Slider sfxSlider; 
        public bool sfx;
        public bool hits;
        public float timer = 0f;
        bool paused = false;
        public bool songLoaded;
        public Text devText;
        public AudioSource source;
        public AudioSource sfxS;

        public AudioClip volClickClip;
        private AudioSource[] audios;
        private bool isVolumeUpdated = false;
        private int currentSceneIndex = -1;
        PostProcessVolume postProcess;
        mainMenu menu;
        private FileSystemWatcher fileWatcher; 
        private HashSet<string> knownFiles = new HashSet<string>();

        SettingsData data;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
               
            }
            else
            {
                Destroy(gameObject);
            }
            volClickClip = Resources.Load<AudioClip>("Audio/SFX/volClick");
            QualitySettings.maxQueuedFrames = 0;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Start()
        {

            InitializeForScene(SceneManager.GetActiveScene().buildIndex);

            masterS.onValueChanged.AddListener(OnMasterVolumeChanged);
            source = GetComponent<AudioSource>();
            source.volume = SettingsFileHandler.LoadSettingsFromFile().musicVol;
            if (Debug.isDebugBuild)
                devText.gameObject.SetActive(true);
            else
                devText.gameObject.SetActive(false);


           

            // Initialize FileSystemWatcher
            string persistentPath = Main.gamePath;
            Debug.Log($"Setting up FileSystemWatcher for path: {persistentPath}");

            string sourceDirectory = Path.Combine(Application.streamingAssetsPath, "music");
            string destinationDirectory = Path.Combine(Main.gamePath, "music");

            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            foreach (string filePath in Directory.GetFiles(sourceDirectory, "*.mp3"))
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(destinationDirectory, fileName);
                File.Copy(filePath, destFilePath, true);
            }
            // Initialize FileSystemWatcher
            fileWatcher = new FileSystemWatcher
            {
                Path = persistentPath,
                Filter = "*.mp3",
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            

            // Subscribe to events
            fileWatcher.Created += OnNewFileDetected;
            fileWatcher.Renamed += OnFileRenamed;
            fileWatcher.Changed += OnNewFileDetected;
            fileWatcher.Deleted += OnFileDeleted;
            fileWatcher.Error += OnWatcherError;

            // Enable watcher
            fileWatcher.EnableRaisingEvents = true;
            Debug.Log("FileSystemWatcher initialized and active.");
            string path = Path.Combine(Main.gamePath, "music");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            // Initial load
            LoadAudioClipsAsync();

        }

        private void OnDestroy()
        {
            if (fileWatcher != null)
            {
                fileWatcher.EnableRaisingEvents = false;
                fileWatcher.Dispose();
            }
        }

        private void OnNewFileDetected(object sender, FileSystemEventArgs e)
        {
            Debug.Log($"New song file detected: {e.FullPath}");
            HandleFileChange(e.FullPath);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            Debug.Log($"Song file renamed or moved: {e.FullPath}");
            HandleFileChange(e.FullPath);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Debug.LogError($"FileSystemWatcher error: {e.GetException()}");
            Notifications.instance.Notify("An error happened in the file watcher system. \nIf you imported a song anywhere, try re-importing it.", null);
        }
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            Debug.Log($"File deleted: {e.FullPath}");
            HandleFileDeletion(e.FullPath);
        }
        private void HandleFileDeletion(string filePath)
        {
            if (knownFiles.Contains(filePath))
            {
            knownFiles.Remove(filePath);
            songPathsList.Remove(filePath);

            // Notify user
            LoadAudioClipsAsync();
            Debug.Log($"File removed from playlist: {filePath}");
            }
        }
        private void HandleFileChange(string filePath)
        {
                
                Debug.Log($"File added to playlist: {filePath}");
                new WaitForEndOfFrame();
                LoadAudioClipsAsync();
            
        }

        public void LoadAudioClipsAsync()
        {

            string persistentPath = Main.gamePath;

             string[] mp3Files = Directory.GetFiles(persistentPath, "*.mp3", SearchOption.AllDirectories)
                                     .Where(file => !file.Contains(Path.DirectorySeparatorChar + "textures" + Path.DirectorySeparatorChar))
                                     .ToArray();

        string[] wavFiles = Directory.GetFiles(persistentPath, "*.wav", SearchOption.AllDirectories)
                                     .Where(file => !file.Contains(Path.DirectorySeparatorChar + "textures" + Path.DirectorySeparatorChar))
                                     .ToArray();

        string[] allAudioFiles = mp3Files.Concat(wavFiles).ToArray();


            foreach (string audioFile in allAudioFiles)
            {
                if (!songPathsList.Contains(audioFile))
                {
                    songPathsList.Add(audioFile);
                    knownFiles.Add(audioFile);
                }
            }

                ShuffleSongPathsList();
            
            

           
        }
        private void InitializeForScene(int sceneIndex)
        {
            // Cache current scene index
            currentSceneIndex = sceneIndex;

            // Refresh scene-specific objects
            if (currentSceneIndex == 1)
            {
                options = FindFirstObjectByType<Options>();
                menu = FindFirstObjectByType<mainMenu>();
            }

            // Refresh AudioSources
            audios = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            // Reset volume updated flag
            isVolumeUpdated = false;
            postProcess = Camera.main.GetComponent<PostProcessVolume>();
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Refresh logic for the new scene
            InitializeForScene(scene.buildIndex);
        }
        public void OnMasterVolumeChanged(float volume)
        {
            volume = masterS.value;
            SetMasterVolume(volume);

            // Save master volume setting
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }
        public void SetMasterVolume(float volume)
        {
            masterVolume = volume;
            UpdateAllVolumes();
        }

        public bool AreAudioClipsLoaded()
        {
            return songPathsList.Count > 0;
        }

        public AudioClip GetCurrentAudioClip()
        {
            if (songPathsList != null && currentClipIndex >= 0 && currentClipIndex < songPathsList.Count)
            {
                return Resources.Load<AudioClip>(songPathsList[currentClipIndex]);
            }

            return null;
        }

        public float GetCurrentTime()
        {
            if (songPathsList != null && currentClipIndex >= 0 && currentClipIndex < songPathsList.Count)
            {

                if (source.isPlaying)
                {
                    return source.time;
                }
            }

            return 0f;
        }

        Dictionary<Text, Font> originalFonts = new Dictionary<Text, Font>();


        private void FixedUpdate()
        {


            data = SettingsFileHandler.LoadSettingsFromFile();
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                if (menu == null)
                    menu = FindFirstObjectByType<mainMenu>();
                if (options == null)
                options = FindFirstObjectByType<Options>();
                if (menu.mainPanel.activeSelf)
                {
                    bgtimer += Time.deltaTime;
                    if (bgtimer >= spriteChangeInterval && data.bgTime == 1)
                    {
                        bgtimer = 0f;
                        StartCoroutine(ChangeSprite(null));

                    }
                    else if (bgtimer >= spriteChangeInterval && data.bgTime == 2)
                    {
                        spriteChangeInterval = 30f;
                        bgtimer = 0f;
                        StartCoroutine(ChangeSprite(null));
                    }

                }

            }
            else if (SceneManager.GetActiveScene().buildIndex != 1)
            {
                bgtimer = 0f;
            }

            postProcess.isGlobal = data.shaders;
        }
    private void ManageAudioSources()
        {
            foreach (AudioSource audio in audios)
            {
                audio.outputAudioMixerGroup = master;


                if (sfx)
                {
                    if (audio.name == "sfx")
                    {
                        audio.enabled = true;
                    }

                }
            }
        }
        private void Update()
{
    

    // Handle global keybinds efficiently
    if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return))
        KeybindPanel.ToggleFunction("Toggle fullscreen", "Alt + Enter");

    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeybindingManager.toggleUI))
    {
        data.canvasOff = !data.canvasOff;
        SettingsFileHandler.SaveSettingsToFile(data);
        KeybindPanel.ToggleFunction("Toggle gameplay interface", $"Shift + {KeybindingManager.toggleUI}");
    }

    if (Input.GetKey(KeyCode.F2) && menu.playPanel.activeSelf)
        KeybindPanel.ToggleFunction("Random level", "F2");

    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Tab))
        KeybindPanel.ToggleFunction("Notifications", "Left Control + Tab");

    if (master.audioMixer.GetFloat("Gain", out float a) && a != data.bassgain)
        master.audioMixer.SetFloat("Gain", data.bass ? data.bassgain : 1f);

    // Handle scene-specific logic
    if (currentSceneIndex == 1)
        HandleSceneSpecificLogic();

    // Manage global audio sources
    ManageAudioSources();

    float scrollValue = Input.GetAxisRaw("Mouse ScrollWheel");

    // Optimize slider scaling logic
    Vector3 smallScale = new(0.75f, 0.75f, 1);
    Vector3 mediumScale = new(0.95f, 0.95f, 1);
    Vector3 largeScale = new(1.25f, 1.25f, 1);

    musicSlider.transform.localScale = IsMouseOverSlider(musicSlider) ? mediumScale : smallScale;
    sfxSlider.transform.localScale = IsMouseOverSlider(sfxSlider) ? mediumScale : smallScale;
    masterS.transform.localScale = IsMouseOverSlider(masterS) ? largeScale : Vector3.one;

    // Volume adjustment with scroll wheel
    if (scrollValue != 0)
    {
        bool masterActive = IsMouseOverSlider(masterS) || Input.GetKey(KeyCode.LeftShift);
        bool musicActive = IsMouseOverSlider(musicSlider) || Input.GetKey(KeyCode.LeftAlt);
        bool sfxActive = IsMouseOverSlider(sfxSlider) || Input.GetKey(KeyCode.LeftControl);

        bool anyActive = masterActive || musicActive || sfxActive;

        masterS.gameObject.SetActive(anyActive);
        musicSlider.gameObject.SetActive(anyActive);
        sfxSlider.gameObject.SetActive(anyActive);

        if (masterActive)
        {
            HandleVolumeChange(masterS, "Master", scrollValue);
            if (options != null) options.masterVolumeSlider.value = masterS.value;
        }
        else if (musicActive)
        {
            HandleVolumeChange(musicSlider, "Music", scrollValue / 100);
            if (options != null) options.musicVolSlider.value = musicSlider.value;
        }
        else if (sfxActive)
        {
            HandleVolumeChange(sfxSlider, "SFX", scrollValue / 100);
            if (options != null) options.sfxSlider.value = sfxSlider.value;
        }
    }

    // Handle auto-hide logic for sliders
    HandleSliderAutoHide(masterS);
    HandleSliderAutoHide(musicSlider);
    HandleSliderAutoHide(sfxSlider);

    // Optimized music playback logic
    bool shouldPlayNext = !songPlayed && !paused && !source.isPlaying ||
                          (source.time >= source.clip.length || !source.isPlaying && !paused);

    if (SceneManager.GetActiveScene().buildIndex == 1 && shouldPlayNext)
    {
        PlayNextSong(songPlayed);
        songPlayed = true;
    }

    if (songPlayed && source.time < 5f && source.isPlaying)
        songPlayed = false;
}

        private void HandleSceneSpecificLogic()
        {
            // Scene-specific keybinds
            if (Input.GetKeyDown(KeybindingManager.nextSong))
                KeybindPanel.ToggleFunction("Next song", $"{KeybindingManager.nextSong}");
            else if (Input.GetKeyDown(KeybindingManager.prevSong))
                KeybindPanel.ToggleFunction("Previous song", $"{KeybindingManager.prevSong}");
            else if (Input.GetKeyDown(KeybindingManager.pause))
                KeybindPanel.ToggleFunction("Pause song", $"{KeybindingManager.pause}");
            else if (Input.GetKeyDown(KeybindingManager.play))
                KeybindPanel.ToggleFunction("Play song", $"{KeybindingManager.play}");
            else if (Input.GetKeyDown(KeyCode.B))
                KeybindPanel.ToggleFunction("Change background", "B");

            // Shuffle songs
            if ((shuffle || Input.GetKey(KeyCode.LeftShift)) && Input.GetKeyDown(KeybindingManager.nextSong))
                KeybindPanel.ToggleFunction("Random song", $"Shift + {KeybindingManager.nextSong}");
        }

        private bool IsMouseOverSlider(Slider slider)
    {
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(sliderRect, Input.mousePosition);
    }

    public void HandleVolumeChange(Slider slider, string category, float value)
    {
        timer = 0f;

        masterS.gameObject.SetActive(true);
        musicSlider.gameObject.SetActive(true);
        sfxSlider.gameObject.SetActive(true);
        float newVolume = Mathf.Clamp(slider.value + value, -80f, 20f);
        slider.value = newVolume;

        PlayVolumeClickSound();

        // Apply the new volume to the appropriate category
        switch (category)
        {
            case "Master":
                foreach (AudioSource audio in audios)
                {
                    if (audio.outputAudioMixerGroup == null)
                        audio.outputAudioMixerGroup = master;
                    audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", newVolume - value);
                    slider.GetComponentInChildren<Text>().text = $"{category}: {newVolume}dB";
                } 
                break;

            case "Music":
                if (source != null)
                {
                    source.volume = slider.value;
                    slider.GetComponentInChildren<Text>().text = $"{category}: {Mathf.RoundToInt(source.volume * 100)}"; 
                    
                }
                break;

            case "SFX":
                foreach (AudioSource audio in audios)
                {
                    if (audio.name == "sfx")
                    {
                        audio.volume = slider.value;
                        slider.GetComponentInChildren<Text>().text = $"{category}: {Mathf.RoundToInt(slider.value * 100)}";
                    }
                }
                break;
        }

        


    }

    private void HandleSliderAutoHide(Slider slider)
    {
        timer += Time.deltaTime;
        if (timer > 2f && slider.gameObject.activeSelf)
        {
        float newVolume = Mathf.Clamp(masterS.value, -80f, 20f);
            data.volume = newVolume;
            data.musicVol = musicSlider.value;
            data.sfxVol = sfxSlider.value;
            slider.gameObject.SetActive(false);
            
                SettingsFileHandler.SaveSettingsToFile(data);
        }
    }

    private void PlayVolumeClickSound()
    {
        sfxS.PlayOneShot(volClickClip, 0.45f);
    }


        private void UpdateAllVolumes()
        {
            foreach (AudioSource audio in audios)
            {
                audio.outputAudioMixerGroup = master;
                if (data.focusVol)
                {
                    if (Application.isFocused)
                    {
                        audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", masterVolume);
                    }
                    else
                    {

                        audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", data.noFocusVolume);
                    }
                }
                else
                {
                    audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", masterVolume);

                }
            }

        }

        public float GetLoadingProgress()
        {
            return loadingProgress;
        }
      

        public void Pause()
        {
            source.Pause();

            paused = true;
        }

        public void Stop()
        {
            source.Pause();
            paused = true;
            source.Stop();
        }
        public void ShuffleSongPathsList()
        {
            // Initialize System.Random with a unique seed each time
            System.Random rng = new System.Random(Guid.NewGuid().GetHashCode());

            int n = songPathsList.Count;
            for (int i = 0; i < n; i++)
            {
                int k = rng.Next(i, n);
                (songPathsList[i], songPathsList[k]) = (songPathsList[k], songPathsList[i]);
            }
            new WaitForChangedResult();
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                List<Dropdown.OptionData> optionDataList = new List<Dropdown.OptionData>();
                foreach (var musicClipPath in songPathsList)
                {
                    // Get just the file name without the extension
                    string songName = Path.GetFileNameWithoutExtension(musicClipPath);

                    // Create a new OptionData object with the song name
                    Dropdown.OptionData optionData = new Dropdown.OptionData(songName);

                    // Add the OptionData to the list
                    optionDataList.Add(optionData);
                }

                // Clear existing options
                options.playlist.ClearOptions();

                // Add the shuffled list to the dropdown
                options.playlist.AddOptions(optionDataList);
            }
        }

        public void PlayPreviousSong()
        {

            if (songPathsList != null && songPathsList.Count > 0)
            {
                currentClipIndex--;
                if (currentClipIndex < 0)
                    currentClipIndex = songPathsList.Count - 1;

                PlayCurrentSong();
                new WaitForSecondsRealtime(1f);
                if (data.bgTime == 0 && menu.mainPanel.activeSelf)
                   StartCoroutine(menu.LoadRandomBackground(null));
            }
        }



        public void PlayNextSong(bool isLoading)
        {
            float cooldown = 0f;
            cooldown += Time.unscaledDeltaTime;
            if (songPathsList != null && songPathsList.Count > 0 && (!isLoading || cooldown >= 5))
            {
                if (Input.GetKey(KeyCode.LeftShift) || shuffle)
                    currentClipIndex = UnityEngine.Random.Range(0, songPathsList.Count);
                else
                    currentClipIndex++;
                if (currentClipIndex >= songPathsList.Count)
                    currentClipIndex = 0;
                UnityEngine.Debug.Log($"Currently playing song index: {currentClipIndex}");
                PlayCurrentSong();
                new WaitForSecondsRealtime(1f);
                if (data.bgTime == 0 && menu.mainPanel.activeSelf)
                    StartCoroutine(menu.LoadRandomBackground(null));

            }
        }



       public IEnumerator ChangeSprite(string filePath)
{
    mainMenu menu = options.GetComponent<mainMenu>();

    // Ensure there are sprites available
    if (menu.sprite.Length > 0 && 
        menu.data.backgroundType >= 1 && 
        menu.data.backgroundType <= 3)
    {
       
        StartCoroutine(menu.LoadRandomBackground(null));
    }
    else if (!string.IsNullOrEmpty(filePath))
    {
        // Load the background specified by the filePath
        yield return menu.LoadLevelBackgroundAsync(filePath);
    }
}
        public void PlayCurrentSong()
        {
            if (songPathsList != null && songPathsList.Count > 0)
            {
                // Ensure currentIndex is within the valid range
                currentClipIndex = Mathf.Clamp(currentClipIndex, 0, songPathsList.Count - 1);

                string clipPath = songPathsList[currentClipIndex];

                StartCoroutine(LoadAudioClip(clipPath));
            }
        }
        public IEnumerator LoadAudioClip(string filePath)
        {
            songLoaded = false;
            filePath = filePath.Replace("\\", "/");
            Resources.UnloadUnusedAssets();
            // Encode the file path to ensure proper URL encoding
            string encodedPath = EncodeFilePath(filePath);
            string fileUri = "file://" + encodedPath;
            if (Application.isEditor)
            Debug.Log(fileUri);
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.UNKNOWN))
            {
                ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

                var requestOperation = www.SendWebRequest();

                while (!requestOperation.isDone)
                {
                    yield return null; // Wait for the next frame
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

                    if (audioClip != null)
                    {
                        audioClip.name = Path.GetFileNameWithoutExtension(filePath);


                        // Set the loaded audio clip to the AudioSource component
                        source.clip = audioClip;
                        options.musicSlider.maxValue = audioClip.length;
                        options.musicSlider.value = 0f;
                        options.musicText.text = "";
                        options.DisplayMusicInfo(audioClip, source.time);
                        options.newSong.Rebind();
                        options.newSong.Play("newSong");

                        source.Play();
                        songLoaded = true;
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

        public void Play(int index)
        {
            source.clip.name = Path.GetFileNameWithoutExtension(songPathsList[index]);
            currentClipIndex = index;
            PlayCurrentSong();
        }

      

        public List<string> GetSongPaths()
        {
            List<string> songPathsList = new List<string>();

            string[] musicFiles = Directory.GetFiles(Main.gamePath, "*.mp3", SearchOption.AllDirectories);
            songPathsList.AddRange(musicFiles);

            return songPathsList;
        }



        public int GetTotalNumberOfSongs()
        {

            string[] musicFiles = Directory.GetFiles(Main.gamePath, "*.mp3", SearchOption.AllDirectories);
            int numberOfMusicFiles = musicFiles.Length;
            return numberOfMusicFiles;
        }

        public int GetLoadedSongsCount()
        {
            return loadedSongsCount;
        }
    }

    public class WaitForAllTasks : CustomYieldInstruction
    {
        private Task[] tasks;

        public WaitForAllTasks(Task[] tasks)
        {
            this.tasks = tasks;
        }

        public override bool keepWaiting
        {
            get { return !Task.WhenAll(tasks).IsCompleted; }
        }
    }



}