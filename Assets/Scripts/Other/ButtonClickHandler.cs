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

namespace JammerDash.Menus.Play
{
    public class ButtonClickHandler : MonoBehaviour
    {
        public ScrollRect scrollRect;
        public float maxSize = 1020f; // Maximum size of the button when centered
        public float minSize = 1000f; // Minimum size of the button when not centered
        public Image bg;
        public Image levelImage;
        private Button button;
        private RectTransform buttonRectTransform;
        public bool isSelected = false;
        public Text levelLength;
        public Text levelBPM;
        public Text diff;
        public Text levelObj;
        public Text bonus;  
        public leaderboard lb;
        void Start()
        {
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
                if (data.Length >= 5) // Ensure there are at least 5 elements in the array
                {
                    string level = data[0];
                    if (level == levelName)
                    {
                        string[] scoreData = new string[3];
                        scoreData[0] = data[1]; // Ranking
                        scoreData[1] = data[4]; // Score
                        scoreData[2] = data[3]; // Accuracy

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

                // Display each ranking, score, and accuracy
                string displayText = string.Format("Score: {0:N0}, Accuracy: {1:F2}%", Convert.ToInt64(scoreData[1]), Convert.ToSingle(scoreData[2]));

                scorePanel.scoreText.text = displayText;
                scorePanel.rankText.text = scoreData[0];
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
            if (Input.GetKeyDown(KeyCode.F4) && isSelected)
            {
                StartCoroutine(Move(0));
            }
            // Calculate the button's distance from the center of the scroll view
            float distanceFromCenter = Mathf.Abs(-buttonRectTransform.localPosition.y - scrollRect.content.localPosition.y - 485);

            // Calculate the ratio of distance from center to the total scroll view height
            float ratio = distanceFromCenter / (scrollRect.content.rect.height - 2);

            // Calculate the size of the button based on the ratio
            float newSize = Mathf.Lerp(300, 820, 1 - ratio); // Buttons closer to the center will have larger sizes

            // Set the size of the button
            buttonRectTransform.sizeDelta = new Vector2(newSize, buttonRectTransform.sizeDelta.y);
            if (isSelected && Input.GetKeyDown(KeyCode.Return))
            {
                if (gameObject.GetComponent<CustomLevelScript>() == null)
                {
                    FindFirstObjectByType<mainMenu>().OpenLevel(GetComponent<RankDisplay>().sceneIndex);
                }
                else
                {
                    // Play custom level
                    this.GetComponent<CustomLevelScript>().PlayLevel();
                }
            }
        }

        public void Change()
        {
            if (GetComponent<CustomLevelScript>().sceneData.picLocation != null && isSelected)
            {
                StartCoroutine(LoadImage(GetComponent<CustomLevelScript>().sceneData.picLocation));
            }
        }
        IEnumerator Move(float lerpSpeed)
        {
            if (lerpSpeed != 0)
            {
                lerpSpeed = 0f;
            }
            Vector3 buttonWorldPos = button.transform.position;
            Vector3 topObjectWorldPos = new Vector3(0, -1, 0);

            Vector3 buttonLocalPos = scrollRect.content.parent.InverseTransformPoint(buttonWorldPos);
            Vector3 topObjectLocalPos = scrollRect.content.parent.InverseTransformPoint(topObjectWorldPos);

            float yOffset = topObjectLocalPos.y - buttonLocalPos.y + 72.5f;
            Vector2 targetPosition = new Vector2(scrollRect.content.localPosition.x, scrollRect.content.localPosition.y + yOffset);
            while (lerpSpeed < 0.25f)
            {
                lerpSpeed += Time.unscaledDeltaTime;
                scrollRect.content.localPosition = Vector2.Lerp(scrollRect.content.localPosition, targetPosition, lerpSpeed);
                yield return null;
            }
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
        public IEnumerator HandleButtonClick()
        {
            if (GetComponent<CustomLevelScript>() != null)
            {
                DestroyLeaderboard();
                DisplayLB(GetComponent<CustomLevelScript>().sceneData.ID.ToString());
                levelLength = GameObject.Find("info").GetComponent<Text>();
                levelBPM = GameObject.Find("infoBPM").GetComponent<Text>();
                diff = GameObject.Find("infodiff").GetComponent<Text>();
                levelObj = GameObject.Find("infoobj").GetComponent<Text>();
                bonus = GameObject.Find("levelbonusinfoi").GetComponent<Text>();
                SceneData data = GetComponent<CustomLevelScript>().sceneData;
                levelLength.text = $"{FormatTime(data.levelLength):N0}";
                levelBPM.text = $"{data.bpm}";
                diff.text = $"{data.calculatedDifficulty:F2}";
                levelObj.text = $"{(data.cubePositions.Count + data.sawPositions.Count + data.longCubePositions.Count):N0} ({data.cubePositions.Count}, {data.sawPositions.Count}, {data.longCubePositions.Count})";
                long unixTime = Convert.ToInt64(data.saveTime);
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToUniversalTime();

                TimeZoneInfo timeZone = TimeZoneInfo.Local;
                DateTimeOffset EUTime = TimeZoneInfo.ConvertTime(dateTimeOffset, timeZone);
                string formattedEUTime = EUTime.ToString("yyyy-MM-dd hh:MM:ss"); 
                bonus.text = $"mapper: {data.creator}, ID: {data.ID}, last saved on {formattedEUTime}";
            }
            if (isSelected)
            {
                if (gameObject.GetComponent<CustomLevelScript>() == null)
                {
                    FindFirstObjectByType<mainMenu>().OpenLevel(GetComponent<RankDisplay>().sceneIndex);
                }
                else
                {
                    // Play custom level
                    this.GetComponent<CustomLevelScript>().PlayLevel();
                }
            }
            // Reset other buttons' selection
            ButtonClickHandler[] buttons = scrollRect.content.GetComponentsInChildren<ButtonClickHandler>();
            foreach (ButtonClickHandler otherButton in buttons)
            {
                if (otherButton != this)
                {
                    otherButton.isSelected = false;
                }
            }

            yield return StartCoroutine(Move(0));

            button.onClick.RemoveAllListeners();
            if (GetComponent<CustomLevelScript>() != null)
            {
                if (GetComponent<CustomLevelScript>().sceneData.clipPath != null)
                {
                    string clipPath = GetComponent<CustomLevelScript>().sceneData.clipPath;
                    int audioClipIndex = -1; // Initialize to a value that indicates no match found

                    // Normalize the clipPath
                    clipPath = Path.GetFullPath(clipPath);

                    // Iterate through the songPathsList
                    for (int i = 0; i < AudioManager.Instance.songPathsList.Count; i++)
                    {
                        // Normalize the current path in songPathsList
                        string normalizedSongPath = Path.GetFullPath(AudioManager.Instance.songPathsList[i]);

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
                        AudioManager.Instance.GetComponent<AudioSource>().loop = true;
                        AudioManager.Instance.GetComponent<AudioSource>().time = UnityEngine.Random.Range(AudioManager.Instance.GetComponent<AudioSource>().clip.length * 0f, AudioManager.Instance.GetComponent<AudioSource>().clip.length * 0.5f);

                    }
                    else
                    {
                        // Handle case where no match was found
                        UnityEngine.Debug.LogError("Clip path not found in songPathsList!");
                    }
                }

                if (GetComponent<CustomLevelScript>().sceneData.picLocation != null && GetComponent<CustomLevelScript>() != null)
                {
                    yield return StartCoroutine(LoadImage(GetComponent<CustomLevelScript>().sceneData.picLocation));
                }

            }


            // Toggle selection
            isSelected = !isSelected;
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
                    UnityEngine.Debug.Log(bg);
                    yield return new WaitForSecondsRealtime(0.2f);
                    bg.sprite = sprite;
                    levelImage.sprite = sprite;
                }
                else
                {
                    levelImage.sprite = Resources.Load<Sprite>("backgrounds/basic/basic.png");
                    StartCoroutine(AudioManager.Instance.ChangeSprite());
                }
            }
        }
    }
}