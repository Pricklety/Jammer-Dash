using System;
using System.Collections.Generic;
using UnityEngine;

namespace JammerDash
{

    [System.Serializable]
    public class SceneData
    {
        public int version = 1;

        // Level info
        public string sceneName;
        public string creator;
        public int ID;
        public bool ground = true;
        public string levelName;
        public int levelLength;
        public float calculatedDifficulty = 0f;
        public Color defBGColor;
        public Color defGColor;

        // Objects
        public List<Vector3> cubePositions;
        public List<Vector3> sawPositions;
        public List<Vector3> longCubePositions;
        public List<float> longCubeWidth;

        // Difficulty
        public int playerHP = 300;
        public float boxSize = 1;

        // Song
        public int bpm;
        public string artist;
        public string romanizedArtist;
        public string songName;
        public string romanizedName;
        public float songLength;
        public float offset = 0;
        public AudioClip clip;

        // Technical
        public string gameVersion;
        public float saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public bool isVerified;
        public bool isUploaded;
        public int playerScore;
        public string rank;

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