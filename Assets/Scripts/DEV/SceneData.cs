using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

[System.Serializable]
public class SceneData
{
    public string sceneName;
    public List<Vector3> cubePositions;
    public List<Vector3> sawPositions;
    public int ID;
    public int bpm;
    public string levelName;
    public int levelLength;
    public string songName = "NikoN1nja - Slowly Going Insane";
    public float calculatedDifficulty = 0f;
    public string gameVersion;
    public float saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public AudioClip clip;
    public string clipPath;
    public string creator;
    public bool ground = true;
    public Color defBGColor;
    public Color defGColor;
    public bool isVerified;
    public bool isUploaded;
    public string picLocation;

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public static SceneData FromJson(string json)
    {
        return JsonUtility.FromJson<SceneData>(json);
    }
}
