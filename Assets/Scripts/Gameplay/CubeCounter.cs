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
                return "<color=#FFC300>SS+</color>";
            }
            else if (destructionPercentage >= maxScore * 0.99f && FindObjectOfType<PlayerMovement>().misses != 1)
            {
                return "<color=#B5CE00>SS</color>";
            }
            else if (destructionPercentage >= maxScore * 0.95f && FindObjectOfType<PlayerMovement>().misses != 1)
            {
                return "S";
            }
            else if (destructionPercentage <= maxScore && destructionPercentage >= maxScore * 0.92f && FindObjectOfType<PlayerMovement>().misses >= 1)
            {
                return "A";
            }
            else if (destructionPercentage <= 0.95f && FindObjectOfType<PlayerMovement>().misses >= 0)
            {
                return "A";
            }
            else if (destructionPercentage >= maxScore * 0.88f)
            {
                return "B";
            }
            else if (destructionPercentage >= maxScore * 0.84f)
            {
                return "C";
            }
            else if (destructionPercentage >= maxScore * 0.8f)
            {
                return "D";
            }
            else if (destructionPercentage >= maxScore * 0.50f)
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