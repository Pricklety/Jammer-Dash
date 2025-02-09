using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JammerDash.Menus.Play
{

    public class RankDisplay : MonoBehaviour
    {
        public Image rankText;
        public int sceneIndex;
        public string keyName;

        private void Start()
        {
            // Accessing the parent GameObject's name as the levelName
            string levelName = GetComponentInChildren<Text>().text;
            DisplayRank(levelName);
        }

        private void DisplayRank(string levelName)
        {
            if (GetComponent<CustomLevelScript>() != null)
            {
                CustomLevelScript script = GetComponent<CustomLevelScript>();
                levelName = script.sceneData.ID.ToString();
            }
            else
            {
                levelName = sceneIndex.ToString();
            }

            // Constructing the file path
            string filePath = Path.Combine(JammerDash.Main.gamePath, "scores.dat");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                return;
            }

            // Variables to store highest rank data found
            string highestRankData = null;
            float highestRank = int.MinValue; // Initialize with lowest possible value

            // Reading the file line by line
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                // Splitting the line into components
                string[] data = line.Split(',');

                // Ensure that the data array has enough elements
                if (data.Length >= 4) // Ensure there are at least 6 elements in the array
                {
                    if (data[0] == levelName)
                    {
                        // Parse the rank value
                        float rank;
                        if (float.TryParse(data[4], out rank))
                        {
                            // If the current rank is higher than the highest rank found so far
                            if (rank > highestRank)
                            {
                                highestRank = rank;
                                highestRankData = line;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid rank data format for level: " + levelName);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Invalid data format for line: " + line);
                }
            }


            // If any rank data for the level ID was found
            if (highestRankData != null)
            {
                // Displaying the highest rank data in the UI
                string[] highestRankDataArray = highestRankData.Split(',');
                rankText.sprite = Resources.Load<Sprite>($"ranking/{highestRankDataArray[1]}");
            }
        }

    }

}