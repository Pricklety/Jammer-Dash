using UnityEngine;
using JammerDash.Game.Player;
namespace JammerDash.Game
{
    public class CubeCounter : MonoBehaviour
    {
        public int destroyedCubes;
        public int maxScore;
        public int score;
        public float accCount;
        public double destructionPercentage;
        public GameObject[] cubes;

        void Start()
        {
            cubes = GameObject.FindGameObjectsWithTag("Cubes");
            maxScore = cubes.Length * 50;
        }

        void FixedUpdate()
        {
            if (cubes.Length == 0)
            {
                cubes = GameObject.FindGameObjectsWithTag("Cubes");
            }
            float destruction = ((float)accCount / FindObjectOfType<PlayerMovement>().Total) * 100;
            // Call GetTier and print the returned tier
            GetTier(destruction);

            destructionPercentage = destruction;
        }

        public string GetTier(float destructionPercentage)
        {
            float maxScore = 100;
            if (destructionPercentage > maxScore)
            {
                return "Invalid";
            }
            if (destructionPercentage == maxScore)
            {
                return "SS+";
            }
            else if (destructionPercentage >= maxScore * 0.99f)
            {
                return "SS";
            }
            else if (destructionPercentage >= maxScore * 0.95f)
            {
                return "S";
            }
            else if (destructionPercentage <= maxScore && destructionPercentage >= maxScore * 0.92f)
            {
                return "A";
            }
            else if (destructionPercentage >= maxScore * 0.86f)
            {
                return "B";
            }
            else if (destructionPercentage >= maxScore * 0.75f)
            {
                return "C";
            }
            else if (destructionPercentage >= maxScore * 0.50f)
            {
                return "D";
            }
            else if (destructionPercentage >= maxScore * 0.25f)
            {
                return "F";
            }
            else
            {
                return "F-";
            }
        }

        public string GetNoColorTier(float destructionPercentage)
        {
            float maxScore = 100;
            if (destructionPercentage > maxScore)
            {
                return "Invalid";
            }
            if (destructionPercentage == maxScore)
            {
                return "SS+";
            }
            else if (destructionPercentage >= maxScore * 0.99f && FindObjectOfType<PlayerMovement>().misses != 1)
            {
                return "SS";
            }
            else if (destructionPercentage >= maxScore * 0.95f && FindObjectOfType<PlayerMovement>().misses != 1)
            {
                return "S";
            }
            else if ((destructionPercentage <= maxScore && destructionPercentage >= maxScore * 0.92f && FindObjectOfType<PlayerMovement>().misses >= 1) || (destructionPercentage <= 0.95f && FindObjectOfType<PlayerMovement>().misses >= 0))
            {
                return "A";
            }
            else if (destructionPercentage >= maxScore * 0.86f)
            {
                return "B";
            }
            else if (destructionPercentage >= maxScore * 0.75f)
            {
                return "C";
            }
            else if (destructionPercentage >= maxScore * 0.50f)
            {
                return "D";
            }
            else if (destructionPercentage >= maxScore * 0.25f)
            {
                return "F";
            }
            else
            {
                return "F-";
            }
        }
    }
}