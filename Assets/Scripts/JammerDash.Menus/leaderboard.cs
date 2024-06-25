using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using JammerDash.Menus.Play.Score;

namespace JammerDash.Menus.Play
{
    public class leaderboard : MonoBehaviour
    {
        public GameObject panelPrefab;
        public Transform panelContainer;

        void Start()
        {
            panelContainer = GameObject.Find("lb content").transform;
        }

        public void DisplayScores(string levelName)
        {
            // Constructing the file path
            string filePath = Path.Combine(Application.persistentDataPath, "scores.dat");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                UnityEngine.Debug.LogWarning("Rank data file does not exist.");
                return;
            }

            // Dictionary to store scores for the selected level
            Dictionary<string, List<string[]>> scoresDictionary = new Dictionary<string, List<string[]>>();

            // Reading the file line by line
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                // Splitting the line into components
                string[] data = line.Split(',');

                // Ensure that the data array has enough elements
                if (data.Length >= 10) // Ensure there are at least 10 elements in the array
                {
                    string level = data[2];
                    if (level == levelName)
                    {
                        string[] scoreData = new string[8];
                        scoreData[0] = data[1]; // Ranking
                        scoreData[1] = data[4]; // Score
                        scoreData[2] = data[3]; // Accuracy
                        scoreData[3] = data[5]; // Five
                        scoreData[4] = data[6]; // Three
                        scoreData[5] = data[7]; // One
                        scoreData[6] = data[8]; // Miss
                        scoreData[7] = data[9]; // Username

                        // Add the score data to the scores dictionary
                        if (!scoresDictionary.ContainsKey(levelName))
                        {
                            scoresDictionary[levelName] = new List<string[]>();
                        }
                        scoresDictionary[levelName].Add(scoreData);
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Invalid data format for line: " + line);
                }
            }

            // Clear existing panels
            foreach (Transform child in panelContainer)
            {
                Destroy(child.gameObject);
            }

            // Instantiate a panel for each score entry in the selected level
            foreach (string[] scoreData in scoresDictionary[levelName])
            {
                GameObject panel = Instantiate(panelPrefab, panelContainer);
                ScorePanel scorePanel = panel.GetComponent<ScorePanel>();

                // Display the ranking, score, accuracy, and other data
                string displayText = string.Format("Score: {0:N0}, Accuracy: {1}%\n", scoreData[1], scoreData[2]);
                string rankText = string.Format("{0}", scoreData[0]);
                string acc = string.Format("5: {0}\n3: {1}\n1: {2}\n0: {3}", scoreData[3], scoreData[4], scoreData[5], scoreData[6]);
                string user = scoreData[7];

                // Debug logs to ensure data is correct
                Debug.Log("Rank: " + rankText);
                Debug.Log("Score: " + displayText);
                Debug.Log("Accuracy breakdown: " + acc);
                Debug.Log("Username: " + user);

                scorePanel.rankText.text = rankText;
                scorePanel.scoreText.text = displayText;
                scorePanel.accuracyText.text = acc;
                scorePanel.username.text = user;
            }
        }
    }
}
