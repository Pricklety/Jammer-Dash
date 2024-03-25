using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Options : MonoBehaviour
{
    public Dropdown windowModeDropdown;
    public Button apply;
    public Dropdown resolutionDropdown;

    // Additional UI elements for new settings
    public InputField fpsInputField;
    public Dropdown qualitySettingsDropdown;

    public GameObject songSel;
    public Text musicText;
    public Slider musicSlider;
    private SettingsData settingsData;
    public bool isLoadingMusic = false;
    public AudioManager audio;
    public Slider masterVolumeSlider;
    public Dropdown playlist;
    public Toggle hitSounds;
    public Toggle artBG;
    public Toggle customBG;
    public Toggle sfx;
    public Toggle focusVol;
    public Toggle vsync;
    public Dropdown playerType;
    public Toggle vidBG;
    public Toggle cursorTrail;
    public Toggle allVis;
    public Toggle bgVis;
    public Toggle lineVis;
    public Toggle logoVis;
    public Dropdown antiAliasing;
    public Slider unfocusVol;
    public Slider lowpass;
    public Dropdown scoreType;
    public Dropdown backgrounds;
    public Slider trailParticleCount;
    public Toggle showFPS;

    public Image backgroundImage;

    public void Start()
    {
        audio = FindObjectOfType<AudioManager>();

        if (audio == null)
        {
            musicText.text = "<color=red><b>Music folder empty, or not found</b></color>";
        }

        // Load or initialize settingsData
        settingsData = SettingsFileHandler.LoadSettingsFromFile();
        PopulateDropdowns();
        
        // Log statements for debugging
        Debug.Log($"audio is {(audio != null ? "not null" : "null")}");
        Debug.Log($"settingsData is {(settingsData != null ? "not null" : "null")}");

        LoadSettings(); // Load settings at the start
        ApplySettings();
        InvokeRepeating("InitializeMusic", 0, 0.2f);
        
        List<Dropdown.OptionData> optionDataList = new List<Dropdown.OptionData>();
        List<string> shuffledSongPaths = audio.songPathsList;

        playlist.ClearOptions();
        foreach (var musicClipPath in shuffledSongPaths)
        {
            // Get just the file name without the extension
            string songName = Path.GetFileNameWithoutExtension(musicClipPath);

            // Create a new OptionData object with the song name
            Dropdown.OptionData optionData = new Dropdown.OptionData(songName);

            // Add the OptionData to the list
            optionDataList.Add(optionData);
        }

        // Now you can add the shuffled list to the dropdown
        playlist.AddOptions(optionDataList);

    }

    public void OnMusicSliderValueChanged()
    {
        audio.GetComponent<AudioSource> ().time = musicSlider.value;
    }

    void PopulateDropdowns()
    {
        // Populate window mode dropdown
        windowModeDropdown.ClearOptions();
        List<string> windowModes = new List<string> { "Windowed", "Windowed Fullscreen", "Fullscreen" };
        windowModeDropdown.AddOptions(windowModes);

        // Populate quality dropdown
        qualitySettingsDropdown.ClearOptions();
        List<string> qualityLevels = new List<string>(QualitySettings.names);
        qualitySettingsDropdown.AddOptions(qualityLevels);

        // Populate resolution dropdown
        resolutionDropdown.ClearOptions();
        // Get the available resolutions for the current display
        Resolution[] resolutions = Screen.resolutions;
        // Create a list of resolution strings
        List<string> resolutionOptions = new List<string>();
        foreach (Resolution resolution in resolutions)
        {
            resolutionOptions.Add($"{resolution.width}x{resolution.height}");
        }
        // Add the resolution options to the dropdown
        resolutionDropdown.AddOptions(resolutionOptions);

        // Populate anti-aliasing dropdown
        antiAliasing.ClearOptions();
        List<string> aaOptions = new List<string> { "None", "2x", "4x", "8x" }; 
        antiAliasing.AddOptions(aaOptions);

        backgrounds.ClearOptions();

        List<Sprite> images = GetImagesFromFolder("backgrounds");

        // Add the images to the dropdown options
        foreach (Sprite sprite in images)
        {
            backgrounds.options.Add(new Dropdown.OptionData(sprite.name));
        }
        backgrounds.RefreshShownValue();
    }
    List<Sprite> GetImagesFromFolder(string folderPath)
    {
        List<Sprite> images = new List<Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>(folderPath);
        images.AddRange(sprites);

        // Get subfolders
        string[] subfolders = GetSubfolders(folderPath);
        foreach (string subfolder in subfolders)
        {
            // Load images recursively from subfolders
            images.AddRange(GetImagesFromFolder(folderPath + "/" + subfolder));     
        }

        return images;
    }

    string[] GetSubfolders(string folderPath)
    {
        List<string> subfolders = new List<string>();
        string path = folderPath;
        GameObject[] folders = Resources.LoadAll<GameObject>(path);
        foreach (GameObject folder in folders)
        {
            // Get the relative path of the subfolder
            string subfolder = folder.name.Substring(folderPath.Length + 1);
            subfolders.Add(subfolder);
        }
        return subfolders.ToArray();
    }
    public void SetBackgroundImage(string imageName)
    {
        // Get the currently selected dropdown option (image name)
        imageName = backgrounds.captionText.text;
        // Iterate through all loaded images
        foreach (Sprite image in GetImagesFromFolder("backgrounds"))
        {
            // Check if the image name matches the requested image
            if (image.name == imageName)
            {
                // Set the found image as the background image
                backgroundImage.sprite = image;
                return;
            }
        }

        // If the image is not found in any subfolder, log a warning
        Debug.LogWarning("Background image not found: " + imageName);
    }
    public void LoadMasterVolume()
    {
        float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        masterVolumeSlider.value = savedMasterVolume;

        // Apply the loaded master volume
        SetMasterVolume(savedMasterVolume);
    }

    public void OnMasterVolumeChanged(float volume)
    {
        volume = masterVolumeSlider.value;
        SetMasterVolume(volume);

        // Save master volume setting
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMasterVolume(float volume)
    {
        // Update AudioManager with the new master volume
        if (audio != null)
        {
            audio.SetMasterVolume(volume);
        }
    }


    public void PlaySelectedAudio(int index)
    {
        audio.Play(index);
    }


    void LoadSettings()
    {
        Application.targetFrameRate = settingsData.selectedFPS;
        fpsInputField.text = settingsData.selectedFPS.ToString();
        windowModeDropdown.value = settingsData.windowMode;
        qualitySettingsDropdown.value = settingsData.qualitySettingsLevel;
        resolutionDropdown.value = settingsData.resolutionValue;
        masterVolumeSlider.value = settingsData.volume;
        artBG.isOn = settingsData.artBG;
        customBG.isOn = settingsData.customBG;
        vidBG.isOn = settingsData.vidBG;
        sfx.isOn = settingsData.sfx;
        hitSounds.isOn = settingsData.hitNotes;
        focusVol.isOn = settingsData.focusVol;
        vsync.isOn = settingsData.vsync;
        playerType.value = settingsData.playerType;
        cursorTrail.isOn = settingsData.cursorTrail;
        allVis.isOn = settingsData.allVisualizers;
        antiAliasing.value = settingsData.antialiasing;
        unfocusVol.value = settingsData.noFocusVolume;
        lowpass.value = settingsData.lowpassValue;
        scoreType.value = settingsData.scoreType;
        trailParticleCount.value = settingsData.mouseParticles;
        showFPS.isOn = settingsData.isShowingFPS;
    }

    public void ApplyOptions()
    {
        // Apply the changes
        ApplySettings();

        // Save the modified settingsData
        SaveSettings();
    }

    public void ApplySettings()
    { 
        settingsData.selectedFPS = int.TryParse(fpsInputField.text, out int fpsCap) ? Mathf.Clamp(fpsCap, 1, 9999) : 60;
        settingsData.windowMode = windowModeDropdown.value;
        settingsData.qualitySettingsLevel = qualitySettingsDropdown.value;
        settingsData.volume = masterVolumeSlider.value;
        settingsData.artBG = artBG.isOn;
        settingsData.customBG = customBG.isOn;
        settingsData.vidBG = vidBG.isOn;
        settingsData.hitNotes = hitSounds.isOn;
        settingsData.sfx = sfx.isOn;
        settingsData.focusVol = focusVol.isOn;
        settingsData.vsync = vsync.isOn;
        settingsData.playerType = playerType.value;
        settingsData.cursorTrail = cursorTrail.isOn;
        settingsData.allVisualizers = allVis.isOn;
        settingsData.lineVisualizer = lineVis.isOn;
        settingsData.bgVisualizer = bgVis.isOn;
        settingsData.logoVisualizer = logoVis.isOn;
        settingsData.antialiasing = antiAliasing.value;
        settingsData.noFocusVolume = unfocusVol.value;
        settingsData.lowpassValue = lowpass.value;
        // Apply settings to the game
        ApplyWindowMode(settingsData.windowMode);
        ApplyQualitySettings(settingsData.qualitySettingsLevel);
        ApplyMasterVolume(settingsData.volume);
        ApplyFPSCap(settingsData.selectedFPS);
        ApplyResolution(settingsData.windowMode);
        HitNotes(settingsData.hitNotes);
        SFX(settingsData.sfx);
        Focus(settingsData.focusVol);
        Vsync(settingsData.vsync);
        Cursor(settingsData.cursorTrail);
        Visualizers(settingsData.allVisualizers);
        AA(settingsData.antialiasing);
    }

    public void AA(int value)
    {
        // Apply anti-aliasing level based on the selected value
        switch (value)
        {
            case 0: // None
                QualitySettings.antiAliasing = 0;
                break;
            case 1: // 2x
                QualitySettings.antiAliasing = 2;
                break;
            case 2: // 4x
                QualitySettings.antiAliasing = 4;
                break;
            case 3: // 8x
                QualitySettings.antiAliasing = 8;
                break;
            default:
                antiAliasing.captionText.text = "Invalid anti-aliasing value";
                break;
        }
    }


    public void Visualizers(bool enabled)
    {
        allVis.isOn = enabled;
    }
    public void HitNotes(bool enabled)
    {
        audio.hits = enabled;
    }
    public void SFX(bool enabled)
    {
        audio.sfx = enabled;
    }
    public void Cursor(bool enabled)
    {
        FindObjectOfType<CursorTrail>().trailImage.gameObject.SetActive(enabled);
        UnityEngine.Cursor.visible = true;
    }
    public void Focus(bool enabled)
    {
        mainMenu menu = GetComponent<mainMenu>();
        menu.focus = enabled;
    }
    public void Vsync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }

    private void ApplyFPSCap(int fpsCap)
    {
        settingsData.selectedFPS = fpsCap;
        Application.targetFrameRate = settingsData.selectedFPS;
    }

    public void Shuffle()
    {
        List<Dropdown.OptionData> optionDataList = new List<Dropdown.OptionData>();
        List<string> shuffledSongPaths = audio.songPathsList;
        FindObjectOfType<AudioManager>().ShuffleSongPathsList(); 
        playlist.ClearOptions();
        foreach (var musicClipPath in shuffledSongPaths)
        {
            // Get just the file name without the extension
            string songName = Path.GetFileNameWithoutExtension(musicClipPath);

            // Create a new OptionData object with the song name
            Dropdown.OptionData optionData = new Dropdown.OptionData(songName);

            // Add the OptionData to the list
            optionDataList.Add(optionData);
        }

        // Now you can add the shuffled list to the dropdown
        playlist.AddOptions(optionDataList);
    }

    private void ApplyWindowMode(int windowMode)
    {
        // Set window mode based on the dropdown selection
        switch (windowMode)
        {
            case 0: // Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 1: // Windowed Fullscreen
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; 
                break;
        }

        settingsData.windowMode = windowMode;
    }
    private void ApplyResolution(int windowMode)
    {
        if (windowMode == 0) 
        {
            string[] resolutionValues = resolutionDropdown.options[resolutionDropdown.value].text.Split('x');
            int width = int.Parse(resolutionValues[0]);
            int height = int.Parse(resolutionValues[1]);

            // Set the screen resolution
            Screen.SetResolution(width, height, false);
        }
        
    }

    private void ApplyQualitySettings(int level)
    {
        // Add logic to set quality settings
        QualitySettings.SetQualityLevel(level);
        settingsData.qualitySettingsLevel = level;
    }

    private void ApplyMasterVolume(float volume)
    {
        masterVolumeSlider.value = volume;

        // Update AudioManager with the new master volume
        if (audio != null)
        {
            audio.SetMasterVolume(volume);
        }
        // Apply the loaded master volume
        SetMasterVolume(volume);
    }

   
    void SaveSettings()
    {
        // Create a new SettingsData instance based on the current UI state
        SettingsData newSettingsData = new SettingsData
        {
            selectedFPS = int.TryParse(fpsInputField.text, out int fpsCap) ? Mathf.Clamp(fpsCap, 1, 9999) : 60,
            vsync = vsync.isOn,
            windowMode = windowModeDropdown.value,
            qualitySettingsLevel = qualitySettingsDropdown.value,
            volume = masterVolumeSlider.value,
            resolutionValue = resolutionDropdown.value,
            artBG = artBG.isOn,
            customBG = customBG.isOn,
            vidBG = vidBG.isOn,
            sfx = sfx.isOn,
            hitNotes = hitSounds.isOn,
            focusVol = focusVol.isOn,
            playerType = playerType.value,
            cursorTrail = cursorTrail.isOn,
            allVisualizers = allVis.isOn,
            lineVisualizer = lineVis.isOn,
            logoVisualizer = logoVis.isOn,
            bgVisualizer = bgVis.isOn,
            antialiasing = antiAliasing.value,
            noFocusVolume = unfocusVol.value,
            lowpassValue = lowpass.value,
            scoreType = scoreType.value,
            mouseParticles = trailParticleCount.value,
            isShowingFPS = showFPS.isOn,
            saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            gameVersion = Application.version
    };

        // Save the newSettingsData to a file
        SettingsFileHandler.SaveSettingsToFile(newSettingsData);
    }

    public void OpenSongSelector()
    {
        songSel.SetActive(true);
    }

    public void CloseSongSelector()
    {
        songSel.SetActive(false);
    }

    public void ResetToDefaults()
    {
        // Reset all settings to default values


        // Window Mode
        windowModeDropdown.value = 0; // Set to default windowed mode index

        int fpsCap;
        if (int.TryParse(fpsInputField.text, out fpsCap))
        {
            // Limit the FPS cap to a specific range (e.g., 60 to 500)
            fpsCap = Mathf.Clamp(fpsCap, 60, 500);

            Application.targetFrameRate = fpsCap;

            // Update the input field with the clamped value
            fpsInputField.text = fpsCap.ToString();
        }


        // Quality Settings
        qualitySettingsDropdown.value = 2; // Set to default quality level index

        // Master Volume
        masterVolumeSlider.value = 1.0f;
        OnMasterVolumeChanged(1.0f);

        // Apply the changes
        ApplyOptions();
    }

    public void OpenExplorerAtMusicFolder()
    {
        string arguments = Path.Combine(Application.persistentDataPath, "music");
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            Process.Start("explorer.exe", "/select," + arguments.Replace("/", "\\"));
        else
            Process.Start("open", arguments);
    }

    public void OpenExplorerAtBGFolder()
    {
        string arguments = Path.Combine(Application.persistentDataPath, "backgrounds");
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            Process.Start("explorer.exe", "/select," + arguments.Replace("/", "\\"));
        else
            Process.Start("open", arguments);
    }
    public void InitializeMusic()
    {
        if (audio != null && audio.AreAudioClipsLoaded())
        {
            // Assuming you have some logic to get the current music clip and time
            AudioClip currentClip = audio.GetComponent<AudioSource>().clip;
            float currentTime = audio.GetCurrentTime();


            // Display music information
            DisplayMusicInfo(currentClip, currentTime);
        }
        else
        {
            // Handle the case when audio clips are not loaded yet
            Debug.LogWarning("Audio clips are not loaded. Make sure AudioManager is initialized.");
        }
    }

    public void DisplayMusicInfo(AudioClip currentClip, float currentTime)
    {
        if (currentClip != null)
        {
            // Get the current audio clip and time
            AudioClip Clip = audio.GetComponent<AudioSource>().clip;
            string clipName = Clip != null ? Clip.name : "Unknown song";
            float Time = audio.GetComponent<AudioSource>().time;
            float length = audio.GetComponent<AudioSource>().clip.length;
            // Format the text
            string formattedText = $"♪ {clipName}\n{FormatTime(Time)}/{FormatTime(length)}";

            // Assign the formatted text to the UI 
            musicText.text = formattedText;
            musicSlider.value = Time;
            musicSlider.maxValue = length;
        }
        else
        {
            Debug.LogWarning("Current audio clip is null. Check AudioManager logic.");
        }
    }

    public void PlayNextSong()
    {
        if (audio != null && !isLoadingMusic && SceneManager.GetActiveScene().buildIndex == 1)
        {


            // Play the next song
            audio.PlayNextSong(false);

            // Initialize the music display
            InitializeMusic();
        }
    }

    public void PlayPreviousSong()
    {
        if (audio != null && !isLoadingMusic && SceneManager.GetActiveScene().buildIndex == 1)
        {

            // Unload the previous audio clip
            Resources.UnloadUnusedAssets();

            // Play the previous song
            audio.PlayPreviousSong();

            // Initialize the music display
            InitializeMusic();
        }
    }

    public void UpdateDropdownSelection()
    {
        // Update the dropdown selection based on the currently playing audio clip
        if (audio != null && audio.isMusicLoaded    )
        {
            // Get the name of the currently playing audio clip
            string currentClipName = audio.GetComponent<AudioSource>().clip.name;

            // Find the index of the audio clip by name in the musicClips list
            int currentIndex = -1;
            for (int i = 0; i < audio.songPathsList.Count; i++)
            {
                string songPath = audio.songPathsList[i];
                string songName = Path.GetFileNameWithoutExtension(songPath);
                if (songName == currentClipName)
                {
                    currentIndex = i;
                    break;
                }
            }

            // Check if the index is valid
            if (currentIndex >= 0 && currentIndex <= playlist.options.Count)
            {
                // Set the dropdown value to the index of the currently playing audio clip
                playlist.value = currentIndex;
            }
        }
        // Handle the case when audio clips are not loaded yet or not playing
        else
        {
            Debug.LogWarning("Audio clips are not loaded or not playing. Make sure AudioManager is initialized.");
        }
    }
   
    public void Update()
    {
        if (audio != null)
        {
            DisplayMusicInfo(audio.GetComponent<AudioSource>().clip, audio.GetComponent<AudioSource>().time);
            musicSlider.maxValue = audio.GetComponent<AudioSource>().clip.length;
            UpdateDropdownSelection();
            playlist.value = audio.currentClipIndex;
            
           
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PlayPreviousSong();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                PlayNextSong();
            }
            
        }

        if (!logoVis.isOn || !lineVis.isOn || !bgVis.isOn)
        {
            allVis.isOn = false;
        }
        else if (logoVis.isOn && lineVis.isOn && bgVis.isOn)
        {
            allVis.isOn = true;
        }
        if (allVis.isOn)
        {
            logoVis.isOn = true;
            bgVis.isOn = true;
            lineVis.isOn = true;
        }

        if (artBG.isOn)
        {
            settingsData.customBG = false;
            settingsData.vidBG = false;
            customBG.interactable = false;
            vidBG.interactable = false;
            customBG.isOn = false;
            vidBG.isOn = false;
        }
        else if (!artBG.isOn)
        {
            customBG.interactable = true;
            vidBG.interactable = true;
        }
        if (customBG.isOn)
        {
            settingsData.artBG = false;
            settingsData.vidBG = false;
            artBG.interactable = false;
            vidBG.interactable = false;
            artBG.isOn = false;
            vidBG.isOn = false;
            
        }
        else if (!customBG.isOn)
        {
            artBG.interactable = true;
            vidBG.interactable = true;
        }
        if (vidBG.isOn)
        {
            settingsData.artBG = false;
            settingsData.customBG = false;
            artBG.interactable = false;
            customBG.interactable = false;
            artBG.isOn = false;
            customBG.isOn = false;
        }
        else if (!vidBG.isOn)
        {
            artBG.interactable = true;
            customBG.interactable = true;
        }
    }

   

    public int FindClosestFPSIndex(int targetFPS)
    {
        List<string> fpsOptions = new List<string> { "60", "144", "240", "360", "500" };

        // Find the closest available FPS to the target
        int closestIndex = 0;
        int closestDifference = Mathf.Abs(targetFPS - int.Parse(fpsOptions[0]));

        for (int i = 1; i < fpsOptions.Count; i++)
        {
            int difference = Mathf.Abs(targetFPS - int.Parse(fpsOptions[i]));
            if (difference < closestDifference)
            {
                closestIndex = i;
                closestDifference = difference;
            }
        }

        return closestIndex;
    }

    public string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        // Ensure seconds don't go beyond 59
        seconds = Mathf.Clamp(seconds, 0, 59);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
