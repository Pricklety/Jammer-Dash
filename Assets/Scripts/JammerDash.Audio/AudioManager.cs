using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using JammerDash.Menus;
using UnityEditor;
using UnityEditor.Experimental;

namespace JammerDash.Audio
{
    public class AudioManager : MonoBehaviour
    {

        bool isLogoSFX = false;
        int counter = 0;
        public AudioClip sfxShort;
        public AudioClip sfxLong;

        bool songPlayed = false;
        private float masterVolume = 1.0f;
        public List<string> songPathsList;
        public int currentClipIndex = -1;
        float bgtimer = 0f;
        float spriteChangeInterval = 15f;
        public static AudioManager Instance { get; private set; }

        public bool isMusicLoaded = false;
        private int loadedSongsCount;
        private float loadingProgress;

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

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                StartCoroutine(LoadAudioClipsAsync());

            }
            else
            {
                Destroy(gameObject);
            }

            QualitySettings.maxQueuedFrames = 0;
        }

        public void Start()
        {
            masterS.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (Debug.isDebugBuild)
            devText.gameObject.SetActive(true);
            else
            devText.gameObject.SetActive(false);


            if (isLogoSFX)
                GetComponent<AudioSource>().PlayOneShot(sfxShort);

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
                AudioSource audioSource = GetComponent<AudioSource>();

                if (audioSource.isPlaying)
                {
                    return audioSource.time;
                }
            }

            return 0f;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9) && SceneManager.GetActiveScene().buildIndex == 1)
            {
                StartCoroutine(LoadAudioClipsAsync());
            }
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (Application.isFocused && data.selectedFPS >= 30)
            {
                Application.targetFrameRate = data.selectedFPS;
            }
            else if (!Application.isFocused && data.selectedFPS >= 30)
            {
                Application.targetFrameRate = 30;
            }
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                options = FindObjectOfType<Options>();
            }
            AudioSource[] audios = FindObjectsOfType<AudioSource>();
            foreach (AudioSource audio in audios)
            {
                audio.outputAudioMixerGroup = master;
                SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1.0f));
            }

            if (sfx)
            {
                foreach (AudioSource audio in audios)
                {
                    if (audio.name == "sfx")
                    {
                        audio.enabled = true;
                    }
                }
            }
            else
            {
                foreach (AudioSource audio in audios)
                {
                    if (audio.name == "sfx")
                    {
                        audio.enabled = false;
                    }
                }
            }
            AudioSource audioSource = GetComponent<AudioSource>();
            float value1 = Input.GetAxisRaw("Mouse ScrollWheel");
            float volumeChangeSpeed = 1f;

            float volumeAdjustmentDelay = 1f;

            foreach (AudioSource audio in audios)
            {
                audio.outputAudioMixerGroup = master;
                audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", Mathf.Clamp(masterS.value, -80f, 20f));
            }
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {

                if (!options.increaseVol.isOn)
                {
                    masterS.maxValue = 0;

                }
                else
                {
                    masterS.maxValue = 20;
                }
            }
                if (Input.GetKey(KeyCode.LeftShift) && value1 != 0 && data.wheelShortcut)
            {
                timer = 0f; // Increment timer each frame
                            // Activate masterS GameObject if it's not active
                if (!masterS.gameObject.activeSelf)
                {
                    masterS.gameObject.SetActive(true);
                }
                // Calculate the new volume within the range of -80 to 0
                if (!options.increaseVol.isOn)
                {
                    masterS.maxValue = 0;
                    float newVolume = Mathf.Clamp(masterS.value + value1, -80f, 20f);
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/SFX/volClick"), 0.75f);
                    float intVol = Mathf.InverseLerp(-80f, 0f, newVolume) * 100f;
                    // Loop through each audio source
                    foreach (AudioSource audio in audios)
                    {

                        // Apply the new volume to the audio source 
                        audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", newVolume);
                        if (SceneManager.GetActiveScene().buildIndex == 1)
                        {
                            options.masterVolumeSlider.value = newVolume;
                        }
                    }
                    masterS.value = newVolume; // Update UI slider text
                    masterS.GetComponentInChildren<Text>().text = "Master: " + (int)intVol;
                }
                else
                {
                    masterS.maxValue = 20;
                    float newVolume = Mathf.Clamp(masterS.value + value1, -80f, 20f);
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/SFX/volClick"), 0.75f);
                    float intVol = Mathf.InverseLerp(-80f, 20f, newVolume) * 120f;
                    // Loop through each audio source
                    foreach (AudioSource audio in audios)
                    {

                        // Apply the new volume to the audio source 
                        audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", newVolume);
                        if (SceneManager.GetActiveScene().buildIndex == 1)
                        {
                            options.masterVolumeSlider.value = newVolume;
                        }
                    }
                    masterS.value = newVolume; // Update UI slider text
                    masterS.GetComponentInChildren<Text>().text = "Master: " + (int)intVol;

                    options.ApplyOptions();
                }
               


               
                // Check if masterS is held and there's no input from Mouse ScrollWheel
                if (EventSystem.current.currentSelectedGameObject == masterS || Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    if (SceneManager.GetActiveScene().buildIndex == 1)
                    {
                        options.masterVolumeSlider.value = masterS.value;
                    }
                    timer = 0f; // Reset the timer when masterS is held
                }
            }
            else
            {
                if (Input.GetMouseButton(0) && timer < 2)
                {
                    if (SceneManager.GetActiveScene().buildIndex == 1)
                    {
                        options.masterVolumeSlider.value = masterS.value;  // Calculate the new volume within the range of -80 to 0
                        float newVolume = Mathf.Clamp(masterS.value, -80f, 0f);
                        float intVol = Mathf.RoundToInt(Mathf.InverseLerp(-80f, 0f, newVolume) * 100f);
                        // Update UI slider text
                        masterS.GetComponentInChildren<Text>().text = "Master: " + intVol;
                    }
                    timer = 1.95f;
                }

                timer += Time.fixedDeltaTime;
            }

          
            if (Mathf.Approximately(timer, 2f))
            {
                if (SceneManager.GetActiveScene().buildIndex == 1)
                    options.masterVolumeSlider.value = masterS.value;

                data.volume = masterS.value;
                SettingsFileHandler.SaveSettingsToFile(data);
                UnityEngine.Debug.Log("asdass");
            }

            // Check if the timer has exceeded 2 seconds and masterS is not held
            if (timer < 2.1f && timer > 2f)
            {
                masterS.gameObject.SetActive(false);
                if (SceneManager.GetActiveScene().buildIndex == 1)
                    options.masterVolumeSlider.value = masterS.value;
            }
            if (!songPlayed && !paused && !audioSource.isPlaying || (audioSource.time >= audioSource.clip.length && SceneManager.GetActiveScene().buildIndex == 1 || !audioSource.isPlaying && !paused))
            {
                PlayNextSong(songPlayed);
                songPlayed = true;
            } 

            // Reset songPlayed if conditions change and need to play the next song again
            if (songPlayed && audioSource.time < 5f && audioSource.isPlaying)
            {
                songPlayed = false;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(2) && !data.loadedLogoSFX)
            {
                counter++;
                if (counter == 3)
                {
                    GetComponent<AudioSource>().volume *= 0.8f;
                    GetComponent<AudioSource>().PlayOneShot(sfxLong, 1);
                    data.loadedLogoSFX = true;
                    SettingsFileHandler.SaveSettingsToFile(data);
                    Notifications.Notifications.instance.Notify("Jammer Dash :)", null);
                    new WaitForSecondsRealtime(8f);
                        GetComponent<AudioSource>().volume = 1;

                }
            }
            else if (data.loadedLogoSFX && Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(2))
            {
                counter++;
                if (counter == 3)
                {
                    GetComponent<AudioSource>().volume *= 0.8f;
                    GetComponent<AudioSource>().PlayOneShot(sfxLong, 1);
                    data.loadedLogoSFX = false;
                    Notifications.Notifications.instance.Notify("oh :(", null);
                    SettingsFileHandler.SaveSettingsToFile(data);

                    new WaitForSecondsRealtime(8f);
                    GetComponent<AudioSource>().volume = 1;

                }
            }
            master.audioMixer.SetFloat("Gain", data.bass ? data.bassgain : 1f);



        }

        private void FixedUpdate()
        {

            if (Input.GetKey(KeyCode.Plus))
            {
                masterS.value++;
                timer = 0f;
            }
            else if (Input.GetKey(KeyCode.Minus))
            {
                timer = 0f;
                masterS.value--;
            }

            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (FindObjectOfType<mainMenu>().mainPanel.activeSelf)
                {
                    bgtimer += Time.deltaTime;
                    if (bgtimer >= spriteChangeInterval && data.bgTime == 1)
                    {
                        bgtimer = 0f;
                        StartCoroutine(ChangeSprite());

                    }
                    else if (bgtimer >= spriteChangeInterval && data.bgTime == 2)
                    {
                        spriteChangeInterval = 30f;
                        bgtimer = 0f;
                        StartCoroutine(ChangeSprite());
                    }

                }

            }
            else if (SceneManager.GetActiveScene().buildIndex != 1)
            {
                bgtimer = 0f;
            }
        }
        bool IsScrollingUI()
        {

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem.currentSelectedGameObject.GetComponent<Scrollbar>() != null || eventSystem.currentSelectedGameObject.GetComponent<ScrollRect>() != null)
            {
                return false;
            }
            else
            {
                return true;
            }
           
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = volume;
            UpdateAllVolumes();
        }


        private void UpdateAllVolumes()
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            AudioSource[] audios = FindObjectsOfType<AudioSource>();
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
        public IEnumerator LoadAudioClipsAsync()
        {
            yield return null;

            string persistentPath = Application.persistentDataPath;

            string sourceFolderPath = Path.Combine(Application.streamingAssetsPath, "music");

            if (Directory.Exists(sourceFolderPath))
            {
                string[] musicFiles = Directory.GetFiles(sourceFolderPath, "*.mp3", SearchOption.AllDirectories);

                // Use Task to perform file copying in parallel
                Task[] copyTasks = new Task[musicFiles.Length];
                for (int i = 0; i < musicFiles.Length; i++)
                {
                    string sourceFilePath = musicFiles[i];
                    string destinationFilePath = Path.Combine(persistentPath, Path.GetFileName(sourceFilePath));

                    copyTasks[i] = Task.Run(() =>
                    {
                        File.Copy(sourceFilePath, destinationFilePath, true);
                        Debug.Log($"Copied: {sourceFilePath} to {destinationFilePath}");
                    });
                }

                // Wait for all file copy tasks to complete
                yield return new WaitUntil(() => copyTasks.All(t => t.IsCompleted));
            }
            else
            {
                Debug.LogError($"Source folder not found: {sourceFolderPath}");
            }

            // Get all .mp3 and .wav files from the persistent path
            string[] mp3Files = Directory.GetFiles(persistentPath, "*.mp3", SearchOption.AllDirectories);
            string[] wavFiles = Directory.GetFiles(persistentPath, "*.wav", SearchOption.AllDirectories);

            // Get all .mp3 files from the streaming assets path
            string[] defaultFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.mp3", SearchOption.AllDirectories);

            // Combine all arrays into one
            string[] copiedFiles = mp3Files.Concat(wavFiles).Concat(defaultFiles).ToArray();


            Debug.Log($"Found {copiedFiles.Length} audio files in {persistentPath}");

            bool newFilesAdded = false;
            foreach (string copiedFile in copiedFiles)
            {
                if (!songPathsList.Contains(copiedFile))
                {
                    songPathsList.Add(copiedFile);
                    newFilesAdded = true;
                    Debug.Log($"Added new file to songPathsList: {copiedFile}");
                }
                else
                {
                    Debug.Log($"File already in songPathsList: {copiedFile}");
                }
            }

          

            // Shuffle the list of song paths only if new files were added
            if (newFilesAdded)
            {
                ShuffleSongPathsList();
                Notifications.Notifications.instance.Notify($"Playlist loaded. \n{copiedFiles.Length} songs found.", null); // Display notification
            }
            else
            {
                Notifications.Notifications.instance.Notify($"No new songs found. \n{songPathsList.Count} songs in the playlist.", null); // Display notification
            }
        }
        public void PlaySource()
        {

            GetComponent<AudioSource>().Play();

            paused = false;
        }

        public void Pause()
        {
            GetComponent<AudioSource>().Pause();

            paused = true;
        }

        public void Stop()
        {
            GetComponent<AudioSource>().Pause();
            paused = true;
            GetComponent<AudioSource>().Stop();
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
                    StartCoroutine(ChangeSprite());
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
                if (Input.GetKey(KeyCode.LeftShift))
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
                    StartCoroutine(ChangeSprite());

                PlayCurrentSong();
            }
        }



        public IEnumerator ChangeSprite()
        {
            mainMenu menu = options.GetComponent<mainMenu>();

            // Ensure there are sprites available
            if (menu.sprite.Length > 0 && menu.data.artBG || menu.sprite.Length > 0 && menu.data.customBG)
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
                menu.LoadRandomBackground();
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
                        GetComponent<AudioSource>().clip = audioClip;
                        options.musicSlider.maxValue = audioClip.length;
                        options.musicSlider.value = 0f;
                        options.musicText.text = "";
                        GetComponent<AudioSource>().Play();
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
            GetComponent<AudioSource>().clip.name = Path.GetFileNameWithoutExtension(songPathsList[index]);
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
