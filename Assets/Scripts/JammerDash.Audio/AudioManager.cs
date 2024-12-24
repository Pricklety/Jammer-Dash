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
using JammerDash.Audio;

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
        public AudioClip sfxShort;
        public AudioClip sfxLong;
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
        public bool sfx;
        public bool hits;
        public float timer = 0f;
        bool paused = false;
        public bool songLoaded;
        public Text devText;
        public AudioSource source;

        public AudioClip volClickClip;
        private AudioSource[] audios;
        private bool isVolumeUpdated = false;
        private int currentSceneIndex = -1;
        PostProcessVolume postProcess;
        mainMenu menu;
        private FileSystemWatcher fileWatcher; 
        private HashSet<string> knownFiles = new HashSet<string>();

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
            QualitySettings.maxQueuedFrames = 3;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Start()
        {
            InitializeForScene(SceneManager.GetActiveScene().buildIndex);

            masterS.onValueChanged.AddListener(OnMasterVolumeChanged);
            source = GetComponent<AudioSource>();
            if (Debug.isDebugBuild)
                devText.gameObject.SetActive(true);
            else
                devText.gameObject.SetActive(false);


            if (isLogoSFX)
                source.PlayOneShot(sfxShort);

            // Initialize FileSystemWatcher
            string persistentPath = Application.persistentDataPath;
            Debug.Log($"Setting up FileSystemWatcher for path: {persistentPath}");

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

            // Initial load
            StartCoroutine(LoadAudioClipsAsync());

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
            Debug.Log($"New file detected: {e.FullPath}");
            StartCoroutine(HandleFileChange(e.FullPath));
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            Debug.Log($"File renamed or moved: {e.FullPath}");
            StartCoroutine(HandleFileChange(e.FullPath));
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Debug.LogError($"FileSystemWatcher error: {e.GetException()}");
            Notifications.instance.Notify("An error happened in the game folder. Restarting may help", null);
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
        private IEnumerator HandleFileChange(string filePath)
        {
            // Ensure the file exists and is valid
            if (File.Exists(filePath) && Path.GetExtension(filePath) == ".mp3")
            {
                if (!knownFiles.Contains(filePath))
                {
                    knownFiles.Add(filePath);
                    yield return LoadAudioClipsAsync(); // Refresh playlist
                }
            }
        }

        public IEnumerator LoadAudioClipsAsync()
        {

            string persistentPath = Application.persistentDataPath;

            // Collect all audio files
            string[] mp3Files = Directory.GetFiles(persistentPath, "*.mp3", SearchOption.AllDirectories);
            string[] wavFiles = Directory.GetFiles(persistentPath, "*.wav", SearchOption.AllDirectories);
            string[] allAudioFiles = mp3Files.Concat(wavFiles).ToArray();

            Debug.Log($"Found {allAudioFiles.Length} audio files in {persistentPath}");

            bool newFilesAdded = false;
            foreach (string audioFile in allAudioFiles)
            {
                if (!songPathsList.Contains(audioFile))
                {
                    songPathsList.Add(audioFile);
                    newFilesAdded = true;
                }
            }

            // Notify user about new files
            if (newFilesAdded)
            {
                ShuffleSongPathsList();
                Notifications.instance.Notify($"Playlist loaded. {allAudioFiles.Length} songs found.", null);
            }
            else
            {
                Notifications.instance.Notify($"No new songs found. {songPathsList.Count} songs in the playlist.", null);
            }

            yield return null;
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


            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
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

        private void Update()
        {
            if (audios == null || volClickClip == null) return;

            // Handle global keybinds
            HandleGlobalKeybinds();

            // Scene-specific logic
            if (currentSceneIndex == 1)
            {
                HandleSceneSpecificLogic();
            }

            // Manage audio sources globally
            ManageAudioSources();

            // Volume controls
            UpdateVolumeControl();

            if (SceneManager.GetActiveScene().buildIndex == 1 && (!songPlayed && !paused && !source.isPlaying || (source.time >= source.clip.length || !source.isPlaying && !paused)))
            {
                PlayNextSong(songPlayed);
                songPlayed = true;
            }

            // Reset songPlayed if conditions change and need to play the next song again
            if (songPlayed && source.time < 5f && source.isPlaying)
            {
                songPlayed = false;
            }

        }

        private void HandleGlobalKeybinds()
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();

            // Example global keybind: Fullscreen toggle
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return))
            {
                KeybindPanel.ToggleFunction("Toggle fullscreen", "Alt + Enter");
            }

            // Toggle UI globally
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeybindingManager.toggleUI))
            {
                data.canvasOff = !data.canvasOff;
                SettingsFileHandler.SaveSettingsToFile(data);
                KeybindPanel.ToggleFunction("Toggle gameplay interface", $"Shift + {KeybindingManager.toggleUI}");
            }

            bool gain = master.audioMixer.GetFloat("Gain", value: out float a);
            if (a != data.bassgain)
            {
                master.audioMixer.SetFloat("Gain", data.bass ? data.bassgain : 1f);
            }

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

        private void ManageAudioSources()
        {
            foreach (AudioSource audio in audios)
            {
                audio.outputAudioMixerGroup = master;

                // Apply master volume once
                if (!isVolumeUpdated)
                {
                    SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1.0f));
                    isVolumeUpdated = true;
                }

                if (sfx)
                {
                    if (audio.name == "sfx")
                    {
                        audio.enabled = true;
                    }

                }
            }
        }

        private void UpdateVolumeControl()
        {
            if (options != null)
            {
                float value1 = Input.GetAxisRaw("Mouse ScrollWheel");

                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (Input.GetKey(KeyCode.LeftShift) && value1 != 0 && data.wheelShortcut)
                {
                    HandleVolumeChange(value1);
                }
                else
                {
                    HandleVolumeChangeWhileHolding();
                }
            }
        }
        private void HandleVolumeChangeWhileHolding()
        {
            if (Input.GetMouseButton(0)) // Left mouse button interaction
            {
                timer = 0f; // Reset the timer
                float newVolume = Mathf.Clamp(masterS.value, -80f, 20f);
                float intVol = Mathf.RoundToInt(Mathf.InverseLerp(-80f, 20f, newVolume) * 120f);

                // Update UI text
                masterS.GetComponentInChildren<Text>().text = "Master: " + intVol;

                // Apply the new volume to the audio mixer
                foreach (AudioSource audio in audios)
                {
                    audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", newVolume);
                }

                if (options != null)
                {
                    options.masterVolumeSlider.value = newVolume;
                }
            }

            // Increment the timer and hide the slider if no interaction happens
            timer += Time.deltaTime;
            if (timer > 2f && masterS.gameObject.activeSelf)
            {
                masterS.gameObject.SetActive(false); // Hide slider after 2 seconds of inactivity
            }
        }

        private void HandleVolumeChange(float value1)
        {
            timer = 0f;

            if (!masterS.gameObject.activeSelf)
            {
                masterS.gameObject.SetActive(true);
            }

            float newVolume = Mathf.Clamp(masterS.value + value1, -80f, 20f);
            PlayVolumeClickSound(newVolume);

            foreach (AudioSource audio in audios)
            {
                audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", newVolume);
            }

            masterS.value = newVolume;
            masterS.GetComponentInChildren<Text>().text = "Master: " + Mathf.RoundToInt(Mathf.InverseLerp(-80f, 20f, newVolume) * 120f);
        }

        private void PlayVolumeClickSound(float newVolume)
        {
            source.PlayOneShot(volClickClip, 0.75f);
        }


        public void SetMasterVolume(float volume)
        {
            masterVolume = volume;
            UpdateAllVolumes();
        }


        private void UpdateAllVolumes()
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
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
      
        public void PlaySource()
        {

            source.Play();

            paused = false;
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

                UnityEngine.Debug.Log(currentClipIndex);
                PlayCurrentSong();
                new WaitForSecondsRealtime(1f);
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (data.bgTime == 0)
                    StartCoroutine(ChangeSprite(null));
            }
        }



        public void PlayNextSong(bool isLoading)
        {
            float cooldown = 0f;
            cooldown += Time.unscaledDeltaTime;
            if (songPathsList != null && songPathsList.Count > 0 && (!isLoading || cooldown >= 5))
            {
                cooldown = 0f;
                isLoading = true;
                if (Input.GetKey(KeyCode.LeftShift) || shuffle)
                    currentClipIndex = UnityEngine.Random.Range(0, songPathsList.Count);
                else
                    currentClipIndex++;
                if (currentClipIndex >= songPathsList.Count)
                    currentClipIndex = 0;
                UnityEngine.Debug.Log(currentClipIndex);
                cooldown = 0f;
                new WaitForSecondsRealtime(1f);
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (data.bgTime == 0)
                    StartCoroutine(ChangeSprite(null));

                PlayCurrentSong();
            }
        }



        public IEnumerator ChangeSprite(string filePath)
        {
            mainMenu menu = options.GetComponent<mainMenu>();

            // Ensure there are sprites available
            if (menu.sprite.Length > 0 && (menu.data.backgroundType >= 1 || menu.data.backgroundType <= 3) && filePath == null)
            {

                // Set the new sprite gradually over a specified duration
                float duration = 0.2f; // Adjust the duration as needed
                float elapsedTime = 0f;

                Image imageComponent = menu.bg; // Assuming menu.bg is of type Image

                Color startColor = imageComponent.color;
                Color targetColor = new(startColor.r, startColor.g, startColor.b, 0f);

                while (elapsedTime < duration)
                {
                    imageComponent.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                startColor = imageComponent.color;
                targetColor = Color.white;
                imageComponent.color = Color.Lerp(startColor, targetColor, 1f);
                menu.LoadRandomBackground(null);
            }
            else
            {
                _ = menu.LoadLevelBackgroundAsync(filePath);
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
            Resources.UnloadUnusedAssets();
            // Encode the file path to ensure proper URL encoding
            string encodedPath = EncodeFilePath(filePath);
            string fileUri = "file://" + encodedPath;
            UnityEngine.Debug.Log(encodedPath);
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
            UnityEngine.Debug.Log(currentClipIndex);
        }

        private void OnAudioClipsLoaded()
        {
            UnityEngine.Debug.Log("Audio clips loaded successfully.");
            isMusicLoaded = true;
        }

        public List<string> GetSongPaths()
        {
            List<string> songPathsList = new List<string>();

            string[] musicFiles = Directory.GetFiles(Application.persistentDataPath, "*.mp3", SearchOption.AllDirectories);
            songPathsList.AddRange(musicFiles);

            return songPathsList;
        }



        public int GetTotalNumberOfSongs()
        {

            string[] musicFiles = Directory.GetFiles(Application.persistentDataPath, "*.mp3", SearchOption.AllDirectories);
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