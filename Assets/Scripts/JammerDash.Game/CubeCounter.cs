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
        public string rank;

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
            float destruction = ((float)accCount / GetComponent<PlayerMovement>().Total) * 100;
            // Call GetTier and print the returned tier
            GetTier(destruction);

            destructionPercentage = destruction;
        }

        public string GetTier(float destructionPercentage)
        {
            float maxScore = 100;
            if (destructionPercentage > maxScore)
            {
                rank = "Invalid";
                return "Invalid";
            }
            if (destructionPercentage == maxScore)
            {
                rank = "SS+";
                return "SS+";
            }
            else if (destructionPercentage >= maxScore * 0.99f)
            {
                rank = "SS";
                return "SS";
            }
            else if (destructionPercentage >= maxScore * 0.95f)
            {
                rank = "S";
                return "S";
            }
            else if (destructionPercentage <= maxScore && destructionPercentage >= maxScore * 0.92f)
            {
                rank = "A";
                return "A";
            }
            else if (destructionPercentage >= maxScore * 0.86f)
            {
                rank = "B";
                return "B";
            }
            else if (destructionPercentage >= maxScore * 0.75f)
            {
                rank = "C";
                return "C";
            }
            else if (destructionPercentage >= maxScore * 0.50f)
            {
                rank = "D";
                return "D";
            }
            else if (destructionPercentage >= maxScore * 0.25f)
            {
                rank = "F";
                return "F";
            }
            else
            {
                rank = "F-";
                return "F-";
            }
        }
    }
}