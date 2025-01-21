using JammerDash.Game;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash.Menus.Play
{
    public class SetMod : MonoBehaviour
    {
        public ModType modName;

        public void Start()
        {
            if (Mods.instance == null)
            {
                Debug.LogError("Mods.instance is not initialized.");
                return;
            }

            Toggle toggle = GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = Mods.instance.modStates.ContainsKey(modName) && Mods.instance.modStates[modName];
            }
        }

        public void Update()
        {
            if (Mods.instance == null)
            {
                Debug.LogError("Mods.instance is not initialized.");
                return;
            }

        }

        public void SetMods(bool value)
        {
            if (Mods.instance == null)
            {
                Debug.LogError("Mods.instance is not initialized.");
                return;
            }

            Debug.Log("Setting mod " + modName + " to " + value);

            Mods.instance.SetMod(modName, value);

            if (modName == ModType.random)
            {
                var random = new System.Random();
                GameObject.Find("randomInput").GetComponent<InputField>().text = random.Next(int.MaxValue).ToString();
            }
        }

        public void DisableAllMods()
        {
            if (Mods.instance == null)
            {
                Debug.LogError("Mods.instance is not initialized.");
                return;
            }

            Mods.instance.DisableAllMods();
        }
    }
}