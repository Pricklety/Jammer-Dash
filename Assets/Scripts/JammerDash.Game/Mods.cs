using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace JammerDash.Game
{
    public class Mods : MonoBehaviour
    {
        public static Mods instance;
        public float scoreMultiplier;
        public AudioMixerGroup master;
        private Dictionary<ModType, bool> modStates;

        public void Awake()
        {
            instance = this;
            modStates = new Dictionary<ModType, bool>();

            foreach (ModType mod in System.Enum.GetValues(typeof(ModType)))
            {
                if (mod != ModType.None)
                {
                    ResetMods();
                }
            }

            ResetMods();
        }

        public void ResetMods()
        {
            SetMod(ModType.None, false);
        }

        public void SetMod(ModType mod, bool enabled)
        {
            modStates[mod] = enabled;
            List<float> multipliers = new List<float>();

            foreach (var state in modStates)
            {
            if (state.Value)
            {
                switch (state.Key)
                {
                case ModType.SpeedIncrease:
                    master.audioMixer.SetFloat("MasterPitch", 1.5f);
                    multipliers.Add(1.08f);
                    DisableMod(ModType.SpeedDecrease);
                    break;
                case ModType.SpeedDecrease:
                    master.audioMixer.SetFloat("MasterPitch", 0.75f);
                    multipliers.Add(0.96f);
                    DisableMod(ModType.SpeedIncrease);
                    break;
                case ModType.hidden:
                    multipliers.Add(1.04f);
                    DisableMod(ModType.remember);
                    break;
                case ModType.remember:
                    multipliers.Add(1.04f);
                    DisableMod(ModType.hidden);
                    break;
                case ModType.oneLine:
                    multipliers.Add(0.89f);
                    break;
                case ModType.noSpikes:
                    multipliers.Add(0.5f);
                    break;
                }
            }
            }

            if (mod == ModType.None)
            {
            DisableAllMods();
            scoreMultiplier = 1;
            return;
            }

            if (multipliers.Count > 0)
            {
            multipliers.Sort();
            scoreMultiplier = multipliers[multipliers.Count / 2];
            }
            else
            {
            scoreMultiplier = 1;
            }
        }

        private void DisableAllMods()
        {
            foreach (ModType mod in System.Enum.GetValues(typeof(ModType)))
            {
                if (mod != ModType.None)
                {
                    DisableMod(mod);
                }
            }
        }

        public void DisableMod(ModType mod)
        {
            modStates[mod] = false;
            List<float> multipliers = new List<float>();

            foreach (var state in modStates)
            {
                if (state.Value)
                {
                    switch (state.Key)
                    {
                        case ModType.SpeedIncrease:
                            master.audioMixer.SetFloat("MasterPitch", 1.5f);
                            multipliers.Add(1.08f);
                            break;
                        case ModType.SpeedDecrease:
                            master.audioMixer.SetFloat("MasterPitch", 0.75f);
                            multipliers.Add(0.96f);
                            break;
                        case ModType.hidden:
                            multipliers.Add(1.04f);
                            break;
                        case ModType.remember:
                            multipliers.Add(1.04f);
                            break;
                        case ModType.oneLine:
                            multipliers.Add(0.89f);
                            break;
                        case ModType.noSpikes:
                            multipliers.Add(0.5f);
                            break;
                    }
                }
            }

            if (multipliers.Count > 0)
            {
                multipliers.Sort();
                scoreMultiplier = multipliers[multipliers.Count / 2];
            }
            else
            {
                scoreMultiplier = 1;
            }
        }

        public bool IsModEnabled(ModType mod)
        {
            return modStates.ContainsKey(mod) && modStates[mod];
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
        /// Sudden Death (SD) - If you miss a note, you fail (1x score bonus)
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
        None, yMirror, autoMove
    }
}