using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JammerDash {
     public class ConfigLoader : MonoBehaviour
    {
        private static Dictionary<string, string> configSettings = new Dictionary<string, string>();
        public static ConfigLoader Instance;
        void Start()
        {
            if (Instance == null) {
                Instance = this;
            }
            LoadConfig();
            Debug.Log("[TEXTURE SYSTEM] Loaded Config. Applying Settings...");
            ApplySettings();
        }

        public void LoadConfig()
        {
            if (!string.IsNullOrEmpty(TexturePack.GetActiveTexturePackPath())) {
                 string configPath = Path.Combine(TexturePack.GetActiveTexturePackPath(), "pack.ini");
            
            if (!File.Exists(configPath))
            {
                Debug.LogError($"[TEXTURE SYSTEM] ❌ Config file not found at {configPath}");
                return;
            }

            Debug.Log($"[TEXTURE SYSTEM] 📄 Loading config from: {configPath}");

            foreach (string line in File.ReadAllLines(configPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("[") || !line.Contains("=")) continue;

                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    configSettings[key] = value;
                    ApplySettings();
                    Debug.Log($"[TEXTURE SYSTEM] 🔹 Loaded: {key} = {value}");
                }
            }
            }
               
        }

        void ApplySettings()
        {
            if (configSettings.TryGetValue("SawAnimation", out string sawAnimationValue) && sawAnimationValue.ToLower() == "false")
            {
                DisableSawAnimators();
            }

            ApplyVisualizerSettings("VisualizerColorLogo", "VisualizerColorLogoAlpha", "spectrumLogo");
            ApplyVisualizerSettings("VisualizerColorBack", "VisualizerColorBackAlpha", "spectrumBack");
        }

        void ApplyVisualizerSettings(string colorKey, string alphaKey, string objectName)
        {
            if (!configSettings.TryGetValue(colorKey, out string hexColor) || !ColorUtility.TryParseHtmlString(hexColor, out Color baseColor))
            {
                Debug.LogWarning($"Invalid or missing color value for {colorKey}. Skipping.");
                return;
            }

            float alpha = 1.0f;
            if (configSettings.TryGetValue(alphaKey, out string alphaValue) && float.TryParse(alphaValue, out float parsedAlpha))
            {
                alpha = Mathf.Clamp01(parsedAlpha);
            }

            baseColor.a = alpha;
            SetColor(objectName, baseColor);
        }

        void SetColor(string objectName, Color newColor)
        {
            GameObject obj = GameObject.Find(objectName);
            if (obj == null)
            {
                Debug.LogWarning($"⚠️ GameObject {objectName} not found.");
                return;
            }

            SimpleSpectrum spectrum = obj.GetComponent<SimpleSpectrum>();
            if (spectrum == null)
            {
                Debug.LogWarning($"⚠️ SimpleSpectrum component not found on {objectName}");
                return;
            }

            spectrum.colorMin = newColor;
            spectrum.colorMax = newColor;
            Debug.Log($"✅ Set color for {objectName} to {newColor}");
            spectrum.RebuildSpectrum();
        }

        void DisableSawAnimators()
        {
            GameObject[] saws = GameObject.FindGameObjectsWithTag("Saw");
            foreach (GameObject saw in saws)
            {
                Animator animator = saw.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.enabled = false;
                    Debug.Log($"✅ Disabled animator for {saw.name}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Animator not found on {saw.name}");
                }
            }
        }
    }
}
