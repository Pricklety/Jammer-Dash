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

public class AudioManager : MonoBehaviour
{
    bool songPlayed = false;
    private float masterVolume = 1.0f;
    public List<string> songPathsList;
    public int currentClipIndex = 0;
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
        
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (!isMusicLoaded)
            {
                StartCoroutine(LoadAudioClipsAsync());
                StartCoroutine(ChangeVol());
            }
            // Find the menuMusicControl script
            menuMusicControl musicControl = FindObjectOfType<menuMusicControl>();
            if (musicControl != null)
            {
                // Determine the desired song index
                int desiredIndex = musicControl.GetDesiredSongIndex(this);
                if (desiredIndex >= 0 && desiredIndex < songPathsList.Count)
                {
                    // Set the current clip index to the desired index
                    currentClipIndex = desiredIndex;
                }
                else
                {
                    Debug.LogWarning("Desired song index is out of range.");
                }
            }
            else
            {
                Debug.LogWarning("menuMusicControl script not found in the scene.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        StartCoroutine(ChangeVol());
        masterS.onValueChanged.AddListener(OnMasterVolumeChanged);
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

    }

    private void FixedUpdate()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (!songPlayed && (audioSource.time >= audioSource.clip.length || (!audioSource.isPlaying && SceneManager.GetActiveScene().buildIndex == 1)))
        {
            PlayNextSong(songPlayed);
            songPlayed = true;
        }

        // Reset songPlayed if conditions change and need to play the next song again
        if (songPlayed && audioSource.time < 5f && audioSource.isPlaying)
        {
            songPlayed = false;
        }
    }

    IEnumerator ChangeVol()
    {
        AudioSource[] audios = FindObjectsOfType<AudioSource>();
        float value1 = Input.GetAxis("Mouse ScrollWheel");
        float volumeChangeSpeed = 1f;

        float volumeAdjustmentDelay = 1f;
        foreach (AudioSource audio in audios)
        {
            audio.outputAudioMixerGroup = master;
            audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", masterVolume);
        }

        if (value1 != 0)
        {
            bool isAdjustingVolume;
            float volumeAdjustmentTimer;
            masterS.gameObject.SetActive(true);

            foreach (AudioSource audio in audios)
            {
                // Calculate the new volume within the range of 0 to 100
                float newVolume = Mathf.Clamp(audio.volume + value1 * volumeChangeSpeed / 200, 0f, 1f);

                // Update UI sliders
                masterS.value = newVolume;

                if (SceneManager.GetActiveScene().buildIndex == 1)
                {
                    // Assuming options.masterVolumeSlider range is also 0 to 100
                    options.masterVolumeSlider.value = masterS.value;
                    options.ApplySettings();
                }

                isAdjustingVolume = true;
                volumeAdjustmentTimer = volumeAdjustmentDelay;

                // Apply the new volume to the audio source
                audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", newVolume); 
            }

            // Start a coroutine for the delay
            yield return StartCoroutine(DelayCoroutine());

            masterS.gameObject.SetActive(false);
        }
    }

    IEnumerator DelayCoroutine()
    {
        float timer = 0f;

        while (timer < 2f)
        {
            // Check for user input (scroll or mouse interaction) only when masterS is held
            if (Input.GetMouseButton(0) && EventSystem.current.currentSelectedGameObject == masterS.gameObject)
            {
                // If there's input and masterS is held, reset the timer
                timer = 0f;
            }

            // Wait for a short time before checking again
            yield return null;

            // Update the timer
            timer += Time.deltaTime;
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        UpdateAllVolumes();
    }

    private void UpdateAllVolumes()
    {
        StartCoroutine(Volume());
    }

    IEnumerator Volume()
    {
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        float targetVolume = SceneManager.GetActiveScene().buildIndex != 1 ? data.volume : (Application.isFocused ? data.volume : data.noFocusVolume);

        float volumeTime = Time.time;
        float duration = 5f; // Transition duration in seconds

        // Apply volume changes to all audio sources
        AudioSource[] audios = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in audios)
        {
            audio.outputAudioMixerGroup = master;
            if (data.focusVol)
            {
                float startVolume;
                audio.outputAudioMixerGroup.audioMixer.GetFloat("Master", out startVolume);

                while (Time.time - volumeTime < duration)
                {
                    float timeElapsed = Time.time - volumeTime; // Calculate time elapsed since the volume change started
                    float interpolationFactor = Mathf.Clamp01(timeElapsed / duration); // Calculate interpolation factor

                    float currentVolume;
                    audio.outputAudioMixerGroup.audioMixer.GetFloat("Master", out currentVolume);
                    float newVolume = Mathf.Lerp(startVolume, targetVolume, interpolationFactor); // Perform interpolation
                    audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", newVolume);

                    yield return null; // Yield each frame
                }

                // Ensure the final volume is set exactly
                audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", targetVolume);
            }
        }
    }

    public float GetLoadingProgress()
    {
        return loadingProgress;
    }

    public IEnumerator LoadAudioClipsAsync()
    {
        if (!isMusicLoaded)
        {
            yield return null;

            string persistentMusicPath = Path.Combine(Application.persistentDataPath, "music");
            bool existsFolder = Directory.Exists(persistentMusicPath);

            if (!existsFolder)
            {
                Directory.CreateDirectory(persistentMusicPath);

                string sourceFolderPath = Path.Combine(Application.streamingAssetsPath, "music");

                if (Directory.Exists(sourceFolderPath))
                {
                    string[] musicFiles = Directory.GetFiles(sourceFolderPath, "*.mp3");
                    List<string> loadedFilesOrder = new List<string>();

                    // Use Task to perform file copying in parallel
                    Task[] copyTasks = new Task[musicFiles.Length];
                    for (int i = 0; i < musicFiles.Length; i++)
                    {
                        string sourceFilePath = musicFiles[i];
                        string fileName = Path.GetFileName(sourceFilePath);
                        string destinationFilePath = Path.Combine(persistentMusicPath, fileName);
                        loadedFilesOrder.Add(destinationFilePath); // Add the path to the list before copying

                        copyTasks[i] = Task.Run(() =>
                        {
                            File.Copy(sourceFilePath, destinationFilePath, true);
                            Debug.Log($"Copied: {sourceFilePath} to {destinationFilePath}");
                        });
                    }

                    // Wait for all file copy tasks to complete
                    yield return new WaitForAllTasks(copyTasks);

                    string[] files = Directory.GetFiles(persistentMusicPath, "*.mp3");
                    loadedFilesOrder.AddRange(files);
                    songPathsList = loadedFilesOrder;
                    StartCoroutine(LoadAudioClipsAsyncCoroutine(loadedFilesOrder));
                }
                else
                {
                    Debug.LogError($"Source folder not found: {sourceFolderPath}");
                }
            }
            else
            {
                string sourceFolderPath = Path.Combine(Application.streamingAssetsPath, "music");
                string[] musicFiles = Directory.GetFiles(sourceFolderPath, "*.mp3");
                List<string> loadedFilesOrder = new List<string>();

                // Use Task to perform file copying in parallel
                Task[] copyTasks = new Task[musicFiles.Length];
                for (int i = 0; i < musicFiles.Length; i++)
                {
                    string sourceFilePath = musicFiles[i];
                    string fileName = Path.GetFileName(sourceFilePath);
                    string destinationFilePath = Path.Combine(persistentMusicPath, fileName);

                    copyTasks[i] = Task.Run(() =>
                    {
                        File.Copy(sourceFilePath, destinationFilePath, true);
                        Debug.Log($"Copied: {sourceFilePath} to {destinationFilePath}");
                    });
                }

                // Wait for all file copy tasks to complete
                yield return new WaitForAllTasks(copyTasks);
                string[] files = Directory.GetFiles(persistentMusicPath, "*.mp3");
                loadedFilesOrder.AddRange(files);
                // Add all loaded file paths to songPathsList
                foreach (string filePath in loadedFilesOrder)
                {
                    songPathsList.Add(filePath);
                }
                ShuffleSongPathsList();
                StartCoroutine(LoadAudioClipsAsyncCoroutine(loadedFilesOrder));
            }
        }

    }


    private IEnumerator LoadAudioClipsAsyncCoroutine(List<string> loadedFilesOrder)
    {
        foreach (string file in loadedFilesOrder)
        {
            loadedSongsCount++;
            yield return null;
        }

        // Shuffle the songPathsList
        ShuffleSongPathsList();

        OnAudioClipsLoaded();
    }

    public void ShuffleSongPathsList()
    {
        System.Random rng = new();
        int n = songPathsList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (songPathsList[n], songPathsList[k]) = (songPathsList[k], songPathsList[n]);
        }
    }


    public void PlayPreviousSong()
    {
        
        if (songPathsList != null && songPathsList.Count > 0)
        {
            currentClipIndex--;
            if (currentClipIndex < 0)
                currentClipIndex = songPathsList.Count - 1;

            StartCoroutine(ChangeSprite());
            Debug.Log(currentClipIndex);
            PlayCurrentSong();
        }
    }



    public void PlayNextSong(bool isLoading)
    {
        float cooldown = 0f;
        cooldown += Time.fixedDeltaTime;
        if (songPathsList != null && songPathsList.Count > 0 && (!isLoading || cooldown >= 5))
        {
            cooldown = 0f;
            isLoading = true;
            currentClipIndex++;
            if (currentClipIndex >= songPathsList.Count) 
            {
                currentClipIndex = 0;
            }
            StartCoroutine(ChangeSprite());
            Debug.Log(currentClipIndex);
            PlayCurrentSong();
            cooldown = 0f;
        }
    }

  

    IEnumerator ChangeSprite()
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
        else if (menu.sprite.Length > 0 && !menu.data.customBG || menu.sprite.Length > 0 && !menu.data.customBG)
        {
            menu.ChangeBasicCol();
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
        Resources.UnloadUnusedAssets();

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            var requestOperation = www.SendWebRequest();

            while (!requestOperation.isDone)
            {
                options.musicSlider.maxValue = 1;
                options.musicSlider.value = requestOperation.progress;
                options.musicText.text = $"Loading song: {www.downloadedBytes / 1024768:0.00} MB / {www.downloadProgress * 100:0.00}%";
                yield return null; // Wait for the next frame
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = ((DownloadHandlerAudioClip)www.downloadHandler).audioClip;

                if (audioClip != null)
                {
                    audioClip.name = Path.GetFileNameWithoutExtension(filePath);

                    // Set the loaded audio clip to the AudioSource component
                    GetComponent<AudioSource>().clip = audioClip;
                    options.musicSlider.maxValue = audioClip.length;
                    options.musicSlider.value = 0f;
                    options.musicText.text = "";
                    GetComponent<AudioSource>().Play();
                }
                else
                {
                    Debug.LogError("Failed to extract audio clip from UnityWebRequest: " + filePath);
                }
            }
            else
            {
                Debug.LogError("Failed to load audio clip: " + www.error);
            }
        }
    }


    public void Play(int index)
    {
        GetComponent<AudioSource>().clip.name = Path.GetFileNameWithoutExtension(songPathsList[index]);
        currentClipIndex = index;
        PlayCurrentSong();
        Debug.Log(currentClipIndex);
    }

    private void OnAudioClipsLoaded()
    {
        Debug.Log("Audio clips loaded successfully.");
        isMusicLoaded = true;
    }

    public List<string> GetSongPaths()
    {
        List<string> songPathsList = new List<string>();
        string persistentMusicPath = Path.Combine(Application.persistentDataPath, "music");

        if (Directory.Exists(persistentMusicPath))
        {
            string[] musicFiles = Directory.GetFiles(persistentMusicPath, "*.mp3");
            songPathsList.AddRange(musicFiles);
        }
        else
        {
            Debug.LogError("Music folder not found in the persistent data path.");
        }

        return songPathsList;
    }



    public int GetTotalNumberOfSongs()
    {
        string persistentMusicPath = Path.Combine(Application.persistentDataPath, "music");
        string[] musicFiles = Directory.GetFiles(persistentMusicPath, "*.mp3");
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
