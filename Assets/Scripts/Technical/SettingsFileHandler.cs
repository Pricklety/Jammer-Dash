using System;
using System.Collections.Generic;
using UnityEngine;

public class SettingsFileHandler : MonoBehaviour
{
    public static void SaveSettingsToFile(SettingsData data)
    {
        string json = JsonUtility.ToJson(data, true);

        // Save the settings to a file
        string filePath = Application.persistentDataPath + "/settings.json";
        System.IO.File.WriteAllText(filePath, json);
    }
    public static SettingsData LoadSettingsFromFile()
    {
        // Load settings from the file
        string filePath = Application.persistentDataPath + "/settings.json";

        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            return JsonUtility.FromJson<SettingsData>(json);
        }
        else
        {
            // Return default settings or handle the absence of a settings file
            return new SettingsData();
        }
    }
}

public class SettingsData
{
    public int resolutionValue = 99;
    public int windowMode = 1;
    public int selectedFPS = 60;
    public bool vsync = true;
    public float volume = 1;
    public int qualitySettingsLevel = 2;
    public bool artBG = false;
    public bool customBG = false;
    public bool vidBG = false;
    public bool sfx = true;
    public bool hitNotes = true;
    public bool focusVol = true;
    public int playerType = 0;
    public int antialiasing = 0;
    public bool cursorTrail = false;
    public bool allVisualizers = true;
    public bool lineVisualizer = true;
    public bool logoVisualizer = true;
    public bool bgVisualizer = true;
    public float noFocusVolume = -20f;
    public float lowpassValue = 500;
    public int scoreType = 0;
    public float mouseParticles = 1000;
    public bool isShowingFPS = false;
    public int hitType = 0;
    public float cursorFade = 0.24f;
    public bool parallax = true;
    public bool randomSFX = true;
    public bool confinedMouse = false;
    public int gameplayDir = 0;
    public string lang = "en_US";
    public bool wheelShortcut = true;
    public string gameVersion = Application.version;
    public string saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

}