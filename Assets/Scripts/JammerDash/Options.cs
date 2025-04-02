using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Input = UnityEngine.Input;
using JammerDash.Audio;
using JammerDash.Menus;
using File = System.IO.File;
using Directory = System.IO.Directory;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.SocialPlatforms;
using JammerDash.Tech;
using SimpleFileBrowser;

namespace JammerDash
{
    public class Options : MonoBehaviour
    {
        public Text path;
        public Animator newSong;
        public Text newName;
        public Text newName2;
        public Button apply;
        public Dropdown resolutionDropdown;
        public GameObject keybind;
        public InputField fpsInputField;
        public GameObject confirm;
        public Text confirnText;
        public Button confirmation;
        public GameObject songSel;
        public GameObject snowObj;
        public Text musicText;
        public Slider musicSlider;
        public Slider musicVolSlider;
        public Slider sfxSlider;
        private SettingsData settingsData;
        public bool isLoadingMusic = false;
        public new AudioManager audio;
        public Slider masterVolumeSlider;
        public Dropdown playlist;
        public Toggle hitSounds;
        public Toggle sfx;
        public Toggle vsync;
        public Dropdown playerType;
        public Slider lowpass;
        public Dropdown backgrounds;
        public Toggle showFPS;
        public Dropdown hitType;
        public Toggle parallax;
        public Image backgroundImage;
        public Dropdown gameplayDir;
        public Toggle confineMouse;
        public Toggle wheelShortcut;
        public Toggle snow;
        public Toggle increaseVol;
        public Dropdown bgTime;
        public Toggle bass;
        public Slider bassGain;
        private static readonly HttpClient client = new();
        public Text version;
        public Slider dim;
        public Dropdown languageDropdown;
        string language;
        public Toggle randomSong;
        bool artBG = false;
        bool customBG = false;
        bool seasonBG = false;
        bool vidBG = false;

        public Toggle shaders;

        public Toggle discordAFK;
        public Toggle discordPlay;
        public Toggle discordEdit;

        public Toggle regionDisplay;

        public Text logInOut;

        public Dropdown textures;
        public Toggle preferNoRomaji;

        [Header("Next Update")]
        public GameObject nextChangelogs;
        public TMP_Text changelogs;
        public Text updateName;
        public Button update;
        public void Start()
        {
           SetLocale("en-US");
        
            CheckForUpdate();
            audio = AudioManager.Instance;
            if (audio == null)
            {
                musicText.text = "<color=red><b>Music folder empty, or not found</b></color>";
            }
            // Load or initialize settingsData
            settingsData = SettingsFileHandler.LoadSettingsFromFile();
            PopulateDropdowns();
            LoadSettings(); // Load settings at the start
            ApplySettings();

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
            version.text = Application.version;
        }

     public GameObject restartPanel;  // Reference to the restart notification panel

    public void ChangePath()
    {
        FileBrowser.ShowLoadDialog(
            onSuccess: (path) =>
            {
                string oldPath = Main.gamePath;
                Main.gamePath = path[0];
                Debug.Log("Path changed to: " + Main.gamePath);

                // Update the UI text
                this.path.text = Main.gamePath;
                
                // Show the restart panel
                ShowRestartPanel();
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "path.txt"), path[0]);
                MoveAllContent(oldPath, Main.gamePath);
            },
            onCancel: () =>
            {
                Debug.Log("Path selection canceled.");
            },
            pickMode: FileBrowser.PickMode.Folders,
            allowMultiSelection: false,
            initialPath: Main.gamePath,
            title: "Select Game Path"
        );

        ApplyOptions();
    }
    private void MoveAllContent(string sourcePath, string destinationPath)
{
    if (Directory.Exists(sourcePath))
    {
        // Ensure the destination directory exists
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        // Copy all files
        string[] files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            // Get the relative path of the file (e.g., subfolders)
            string relativePath = file.Substring(sourcePath.Length + 1);

            // Determine the destination file path
            string destFile = Path.Combine(destinationPath, relativePath);

            // Ensure the destination directory exists
            string destDir = Path.GetDirectoryName(destFile);
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // Copy the file
            File.Copy(file, destFile, true);
        }

        // Delete all files from the source
        foreach (string file in files)
        {
            if (Path.GetFileName(file) != "path.txt")
    {
        File.Delete(file);
    }
        }

        // Copy all subdirectories
        string[] directories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
        foreach (string directory in directories)
        {
            // Get the relative path of the directory
            string relativePath = directory.Substring(sourcePath.Length + 1);

            // Determine the destination directory path
            string destDir = Path.Combine(destinationPath, relativePath);

            // Create the destination directory if it doesn't exist
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
        }

        // Delete all directories from the source
        foreach (string directory in directories)
        {
            Directory.Delete(directory, true);
        }

        // Finally, delete the source directory itself
        Directory.Delete(sourcePath, true);
        
    }
    else
    {
        Debug.LogWarning($"Source path {sourcePath} does not exist.");
    }
}


    private void ShowRestartPanel()
    {
        // Activate the restart panel
        if (restartPanel != null)
        {
            restartPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Restart panel not assigned!");
        }
    }

    public void RestartGame()
    {
        // Logic to restart the game
        Debug.Log("Restarting game...");
        Application.Quit();
#if UNITY_EDITOR
        if (Application.isEditor) {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        #endif
    }

        public enum Language
        {
            English, // English
            日本語,  // Japanese
            Русский,  // Russian
            Español, // Spanish (Latin America)
            Deutsch, // German
            Français, // French
            BahasaIndonesia, // Indonesian
            portuguêsBrasil, // Brazilian Portuguese
            Italiano, // Italian
            čeština, // Czech
            Hrvatski, // Croatian
            Српски,  // Serbian Cyrillic
            Polski, // Polish
            TiếngViệt,  // Vietnamese
            Zénorik, // Jammer Dash's language
            nya // uwu nya mrrp~
        }

        public void OnLanguageChanged(int index)
        {
            Dictionary<string, Language> dropdownToEnumMapping = new()
            {
                { "English", Language.English },
                { "日本語", Language.日本語 },
                { "Русский", Language.Русский },
                { "Español (américa latina)", Language.Español },
                { "Deutsch", Language.Deutsch },
                { "Français", Language.Français },
                { "bahasa indonesia", Language.BahasaIndonesia },
                { "português (brasil)", Language.portuguêsBrasil },
                { "italiano", Language.Italiano },
                { "čeština", Language.čeština },
                { "Hrvatski", Language.Hrvatski },
                { "Српски", Language.Српски },
                { "Polski", Language.Polski },
                { "Tiếng Việt", Language.TiếngViệt },
                { "Zénorik", Language.Zénorik },
                { "meow", Language.nya }
            };

            Dictionary<Language, (string Locale, string Language)> languageMapping = new()
            {
                { Language.English, ("en-US", "en-US") },
                { Language.日本語, ("ja-JP", "ja-JP") },
                { Language.Русский, ("ru-RU", "ru-RU") },
                { Language.Español, ("es", "es") },
                { Language.Deutsch, ("de-DE", "de-DE") },
                { Language.Français, ("fr", "fr") },
                { Language.BahasaIndonesia, ("id-ID", "id-ID") },
                { Language.portuguêsBrasil, ("pt-BR", "pt-BR") },
                { Language.Italiano, ("it", "it")},
                { Language.čeština, ("cs", "cs") },
                { Language.Hrvatski, ("hr-HR", "hr-HR") },
                { Language.Српски, ("sr-Cyrl-RS", "sr-Cyrl-RS") },
                { Language.Polski, ("pl", "pl") },
                { Language.TiếngViệt, ("vi-VN", "vi-VN") },
                { Language.Zénorik, ("ze-ZE", "ze-ZE") },
                { Language.nya, ("nya-UWU", "nya-UWU") }
        };

            string selectedLanguageText = languageDropdown.options[index].text;

            if (dropdownToEnumMapping.TryGetValue(selectedLanguageText, out Language selectedLanguage))
            {
                if (languageMapping.TryGetValue(selectedLanguage, out var data))
                {
                    SetLocale(data.Locale);
                    language = data.Language;
                }
                else
                {
                    SetLocale("en-US");
                    language = "en-US";
                }
            }
            else
            {
                SetLocale("en-US");
                language = "en-US";
            }
        }

        private void SetLocale(string localeCode)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
                Debug.Log($"Language set to {locale.LocaleName} ({locale.Identifier.Code})");
            }
            else
            {
                SetLocale("en-US");
                language = "en-US";
            }
        }

        public void ToggleKeybinds()
        {
            keybind.SetActive(!keybind.activeSelf);
        }
        public void OnMusicSliderValueChanged()
        {
            if (Input.GetMouseButtonDown(0))
                audio.source.time = musicSlider.value;
        }

        void PopulateDropdowns()
{
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

    // Populate textures dropdown
    textures.ClearOptions();

    // Get all available texture packs from the "persistent/textures" folder
    string texturePackPath = Path.Combine(Main.gamePath, "textures");
    if (Directory.Exists(texturePackPath))
    {
        string[] directories = Directory.GetDirectories(texturePackPath);
        List<string> texturePackOptions = new List<string>();

        foreach (string directory in directories)
        {
            // Extract folder name from the path
            string folderName = new DirectoryInfo(directory).Name;
            texturePackOptions.Add(folderName);
        }

        // Add the texture pack options to the dropdown
        textures.AddOptions(texturePackOptions);
    }
    else
    {
        Directory.CreateDirectory(texturePackPath);
    }
}


      
        public void SetBackgroundImage()
        {
            string text = backgrounds.captionText.text;
            artBG = text == "Community";
            seasonBG = text == "Seasonal";
            vidBG = text == "Custom video";
            customBG = text == "Custom image";

        }

      public void OnVolumeChanged(Slider slider)
    {
        // Call HandleVolumeChange based on the slider that was changed
        if (slider == masterVolumeSlider)
        {
            audio.masterS.value = slider.value;
            audio.masterS.GetComponentInChildren<Text>().text = $"Master: {Mathf.Round(Mathf.Clamp(slider.value, -80f, 20f))}dB";
            audio.HandleVolumeChange(slider, "Master", 0f);
        }
        else if (slider == musicVolSlider)
        {
            audio.musicSlider.value = slider.value;
            audio.musicSlider.GetComponentInChildren<Text>().text = $"Music: {Mathf.Round(slider.value * 100)}";
            audio.HandleVolumeChange(slider, "Music", 0f);
        }
        else if (slider == sfxSlider)
        {
            audio.sfxSlider.value = slider.value;
            audio.sfxSlider.GetComponentInChildren<Text>().text = $"SFX: {Mathf.Round(slider.value * 100)}";
            audio.HandleVolumeChange(slider, "SFX", 0f);
        }

    }

        private bool isUserInteraction = false;
        public void PlaySelectedAudio(int index)
        {
            if (isUserInteraction)
            {
                audio.Play(index);
                StartCoroutine(audio.ChangeSprite(null));
            }
        }


        void LoadSettings()
        {
            Application.targetFrameRate = settingsData.selectedFPS;
            fpsInputField.text = settingsData.selectedFPS.ToString();
            resolutionDropdown.value = settingsData.resolutionValue;
            masterVolumeSlider.value = settingsData.volume;
            musicVolSlider.value =  settingsData.musicVol;
            sfxSlider.value = settingsData.sfxVol;
            audio.masterS.value = settingsData.volume;
            audio.musicSlider.value =  settingsData.musicVol;
            audio.sfxSlider.value = settingsData.sfxVol;
            backgrounds.value = settingsData.backgroundType;
            sfx.isOn = settingsData.sfx;
            hitSounds.isOn = settingsData.hitNotes;
            vsync.isOn = settingsData.vsync;
            playerType.value = settingsData.playerType;
            lowpass.value = settingsData.lowpassValue;
            showFPS.isOn = settingsData.isShowingFPS;
            hitType.value = settingsData.hitType;
            parallax.isOn = settingsData.parallax;
            confineMouse.isOn = settingsData.confinedMouse;
            wheelShortcut.isOn = settingsData.wheelShortcut;
            bgTime.value = settingsData.bgTime;
            snow.isOn = settingsData.snow;
            increaseVol.isOn = settingsData.volumeIncrease;
            bass.isOn = settingsData.bass;
            bassGain.value = settingsData.bassgain;
            dim.value = settingsData.dim * 100;
            shaders.isOn = settingsData.shaders;
            discordPlay.isOn = settingsData.discordPlay;
            discordEdit.isOn = settingsData.discordEdit;
            discordAFK.isOn = settingsData.discordAFK;
            regionDisplay.isOn = settingsData.region;
            randomSong.isOn = settingsData.randomSong;
            path.text = Main.gamePath;
            preferNoRomaji.isOn = settingsData.preferNoRomaji; 
            textures.value = settingsData.texture;
            SetDropdownValueFromSettings();
            
        }
        private readonly static string[] localeCodes =
   {
        "en-US", // English
        "ja-JP", // Japanese
        "ru-RU", // Russian
        "es", // Spanish (Latin America)
        "de-DE", // German
        "id-ID", // Indonesian
        "pt-BR", // Portuguese (Brazil)
        "hr-HR", // Croatian
        "sr-Cyrl-RS", // Serbian
        "pl",  // Polish
        "vi-VN", // Vietnamese
        "ze-ZE", // Zenorik
        "nya-UWU" // meow
    };
        private void SetDropdownValueFromSettings()
        {
            string currentLanguage = settingsData.language;

            for (int i = 0; i < localeCodes.Length; i++)
            {
                if (localeCodes[i] == currentLanguage)
                {
                    languageDropdown.value = i;
                    languageDropdown.RefreshShownValue();
                    return;
                }
            }

            languageDropdown.value = 0;
            languageDropdown.RefreshShownValue();
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
            settingsData.volume = audio.masterS.value;
            settingsData.musicVol = audio.musicSlider.value;
            settingsData.sfxVol = audio.sfxSlider.value;
            settingsData.backgroundType = backgrounds.value;
            settingsData.hitNotes = hitSounds.isOn;
            settingsData.sfx = sfx.isOn;
            settingsData.vsync = vsync.isOn;
            settingsData.playerType = playerType.value;
            settingsData.lowpassValue = lowpass.value;
            settingsData.hitType = hitType.value;
            settingsData.isShowingFPS = showFPS.isOn;
            settingsData.parallax = parallax.isOn;
            settingsData.confinedMouse = confineMouse.isOn;
            settingsData.wheelShortcut = wheelShortcut.isOn;
            settingsData.bgTime = bgTime.value;
            settingsData.snow = snow.isOn;
            settingsData.volumeIncrease = increaseVol.isOn;
            settingsData.bass = bass.isOn;
            settingsData.bassgain = bassGain.value;
            settingsData.dim = dim.value / 100;
            settingsData.language = language;
            settingsData.discordAFK = discordAFK.isOn;
            settingsData.discordPlay = discordPlay.isOn;
            settingsData.discordEdit = discordEdit.isOn;
            settingsData.shaders = shaders.isOn;
            settingsData.region = regionDisplay.isOn;
            settingsData.randomSong = randomSong.isOn;
            settingsData.gamePath = Main.gamePath;
            settingsData.preferNoRomaji = preferNoRomaji.isOn;
            settingsData.texture = textures.value;
            TexturePack.Instance.UpdateTexture();
            ApplyFPSCap(settingsData.selectedFPS);
            ApplyResolution();
            HitNotes(settingsData.hitNotes);
            SFX(settingsData.sfx);
            Vsync(settingsData.vsync);
            SetLocale(settingsData.language);
        }

       
        public void HitNotes(bool enabled)
        {
            audio.hits = enabled;
        }
        public void SFX(bool enabled)
        {
            audio.sfx = enabled;
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
            FindFirstObjectByType<AudioManager>().ShuffleSongPathsList();
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

      
        private void ApplyResolution()
        {
                string[] resolutionValues = resolutionDropdown.options[resolutionDropdown.value].text.Split('x');
                int width = int.Parse(resolutionValues[0]);
                int height = int.Parse(resolutionValues[1]);
            
                // Set the screen resolution
                Screen.SetResolution(width, height, Screen.fullScreenMode);
            

        }

        private void ApplyMasterVolume(float volume)
        {
            masterVolumeSlider.value = volume;

            // Update AudioManager with the new master volume
            if (audio != null)
            {
                audio.SetMasterVolume(volume);
            }
        }


        void SaveSettings()
        {
            // Create a new SettingsData instance based on the current UI state
            SettingsData newSettingsData = new SettingsData
            {
                language = language,
                selectedFPS = int.TryParse(fpsInputField.text, out int fpsCap) ? Mathf.Clamp(fpsCap, 1, 9999) : 60,
                vsync = vsync.isOn,
                volume = audio.masterS.value,
                musicVol = audio.musicSlider.value,
                sfxVol = audio.sfxSlider.value,
                resolutionValue = resolutionDropdown.value,
                backgroundType = backgrounds.value,
                sfx = sfx.isOn,
                hitNotes = hitSounds.isOn,
                playerType = playerType.value,
                lowpassValue = lowpass.value,
                isShowingFPS = showFPS.isOn,
                hitType = hitType.value,
                parallax = parallax.isOn,
                confinedMouse = confineMouse.isOn,
                wheelShortcut = wheelShortcut.isOn,
                bgTime = bgTime.value,
                volumeIncrease = increaseVol.isOn,
                snow = snow.isOn,
                bass = bass.isOn,
                bassgain = bassGain.value,
                dim = dim.value / 100,
                shaders = shaders.isOn,
                discordAFK = discordAFK.isOn,
                discordPlay = discordPlay.isOn,
                discordEdit = discordEdit.isOn,
                region = regionDisplay.isOn,
                randomSong = randomSong.isOn,
                preferNoRomaji = preferNoRomaji.isOn,
                texture = textures.value,
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                gameVersion = Application.version,
                gamePath = path.text
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
           
            int fpsCap;
            if (int.TryParse(fpsInputField.text, out fpsCap))
            {
                // Limit the FPS cap to a specific range (e.g., 60 to 500)
                fpsCap = Mathf.Clamp(fpsCap, 60, 500);

                Application.targetFrameRate = fpsCap;

                // Update the input field with the clamped value
                fpsInputField.text = fpsCap.ToString();
            }
            
            // Master Volume
            masterVolumeSlider.value = 1.0f;

            // Apply the changes
            ApplyOptions();
        }

        public void OpenExplorerAtMusicFolder()
        {
            string arguments = Path.Combine(Main.gamePath, "music");
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                Process.Start(arguments);
            else
                Process.Start("open", arguments);
        }

        public void OpenExplorerAtBGFolder()
        {
            string arguments = Path.Combine(Main.gamePath, "backgrounds");
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
                AudioClip currentClip = audio.source.clip;
                float currentTime = audio.GetCurrentTime();


                // Display music information
                DisplayMusicInfo(currentClip, currentTime);
                newSong.Rebind();
                newSong.Play("newSong");

            }
            else
            {
                AudioManager.Instance.LoadAudioClipsAsync();
                if (AudioManager.Instance == null)
                Notifications.instance.Notify("This game instance is corrupted. Please restart the game (Click to close)", () => Application.Quit());
            }
        }

        public async void CheckForUpdate()
        {
            string user = "pricklety";
            string repo = "Jammer-Dash";
            string url = $"https://api.github.com/repos/{user}/{repo}/releases/latest";

            // GitHub API requires a user-agent header
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Jammer Dash");

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                JObject release = JObject.Parse(responseBody);
                string latestVersion = release["tag_name"].ToString();
                string uploadDate = release["published_at"].ToString();
                string releaseNotes = release["body"].ToString();


                // Compare the latest version with the current version and notify if there's a new update available
                if (IsNewVersionAvailable(latestVersion))
                {
                    // Create a UnityAction delegate for the update action
                    UnityAction updateAction = () => OpenChangelog();
                    UnityAction update = () => OpenUpdate(latestVersion);
                    this.update.onClick.AddListener(update);
                    changelogs.text = releaseNotes;
                    updateName.text = $"{latestVersion}\n<size=6>{uploadDate}</size>";
                    Notifications.instance.Notify($"There's a new update available! Version: {latestVersion}.\nClick to open changelogs and update.", updateAction);
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError("Error fetching the latest release: " + e.Message);
            }
        }

        private bool IsNewVersionAvailable(string latestVersion)
        {
            string currentVersion = Application.version;
            return latestVersion != currentVersion;
        }
        private void OpenChangelog()
        {
            nextChangelogs.SetActive(true);
        }
        private void OpenUpdate(string latestVersion)
        {
            Application.OpenURL($"https://github.com/Pricklety/Jammer-Dash/releases/download/{latestVersion}/Jammer.Dash.{latestVersion}.zip");
        }
    
        public void DisplayMusicInfo(AudioClip currentClip, float currentTime)
        {
            if (currentClip != null)
            {
                // Get the current audio clip and time
                AudioClip Clip = audio.source.clip;
                string clipName = Clip != null ? Clip.name : "Unknown song";
                float Time = currentTime;
                float length = audio.source.clip.length;
                // Format the text
                string formattedText = $"♪ {clipName}\n{FormatTime (Time)}/{FormatTime(length)}";

                // Assign the formatted text to the UI 
                musicText.text = formattedText;
                musicSlider.value = (int)currentTime;
                musicSlider.maxValue = length;
                string[] n = clipName.Split(" - ");

                if (n.Length >= 2) 
                {
                    newName.text = n[0];
                    newName2.text = n[1];
                }
                else
                {
                    newName.text = "Unknown";
                    newName2.text = clipName; 
                }
            }
            else
            {
                // nothing
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
        public void Play()
        {
            audio.source.Play();
        }

        public void Pause()
        {
            audio.Pause();
        }

        public void Stop()
        {
            audio.Stop();
        }

        public void UpdateDropdownSelection()
        {
            // Update the dropdown selection based on the currently playing audio clip
            if (audio != null)
            {
                // Get the name of the currently playing audio clip
                string currentClipName = audio.source.clip.name;

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
                    isUserInteraction = false;
                    // Set the dropdown value to the index of the currently playing audio clip
                    playlist.value = currentIndex;
                    isUserInteraction = true;
                }
            }
        }
       
       public InputField username;
        public void Update()
{
    var account = Account.Instance;
    var currentSelected = EventSystem.current.currentSelectedGameObject;

    // Update account display text only when necessary
    string newUsername = $"{account.nickname} (@{account.username})";
    if (username.text != newUsername)
        username.text = newUsername;

    string newLogText = account.loggedIn 
        ? LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Account settings") 
        : LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Log in");

    if (logInOut.text != newLogText)
        logInOut.text = newLogText;

    if (audio != null)
    {
        var source = audio.source;
        if (source.clip != null)
        {
            DisplayMusicInfo(source.clip, source.time);

            int clipLength = (int)source.clip.length;
            if (musicSlider.maxValue != clipLength)
                musicSlider.maxValue = clipLength;

            UpdateDropdownSelection();

            // Avoid redundant updates
            if (playlist.value != audio.currentClipIndex)
            {
                isUserInteraction = false;
                playlist.value = audio.currentClipIndex;
                isUserInteraction = true;
            }
        }

        // Handle keybindings for music controls
        if (currentSelected == null || currentSelected.GetComponent<InputField>() == null)
        {
            switch (true)
            {
                case var _ when Input.GetKeyDown(KeybindingManager.prevSong):
                    PlayPreviousSong();
                    break;
                case var _ when Input.GetKeyDown(KeybindingManager.play):
                    Play();
                    break;
                case var _ when Input.GetKeyDown(KeybindingManager.pause):
                    Pause();
                    break;
                case var _ when Input.GetKeyDown(KeybindingManager.nextSong):
                    PlayNextSong();
                    break;
            }
        }

        float timerThreshold = 2f;

        // Optimized volume slider handling
        audio.masterS.gameObject.SetActive(currentSelected == masterVolumeSlider.gameObject || audio.timer <= timerThreshold);
        audio.musicSlider.gameObject.SetActive(currentSelected == musicVolSlider.gameObject || audio.timer <= timerThreshold);
        audio.sfxSlider.gameObject.SetActive(currentSelected == sfxSlider.gameObject || audio.timer <= timerThreshold);
    }

    // Toggle snow effect only if value changed
    if (snowObj.activeSelf != settingsData.snow)
        snowObj.SetActive(settingsData.snow);

    // Update max volume only if needed
    float newMaxVolume = increaseVol.isOn ? 20f : 0f;
    if (masterVolumeSlider.maxValue != newMaxVolume)
        masterVolumeSlider.maxValue = newMaxVolume;

    // Optimize cursor state toggle
    UnityEngine.Cursor.lockState = confineMouse.isOn ? CursorLockMode.Confined : CursorLockMode.None;
}


        public GameObject sawGameObject;          // GameObject with animation to enable/disable
    public GameObject visualizerLogo;        // Visualizer logo GameObject
    public GameObject visualizerBack;        // Visualizer background GameObject

    private Dictionary<string, Dictionary<string, string>> iniData;
        public void ApplySettingsFromIni(string iniPath)
    {
        IniParser parser = new IniParser();
        iniData = parser.ParseIni(iniPath);

        if (iniData == null)
        {
            Debug.LogError("Failed to parse INI file.");
            return;
        }

        // Handle [Animations]
        if (iniData.ContainsKey("Animations"))
        {
            var animations = iniData["Animations"];
            if (animations.ContainsKey("SawAnimation") && sawGameObject != null)
            {
                bool enableSawAnimation = animations["SawAnimation"].ToLower() == "true";
                Animator sawAnimator = sawGameObject.GetComponent<Animator>();
                if (sawAnimator != null)
                {
                    sawAnimator.enabled = enableSawAnimation;
                    Debug.Log($"SawAnimation set to: {enableSawAnimation}");
                }
            }
        }

        // Handle [Visualizers]
        if (iniData.ContainsKey("Visualizers"))
        {
            var visualizers = iniData["Visualizers"];

            // VisualizerColorLogo
            if (visualizers.ContainsKey("VisualizerColorLogo") && visualizerLogo != null)
            {
                string colorHex = visualizers["VisualizerColorLogo"];
                if (ColorUtility.TryParseHtmlString(colorHex, out Color color))
                {
                    float alpha = visualizers.ContainsKey("VisualizerColorLogoAlpha")
                        ? Mathf.Clamp(float.Parse(visualizers["VisualizerColorLogoAlpha"]) / 100f, 0, 1)
                        : 1f;

                    color.a = alpha;
                    ApplyColorToVisualizer(visualizerLogo, color);
                    Debug.Log($"VisualizerColorLogo set to: {color}");
                }
            }

            // VisualizerColorBack
            if (visualizers.ContainsKey("VisualizerColorBack") && visualizerBack != null)
            {
                string colorHex = visualizers["VisualizerColorBack"];
                if (ColorUtility.TryParseHtmlString(colorHex, out Color color))
                {
                    float alpha = visualizers.ContainsKey("VisualizerColorBackAlpha")
                        ? Mathf.Clamp(float.Parse(visualizers["VisualizerColorBackAlpha"]) / 100f, 0, 1)
                        : 1f;

                    color.a = alpha;
                    ApplyColorToVisualizer(visualizerBack, color);
                    Debug.Log($"VisualizerColorBack set to: {color}");
                }
            }
        }
    }
    public void OnTexturePackSelected(int index)
{
    string selectedTexturePack = textures.options[index].text;
    string selectedPath = Path.Combine(Main.gamePath, "textures", selectedTexturePack);
    TexturePack.Instance.SetActiveTexturePack(selectedPath);
    Debug.Log($"Selected Texture Pack: {selectedTexturePack}");
}

    private void ApplyColorToVisualizer(GameObject visualizer, Color color)
    {
        var spectrum = visualizer.GetComponent<SimpleSpectrum>();
        if (spectrum != null)
        {
            spectrum.colorMin = color;
        }
    }

        public void CloseConfirm()
        {
            confirm.SetActive(false);
            confirmation.onClick.RemoveAllListeners();
        }
        public void DeleteJDLevels()
        {
            confirm.SetActive(true);
            confirnText.text = "Deleting all levels is going to cause irreversible damage, and <color=red>won't create a backup</color>. Do you confirm?";
            confirmation.onClick.AddListener(Delete);
        }

        public void Delete()
        {
            string path = Path.Combine(Main.gamePath, "levels");
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    File.Delete(file);
                }
            }

            confirmation.onClick.RemoveAllListeners();
        }

        public void ResetData()
        {
            confirm.SetActive(true);
            confirnText.text = "Deleting the player is going to cause irreversible damage and <color=red>won't create a backup</color>. Do you confirm?";
            confirmation.onClick.AddListener(ResetPlayer);
        }

        public void ResetPlayer()
        {
            string playerpath = Path.Combine(Main.gamePath, "playerData.dat");
            string scorespath = Path.Combine(Main.gamePath, "scores.dat");
            File.Delete(playerpath);
            File.Delete(scorespath);
            Account.Instance.totalXP = 0;
            Account.Instance.level = 0;
            Account.Instance.SavePlayerData(Account.Instance.user, Account.Instance.email);
            confirmation.onClick.RemoveAllListeners();

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

        public void FixedUpdate()
        {
            dim.GetComponentInChildren<Text>().text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Background visibility")} ({dim.value}%)";


        }
    }
}
