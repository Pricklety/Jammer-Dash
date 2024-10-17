using UnityEngine;
using JammerDash.Game.Player;
using NUnit.Framework;
using System.Collections.Generic;
namespace JammerDash.Game
{
    public class CubeCounter : MonoBehaviour
    {
        public int destroyedCubes;
        public int maxScore;
        public int score;
        public float accCount;
        public double destructionPercentage;
        public List<GameObject> cubes = new();

        void Start()
        {
            
            maxScore = cubes.Count * 50;
        }

        void FixedUpdate()
        {
            if (cubes.Count == 0)
            {
                foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cubes"))
                    cubes.Add(cube);
                foreach (GameObject longCube in GameObject.FindGameObjectsWithTag("LongCube"))
                    cubes.Add(longCube);
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
            else if (destructionPercentage >= maxScore * 0.92f)
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