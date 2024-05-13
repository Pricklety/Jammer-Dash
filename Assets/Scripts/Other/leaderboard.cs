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
                if (data.Length >= 5) // Ensure there are at least 5 elements in the array
                {
                    string level = data[2];
                    if (level == levelName)
                    {
                        string[] scoreData = new string[3];
                        scoreData[0] = data[1]; // Ranking
                        scoreData[1] = data[4]; // Score
                        scoreData[2] = data[3]; // Accuracy

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

            // Instantiate a panel for the selected level
            foreach (string levelName2 in scoresDictionary.Keys)
            {
                GameObject panel = Instantiate(GetComponent<leaderboard>().panelPrefab, GetComponent<leaderboard>().panelContainer);
                ScorePanel scorePanel = panel.GetComponent<ScorePanel>();

                // Display each ranking, score, and accuracy
                string displayText = "";
                string rankText = "";
                foreach (string[] scoreData in scoresDictionary[levelName2])
                {
                    displayText = string.Format("Score: {0:N0}, Accuracy: {1}%\n", scoreData[1], scoreData[2]);
                    rankText = string.Format("{0}", scoreData[0]);
                }
                scorePanel.rankText.text = rankText;
                scorePanel.scoreText.text = displayText;
            }
        }
    }
}