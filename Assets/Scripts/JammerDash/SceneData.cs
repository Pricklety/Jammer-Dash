using System;
using System.Collections.Generic;
using UnityEngine;

namespace JammerDash
{

    [System.Serializable]
    public class SceneData
    {
        public string sceneName;
        public List<Vector3> cubePositions;
        public List<Vector3> sawPositions;
        public List<Vector3> longCubePositions;
        public List<float> longCubeWidth;
        public int ID;
        public int bpm;
        public string levelName;
        public int levelLength;
        public string artist;
        public string songName;
        public float songLength;
        public float calculatedDifficulty = 0f;
        public string gameVersion;
        public float saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public AudioClip clip;
        public string creator;
        public bool ground = true;
        public Color defBGColor;
        public Color defGColor;
        public bool isVerified;
        public bool isUploaded;
        public int playerScore;
        public int playerHP = 300;
        public float boxSize = 1;
        public string rank;
        public float offset = 0;

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public static SceneData FromJson(string json)
        {
            return JsonUtility.FromJson<SceneData>(json);
        }
    }

}