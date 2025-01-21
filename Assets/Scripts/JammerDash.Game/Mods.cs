using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using JammerDash.Tech;

namespace JammerDash.Game
{
    public class Mods : MonoBehaviour
    {
        public static Mods instance;
        public float scoreMultiplier;
        public AudioMixer master;
        public Dictionary<ModType, bool> modStates = new Dictionary<ModType, bool>();

        void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void SetMod(ModType mod, bool enabled)
        {
            if (enabled)
            {
                if (mod != ModType.None)
                {
                    modStates[mod] = enabled;
                }
                else
                {
                    DisableAllMods();
                }
            }
            else
            {
                modStates.Remove(mod);
                if (mod == ModType.SpeedIncrease || mod == ModType.SpeedDecrease)
                {
                    master.SetFloat("MasterPitch", 1f);
                }
            }

            var multipliers = new List<float>();
            var modsToDisable = new List<ModType>();

            foreach (var state in modStates)
            {
                if (state.Value)
                {
                    switch (state.Key)
                    {
                        case ModType.SpeedIncrease:
                            master.SetFloat("MasterPitch", 1.5f);
                            multipliers.Add(1.16f);
                            modsToDisable.Add(ModType.SpeedDecrease);
                            break;
                        case ModType.SpeedDecrease:
                            master.SetFloat("MasterPitch", 0.75f);
                            multipliers.Add(0.84f);
                            modsToDisable.Add(ModType.SpeedIncrease);
                            break;
                        case ModType.hidden:
                            multipliers.Add(1.04f);
                            modsToDisable.Add(ModType.remember);
                            break;
                        case ModType.remember:
                            multipliers.Add(1.04f);
                            modsToDisable.Add(ModType.hidden);
                            break;
                        case ModType.flashlight:
                            multipliers.Add(1.06f);
                            break;
                        case ModType.perfect:
                            multipliers.Add(1.02f);
                            break;
                        case ModType.random:
                            multipliers.Add(1.04f);
                            break;
                        case ModType.suddenDeath:
                            multipliers.Add(1.0f);
                            break;
                        case ModType.oneLine:
                            multipliers.Add(0.89f);
                            break;
                        case ModType.noSpikes:
                            float spikePercentage = CalculateSpikePercentage();
                            float noSpikesMultiplier = Mathf.Lerp(1f, 0.25f, spikePercentage / 0.75f);
                            multipliers.Add(noSpikesMultiplier);
                            break;
                        case ModType.easy:
                            multipliers.Add(0.94f);
                            break;
                        case ModType.yMirror:
                            multipliers.Add(1.0f);
                            break;
                        case ModType.autoMove:
                            multipliers.Add(0.0f);
                            break;
                        case ModType.auto:
                            multipliers.Add(1.0f);
                            break;
                    }
                }
            }

            // Disable conflicting mods
            foreach (var modToDisable in modsToDisable)
            {
                modStates[modToDisable] = false;
            }

            // Calculate and apply the score multiplier
            if (multipliers.Count > 0)
            {
                float totalMultiplier = 1;
                for (int i = 0; i < multipliers.Count; i++)
                {
                    totalMultiplier *= multipliers[i];
                }
                scoreMultiplier = totalMultiplier;
            }
            else
            {
                scoreMultiplier = 1;
            }

            CustomLevelDataManager.Instance.scoreMultiplier = scoreMultiplier;
            CustomLevelDataManager.Instance.modStates = new Dictionary<ModType, bool>(modStates);
        }

        public void DisableAllMods()
        {
            modStates.Clear();
            master.SetFloat("MasterPitch", 1f);
            scoreMultiplier = 1;
            CustomLevelDataManager.Instance.scoreMultiplier = scoreMultiplier;
            CustomLevelDataManager.Instance.modStates = new Dictionary<ModType, bool>(modStates);
        }

        private float CalculateSpikePercentage()
        {
            // Implement the logic to calculate the percentage of spike objects in the level
            // For example, you can count the number of spike objects and divide by the total number of objects
            int totalObjects = CustomLevelDataManager.Instance.data.cubePositions.Count + CustomLevelDataManager.Instance.data.sawPositions.Count + CustomLevelDataManager.Instance.data.longCubePositions.Count;
            int spikeObjects = CustomLevelDataManager.Instance.data.sawPositions.Count;

            if (totalObjects == 0) return 0f;

            return (float)spikeObjects / totalObjects;
        }
    }


    public enum ModType
    {
        // Increased diff
        /// SpeedIncrease (SI) - Speeds up the song (1.5x) (1.08x score bonus)
        /// hidden (HD) - Hides the notes (like HD in osu!) (1.04x score bonus)
        /// Remember (RM) - Does the opposite of hidden and shows the notes for a short time (1.04x score bonus)
        /// Flashlight (FL) - Draws a black outline and everything is black 5 tiles after the player (This only affects X pos) (1.06x score bonus)
        /// Perfect (PF) - You can only have Factor 5 clicks. (1.02x score bonus)
        /// Random (RD) - Randomizes the notes (Unranked) (1.04x score bonus)
        /// Sudden Death (SuD) - If you miss a note, you fail (1x score bonus)
        SpeedIncrease, hidden, remember, flashlight, perfect, random, suddenDeath,

        // Decreased diff
        /// SpeedDecrease (SD) - Slows down the song (0.75x) (0.88x score bonus)
        /// oneLine (OL) - All cubes are moved to one line, and no spikes (0.89x score bonus)
        /// noSpikes (NS) - Removes all spikes (0.5-1x score bonus, depending on how much saws there are per 10 cubes)
        /// Easy (EZ) - Slows down the overall player speed, and makes the cubes closer (0.89x score bonus)
        SpeedDecrease, oneLine, noSpikes, easy,

        // Other
        /// None - Default JD Experience
        /// Mirror (MR) - Mirrors the game on the Y scale. (1x score bonus)
        /// Auto Move (AM) - Automatically moves the player (0x score bonus)
        /// Auto (AU) - Automatically plays the game (0x score bonus)
        /// No Death (ND) - You can't die (1x score bonus)
        None, yMirror, autoMove, auto, noDeath
    }
}