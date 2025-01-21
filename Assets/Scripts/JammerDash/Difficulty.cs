using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JammerDash.Game;
using JammerDash.Tech;
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
                return 0;
            }
        }

        public static PlayerStats CalculateOtherPlayerInfo(string filePath)
        {
            PlayerStats stats = new PlayerStats
            {
                RankCounts = new Dictionary<string, int>
            {
                {"SS+", 0}, {"SS", 0}, {"S", 0}, {"A", 0}, {"B", 0}, {"C", 0}, {"D", 0}, {"F", 0}
            }
            };

            try
            {
                // Read all lines from the file
                var rows = File.ReadAllLines(Path.Combine(Application.persistentDataPath, filePath));

                stats.TotalPlays = rows.Length;

                foreach (var row in rows)
                {
                    // Split the row by commas
                    var entries = row.Split(',');

                    // Ensure the row has at least 11 columns
                    if (entries.Length < 11) continue;

                    // Parse and count Rank (index 1)
                    string rank = entries[1].Trim();
                    if (stats.RankCounts.ContainsKey(rank))
                    {
                        stats.RankCounts[rank]++;
                    }

                    // Parse and accumulate Total Score (index 4)
                    if (float.TryParse(entries[4], out float score))
                    {
                        stats.TotalScore += score;
                    }

                    // Parse and accumulate Amount of Hits
                    if (int.TryParse(entries[5], out int perfect)) stats.TotalPerfect += perfect;
                    if (int.TryParse(entries[6], out int great)) stats.TotalGreat += great;
                    if (int.TryParse(entries[7], out int okay)) stats.TotalOkay += okay;
                    if (int.TryParse(entries[8], out int misses)) stats.TotalMisses += misses;

                    // Determine the Highest Combo (index 9)
                    if (int.TryParse(entries[9], out int combo))
                    {
                        if (combo > stats.HighestCombo) stats.HighestCombo = combo;
                    }
                }
            }
            catch (Exception ex)
            {
                return new PlayerStats();
            }

            return stats;
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
       public static Task<float> CalculateDifficultyAsync(
    List<GameObject> cubes,
    List<GameObject> saws,
    List<GameObject> longCubes,
    Slider hp,
    Slider size,
    Vector2[] cubePositions,
    float bpm,
    Action<string> updateLoadingText)
{
    try
    {
        float difficulty = 0f;

        updateLoadingText("Starting section-based difficulty calculation...");
        Debug.Log("Starting section-based difficulty calculation...");
        List<float> sectionDifficulties = new List<float>();

        // Split the level into sections based on X axis (or other factors if needed)
        float sectionLength = 70f;
        int totalSections = Mathf.CeilToInt(cubes.Count / sectionLength);

        for (int sectionIndex = 0; sectionIndex < totalSections; sectionIndex++)
        {
            updateLoadingText($"Calculating difficulty for section {sectionIndex + 1} of {totalSections}...");
            Debug.Log($"Calculating difficulty for section {sectionIndex + 1} of {totalSections}...");

            float sectionDifficulty = CalculateSectionDifficulty(
                cubes,
                saws,
                longCubes,
                cubePositions,
                bpm,
                sectionIndex,
                sectionLength
            );

            sectionDifficulties.Add(sectionDifficulty);
            Debug.Log($"Section {sectionIndex} difficulty: {sectionDifficulty}");
        }

        foreach (var section in sectionDifficulties)
        {
            difficulty += section;
        }

        // Apply additional impacts
        difficulty += CalculateHealthImpact(hp);
        difficulty += CalculateSizeImpact(size);

        // Scale difficulty based on BPM
        difficulty *= Mathf.Clamp(bpm * 0.1f, 1f, 5f); // Adjust the scale with BPM for reasonable results

        Debug.Log($"Final difficulties: {difficulty} / 30: {difficulty / 30}");

        // Apply final scaling to match desired difficulty progression
        if (difficulty < 0.01f) difficulty = 0.01f;

        return Task.FromResult(difficulty / 30);
    }
    catch (Exception e)
    {
        Debug.LogError(e);
        return Task.FromResult(0f);
    }
}
 public static float CalculateSectionDifficulty(
            List<GameObject> cubes,
            List<GameObject> saws,
            List<GameObject> longCubes,
            Vector2[] cubePositions,
            float bpm,
            int sectionIndex,
            float sectionLength)
        {
            float sectionDifficulty = 0f;

            // Calculate the cube density and how hard it is to move between them (Y-Axis and X-Axis)
            GameObject[] sectionCubes = cubes.Where(cube => IsCubeInSection(cube.transform.position, sectionIndex, sectionLength)).ToArray();
            sectionDifficulty += CalculateCubeDensity(sectionCubes, cubePositions);

            // Consider saws impact in this section
            List<GameObject> sectionSaws = saws.Where(saw => IsObjectInSection(saw.transform.position, sectionIndex, sectionLength)).ToList();
            sectionDifficulty += CalculateSawImpact(sectionSaws);

            // Add impact from long cubes
            List<GameObject> sectionLongCubes = longCubes.Where(longCube => IsObjectInSection(longCube.transform.position, sectionIndex, sectionLength)).ToList();
            sectionDifficulty += CalculateLongCubeImpact(sectionLongCubes);

           

            return sectionDifficulty;
        }
  public static bool IsCubeInSection(Vector3 position, int sectionIndex, float sectionLength)
        {
            return position.x >= sectionIndex * sectionLength && position.x < (sectionIndex + 1) * sectionLength;
        }

        public static bool IsObjectInSection(Vector3 position, int sectionIndex, float sectionLength)
        {
            return position.x >= sectionIndex * sectionLength && position.x < (sectionIndex + 1) * sectionLength;
        }
public static float CalculateCubeDensity(GameObject[] sectionCubes, Vector2[] cubePositions)
{
    float density = 0f;

    // First, calculate the X-axis proximity and Y-axis activity
    List<float> xDistances = new List<float>();  // Store distances between cubes on the X-axis
    List<float> yDifferences = new List<float>(); // Store Y differences between cubes

    // Collect all X and Y distances between cubes
    for (int i = 0; i < sectionCubes.Length; i++)
    {
        for (int j = i + 1; j < sectionCubes.Length; j++)
        {
            // Calculate horizontal distance (X-axis)
            float xDistance = Mathf.Abs(sectionCubes[i].transform.position.x - sectionCubes[j].transform.position.x);
            xDistances.Add(xDistance);

            // Calculate vertical distance (Y-axis)
            float yDifference = Mathf.Abs(sectionCubes[i].transform.position.y - sectionCubes[j].transform.position.y);
            yDifferences.Add(yDifference);
        }
    }

    // Calculate proximity weight based on X-axis distances
    float averageXDistance = xDistances.Count > 0 ? xDistances.Average() : 1f; // Avoid division by zero
    float proximityWeight = Mathf.Clamp(1f - (averageXDistance / 2f), 0f, 1f); // Closer cubes, higher weight

    // Calculate Y-axis activity
    float averageYDifference = yDifferences.Count > 0 ? yDifferences.Average() : 0f;
    float yActivityWeight = Mathf.Clamp(averageYDifference / 10f, 0f, 1f); // More Y variation, higher weight

    // Combine proximity and Y-axis activity into the density calculation
    density += proximityWeight * (1f + yActivityWeight); // Higher density with both smaller X distances and larger Y differences

    // Return the final density
    return density;
}


public static float CalculateSawImpact(List<GameObject> sectionSaws)
{
    float sawImpact = 0f;
    int sawCount = sectionSaws.Count;

    if (sawCount < 2) return sawImpact;

    float totalYDistance = 0f;
    int pairCount = 0;

    // Calculate the average Y-distance between saws
    sectionSaws.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y)); // Sort by Y

    for (int i = 0; i < sawCount - 1; i++)
    {
        float yDistance = Mathf.Abs(sectionSaws[i].transform.position.y - sectionSaws[i + 1].transform.position.y);
        totalYDistance += yDistance;
        pairCount++;
    }

    float averageYDistance = totalYDistance / pairCount;

    // The more spaced out the saws are, the higher the difficulty
    sawImpact = Mathf.Clamp(1 / (averageYDistance + 0.1f), 0f, 5f);

    return sawImpact;
}

public static float CalculateLongCubeImpact(List<GameObject> sectionLongCubes)
{
    float longCubeImpact = 0f;

    // Long cubes add more difficulty to the section
    foreach (GameObject longCube in sectionLongCubes)
    {
        longCubeImpact += 1f; // Adjust this number based on the impact you want
    }

    return longCubeImpact;
}

public static float CalculateHealthImpact(Slider hp)
{
    // Health impact increases as the slider moves towards 0
    return 1 / (hp.value + 1) * 5f; // Arbitrary scaling factor
}

public static float CalculateSizeImpact(Slider size)
{
    // Size impact is exponential, more impact as the size increases
    return Mathf.Exp(0.05f * (1f - size.value) * 2f); // Exponential scaling for difficulty
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
        private float GetDifficultyExponent()
        {
            if (_gameDifficulty <= 1)  // Lower difficulties (0*, 1*)
            {
                return 0.3f;
            }
            else if (_gameDifficulty <= 3)  // Medium difficulties (2*, 3*)
            {
                return 0.5f; 
            }
            else  // Higher difficulties (4*+)
            {
                return 0.8f;
            }
        }
        // Calculate versatility based on hit types and missed hits
        private void CalculateVersatility()
        {
            // Calculate the exponential adjustment based on difficulty, clamped to avoid too small values
             float expAdjustment = Mathf.Exp(_gameDifficulty * GetDifficultyExponent()) - 1;
           

            _versatility = _perfectHits * expAdjustment * 0.41f +
                   _greatHits * expAdjustment * -0.1f +
                   _goodHits * expAdjustment * -0.54f +
                   _missedHits * expAdjustment * -4.45f;
        }


        // Exponential function for accuracy scaling
        private float CalculateAccuracyMultiplier(float accuracy)
        {
            
            float k = 12f; 
            return (float)Math.Exp(-k * (1 - accuracy));
        }

        // Calculate the performance score considering accuracy, sequence efficiency, versatility, and difficulty
        private void CalculatePerformanceScore()
        {
           
float masterPitch;
            Mods.instance.master.GetFloat("MasterPitch", out masterPitch);
            // BPM scaling (considered inversely)
            float bpmScaling = 1 / _bpm;

            // Adjusted accuracy multiplier
            float accuracyMultiplier = CalculateAccuracyMultiplier(_precision);

            // Level length adjustment (bonus for long levels)
            float levelLengthAdjustment = _levelLength / Math.Max(1000, _levelLength);

            // Calculate the total performance score
            _performanceScore = Math.Max(
      accuracyMultiplier * 
      (MathF.Pow(_precision, 2) * 0.98f +  
      _sequenceEfficiency * 10 +
      _versatility * 5 +
      levelLengthAdjustment +
      (MathF.Exp(_gameDifficulty * GetDifficultyExponent()) - 1) * 3f +  
      (_cubeCount % 150)
      ) * bpmScaling, 0f) * CustomLevelDataManager.Instance.scoreMultiplier * masterPitch;  // Apply BPM scaling

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
namespace JammerDash
{
    public class PlayerStats
    {
        public int TotalPlays { get; set; }
        public Dictionary<string, int> RankCounts { get; set; }
        public float TotalScore { get; set; }
        public int TotalPerfect { get; set; }
        public int TotalGreat { get; set; }
        public int TotalOkay { get; set; }
        public int TotalMisses { get; set; }
        public int HighestCombo { get; set; }
    }
}