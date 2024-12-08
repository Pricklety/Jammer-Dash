using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using JammerDash.Audio;
using JammerDash.Menus.Play.Score;
using JammerDash.Tech;
using UnityEngine.Localization.Settings;

namespace JammerDash.Menus.Play
{
    public class ButtonClickHandler : MonoBehaviour
    {
        public ScrollRect scrollRect;
        public float maxSize = 1020f; // Maximum size of the button when centered
        public float minSize = 1000f; // Minimum size of the button when not centered
        public Image bg;
        public Image levelImage;
        public Button button;
        public RectTransform buttonRectTransform;
        public bool isSelected = false;
        public Text levelLength;
        public Text levelBPM;
        public Text diff;
        public Text levelObj;
        public Text hp;
        public Text size;
        public Text bonus;  
        public leaderboard lb;
        public AudioSource sfx;
        public AudioClip sfxclip;
        void Start()
        {
            new WaitForEndOfFrame();
            button = GetComponent<Button>();
            buttonRectTransform = GetComponent<RectTransform>();
            button.onClick.AddListener(OnClick);
            button.onClick.RemoveAllListeners();
            scrollRect = GetComponentInParent<ScrollRect>();
        }
        public string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            // Ensure seconds don't go beyond 59
            seconds = Mathf.Clamp(seconds, 0, 59);

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        void DisplayLB(string levelName)
        {
            // Constructing the file path
            string filePath = Path.Combine(Application.persistentDataPath, "scores.dat");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                UnityEngine.Debug.LogWarning("Rank data file does not exist.");
                return;
            }

            // List to store scores for the selected level
            List<string[]> scoresList = new List<string[]>();

            // Reading the file line by line
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                // Splitting the line into components
                string[] data = line.Split(',');

                // Ensure that the data array has enough elements
                if (data.Length >= 10) // Ensure there are at least 10 elements in the array
                {
                    string level = data[0];
                    if (level == levelName)
                    {
                        string[] scoreData = new string[10];
                        scoreData[0] = data[1]; // Ranking
                        scoreData[8] = data[2]; // Shine performance
                        scoreData[2] = data[3]; // Accuracy
                        scoreData[1] = data[4]; // Score
                        scoreData[3] = data[5]; // Five
                        scoreData[4] = data[6]; // Three
                        scoreData[5] = data[7]; // One
                        scoreData[6] = data[8]; // Miss
                        scoreData[9] = data[9]; // Combo
                        scoreData[7] = data[10]; // Username
                        

                        
                        scoresList.Add(scoreData);
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Invalid data format for line: " + line);
                }
            }

        

        // Sort scores by score value (element at index 1)
        scoresList = scoresList.OrderByDescending(scoreData => float.Parse(scoreData[1])).ToList();

            // Instantiate a panel for the selected level
            foreach (string[] scoreData in scoresList)
            {
                GameObject panel = Instantiate(GetComponent<leaderboard>().panelPrefab, GetComponent<leaderboard>().panelContainer);
                ScorePanel scorePanel = panel.GetComponent<ScorePanel>();

                // Display the ranking, score, accuracy, and other data
                string displayText = string.Format("{0:N0} ({2}x)\n{1}%", scoreData[1], scoreData[2], scoreData[9]); // Score, Accuracy, Combo
                string rankText = string.Format("{0}", scoreData[0]); // Ranking
                string acc = string.Format("5: {0}\n3: {1}\n1: {2}\n0: {3}\n{4}sp", scoreData[3], scoreData[4], scoreData[5], scoreData[6], Mathf.RoundToInt(float.Parse(scoreData[8])).ToString()); // Accuracy breakdown (Five, Three, One, Miss)
                string user = scoreData[7]; // Username

                // Debug logs to ensure data is correct
                Debug.Log("Rank: " + rankText);
                Debug.Log("Score: " + displayText);
                Debug.Log("Accuracy breakdown: " + acc);
                Debug.Log("Username: " + user);

                scorePanel.rankText.sprite = Resources.Load<Sprite>($"ranking/{rankText}");
                scorePanel.scoreText.text = displayText;
                scorePanel.accuracyText.text = acc;
                scorePanel.username.text = user;
            }
        }
        
        void Update()
        {
            bg = GameObject.FindGameObjectWithTag("BG").GetComponent<Image>();
            levelImage = GameObject.Find("levelImage").GetComponent<Image>();
            if (button.onClick.GetPersistentEventCount() == 0)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
            if (Input.GetKeyDown(KeybindingManager.goToSelectedLevel) && isSelected)
            {
                StartCoroutine(Move(0));
            }
           
            if (isSelected && Input.GetKeyDown(KeyCode.Return))
            {
                if (gameObject.GetComponent<CustomLevelScript>() == null)
                {
                    FindFirstObjectByType<mainMenu>().OpenLevel(GetComponent<RankDisplay>().sceneIndex);
                }
                else
                {
                    // Play custom level
                    GetComponent<CustomLevelScript>().PlayLevel();
                }
            }
        }

        public void Change()
        {
            if (GetComponent<CustomLevelScript>().sceneData != null && isSelected)
            {
                StartCoroutine(LoadImage(Path.Combine(Application.persistentDataPath, "levels", "extracted", GetComponent<CustomLevelScript>().sceneData.ID + " - " + GetComponent<CustomLevelScript>().sceneData.sceneName, "bgImage.png")));
            }
        }
        public IEnumerator Move(float lerpSpeed)
        {
            if (lerpSpeed != 0)
            {
                lerpSpeed = 0f;
            }

            Vector3 buttonWorldPos = button.transform.position;

            // Convert the button's world position to the scrollRect's content parent local position
            Vector3 buttonLocalPos = scrollRect.content.parent.InverseTransformPoint(buttonWorldPos);

            // Calculate the target position directly to match the button's local position
            Vector2 targetPosition = new Vector2(scrollRect.content.localPosition.x, scrollRect.content.localPosition.y - buttonLocalPos.y - 495);

            // Smoothly move the content to the target position
            while (lerpSpeed < 0.25f)
            {
                lerpSpeed += Time.unscaledDeltaTime;
                scrollRect.content.localPosition = Vector2.Lerp(scrollRect.content.localPosition, targetPosition, lerpSpeed);
                yield return null;
            }

            // Ensure the final position is set
            scrollRect.content.localPosition = targetPosition;

        }
        void OnClick()
        {
            StartCoroutine(HandleButtonClick());
        }
        void DestroyLeaderboard()
        {
            foreach (Transform child in GetComponent<leaderboard>().panelContainer)
            {
                Destroy(child.gameObject);
            }
        }
        int selectedLevelIndex = -1;

        public IEnumerator PlayAudioOnlyMode()
        {
            ButtonClickHandler[] levels = FindObjectsOfType<ButtonClickHandler>();
            var customLevel = GetComponent<CustomLevelScript>();
            if (customLevel != null)
            {
                CustomLevelDataManager data = CustomLevelDataManager.Instance;
                data.sceneLoaded = false;
                levelLength = GameObject.Find("info").GetComponent<Text>();
                levelBPM = GameObject.Find("infoBPM").GetComponent<Text>();
                diff = GameObject.Find("infodiff").GetComponent<Text>();
                levelObj = GameObject.Find("infoobj").GetComponent<Text>();
                hp = GameObject.Find("health").GetComponent<Text>();
                size = GameObject.Find("cubeSize").GetComponent<Text>();
                bonus = GameObject.Find("levelbonusinfoi").GetComponent<Text>();

                // Find the selected level's index
               
                new WaitUntil(() => GetComponent<leaderboard>().panelContainer != null);
                DestroyLeaderboard();
                DisplayLB(customLevel.sceneData.ID.ToString());

                // Update UI elements
                UpdateUIWithLevelData(customLevel.sceneData);

                // Display formatted save time
                bonus.text = GetFormattedBonusInfo(customLevel.sceneData);
            }

            // Reset other buttons' selection
            DeselectOtherButtons();

            isSelected = true;
            yield return StartCoroutine(Move(5));

            button.onClick.RemoveAllListeners();

            // Start Lerp for button scale
            yield return StartCoroutine(LerpButtonSize(buttonRectTransform, new Vector3(850f, 120f, 0f)));

            if (customLevel.sceneData != null)
            {
                string clipPath = Path.Combine(Application.persistentDataPath, "levels", "extracted", customLevel.sceneData.ID + " - " + customLevel.sceneData.sceneName, customLevel.sceneData.artist + " - " + customLevel.sceneData.songName + ".mp3");
                int audioClipIndex = -1; // Initialize to a value that indicates no match found
                                         // Normalize the clipPath
                clipPath.Replace("/", "\\");
                // Iterate through the songPathsList
                for (int i = 0; i < AudioManager.Instance.songPathsList.Count; i++)
                {
                    // Normalize the current path in songPathsList
                    string normalizedSongPath = AudioManager.Instance.songPathsList[i];

                    if (string.Equals(normalizedSongPath, clipPath, StringComparison.OrdinalIgnoreCase)) // Case-insensitive comparison
                    {
                        audioClipIndex = i; // Set the index if a match is found
                        break; // No need to continue searching once a match is found
                    }
                }

                // If a match was found, set the audioClipIndex and load the audio clip
                if (audioClipIndex != -1)
                {
                    // Set the audioClipIndex on the AudioManager.Instance
                    AudioManager.Instance.currentClipIndex = audioClipIndex;

                    // Load the audio clip asynchronously
                    yield return StartCoroutine(AudioManager.Instance.LoadAudioClip(clipPath));
                    yield return new WaitUntil(() => AudioManager.Instance.songLoaded);
                    yield return new WaitForEndOfFrame();
                    UnityEngine.Debug.LogWarning("hi2");
                    AudioManager.Instance.source.loop = true;
                    AudioManager.Instance.source.time = UnityEngine.Random.Range(AudioManager.Instance.source.clip.length * 0f, AudioManager.Instance.source.clip.length * 0.5f);

                }
                else
                {
                    // Handle case where no match was found
                    UnityEngine.Debug.LogError("Clip path not found in songPathsList!");
                    Debug.LogError(clipPath);
                }
            }
            int selectedLevelIndex = Array.FindIndex(levels, level => level.isSelected);
            FindAnyObjectByType<mainMenu>().levelRow = selectedLevelIndex;
            yield return StartCoroutine(LoadImage(Path.Combine(Application.persistentDataPath, "levels", "extracted", customLevel.sceneData.ID + " - " + customLevel.sceneData.sceneName, "bgImage.png")));
        }
        private IEnumerator LerpButtonSize(RectTransform rectTransform, Vector2 sizeChange, float duration = 0.1f)
        {
            Vector2 initialSize = rectTransform.sizeDelta;

            float timeElapsed = 0;

            while (timeElapsed < duration)
            {
                rectTransform.sizeDelta = Vector2.Lerp(initialSize, sizeChange, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            rectTransform.sizeDelta = sizeChange;
        }


        public IEnumerator HandleButtonClick()
        {
            var customLevel = GetComponent<CustomLevelScript>();
            ButtonClickHandler[] levels = FindObjectsOfType<ButtonClickHandler>();
            if (customLevel != null)
            {

                levelLength = GameObject.Find("info").GetComponent<Text>();
                levelBPM = GameObject.Find("infoBPM").GetComponent<Text>();
                diff = GameObject.Find("infodiff").GetComponent<Text>();
                levelObj = GameObject.Find("infoobj").GetComponent<Text>();
                hp = GameObject.Find("health").GetComponent<Text>();
                size = GameObject.Find("cubeSize").GetComponent<Text>();
                bonus = GameObject.Find("levelbonusinfoi").GetComponent<Text>();
                new WaitUntil(() => GetComponent<leaderboard>().panelContainer != null);
                DestroyLeaderboard();
                DisplayLB(customLevel.sceneData.ID.ToString());
                // Update UI elements
                UpdateUIWithLevelData(customLevel.sceneData);

                // Display formatted save time
                bonus.text = GetFormattedBonusInfo(customLevel.sceneData);
            }

            if (isSelected)
            {
                PlayLevelAudioOrSFX();
            }

            yield return StartCoroutine(LerpButtonSize(buttonRectTransform, new Vector3(850f, 120f, 0f)));
            // Reset other buttons' selection
            DeselectOtherButtons();

            isSelected = true;
            yield return StartCoroutine(Move(0));

            button.onClick.RemoveAllListeners();
           
            if (customLevel.sceneData != null)
            {
                string clipPath = Path.Combine(Application.persistentDataPath, "levels", "extracted", customLevel.sceneData.ID + " - " + customLevel.sceneData.sceneName, customLevel.sceneData.artist + " - " + customLevel.sceneData.songName + ".mp3");
                int audioClipIndex = -1; // Initialize to a value that indicates no match found
                                         // Normalize the clipPath
                clipPath.Replace("/", "\\");
                // Iterate through the songPathsList
                for (int i = 0; i < AudioManager.Instance.songPathsList.Count; i++)
                {
                    // Normalize the current path in songPathsList
                    string normalizedSongPath = AudioManager.Instance.songPathsList[i];

                    if (string.Equals(normalizedSongPath, clipPath, StringComparison.OrdinalIgnoreCase)) // Case-insensitive comparison
                    {
                        audioClipIndex = i; // Set the index if a match is found
                        break; // No need to continue searching once a match is found
                    }
                }

                // If a match was found, set the audioClipIndex and load the audio clip
                if (audioClipIndex != -1)
                {
                    // Set the audioClipIndex on the AudioManager.Instance
                    AudioManager.Instance.currentClipIndex = audioClipIndex;

                    // Load the audio clip asynchronously
                    yield return StartCoroutine(AudioManager.Instance.LoadAudioClip(clipPath));
                    yield return new WaitUntil(() => AudioManager.Instance.songLoaded);
                    yield return new WaitForEndOfFrame();
                    UnityEngine.Debug.LogWarning("hi2");
                    AudioManager.Instance.source.loop = true;
                    AudioManager.Instance.source.time = UnityEngine.Random.Range(AudioManager.Instance.source.clip.length * 0f, AudioManager.Instance.source.clip.length * 0.5f);

                }
                else
                {
                    // Handle case where no match was found
                    UnityEngine.Debug.LogError("Clip path not found in songPathsList!");
                    Debug.LogError(clipPath);
                }
            }
            int selectedLevelIndex = Array.FindIndex(levels, level => level.isSelected);
            FindAnyObjectByType<mainMenu>().levelRow = selectedLevelIndex;
            yield return StartCoroutine(LoadImage(Path.Combine(Application.persistentDataPath, "levels", "extracted", customLevel.sceneData.ID + " - " + customLevel.sceneData.sceneName, "bgImage.png")));



        }

        // Helper function to update UI with scene data
        private void UpdateUIWithLevelData(SceneData data)
        {
            levelLength.text = $"{FormatTime(data.songLength):N0}";
            levelBPM.text = $"{data.bpm}";
            diff.text = $"{data.calculatedDifficulty:F2}";
            levelObj.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Objects")}: {(data.cubePositions.Count + data.sawPositions.Count + data.longCubePositions.Count):N0} ({data.cubePositions.Count}, {data.sawPositions.Count}, {data.longCubePositions.Count})";
            hp.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Player HP")}: {data.playerHP}";
            size.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Object size")}: {data.boxSize}";
        }

        // Format and return the bonus information as a string
        private string GetFormattedBonusInfo(SceneData data)
        {
            long unixTime = Convert.ToInt64(data.saveTime);
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToUniversalTime();
            DateTimeOffset EUTime = TimeZoneInfo.ConvertTime(dateTimeOffset, TimeZoneInfo.Local);
            return $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "mapper")}: {data.creator}, {LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Local ID")}: {data.ID}, {LocalizationSettings.StringDatabase.GetLocalizedString("lang", "last saved on")} {EUTime:yyyy-MM-dd hh:MM:ss}";
        }

        // Handle playing the SFX or custom level audio
        private void PlayLevelAudioOrSFX()
        {
            sfx.PlayOneShot(sfxclip);
            if (GetComponent<CustomLevelScript>() == null)
            {
                FindFirstObjectByType<mainMenu>().OpenLevel(GetComponent<RankDisplay>().sceneIndex);
            }
            else
            {
                GetComponent<CustomLevelScript>().PlayLevel();
            }
            StopAllCoroutines();
        }

        // Deselect other buttons in the scroll rect content
        private void DeselectOtherButtons()
        {
            ButtonClickHandler[] buttons = scrollRect.content.GetComponentsInChildren<ButtonClickHandler>();
            foreach (ButtonClickHandler otherButton in buttons)
            {
                if (otherButton != this)
                {

                    StartCoroutine(LerpButtonSize(otherButton.buttonRectTransform, new Vector3(800f, 90f, 0f)));
                    otherButton.isSelected = false;
                    otherButton.selectedLevelIndex = -1;
                }
            }
        }

        // Helper function to generate clip path
        private string GetClipPath(SceneData data)
        {
            return Path.Combine(Application.persistentDataPath, "levels", "extracted", data.sceneName, $"{data.artist} - {data.songName}.mp3").Replace("/", "\\");
        }

        // Helper function to find index of clip path in songPathsList
        private int GetAudioClipIndex(List<string> songPathsList, string clipPath)
        {
            return songPathsList.FindIndex(path => string.Equals(path, clipPath, StringComparison.OrdinalIgnoreCase));
        }

        // Configure audio source settings
        private void ConfigureAudioSource()
        {
            AudioManager.Instance.source.loop = true;
            AudioManager.Instance.source.time = UnityEngine.Random.Range(AudioManager.Instance.source.clip.length * 0f, AudioManager.Instance.source.clip.length * 0.5f);
        }

        // Coroutine to load image from URL
        IEnumerator LoadImage(string url)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    // Create a Sprite from the downloaded texture and set it to the Image component
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    UnityEngine.Debug.Log(url);
                    bg.sprite = sprite;
                    levelImage.sprite = sprite;
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
        }
    }
}