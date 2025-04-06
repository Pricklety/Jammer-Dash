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
                var rows = File.ReadAllLines(Path.Combine(Main.gamePath, filePath));

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
                var rows = File.ReadAllLines(Path.Combine(Main.gamePath, filePath));

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
{
    // List to store the highest adjusted third values for each row
    List<float> adjustedThirdValues = new List<float>();

    try
    {
        // Read all lines from the file
        var rows = File.ReadAllLines(Path.Combine(Main.gamePath, filePath));

        // A dictionary to track the highest third entry (3rd column) per level ID (1st column)
        Dictionary<string, float> levelMaxThirdValues = new Dictionary<string, float>();

        foreach (var row in rows)
        {
            // Split the row by commas
            var entries = row.Split(',');

            if (entries.Length < 12) continue; // Ensure at least twelve columns are present

            // Get the level ID (1st column) and the 3rd column value
            string levelId = entries[0];

            // Extract mod string and check if it contains any unranked mod
            string modString = entries[11];
            if (modString.Contains(ModType.random.ToString()) ||
                modString.Contains(ModType.autoMove.ToString()) ||
                modString.Contains(ModType.auto.ToString()))
            {
                continue; // Skip unranked plays
            }

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
    public static bool IsObjectInSection(Vector2 pos, int index, float length)
    {
        return pos.x >= index * length && pos.x < (index + 1) * length;
    }

  

 public static Task<float> CalculateDifficultyAsync(
    List<Vector2> cubes,
    List<Vector2> saws,
    List<Vector2> longCubes,
    int hp,
    int size,
    Vector2[] cubePositions,
    List<Vector2> longCubePositions,
    float bpm,
    float levelTime,
    Action<string> updateLoadingText)
{
    try
{
    updateLoadingText("Starting section-based difficulty calculation...");
    List<float> sectionDifficulties = new List<float>();

    float sectionLength = 70f;
    int totalSections = Mathf.CeilToInt(cubes.Count / sectionLength);

    // Difficulty contribution weights
    float strainWeight = 0.45f;
    float spamWeight = 0.35f;
    float longCubeWeight = 0.65f;
    float globalDampening = 0.8f;

    for (int sectionIndex = 0; sectionIndex < totalSections; sectionIndex++)
    {
        updateLoadingText($"Calculating section {sectionIndex + 1} of {totalSections}...");

        Vector2[] sectionCubes = cubes
            .Where(c => IsObjectInSection(c, sectionIndex, sectionLength))
            .ToArray();

        List<Vector2> sectionLongCubes = longCubes
            .Where(c => IsObjectInSection(c, sectionIndex, sectionLength))
            .ToList();

        float strain = CalculateStrain(sectionCubes.ToList());
        float spam = CalculateSpamFactor(sectionCubes.ToList());
        float longImpact = CalculateLongCubeImpact(sectionLongCubes) + CalculateLongCubeMetaBuffs(sectionLongCubes);

        float sectionDifficulty = (strain * strainWeight) + (spam * spamWeight) + (longImpact * longCubeWeight);
        sectionDifficulties.Add(sectionDifficulty);
    }

    float averageSectionDifficulty = sectionDifficulties.Count > 0 ? sectionDifficulties.Average() : 0f;

    // Global modifiers
    float hpPenalty = CalculateHealthImpact(hp);
    float sizePenalty = CalculateSizeImpact(size);
    float bpmScaling = bpm * 0.002f;

    float finalDifficulty = (averageSectionDifficulty + hpPenalty + sizePenalty + bpmScaling) * globalDampening;


        // Slight global dampening
        finalDifficulty *= 1.3f;

        return Task.FromResult(finalDifficulty);
    }
    catch (Exception e)
    {
        Debug.LogError(e);
        return Task.FromResult(0.01f);
    }
}

    public static float CalculateStrain(List<Vector2> cubes)
    {
        if (cubes == null || cubes.Count < 2) return 0f;

        cubes = cubes.OrderBy(c => c.x).ToList();
        const float decayBase = 0.9f;
        float lastStrain = 0f;
        float peakStrain = 0f;
        float strainSum = 0f;
        float weight = 1f;
        float totalWeight = 0f;

        for (int i = 1; i < cubes.Count; i++)
        {
            float dx = Mathf.Max(0.1f, cubes[i].x - cubes[i - 1].x);
            float dy = Mathf.Abs(cubes[i].y - cubes[i - 1].y);
            float strain = dy / dx;

            lastStrain *= Mathf.Pow(decayBase, dx / 2f);
            lastStrain += strain;

            strainSum += lastStrain * weight;
            peakStrain = Mathf.Max(peakStrain, lastStrain);
            weight *= 0.98f;
            totalWeight += weight;
        }

        float avgStrain = strainSum / Mathf.Max(1f, totalWeight);
        return avgStrain * 0.6f + peakStrain * 0.4f;
    }

    // Spam Detection (tight objects in same lane)
    public static float CalculateSpamFactor(List<Vector2> cubes)
    {
        float spamFactor = 0f;
        int streak = 1;
        cubes = cubes.OrderBy(c => c.x).ToList();

        for (int i = 1; i < cubes.Count; i++)
        {
            float dx = cubes[i].x - cubes[i - 1].x;
            bool sameLane = Mathf.Approximately(cubes[i].y, cubes[i - 1].y);

            if (sameLane && dx < 1f)
            {
                streak++;
                spamFactor += streak * 1.5f;
            }
            else if (sameLane && dx < 2.5f)
            {
                streak++;
                spamFactor += streak * 0.2f;
            }
            else
            {
                streak = 1;
            }
        }

        return Mathf.Clamp(spamFactor, 0f, 3f);
    }

    // Long Note Arrangement Impact
    public static float CalculateLongCubeImpact(List<Vector2> sectionLongCubes)
{
    float impact = 0f;

    if (sectionLongCubes.Count < 4)
    {
        impact = sectionLongCubes.Count * 0.4f;  // Slight nerf
    }
    else
    {
        impact = sectionLongCubes.Count * 0.6f;  // Nerfed from 1.0x
        List<float> xDistances = new List<float>();
        for (int i = 0; i < sectionLongCubes.Count; i++)
        {
            for (int j = i + 1; j < sectionLongCubes.Count; j++)
            {
                float xDist = Mathf.Abs(sectionLongCubes[i].x - sectionLongCubes[j].x);
                float yDiff = Mathf.Abs(sectionLongCubes[i].y - sectionLongCubes[j].y);

                if (xDist < 5f && yDiff > 1f)
                {
                    impact *= 1.05f;  // Slightly softer buff
                }
            }
        }
    }

    return impact;
}


    // Additional meta buffs for long notes (lane variety and bursts)
   public static float CalculateLongCubeMetaBuffs(List<Vector2> longCubePositions)
{
    if (longCubePositions == null || longCubePositions.Count == 0) return 0f;

    longCubePositions = longCubePositions.OrderBy(c => c.x).ToList();

    var lanesUsed = new HashSet<int>(longCubePositions.Select(p => (int)p.y));
    float diversityBuff = lanesUsed.Count / 4f;

    float spamBuff = 0f;
    int streak = 1;

    for (int i = 1; i < longCubePositions.Count; i++)
    {
        float dx = longCubePositions[i].x - longCubePositions[i - 1].x;
        bool sameLane = Mathf.Approximately(longCubePositions[i].y, longCubePositions[i - 1].y);
        if (sameLane && dx < 120f)
        {
            streak++;
            spamBuff += streak * 0.025f;
        }
        else
        {
            streak = 1;
        }
    }

    spamBuff = Mathf.Clamp(spamBuff, 0f, 1.2f);

    float baseLongCubeCount = longCubePositions.Count;
    return baseLongCubeCount * (0.07f * diversityBuff + 0.035f * spamBuff);  // Nerfed
}

    // Health affects difficulty (lower HP = harder)
    public static float CalculateHealthImpact(int hp)
    {
        return Mathf.Clamp(1f / (hp + 1f), 0.2f, 2f);
    }

    // Size affects visibility and spacing
    public static float CalculateSizeImpact(int size)
    {
        return Mathf.Exp(0.04f * (1f - size) * 1.10f);
    }
    public static float CalculateCubeDensity(Vector2[] sectionCubes)
{
    float density = 0f;
    List<float> xDistances = new List<float>();
    List<float> yDifferences = new List<float>();

    for (int i = 0; i < sectionCubes.Length; i++)
    {
        for (int j = i + 1; j < sectionCubes.Length; j++)
        {
            float xDist = Mathf.Abs(sectionCubes[i].x - sectionCubes[j].x);
            float yDiff = Mathf.Abs(sectionCubes[i].y - sectionCubes[j].y);
            xDistances.Add(xDist);
            yDifferences.Add(yDiff);
        }
    }

    float avgX = xDistances.Count > 0 ? xDistances.Average() : 1f;
    float avgY = yDifferences.Count > 0 ? yDifferences.Average() : 0f;

    float proximityWeight = Mathf.Clamp01(1f - avgX / 3.5f);  // Slightly softened
    float yActivityWeight = Mathf.Clamp01(avgY / 8f);

    density += proximityWeight * (1f + yActivityWeight);

    if (avgY > 1.5f && avgX < 4f)
    {
        density *= 0.4f;  // Nerfed from 0.5f
    }

    if (avgX > 4f)
    {
        density *= 0.1f;
    }

    return density;
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
