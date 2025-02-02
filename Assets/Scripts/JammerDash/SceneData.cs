using System;
using System.Collections.Generic;
using UnityEngine;

namespace JammerDash
{

    [System.Serializable]
    public class SceneData
    {
        public int version = 1; 
        // Update to version 2 once all the following changes are done:
        /// sceneName renamed to name - DONE
        /// Deleted levelName - DONE
        /// Added sawType
        /// Made cubePositions, longCubePositions, sawPositions save 2D positions (X, Y) rather than 3D (X, Y, Z) - DONE
        /// Added sections - This will show individual infos for level sections (The 10 second parts of the level, won't be useful for many maps, but I'd love to expand this as mid-level BPM change, and more)
        /// Added breakTimes - This way the game will automatically add a break 3 seconds after an object IF there's no object following it for 7+ seconds
        

        // Level info
        public string name;
        public string creator;
        public int ID;
        public bool ground = true;
        public int levelLength;
        public float calculatedDifficulty = 0f;
        

        // Difficulty
        public int playerHP = 300;
        public float boxSize = 1;

        // Song
        public float bpm;
        public string artist;
        public string romanizedArtist;
        public string songName;
        public string romanizedName;
        public string source = "Unknown";
        public float songLength;
        public float offset = 0;

        // Technical
        public string gameVersion;
        public float saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        public Color defBGColor;

        // Objects
        public List<Vector2> cubePositions; 
        public List<Vector2> sawPositions;
        public List<Vector2> longCubePositions;
        public List<float> longCubeWidth;
        public List<int> cubeType;
        public List<int> sawType;
        public List<float> breakTimes; // x value where the break should start
        public Section[] sections;

#region OBSOLETE
        public string sceneName;
        public string levelName;
#endregion

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public static SceneData FromJson(string json)
        {
            return JsonUtility.FromJson<SceneData>(json);
        }
    }

    public class Section {
        public float sectionBegin;
        public float sectionEnd;
        public int bpm;
        public float difficulty;
        public bool visuals; // v1.1 or above - Section Manager: Can make visuals display different stuff (Kinda like Storyboards in osu!, yet not for the full map)
    }

    public class SceneDataV1 
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
        public List<int> cubeType;

        // Difficulty
        public int playerHP = 300;
        public float boxSize = 1;

        // Song
        public float bpm;
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

    }

}