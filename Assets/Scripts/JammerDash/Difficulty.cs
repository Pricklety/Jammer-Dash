using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash
{

    public class Difficulty : MonoBehaviour
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
                    float contribution = (timingWindow / 0.64f
                     + cubeCount / 200f
                     * (cubes.Count / 120f)
                     * (saws.Count / 20f)
                     * (longCubes.Count / 100f)
                     * (precisionFactor / divisor)
                     * 1 / (hp.value + 1) * 500f
                     + Mathf.Exp(0.1f * (0.5f - size.value) * 70f)
                     * averageDistance / 15) / 10f;

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
}