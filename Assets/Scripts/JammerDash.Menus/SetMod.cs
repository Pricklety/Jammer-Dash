using JammerDash.Game;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash.Menus.Play
{
    public class SetMod : MonoBehaviour
    {
        public string modName;

        public void SetMods()
        {
            if (Mods.instance == null)
            {
                Debug.LogError("Mods.instance is not initialized.");
                return;
            }

            ModType mod;
            if (string.IsNullOrEmpty(modName))
            {
                mod = ModType.None;
            }
            else if (!Enum.TryParse(modName, out mod))
            {
                Debug.LogError($"Invalid mod name: {modName}");
                return;
            }

            Mods.instance.SetMod(mod, GetComponent<Toggle>().isOn);
        }
    }
}
