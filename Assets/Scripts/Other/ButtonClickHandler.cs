using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float maxSize = 1020f; // Maximum size of the button when centered
    public float minSize = 1000f; // Minimum size of the button when not centered
    public Image bg;
    private Button button;
    private RectTransform buttonRectTransform;
    public bool isSelected = false;
    public Text lvlInfo;
    public leaderboard lb;
    void Start()
    {
        button = GetComponent<Button>();
        buttonRectTransform = GetComponent<RectTransform>();
        button.onClick.AddListener(OnClick);
        button.onClick.RemoveAllListeners();
        scrollRect = GetComponentInParent<ScrollRect>();
    }
    void DisplayLB(string levelName)
    {
        // Constructing the file path
        string filePath = Path.Combine(Application.persistentDataPath, "scores.dat");

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Rank data file does not exist.");
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
                Debug.LogWarning("Invalid data format for line: " + line);
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
        if (button.onClick.GetPersistentEventCount() == 0)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
        if (Input.GetKeyDown(KeyCode.F4))
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
            lvlInfo = GameObject.Find("info").GetComponent<Text>();
            SceneData data = GetComponent<CustomLevelScript>().sceneData;
            lvlInfo.text = $"Length: {data.levelLength:N0} sec | BPM: {data.bpm} | Difficulty: {data.calculatedDifficulty:F2}sn\n{data.cubePositions.Count} cubes, {data.sawPositions.Count} saws, {data.longCubePositions.Count} long cubes\n\nClick Enter to start playing!";
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
                    Debug.LogWarning("hi2");
                    AudioManager.Instance.GetComponent<AudioSource>().loop = true;
                    AudioManager.Instance.GetComponent<AudioSource>().time = UnityEngine.Random.Range(AudioManager.Instance.GetComponent<AudioSource>().clip.length * 0f, AudioManager.Instance.GetComponent<AudioSource>().clip.length * 0.5f);

                }
                else
                {
                    // Handle case where no match was found
                    Debug.LogError("Clip path not found in songPathsList!");
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
                Debug.Log(bg);
                yield return new WaitForSecondsRealtime(0.2f);
                bg.sprite = sprite;
            }
            else
            {
                StartCoroutine(AudioManager.Instance.ChangeSprite());
            }
        }
    }
}
