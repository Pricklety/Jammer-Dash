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
using System.Data.Common;
using JammerDash.Game;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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
        public TextMeshProUGUI desc;
        public Text levelLength;
        public Text levelBPM;
        public Text diff;
        public Text levelObj;
        public Text hp;
        public Text size;
        public Text bonus;  
        public leaderboard lb;
        public Slider slider;
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
            string filePath = Path.Combine(JammerDash.Main.gamePath, "scores.dat");

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
                if (data.Length >= 11) // Ensure there are at least 10 elements in the array
                {
                    string level = data[0];
                    if (level == levelName)
                    {
                        string[] scoreData = new string[12];
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
                        scoreData[10] = (data.Length > 11) ? data[11] : ""; // Mods
                        

                        
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
                string mods = string.Join("\t", scoreData[10]
    .Split(';')
    .Select(m => modAbbreviations.ContainsKey(m) ? modAbbreviations[m] : m));

               
                scorePanel.modText.text = mods;
                scorePanel.rankText.sprite = Resources.Load<Sprite>($"ranking/{rankText}");
                scorePanel.scoreText.text = displayText;
                scorePanel.accuracyText.text = acc;
                scorePanel.username.text = user;
            }
        }
        string InsertSpacesBeforeCaps(string input)
{
    return System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
}

        Dictionary<string, string> modAbbreviations = new Dictionary<string, string>
        {
            { "SpeedIncrease", "SI" },
            { "hidden", "HD" },
            { "remember", "RM" },
            { "perfect", "PF" },
            { "random", "RD" },
            { "suddenDeath", "SuD" },
            { "SpeedDecrease", "SD" },
            { "oneLine", "OL" },
            { "noSpikes", "NS" },
            { "easy", "EZ" },
            { "yMirror", "MR" },
            { "autoMove", "AM" },
            { "auto", "AU" },
            { "noDeath", "ND" }
        };

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
                StartCoroutine(LoadImage(Path.Combine(JammerDash.Main.gamePath, "levels", "extracted", GetComponent<CustomLevelScript>().sceneData.ID + " - " + GetComponent<CustomLevelScript>().sceneData.name, "bgImage.png")));
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
        private Color easyColor = Color.green;
    private Color mediumColor = new Color(1f, 0.6f, 0.6f); // Light red
    private Color hardColor = new Color(0.8f, 0f, 0f); // Dark red
    private Color insaneColor = new Color(0.5f, 0f, 0.5f); // Purple
    private Color extremeColor = Color.black;
 public void UpdateDifficultyColor(float difficulty)
    {
        // Smoothly interpolate between colors based on difficulty
        Color color;

        if (difficulty < 2)
        {
            color = Color.Lerp(easyColor, mediumColor, difficulty / 2f);
        }
        else if (difficulty < 4)
        {
            color = Color.Lerp(mediumColor, hardColor, (difficulty - 2f) / 2f);
        }
        else if (difficulty < 6)
        {
            color = Color.Lerp(hardColor, insaneColor, (difficulty - 4f) / 2f);
        }
        else if (difficulty < 9)
        {
            color = Color.Lerp(insaneColor, extremeColor, (difficulty - 6f) / 3f);
        }
        else
        {
            color = extremeColor;
        }

        // Apply the interpolated color to the fill image
        slider.fillRect.GetComponent<Image>().color = color;
    }
     public async Task PlayAudioOnlyModeAsync()
{
    ButtonClickHandler[] levels = FindObjectsByType<ButtonClickHandler>(FindObjectsSortMode.InstanceID);
    var customLevel = GetComponent<CustomLevelScript>();

    if (customLevel != null)
    {
        CustomLevelDataManager data = CustomLevelDataManager.Instance;
        data.sceneLoaded = false;

        // UI elements
        desc = GameObject.Find("description").GetComponent<TextMeshProUGUI>();
        levelLength = GameObject.Find("info").GetComponent<Text>();
        levelBPM = GameObject.Find("infoBPM").GetComponent<Text>();
        levelObj = GameObject.Find("infoobj").GetComponent<Text>();
        hp = GameObject.Find("health").GetComponent<Text>();
        size = GameObject.Find("cubeSize").GetComponent<Text>();
        bonus = GameObject.Find("levelbonusinfoi").GetComponent<Text>();
        slider = GameObject.Find("sliderDiff").GetComponent<Slider>();

        // Wait until the leaderboard is ready
        await Task.Yield();  // Equivalent to `yield return null` in Unity
        DestroyLeaderboard();
        DisplayLB(customLevel.sceneData.ID.ToString());


        // Display formatted save time
        bonus.text = GetFormattedBonusInfo(customLevel.sceneData);

        
             List<Vector2> cubePositions = new List<Vector2>();
            List<Vector2> longCubePositions = new List<Vector2>();
            List<Vector2> sawPositions = new List<Vector2>();

            // Add cube positions from customLevel
            cubePositions.AddRange(customLevel.sceneData.cubePositions); 
            sawPositions.AddRange(customLevel.sceneData.sawPositions);
            longCubePositions.AddRange(customLevel.sceneData.longCubePositions);

              
            
        // After difficulty calculation, update the difficulty slider
        float duration = 0.1f;  // Duration for the lerp
        float masterPitch;
        Mods.instance.master.GetFloat("MasterPitch", out masterPitch);
        float elapsedTime = 0f;
        float startValue = slider.value * CustomLevelDataManager.Instance.scoreMultiplier * masterPitch;
        float targetValue = c * CustomLevelDataManager.Instance.scoreMultiplier * masterPitch;

        // Start the difficulty slider update coroutine
        StartCoroutine(UpdateDifficultySlider(targetValue, duration));

        // Reset other buttons' selection
        StartCoroutine(DeselectOtherButtons());

        isSelected = true;
        StartCoroutine(Move(5));

        button.onClick.RemoveAllListeners();

        // Start Lerp for button scale
        StartCoroutine(LerpButtonSize(buttonRectTransform, new Vector3(850f, 120f, 0f)));

        // Load the audio clip
        if (customLevel.sceneData != null)
        {
            string clipPath = Path.Combine(JammerDash.Main.gamePath, "levels", "extracted", customLevel.sceneData.ID + " - " + customLevel.sceneData.name, customLevel.sceneData.artist + " - " + customLevel.sceneData.songName + ".mp3");
            int audioClipIndex = -1;

            clipPath.Replace("/", "\\");

            // Iterate through the songPathsList to find a matching path
            for (int i = 0; i < AudioManager.Instance.songPathsList.Count; i++)
            {
                string normalizedSongPath = AudioManager.Instance.songPathsList[i];
                if (string.Equals(normalizedSongPath, clipPath, StringComparison.OrdinalIgnoreCase))
                {
                    audioClipIndex = i;
                    break;
                }
            }

            // If a match was found, load the audio clip
            if (audioClipIndex != -1)
            {
                AudioManager.Instance.currentClipIndex = audioClipIndex;
                StartCoroutine(AudioManager.Instance.LoadAudioClip(clipPath));
                await Task.Yield();  // Equivalent to `yield return null` in Unity
                AudioManager.Instance.source.loop = true;
                AudioManager.Instance.source.time = UnityEngine.Random.Range(AudioManager.Instance.source.clip.length * 0f, AudioManager.Instance.source.clip.length * 0.5f);
            }
            else
            {
                UnityEngine.Debug.LogError("Clip path not found in songPathsList!");
                Debug.LogError(clipPath);
            }
        }   

        // Final
        int selectedLevelIndex = Array.FindIndex(levels, level => level.isSelected);
        FindAnyObjectByType<mainMenu>().levelRow = selectedLevelIndex;
        StartCoroutine(LoadImage(Path.Combine(JammerDash.Main.gamePath, "levels", "extracted", customLevel.sceneData.ID + " - " + customLevel.sceneData.name, "bgImage.png")));
    }
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


// Method to update the difficulty slider
public IEnumerator UpdateDifficultySlider(float targetValue, float duration)
{
    float startValue = slider.value;
    float elapsedTime = 0f;

    while (elapsedTime < duration)
    {
        slider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    slider.value = targetValue;
    UpdateDifficultyColor(slider.value);  // You can call a method to update the slider's color
}
        public IEnumerator HandleButtonClick()
        {
            var customLevel = GetComponent<CustomLevelScript>();
            ButtonClickHandler[] levels = FindObjectsByType<ButtonClickHandler>(FindObjectsSortMode.InstanceID);
            if (customLevel != null)
            {
                desc = GameObject.Find("description").GetComponent<TextMeshProUGUI>();
                levelLength = GameObject.Find("info").GetComponent<Text>();
                levelBPM = GameObject.Find("infoBPM").GetComponent<Text>();
                diff = GameObject.Find("infodiff").GetComponent<Text>();
                levelObj = GameObject.Find("infoobj").GetComponent<Text>();
                hp = GameObject.Find("health").GetComponent<Text>();
                size = GameObject.Find("cubeSize").GetComponent<Text>();
                bonus = GameObject.Find("levelbonusinfoi").GetComponent<Text>();
                slider = GameObject.Find("sliderDiff").GetComponent<Slider>();
                new WaitUntil(() => GetComponent<leaderboard>().panelContainer != null);
                DestroyLeaderboard();
                DisplayLB(customLevel.sceneData.ID.ToString());

                // Display formatted save time
                bonus.text = GetFormattedBonusInfo(customLevel.sceneData);
            }

            if (isSelected)
            {
                PlayLevelAudioOrSFX();
            }
            float difficulty = 0f;
        
             List<Vector2> cubePositions = new List<Vector2>();
            List<Vector2> longCubePositions = new List<Vector2>();

            List<Vector2> sawPositions = new List<Vector2>();
            // Add cube positions from customLevel
            cubePositions.AddRange(customLevel.sceneData.cubePositions); 
            sawPositions.AddRange(customLevel.sceneData.sawPositions);
            longCubePositions.AddRange(customLevel.sceneData.longCubePositions);

                Debug.Log("Starting difficulty calculation...");

                // Call CalculateDifficultyAsync and await the result
                 yield return StartCoroutine(CalculateDifficultyCoroutine(customLevel, difficulty, cubePositions, sawPositions, longCubePositions));

            
        
        // Update UI with custom level data
        // After difficulty calculation, update the difficulty slider
        float duration = 0.1f;  // Duration for the lerp
        float masterPitch;
        Mods.instance.master.GetFloat("MasterPitch", out masterPitch);
        float elapsedTime = 0f;
        float startValue = slider.value * CustomLevelDataManager.Instance.scoreMultiplier * masterPitch;
        float targetValue = c * CustomLevelDataManager.Instance.scoreMultiplier * masterPitch;
        
        UpdateUIWithLevelData(customLevel.sceneData, targetValue);
        CustomLevelDataManager.Instance.diff = targetValue;

        // Start the difficulty slider update coroutine
        StartCoroutine(UpdateDifficultySlider(targetValue, duration));
        StartCoroutine(LerpButtonSize(buttonRectTransform, new Vector3(850f, 120f, 0f)));

        // Reset other buttons' selection
        StartCoroutine(DeselectOtherButtons());

        isSelected = true;
            yield return StartCoroutine(Move(5));

            button.onClick.RemoveAllListeners();
           
            if (customLevel.sceneData != null)
            {
                string clipPath = Path.Combine(JammerDash.Main.gamePath, "levels", "extracted", customLevel.sceneData.ID + " - " + customLevel.sceneData.name, customLevel.sceneData.artist + " - " + customLevel.sceneData.songName + ".mp3");
                int audioClipIndex = -1; // Initialize to a value that indicates no match found
                                         
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
                    AudioManager.Instance.source.loop = true;
                    AudioManager.Instance.source.time = UnityEngine.Random.Range(AudioManager.Instance.source.clip.length * 0f, AudioManager.Instance.source.clip.length * 0.5f);

                }
                else
                {
                    // Handle case where no match was found
                    UnityEngine.Debug.LogError(clipPath + " not found in songPathsList!");
                }
            }
            int selectedLevelIndex = Array.FindIndex(levels, level => level.isSelected);
            FindAnyObjectByType<mainMenu>().levelRow = selectedLevelIndex;
            yield return StartCoroutine(LoadImage(Path.Combine(JammerDash.Main.gamePath, "levels", "extracted", customLevel.sceneData.ID + " - " + customLevel.sceneData.name, "bgImage.png")));



        }
        public float c;
       private IEnumerator CalculateDifficultyCoroutine(CustomLevelScript customLevel, float difficulty, List<Vector2> cubePositions, List<Vector2> sawPositions, List<Vector2> longCubePositions)
{
    // Initialize variables for hp and size, default to 0 if parsing fails
    int hpValue = 0;
    int sizeValue = 0;

    // Extract numeric part of hp.text and size.text using a simple method (remove non-numeric characters)
    hpValue = ExtractNumberFromString(hp.text);
    sizeValue = ExtractNumberFromString(size.text);

    // Log the extracted values to verify
    Debug.Log("Extracted HP: " + hpValue);
    Debug.Log("Extracted Size: " + sizeValue);

    Task<float> difficultyTask = Task.Run(() => 
    {
        return Difficulty.Calculator.CalculateDifficultyAsync(
            cubes: cubePositions,  // Pass the appropriate cubes
            saws: sawPositions,   // Pass the appropriate saws
            longCubes: longCubePositions,  // Pass the appropriate long cubes
            hp: hpValue,  // Use the extracted health value     
            size: sizeValue,  // Use the extracted size value
            bpm: customLevel.sceneData.bpm,  // Use level BPM
            updateLoadingText: (text) => { Debug.Log(text); }, // Update loading text during the process
            levelTime: customLevel.sceneData.songLength, // Use level length
            cubePositions: customLevel.sceneData.cubePositions.ToArray(),
            longCubePositions: customLevel.sceneData.longCubePositions
        ).Result;  // Get the result of the task synchronously
    });

    // Wait until the task finishes
    while (!difficultyTask.IsCompleted)
    {
        yield return null;  // Yield control back to Unity to avoid blocking the main thread
    }

    // Now that the task is complete, get the result
    difficulty = difficultyTask.Result;
    c = difficulty;
    Debug.Log("Difficulty calculated: " + difficulty);
}

// Helper method to extract the numeric part of a string
private int ExtractNumberFromString(string input)
{
    // This method uses a regular expression to extract digits from the string.
    var number = new System.Text.RegularExpressions.Regex(@"[^0-9]");
    var result = number.Replace(input, "");  // Remove all non-numeric characters
    int parsedValue;
    // Try parsing the result, default to 0 if parsing fails
    if (int.TryParse(result, out parsedValue))
    {
        return parsedValue;
    }
    else
    {
        return 0;  // Default to 0 if parsing fails
    }
}
          public string ParseBBCode(string bbCodeText)
    {
        string parsedText = bbCodeText;

       // Basic Formatting
        parsedText = Regex.Replace(parsedText, @"\[b\](.*?)\[/b\]", "<b>$1</b>");
        parsedText = Regex.Replace(parsedText, @"\[i\](.*?)\[/i\]", "<i>$1</i>");
        parsedText = Regex.Replace(parsedText, @"\[u\](.*?)\[/u\]", "<u>$1</u>");
        parsedText = Regex.Replace(parsedText, @"\[s\](.*?)\[/s\]", "<s>$1</s>"); // Strikethrough

        // Text Color & Size
        parsedText = Regex.Replace(parsedText, @"\[color=(.*?)\](.*?)\[/color\]", "<color=$1>$2</color>");
        parsedText = Regex.Replace(parsedText, @"\[size=(.*?)\](.*?)\[/size\]", "<size=$1>$2</size>");

        // Alignment
        parsedText = Regex.Replace(parsedText, @"\[align=(left|center|right|justified)\](.*?)\[/align\]", "<align=$1>$2</align>");

        // Hyperlinks
        parsedText = Regex.Replace(parsedText, @"\[url=(.*?)\](.*?)\[/url\]", "<link=\"$1\">$2</link>");

        // Superscript & Subscript
        parsedText = Regex.Replace(parsedText, @"\[sup\](.*?)\[/sup\]", "<sup>$1</sup>");
        parsedText = Regex.Replace(parsedText, @"\[sub\](.*?)\[/sub\]", "<sub>$1</sub>");

        // Quotes & Indent
        parsedText = Regex.Replace(parsedText, @"\[quote\](.*?)\[/quote\]", "<indent=10%><i>$1</i></indent>");
        parsedText = Regex.Replace(parsedText, @"\[indent=(\d+)\](.*?)\[/indent\]", "<indent=$1>$2</indent>");

        // Image Support (TextMeshPro Sprites)
        parsedText = Regex.Replace(parsedText, @"\[img=(.*?)\]", "<sprite name=\"$1\">");

        // Line Height
        parsedText = Regex.Replace(parsedText, @"\[line-height=(\d+)\](.*?)\[/line-height\]", "<line-height=$1>$2</line-height>");

        // Centering (Custom workaround since TMP does not support <center>)
        parsedText = Regex.Replace(parsedText, @"\[center\](.*?)\[/center\]", "<align=center>$1</align>");

        return parsedText;
    }
        // Helper function to update UI with scene data
        private void UpdateUIWithLevelData(SceneData data, float difficulty)
        {
            levelLength.text = $"{FormatTime(data.songLength):N0}";
            float masterPitch;
            Mods.instance.master.GetFloat("MasterPitch", out masterPitch);
            desc.text = string.IsNullOrEmpty(data.description) ? "" : ParseBBCode(data.description);
            levelBPM.text = $"{data.bpm * masterPitch:F0} ({data.bpm:F0})";
            diff.text = $"{difficulty:F2}"; 	
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
            return $"<size=18>(from {data.source}) {data.artist} - {data.songName}</size>\n{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "mapped by")} {data.creator}\n{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "Local ID")}: {data.ID}, {LocalizationSettings.StringDatabase.GetLocalizedString("lang", "last saved on")} {EUTime:yyyy-MM-dd hh:MM:ss}";
        }

        // Handle playing the SFX or custom level audio
        private void PlayLevelAudioOrSFX()
        {
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
        private IEnumerator DeselectOtherButtons()
        {
            yield return new WaitForEndOfFrame();
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
            return Path.Combine(JammerDash.Main.gamePath, "levels", "extracted", data.name, $"{data.artist} - {data.songName}.mp3").Replace("/", "\\");
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
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    // Create a Sprite from the downloaded texture and set it to the Image component
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    levelImage.sprite = sprite;
                    new WaitForEndOfFrame();
                    StartCoroutine(AudioManager.Instance.ChangeSprite(url));
                }
                else
                {
                    StartCoroutine(FindFirstObjectByType<mainMenu>().LoadRandomBackground(null)); 
                    levelImage.sprite = null;
                    Debug.LogError(www.error);
                }
            }
        }
    }
}