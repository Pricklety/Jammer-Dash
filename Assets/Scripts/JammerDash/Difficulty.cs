using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash.Difficulty
{

    public class Calculator : MonoBehaviour
    {
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

            // Synchronously retrieve objects with the specified tags
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

        public static int[] CalculateCubesPerY(Vector2[] cubePositions)
        {
            int[] cubesPerY = new int[6];
            foreach (Vector2 position in cubePositions)
            {
                int yLevel = Mathf.RoundToInt(position.y);
                yLevel = Mathf.Clamp(yLevel, -1, 4);
                cubesPerY[yLevel + 1]++;
            }
            return cubesPerY;
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
        public ShinePerformance(int perfectHits, int greatHits, int goodHits, int missedHits, int currentCombo, int bestCombo, float gameDifficulty, float levelLength)
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

            CalculatePrecision();
            CalculateSequenceEfficiency();
            CalculateVersatility(_perfectHits, _greatHits, _goodHits);
            CalculatePerformanceScore();
        }

        private void CalculatePrecision()
        {
            _precision = Clamp((float)(_perfectHits + _greatHits + _goodHits) / _totalActions, 0, 1);
        }

        private void CalculateSequenceEfficiency()
        {
            _sequenceEfficiency = _bestCombo != 0 ? Clamp((float)_currentCombo / _bestCombo + 1, 0, 1) : 0;
        }

        private void CalculateVersatility(float high, float medium, float low)
        {

            _versatility = high * 0.05f + medium * 0.025f + low * -0.1f + _missedHits * -0.5f;
        }





        private void CalculatePerformanceScore()
        {
            float precisionWeight = 1.24f;

            _performanceScore = Math.Max((
                MathF.Pow(_precision, 2) * precisionWeight +
                _sequenceEfficiency * 10 +
                _versatility * 5 +
                _levelLength / 100 +
                _gameDifficulty * 0.35f) / 10,
                0f
            );
        }

        public float PerformanceScore => _performanceScore;

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0, 1);
        }


    }
}