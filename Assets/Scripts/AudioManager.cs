using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Localization.SmartFormat.Utilities;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    bool songPlayed = false;
    private float masterVolume = 1.0f;
    public List<string> songPathsList;
    public int currentClipIndex = -1;
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
    float timer = 0f;
    bool paused = false;
        
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (!isMusicLoaded)
            {

                StartCoroutine(LoadAudioClipsAsync());
            }
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

    }

    private void FixedUpdate()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (!songPlayed && !paused && (audioSource.time >= audioSource.clip.length || (!audioSource.isPlaying && SceneManager.GetActiveScene().buildIndex == 1)))
        {
            PlayNextSong(songPlayed);
            songPlayed = true;
        }

        // Reset songPlayed if conditions change and need to play the next song again
        if (songPlayed && audioSource.time < 5f && audioSource.isPlaying)
        {
            songPlayed = false;
        }


        AudioSource[] audios = FindObjectsOfType<AudioSource>();
        float value1 = Input.GetAxis("Mouse ScrollWheel");
        float volumeChangeSpeed = 0.25f;


        foreach (AudioSource audio in audios)
        {
            audio.outputAudioMixerGroup = master;
            audio.outputAudioMixerGroup.audioMixer.SetFloat("Master", Mathf.Clamp(masterS.value, -80f, 0f));
        }
        


        if (value1 != 0 && !IsScrollingUI())
        {
            timer = 0f; // Increment timer each frame
            // Activate masterS GameObject if it's not active
            if (!masterS.gameObject.activeSelf)
            {
                masterS.gameObject.SetActive(true);
            }
            // Calculate the new volume within the range of -80 to 0
            float newVolume = Mathf.Clamp(masterS.value + value1 * volumeChangeSpeed, -80f, 0f);
            float intVol = Mathf.RoundToInt(Mathf.InverseLerp(-80f, 0f, newVolume) * 100f);
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
            masterS.value = newVolume;


            // Update UI slider text
            masterS.GetComponentInChildren<Text>().text = "Master: " + intVol;
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
                    options.masterVolumeSlider.value = masterS.value;
                }
                timer = 1.95f;
            }

            timer += Time.fixedDeltaTime;
        }
        if (Mathf.Approximately(timer, 2f))
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
                options.masterVolumeSlider.value = masterS.value;

            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            data.volume = masterS.value;
            SettingsFileHandler.SaveSettingsToFile(data);
            Debug.Log("asdass");
        }

        // Check if the timer has exceeded 2 seconds and masterS is not held
        if (timer < 2.1f && timer > 2f)
        {
            masterS.gameObject.SetActive(false);
            if (SceneManager.GetActiveScene().buildIndex == 1)
                options.masterVolumeSlider.value = masterS.value;
        }
        
    }
    bool IsScrollingUI()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            Scrollbar scrollbar = eventSystem.currentSelectedGameObject.GetComponent<Scrollbar>();
            ScrollRect scrollRect = eventSystem.currentSelectedGameObject.GetComponentInParent<ScrollRect>();
            if (scrollbar != null || scrollRect != null)
                return true;
        }

        // Check if mouse is over a ScrollRect
        if (eventSystem != null && eventSystem.IsPointerOverGameObject())
        {
            ScrollRect scrollRect = FindObjectOfType<ScrollRect>();
            if (scrollRect != null && RectTransformUtility.RectangleContainsScreenPoint(scrollRect.viewport, Input.mousePosition))
                return true;
        }

        return false;
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

            string persistentMusicPath = Path.Combine(Application.persistentDataPath);
            bool existsFolder = Directory.Exists(persistentMusicPath);

            if (!existsFolder)
            {
                Directory.CreateDirectory(persistentMusicPath);

                string sourceFolderPath = Path.Combine(Application.streamingAssetsPath, "music");

                if (Directory.Exists(sourceFolderPath))
                {
                    string[] musicFiles = Directory.GetFiles(sourceFolderPath, "*.mp3", SearchOption.AllDirectories);

                    // Use Task to perform file copying in parallel
                    Task[] copyTasks = new Task[musicFiles.Length];
                    for (int i = 0; i < musicFiles.Length; i++)
                    {
                        string sourceFilePath = musicFiles[i];
                        string destinationFilePath = Path.Combine(persistentMusicPath, "music", Path.GetFileName(sourceFilePath));

                        copyTasks[i] = Task.Run(() =>
                        {
                            File.Copy(sourceFilePath, destinationFilePath, true);
                            Debug.Log($"Copied: {sourceFilePath} to {destinationFilePath}");
                        });
                    }

                    // Wait for all file copy tasks to complete
                    yield return new WaitForAllTasks(copyTasks);
                }
                else
                {
                    Debug.LogError($"Source folder not found: {sourceFolderPath}");
                }
            }

            // After copying files, add unique file paths to songPathsList
            string[] copiedFiles = Directory.GetFiles(persistentMusicPath, "*.mp3", SearchOption.AllDirectories);
            HashSet<string> encounteredFileNames = new HashSet<string>();
            foreach (string copiedFile in copiedFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(copiedFile);
                if (!encounteredFileNames.Contains(fileName))
                {
                    encounteredFileNames.Add(fileName);
                    songPathsList.Add(copiedFile);
                }
            }

            ShuffleSongPathsList(); // Shuffle the list of song paths
        
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
        System.Random rng = new();
        int n = songPathsList.Count;
        for (int i = 0; i < n; i++)
        {
            int k = rng.Next(i, n);
            (songPathsList[i], songPathsList[k]) = (songPathsList[k], songPathsList[i]);
        }
    }

    public void PlayPreviousSong()
    {
        
        if (songPathsList != null && songPathsList.Count > 0)
        {
            currentClipIndex--;
            if (currentClipIndex < 0)
                currentClipIndex = songPathsList.Count - 1;

            Debug.Log(currentClipIndex);
            PlayCurrentSong();
            new WaitForSecondsRealtime(0.5f);
            StartCoroutine(ChangeSprite());
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
            Debug.Log(currentClipIndex);
            PlayCurrentSong();
            cooldown = 0f;
            new WaitForSecondsRealtime(0.5f);
            StartCoroutine(ChangeSprite());
        }
    }


    IEnumerator ChangeSprite()
    {
        mainMenu menu = options.GetComponent<mainMenu>();

        // Ensure there are sprites available
        if (menu.sprite.Length > 0 && (menu.data.artBG || menu.data.customBG))
        {
            // Set the new sprite gradually over a specified duration
            float duration = 0.2f; // Adjust the duration as needed
            float elapsedTime = 0f;

            Image imageComponent = menu.bg; // Assuming menu.bg is of type Image

            // Determine the target sprite
            Sprite targetSprite = menu.sprite[Random.Range(0, menu.sprite.Length)];

            // Store the initial alpha value of the image
            float startAlpha = imageComponent.color.a;

            while (elapsedTime < duration)
            {
                // Calculate the interpolation factor
                float t = elapsedTime / duration;

                // Lerp between the start and target alpha
                Color currentColor = imageComponent.color;
                currentColor.a = Mathf.Lerp(startAlpha, 0f, t);
                imageComponent.color = currentColor;

                // Update elapsed time
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            // Set the final sprite and reset alpha
            imageComponent.sprite = targetSprite;
            Color finalColor = imageComponent.color;
            finalColor.a = 1f;
            imageComponent.color = finalColor;
        }
        else if (menu.sprite.Length > 0 && (!menu.data.customBG || !menu.data.customBG))
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
        string persistentMusicPath = Path.Combine(Application.persistentDataPath);

        if (Directory.Exists(persistentMusicPath))
        {
            string[] musicFiles = Directory.GetFiles(persistentMusicPath, "*.mp3", SearchOption.AllDirectories);
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
        string persistentMusicPath = Path.Combine(Application.persistentDataPath);
        string[] musicFiles = Directory.GetFiles(persistentMusicPath, "*.mp3", SearchOption.AllDirectories);
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
