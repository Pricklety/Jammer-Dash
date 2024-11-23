using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash.Difficulty
{

    public class Calculator : MonoBehaviour
    {

        // Method to calculate the average of all the 4th entry values
        public static float CalculateAccuracy(string filePath)
        {
            // List to store the 4th entries
            List<float> fourthEntries = new List<float>();

            try
            {
                // Read all lines from the file
                var rows = File.ReadAllLines(Path.Combine(Application.persistentDataPath, filePath));

                foreach (var row in rows)
                {
                    // Split the row by commas
                    var entries = row.Split(',');

                    // Ensure the row has at least 4 columns
                    if (entries.Length < 4) continue;

                    // Parse the 4th value (index 3)
                    if (float.TryParse(entries[3], out float fourthValue))
                    {
                        // Add the parsed value to the list
                        fourthEntries.Add(fourthValue);
                    }
                }

                // Calculate the average of the 4th entries
                if (fourthEntries.Count > 0)
                {
                    float average = fourthEntries.Average();
                    return average;
                }
                else
                {
                    Debug.LogWarning("No valid 4th entry values found.");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading or processing file: {ex.Message}");
                return 0;
            }
        }

        public static float CalculateSP(string filePath)
        {// List to store the highest adjusted third values for each row
            List<float> adjustedThirdValues = new List<float>();

            try
            {
                // Read all lines from the file
                var rows = File.ReadAllLines(Path.Combine(Application.persistentDataPath, filePath));

                // A dictionary to track the highest third entry (3rd column) per level ID (1st column)
                Dictionary<string, float> levelMaxThirdValues = new Dictionary<string, float>();

                foreach (var row in rows)
                {
                    // Split the row by commas
                    var entries = row.Split(',');

                    if (entries.Length < 3) continue; // Ensure at least three columns are present

                    // Get the level ID (1st column) and the 3rd column value
                    string levelId = entries[0];
                    if (!float.TryParse(entries[2], out float thirdValue) || float.IsNaN(thirdValue)) continue;

                    // Update the dictionary with the highest third value for each level ID
                    if (levelMaxThirdValues.ContainsKey(levelId))
                    {
                        // Keep the maximum third value for the same level ID
                        levelMaxThirdValues[levelId] = Math.Max(levelMaxThirdValues[levelId], thirdValue);
                    }
                    else
                    {
                        levelMaxThirdValues[levelId] = thirdValue;
                    }
                }

                // Now calculate the SP score using the decay formula for the top 50 values
                List<float> topValues = levelMaxThirdValues.Values.OrderByDescending(v => v).Take(50).ToList();
                float totalSP = 0f;

                // Apply the decay formula: Total sp = p * 0.96^(n-1)
                for (int i = 0; i < topValues.Count; i++)
                {
                    float p = topValues[i];
                    float adjustedValue = p * Mathf.Pow(0.96f, i); // Apply decay for each value
                    totalSP += adjustedValue;
                }

                return totalSP;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading or processing file: {ex.Message}");
                return 0;
            }
        }


        public static float CalculateDifficulty(
            List<GameObject> cubes,
            List<GameObject> saws,
            List<GameObject> longCubes,
            Slider hp,
            Slider size,
            int[] cubeCountsPerY,
            Vector2[] cubePositions,
            float clickTimingWindow,
            Action<string> updateLoadingText)
        {
            float difficulty = 0f;

            updateLoadingText("Starting difficulty calculation...");

            // Asynchronously retrieve objects with the specified tags
            GameObject[] foundObjects = FindObjectsWithTags(new string[] { "Cubes", "Saw", "LongCube" });

            if (foundObjects.Length == 0)
            {
                Debug.LogError("No target objects found.");
                return difficulty; // Exit with difficulty set to zero if no objects are found
            }

            // Find the farthest object
            Transform targetObject = FindFarthestObjectInX(foundObjects);
            if (targetObject == null)
            {
                Debug.LogError("No target object found.");
                return difficulty;
            }

            float distance = targetObject.position.x + 5;
            float theoreticalMaxDifficulty = 999f;
            float globalDifficultyFactor = 0.1f; // Adjust this factor based on overall scaling needs

            updateLoadingText("Calculating contributions...");

            // Iterate over each Y level
            for (int y = -1; y <= 4; y++)
            {
                int cubeCount = cubeCountsPerY[y + 1];
                if (cubeCount == 0) continue;

                // Reset difficulty for each Y level
                float yLevelDifficulty = 0f;

                for (int i = 0; i < cubeCount - 1; i++)
                {
                    // Perform calculations
                    float timingWindow = CalculateTimingWindow(cubePositions[i], cubePositions[i + 1], y);
                    float xPositionVariation = Mathf.Abs(cubePositions[i].x - cubePositions[i + 1].x);
                    float precisionFactor = CalculatePrecisionFactor(xPositionVariation);
                    float averageDistance = CalculateAverageCubeDistance(cubes);

                    updateLoadingText($"Processing Y level {y}: {cubeCount} cubes...");

                    // Contribution calculation
                    float divisor = (clickTimingWindow + distance != 0) ? clickTimingWindow + distance : float.Epsilon;

                    // Apply nerf fixes:
                    float cappedTimingWindow = Mathf.Clamp(timingWindow, 0f, 1f); // Limit timing window
                    float cappedPrecisionFactor = Mathf.Clamp(precisionFactor, 0f, 1f); // Limit precision factor

                    // Calculate contributions with respect to difficulty limits
                    float difficultyContribution = (
                        (cappedTimingWindow * 0.56f) + // Scale timing impact
                        (cubeCount / 60f) + // 60 cubes contribute up to 1
                        (cubes.Count / 40f) + // Scale for cubes (less impact)
                        (saws.Count / 6f) + // More impact from saws
                        (longCubes.Count / 90f) + // Consider long cubes
                        ((cappedPrecisionFactor + 1) / divisor) * 2f + // Adjusted precision factor
                        (1 / (hp.value + 1)) * 5f + // More weight to health impact
                        (Mathf.Exp(0.05f * (0.5f - size.value) * 10f) +
                        averageDistance / 2f) // Scale average distance
                    );

                    // Log contribution before adding it to the difficulty
                    Debug.Log($"Difficulty Contribution from Y level {y}, Cube {i}: {difficultyContribution}");

                    // Add to the Y level difficulty
                    yLevelDifficulty += difficultyContribution;
                }

                // After processing all cubes for this Y level, scale it with the global factor
                yLevelDifficulty *= globalDifficultyFactor;

                // Cap the Y level difficulty to prevent extreme values
                yLevelDifficulty = Mathf.Clamp(yLevelDifficulty, 0f, theoreticalMaxDifficulty);

                // Add to the overall difficulty
                difficulty += yLevelDifficulty;

                // Log final difficulty for this Y level
                Debug.Log($"Y level {y}: Current Difficulty Contribution: {yLevelDifficulty}");
            }

            // Cap the final difficulty if needed
            difficulty = Mathf.Clamp(difficulty, 0f, theoreticalMaxDifficulty); // Allow for values above 15

            // Log final difficulty
            Debug.Log("Finished difficulty calculation. Final difficulty: " + difficulty / 683);
            return difficulty / 683;
        }

        public static float CalculateTimingWindow(Vector2 position1, Vector2 position2, int yLevelDifference)
        {
            float yDistance = Mathf.Abs(position2.y - position1.y);
            return yDistance * 0.1f;
        }

        public static float CalculatePrecisionFactor(float xPositionVariation)
        {
            return Mathf.Clamp01(1 - xPositionVariation / 20f);
        }

        public static Task<int[]> CalculateCubesPerY(Vector2[] cubePositions, Action<string> updateLoadingText)
        {
            int[] cubesPerY = new int[6];
            foreach (Vector2 position in cubePositions)
            {
                int yLevel = Mathf.RoundToInt(position.y);
                yLevel = Mathf.Clamp(yLevel, -1, 4);
                cubesPerY[yLevel + 1]++;
                
            }
            return Task.FromResult(cubesPerY);
        }

        public static float CalculateAverageCubeDistance(List<GameObject> cubes)
        {
            if (cubes.Count < 2)
            {
                UnityEngine.Debug.Log("Not enough cubes in the list.");
                return 0;
            }

            float totalDistance = 0f;
            int numDistances = 0;
            for (int i = 0; i < cubes.Count; i++)
            {
                for (int j = i + 1; j < cubes.Count; j++)
                {
                    totalDistance += Vector3.Distance(cubes[i].transform.position, cubes[j].transform.position);
                    numDistances++;
                }
            }

            return totalDistance / numDistances;
        }

        public static Transform FindFarthestObjectInX(GameObject[] objects)
        {
            Transform farthestObject = null;
            float maxX = float.MinValue;

            foreach (GameObject obj in objects)
            {
                float x = obj.transform.position.x;
                if (x > maxX)
                {
                    maxX = x;
                    farthestObject = obj.transform;
                }
            }

            return farthestObject;
        }

        public static GameObject[] FindObjectsWithTags(string[] tags)
        {
            HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();
            foreach (string tag in tags)
            {
                GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject obj in objectsWithTag)
                {
                    uniqueObjects.Add(obj);
                }
            }
            return uniqueObjects.ToArray();
        }
    }


    public static class Object
    {
        public static async Task<GameObject[]> FindObjectsWithTags(string[] tags)
        {
            // Create a TaskCompletionSource to provide a Task that can be awaited
            var tcs = new TaskCompletionSource<GameObject[]>();

            // Enqueue the action to run on the main thread
            MainThreadDispatcher.Enqueue(() =>
            {
                HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

                foreach (string tag in tags)
                {
                    GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
                    foreach (GameObject obj in objectsWithTag)
                    {
                        uniqueObjects.Add(obj); // HashSet avoids duplicates
                    }
                }

                // Convert HashSet to array and set the result in TaskCompletionSource
                tcs.SetResult(uniqueObjects.ToArray());
            });

            // Return the Task to be awaited
            return await tcs.Task;
        }
    }

    public class ShinePerformance
    {
        private float _precision;
        private float _sequenceEfficiency;
        private float _versatility;
        private float _performanceScore;
        public float _levelLength;
        public int _perfectHits, _greatHits, _goodHits, _missedHits;
        public int _currentCombo, _bestCombo;
        private int _totalActions;
        private float _gameDifficulty;
        private int _cubeCount, _sawCount;
        private float _bpm;

        public ShinePerformance(int perfectHits, int greatHits, int goodHits, int missedHits, int currentCombo, int bestCombo, float gameDifficulty, float levelLength, int cubeCount, int sawCount, float bpm, float acc)
        {
            _perfectHits = perfectHits;
            _greatHits = greatHits;
            _goodHits = goodHits;
            _missedHits = missedHits;
            _currentCombo = currentCombo;
            _bestCombo = bestCombo;
            _totalActions = perfectHits + greatHits + goodHits + missedHits;
            _gameDifficulty = gameDifficulty;
            _levelLength = levelLength;
            _cubeCount = cubeCount;
            _sawCount = sawCount;
            _bpm = bpm;

            // Calculate performance components
            CalculateAccuracy(acc);
            CalculateSequenceEfficiency();
            CalculateVersatility();
            CalculatePerformanceScore();
        }

        // Calculate accuracy as the ratio of successful hits to total actions
        public void CalculateAccuracy(float acc)
        {
            // Since all the cubes are perfectly hit, accuracy is 100% and perfect hits = cube count
            _precision = acc / 100; // Directly applying the percentage accuracy here
        }

        // Calculate sequence efficiency based on combo performance
        private void CalculateSequenceEfficiency()
        {
            _sequenceEfficiency = _bestCombo != 0 ? Clamp((float)_currentCombo / _bestCombo + 1, 0, 1) : 0;
        }

        // Calculate versatility based on hit types and missed hits
        private void CalculateVersatility()
        {
            _versatility = _perfectHits * 0.07f + _greatHits * 0.04f + _goodHits * -0.04f + _missedHits * -0.15f;
        }

        // Exponential function for accuracy scaling
        private float CalculateAccuracyMultiplier(float accuracy)
        {
            // Use a constant 'k' to control how steeply the score drops with accuracy
            float k = 8f;  // You can adjust this to make the drop-off sharper or more gradual
            return (float)Math.Exp(-k * (1 - accuracy));
        }

        // Calculate the performance score considering accuracy, sequence efficiency, versatility, and difficulty
        private void CalculatePerformanceScore()
        {
            // Difficulty-based scaling
            float difficultyMultiplier = 1 + (_gameDifficulty / 10f);  // Scales with difficulty, e.g., 1.16 becomes 1.116 multiplier

            // Cube interaction weight
            float c = 5f; // Each hit contributes 5 points
                          // Saw obstacle weight
            float s = 20f;

            // BPM scaling (considered inversely)
            float bpmScaling = 1 / _bpm;

            // Adjusted accuracy multiplier
            float accuracyMultiplier = CalculateAccuracyMultiplier(_precision);

            // Level length adjustment (bonus for long levels)
            float levelLengthAdjustment = _levelLength / Math.Max(1000, _levelLength); 

            // Calculate the total performance score
            _performanceScore = Math.Max(
                accuracyMultiplier *  // Apply accuracy-based scaling
                (MathF.Pow(_precision, 2) * 1.24f +  // Accuracy weight
                _sequenceEfficiency * 10 +
                _versatility * 5 +
                (levelLengthAdjustment) +  // Adjust for long levels (further nerfed)
                (_gameDifficulty * 0.35f) +
                (c * _cubeCount) +  // Cube count effect, 5 points per hit
                (s * _sawCount)    // Saw obstacles effect
                ) * difficultyMultiplier * bpmScaling, 0f);  // Apply difficulty and BPM scaling
        }

        // Property to get the final performance score
        public float PerformanceScore => _performanceScore;

        // Utility method to clamp values within a given range
        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }

}