using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash.Difficulty
{

    public class Calculator : MonoBehaviour
    {
        public static float CalculateDifficulty(List<GameObject> cubes, List<GameObject> saws, List<GameObject> longCubes, Slider hp, Slider size, int[] cubeCountsPerY, Vector2[] cubePositions, float clickTimingWindow)
        {
            float difficulty = 0f;
            Transform targetObject = FindFarthestObjectInX(Object.FindObjectsWithTags(new string[2] { "Cubes", "Saw" }));

            // Calculate the distance based on the position of the farthest object and additional distance
            float distance = targetObject.position.x + 5;

            // Iterate over each Y level
            for (int y = -1; y <= 4; y++)
            {
                // Get the number of cubes on this Y level
                int cubeCount = cubeCountsPerY[y + 1];

                // If there are no cubes on this level, continue
                if (cubeCount == 0)
                    continue;

                // Iterate over each pair of consecutive cubes on this Y level
                for (int i = 0; i < cubeCount - 1; i++)
                {
                    // Calculate timing window based on Y level difference and player movement speed
                    float timingWindow = CalculateTimingWindow(cubePositions[i], cubePositions[i + 1], y);

                    // Consider X position variation for precision calculation
                    float xPositionVariation = Mathf.Abs(cubePositions[i].x - cubePositions[i + 1].x);

                    // Factor in precision required for clicking correctly
                    float precisionFactor = CalculatePrecisionFactor(xPositionVariation);

                    // Calculate the average cube distance
                    float averageDistance = CalculateAverageCubeDistance(cubes);
                    // Avoid division by zero by ensuring clickTimingWindow + distance is not zero
                    float divisor = clickTimingWindow + distance != 0 ? clickTimingWindow + distance : float.Epsilon;
                    float contribution = (
    (timingWindow / 0.98f) +
    (cubeCount / 300f) *  
    (cubes.Count / 160f) * 
    (saws.Count / 30f) *  
    (longCubes.Count / 60f) * 
    (precisionFactor / divisor) *  // Increase divisor impact
    (1 / (hp.value + 1)) * 30f +  // Reduce multiplier
    Mathf.Exp(0.02f * (0.5f - size.value) * 20f) *  // Significantly reduce exponential growth
    (averageDistance / 5f))  // Lower distance multiplier
    / 6000f;  // Further increase the final division to lower overall score


            difficulty += contribution;
                }
            }
            return difficulty;
        }
        public static float CalculateTimingWindow(Vector2 position1, Vector2 position2, int yLevelDifference)
        {
            // Calculate the Y-axis distance between the cubes
            float yDistance = Mathf.Abs(position2.y - position1.y);

            // Calculate timing window based on Y-axis distance and player movement speed
            float timeWindow = yDistance * 0.1f;

            return timeWindow;
        }


        public static float CalculatePrecisionFactor(float xPositionVariation)
        {
            float precisionFactor = 1 - xPositionVariation / 20f;

            return Mathf.Clamp01(precisionFactor);
        }
        public static int[] CalculateCubesPerY(Vector2[] cubePositions)
        {
            // Initialize an array to store the number of cubes per Y level
            int[] cubesPerY = new int[6]; // Y levels range from -1 to 4, so 6 elements are needed

            // Iterate over each cube position and count the number of cubes per Y level
            foreach (Vector2 position in cubePositions)
            {
                int yLevel = Mathf.RoundToInt(position.y); // Round Y position to the nearest integer
                yLevel = Mathf.Clamp(yLevel, -1, 4); // Ensure Y level is within valid range

                // Increment the count for the corresponding Y level
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
                    Vector3 positionA = cubes[i].transform.position;
                    Vector3 positionB = cubes[j].transform.position;

                    // Calculate distance between cubes
                    float distance = Vector3.Distance(positionA, positionB);
                    totalDistance += distance;
                    numDistances++;
                }
            }

            // Calculate average distance
            float averageDistance = totalDistance / numDistances;

            return averageDistance;
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
    }
    public class Object : MonoBehaviour
    {
        public static GameObject[] FindObjectsWithTags(string[] tags)
        {
            GameObject[] objects = new GameObject[0];

            foreach (string tag in tags)
            {
                objects = objects.Concat(GameObject.FindGameObjectsWithTag(tag)).ToArray();
            }

            return objects;
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