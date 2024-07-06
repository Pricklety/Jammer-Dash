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
using JammerDash.Notifications;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;
namespace JammerDash
{
    public class Options : MonoBehaviour
    {
        public Button apply;
        public Dropdown resolutionDropdown;
        public GameObject keybind;
        public InputField fpsInputField;
        public Dropdown qualitySettingsDropdown;
        public GameObject confirm;
        public Text confirnText;
        public Button confirmation;
        public GameObject songSel;
        public GameObject snowObj;
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
        public Toggle vsync;
        public Dropdown playerType;
        public Toggle vidBG;
        public Toggle cursorTrail;
        public Toggle allVis;
        public Toggle lineVis;
        public Toggle logoVis;
        public Dropdown antiAliasing;
        public Slider lowpass;
        public Dropdown backgrounds;
        public Slider trailParticleCount;
        public Toggle showFPS;
        public Dropdown hitType;
        public Slider trailFade;
        public Text trailFadeText;
        public Toggle parallax;
        public Image backgroundImage;
        public Dropdown gameplayDir;
        public Toggle randomSFX;
        public Toggle confineMouse;
        public Toggle wheelShortcut;
        public Toggle snow;
        public Toggle increaseVol;
        public Dropdown bgTime;
        public Toggle bass;
        public Slider bassGain;
        public Toggle visualizerColor;
        private static readonly HttpClient client = new(); 

        public void Start()
        {
           
            CheckForUpdate();
            audio = FindObjectOfType<AudioManager>();
            if (audio == null)
            {
                musicText.text = "<color=red><b>Music folder empty, or not found</b></color>";
            }
            // Load or initialize settingsData
            settingsData = SettingsFileHandler.LoadSettingsFromFile();
            PopulateDropdowns();

            // Log statements for debugging
            UnityEngine.Debug.Log($"audio is {(audio != null ? "not null" : "null")}");
            UnityEngine.Debug.Log($"settingsData is {(settingsData != null ? "not null" : "null")}");

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
        }
       
        public void ToggleKeybinds()
        {
            keybind.SetActive(!keybind.activeSelf);
        }
        public void OnMusicSliderValueChanged()
        {
            if (Input.GetMouseButtonDown(0))
                audio.GetComponent<AudioSource>().time = musicSlider.value;
        }

        void PopulateDropdowns()
        {
            // Populate window mode dropdown
           
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
            UnityEngine.Debug.LogWarning("Background image not found: " + imageName);
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
                audio.masterS.value = volume;
                if (increaseVol.isOn)
                {
                    float intVol = Mathf.InverseLerp(-80f, 20f, volume) * 120f;
                    audio.masterS.GetComponentInChildren<Text>().text = "Master: " + intVol.ToString("f0");
                }
                else
                {
                    float intVol = Mathf.InverseLerp(-80f, 0f, volume) * 100f;
                    audio.masterS.GetComponentInChildren<Text>().text = "Master: " + intVol.ToString("f0");
                }
            }
        }


        public void PlaySelectedAudio(int index)
        {
            audio.Play(index);
            StartCoroutine(audio.ChangeSprite());
        }


        void LoadSettings()
        {
            Application.targetFrameRate = settingsData.selectedFPS;
            fpsInputField.text = settingsData.selectedFPS.ToString();
            qualitySettingsDropdown.value = settingsData.qualitySettingsLevel;
            resolutionDropdown.value = settingsData.resolutionValue;
            masterVolumeSlider.value = settingsData.volume;
            artBG.isOn = settingsData.artBG;
            customBG.isOn = settingsData.customBG;
            vidBG.isOn = settingsData.vidBG;
            sfx.isOn = settingsData.sfx;
            hitSounds.isOn = settingsData.hitNotes;
            vsync.isOn = settingsData.vsync;
            playerType.value = settingsData.playerType;
            cursorTrail.isOn = settingsData.cursorTrail;
            allVis.isOn = settingsData.allVisualizers;
            antiAliasing.value = settingsData.antialiasing;
            lowpass.value = settingsData.lowpassValue;
            trailParticleCount.value = settingsData.mouseParticles;
            showFPS.isOn = settingsData.isShowingFPS;
            hitType.value = settingsData.hitType;
            trailFade.value = settingsData.cursorFade;
            parallax.isOn = settingsData.parallax;
            randomSFX.isOn = settingsData.randomSFX;
            confineMouse.isOn = settingsData.confinedMouse;
            wheelShortcut.isOn = settingsData.wheelShortcut;
            bgTime.value = settingsData.bgTime;
            snow.isOn = settingsData.snow;
            increaseVol.isOn = settingsData.volumeIncrease;
            bass.isOn = settingsData.bass;
            bassGain.value = settingsData.bassgain;
            visualizerColor.isOn = settingsData.visualizerColor;
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
            settingsData.qualitySettingsLevel = qualitySettingsDropdown.value;
            settingsData.volume = masterVolumeSlider.value;
            settingsData.artBG = artBG.isOn;
            settingsData.customBG = customBG.isOn;
            settingsData.vidBG = vidBG.isOn;
            settingsData.hitNotes = hitSounds.isOn;
            settingsData.sfx = sfx.isOn;
            settingsData.vsync = vsync.isOn;
            settingsData.playerType = playerType.value;
            settingsData.cursorTrail = cursorTrail.isOn;
            settingsData.allVisualizers = allVis.isOn;
            settingsData.lineVisualizer = lineVis.isOn;
            settingsData.logoVisualizer = logoVis.isOn;
            settingsData.antialiasing = antiAliasing.value;
            settingsData.lowpassValue = lowpass.value;
            settingsData.hitType = hitType.value;
            settingsData.isShowingFPS = showFPS.isOn;
            settingsData.cursorFade = trailFade.value;
            settingsData.parallax = parallax.isOn;
            settingsData.randomSFX = randomSFX.isOn;
            settingsData.confinedMouse = confineMouse.isOn;
            settingsData.wheelShortcut = wheelShortcut.isOn;
            settingsData.bgTime = bgTime.value;
            settingsData.snow = snow.isOn;
            settingsData.volumeIncrease = increaseVol.isOn;
            settingsData.bass = bass.isOn;
            settingsData.bassgain = bassGain.value;
            settingsData.visualizerColor = visualizerColor.isOn;
            ApplyQualitySettings(settingsData.qualitySettingsLevel);
            ApplyMasterVolume(settingsData.volume);
            ApplyFPSCap(settingsData.selectedFPS);
            ApplyResolution();
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

      
        private void ApplyResolution()
        {
                string[] resolutionValues = resolutionDropdown.options[resolutionDropdown.value].text.Split('x');
                int width = int.Parse(resolutionValues[0]);
                int height = int.Parse(resolutionValues[1]);
            
                // Set the screen resolution
                Screen.SetResolution(width, height, Screen.fullScreenMode);
            

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
                qualitySettingsLevel = qualitySettingsDropdown.value,
                volume = masterVolumeSlider.value,
                resolutionValue = resolutionDropdown.value,
                artBG = artBG.isOn,
                customBG = customBG.isOn,
                vidBG = vidBG.isOn,
                sfx = sfx.isOn,
                hitNotes = hitSounds.isOn,
                playerType = playerType.value,
                cursorTrail = cursorTrail.isOn,
                allVisualizers = allVis.isOn,
                lineVisualizer = lineVis.isOn,
                logoVisualizer = logoVis.isOn,
                antialiasing = antiAliasing.value,
                lowpassValue = lowpass.value,
                mouseParticles = trailParticleCount.value,
                isShowingFPS = showFPS.isOn,
                hitType = hitType.value,
                cursorFade = trailFade.value,
                parallax = parallax.isOn,
                randomSFX = randomSFX.isOn,
                confinedMouse = confineMouse.isOn,
                wheelShortcut = wheelShortcut.isOn,
                bgTime = bgTime.value,
                volumeIncrease = increaseVol.isOn,
                snow = snow.isOn,
                bass = bass.isOn,
                bassgain = bassGain.value,
                visualizerColor = visualizerColor.isOn,
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
                UnityEngine.Debug.LogWarning("Audio clips are not loaded. Make sure AudioManager is initialized.");
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
                string releaseNotes = release["body"].ToString();

                Debug.Log($"Latest version: {latestVersion}");
                Debug.Log("Release Notes:");
                Debug.Log(releaseNotes);

                // Compare the latest version with the current version and notify if there's a new update available
                if (IsNewVersionAvailable(latestVersion))
                {
                    // Create a UnityAction delegate for the update action
                    UnityAction updateAction = () => OpenUpdate(latestVersion);
                    Notifications.Notifications.instance.Notify($"There's a new update available! ({latestVersion}).\nClick to update (manual).", updateAction);
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

        private void OpenUpdate(string latestVersion)
        {
            Application.OpenURL($"https://api.github.com/repos/Pricklety/Jammer-Dash/tarball/{latestVersion}");
        }
    
        public void DisplayMusicInfo(AudioClip currentClip, float currentTime)
        {
            if (currentClip != null)
            {
                // Get the current audio clip and time
                AudioClip Clip = audio.GetComponent<AudioSource>().clip;
                string clipName = Clip != null ? Clip.name : "Unknown song";
                float Time = currentTime;
                float length = audio.GetComponent<AudioSource>().clip.length;
                // Format the text
                string formattedText = $"♪ {clipName}\n{FormatTime (Time)}/{FormatTime(length)}";

                // Assign the formatted text to the UI 
                musicText.text = formattedText;
                musicSlider.value = (int)currentTime;
                musicSlider.maxValue = length;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Current audio clip is null. Check AudioManager logic.");
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
            audio.PlaySource();
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
        }
       
       
        public void Update()
        {
           
            if (audio != null)
            {
                DisplayMusicInfo(audio.GetComponent<AudioSource>().clip, audio.GetComponent<AudioSource>().time);
                musicSlider.maxValue = (int)audio.GetComponent<AudioSource>().clip.length;
                UpdateDropdownSelection();
                playlist.value = audio.currentClipIndex;


                if (Input.GetKeyDown(KeyCode.F6))
                {
                    PlayPreviousSong();
                }
                else if (Input.GetKeyDown(KeyCode.F7))
                {
                    PlayNextSong();  
                }

            }
            if (EventSystem.current.currentSelectedGameObject == masterVolumeSlider.gameObject)
            {
                audio.masterS.gameObject.SetActive(true);
            }
            else if (EventSystem.current.currentSelectedGameObject != masterVolumeSlider.gameObject && audio.timer > 2f)
            {
                audio.masterS.gameObject.SetActive(false);
            }

            if (allVis.isOn)
            {
                logoVis.isOn = true;
                lineVis.isOn = true;
            }
            else if (!logoVis.isOn || !lineVis.isOn)
                allVis.isOn = false;
            else if (!allVis.isOn)
            {
                logoVis.isOn = false;
                lineVis.isOn = false;
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
            snowObj.SetActive(snow.isOn);

            if (increaseVol.isOn)
            {
                masterVolumeSlider.maxValue = 20f;
            }
            else
            {
                masterVolumeSlider.maxValue = 0f;
            }

            if (confineMouse.isOn)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
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
            string path = Path.Combine(Application.persistentDataPath, "levels");
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
            string playerpath = Path.Combine(Application.persistentDataPath, "playerData.dat");
            string scorespath = Path.Combine(Application.persistentDataPath, "scores.dat");
            File.Delete(playerpath);
            File.Delete(scorespath);
            Account.Instance.totalXP = 0;
            Account.Instance.level = 0;
            Account.Instance.currentXP = 0;
            Account.Instance.SavePlayerData();
           FindObjectOfType<mainMenu>().LoadLevelFromLevels();
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
            trailFadeText.text = $"Trail fade ({trailFade.value:0.00}s)";
        }
    }
}
