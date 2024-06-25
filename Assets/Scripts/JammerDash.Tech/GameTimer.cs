using UnityEngine;

namespace JammerDash.Tech
{

    public class GameTimer : MonoBehaviour
    {
        private static float gameStartTime;

        void FixedUpdate()
        {
            // Set the game start time when the script is loaded
            gameStartTime = Time.realtimeSinceStartup;
        }

        public static float GetRunningTime()
        {
            // Return the running time since the game opened
            return gameStartTime;
        }
    }

}