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
            float total = scoreMultiplier;

            if (mod == ModType.SpeedIncrease)
            {
                if (enabled)
                {
                    master.audioMixer.SetFloat("MasterPitch", 1.5f);
                    total += 0.25f;
                    DisableMod(ModType.SpeedDecrease);
                }
                else
                    DisableMod(ModType.SpeedIncrease);
            }

            if (mod == ModType.SpeedDecrease)
            {
                if (enabled)
                {
                    master.audioMixer.SetFloat("MasterPitch", 0.75f);
                    total -= 0.25f;
                    DisableMod(ModType.SpeedIncrease);
                }
                else
                    DisableMod(ModType.SpeedDecrease);
            }

            if (mod == ModType.hidden)
            {
                if (enabled)
                {
                    total += 0.1f;
                    DisableMod(ModType.remember);
                }
                else
                    DisableMod(ModType.hidden);
            }

            if (mod == ModType.remember)
            {
                if (enabled)
                {
                    total += 1f;
                    DisableMod(ModType.hidden);
                }
                else
                    DisableMod(ModType.remember);
            }

            if (mod == ModType.oneLine)
            {
                if (enabled)
                    total += 0.25f;
                else
                    DisableMod(ModType.oneLine);
            }

            if (mod == ModType.None)
            {
                total = 1;
                DisableAllMods();
            }

            scoreMultiplier = total;
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
            float total = scoreMultiplier;

            if (mod == ModType.SpeedIncrease)
            {
                master.audioMixer.SetFloat("MasterPitch", 1.0f);
                total -= 0.25f;
            }

            if (mod == ModType.SpeedDecrease)
            {
                master.audioMixer.SetFloat("MasterPitch", 1f);
                total += 0.25f;
            }

            if (mod == ModType.hidden)
            {
                total -= 0.1f;
            }

            if (mod == ModType.remember)
            {
                total -= 1f;
            }

            if (mod == ModType.oneLine)
            {
                total -= 0.25f;
            }

            scoreMultiplier = total;
        }

        public bool IsModEnabled(ModType mod)
        {
            return modStates.ContainsKey(mod) && modStates[mod];
        }
    }

    public enum ModType
    {
        // Increased diff
        SpeedIncrease, hidden, remember,

        // Decreased diff
        SpeedDecrease, oneLine,

        // Other
        None
    }
}