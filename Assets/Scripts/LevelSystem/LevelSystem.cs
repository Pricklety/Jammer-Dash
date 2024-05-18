using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace JammerDash.Tech.Levels
{

    public class LevelSystem : MonoBehaviour
    {
        public int level = 0;
        public long currentXP = 0;
        public long[] xpRequiredPerLevel;
        public long totalXP = 0;
        public static LevelSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Duplicate instance of LevelSystem found. Destroying the new one.");
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            CalculateXPRequirements();
            LoadTotalXP();
        }

        public void GainXP(long amount)
        {
            currentXP += amount;
            totalXP += amount;
            if (currentXP >= xpRequiredPerLevel[level])
            {
                LevelUp();
                SavePlayerData();
            }
            else
            {
                SavePlayerData();
            }
        }

        // Method to level up
        private void LevelUp()
        {
            currentXP -= xpRequiredPerLevel[level];
            level++;

            if (currentXP >= xpRequiredPerLevel[level] && level <= 199)
            {
                LevelUp();
            }
            Debug.Log("Level Up! You are now level " + level);
        }

        public void CalculateXPRequirements()
        {
            long initialXP = 250000L;
            float growthRate = 1.05f;
            xpRequiredPerLevel = new long[201];

            xpRequiredPerLevel[0] = initialXP;
            for (int i = 1; i < xpRequiredPerLevel.Length; i++)
            {
                Debug.Log((long)(xpRequiredPerLevel[i - 1] * growthRate));
                xpRequiredPerLevel[i] = (long)(xpRequiredPerLevel[i - 1] * growthRate);
            }

            // Debug logs to check the array
            Debug.Log("XP Required Per Level:");
            for (int i = 0; i < xpRequiredPerLevel.Length; i++)
            {
                Debug.Log("Level " + i + ": " + xpRequiredPerLevel[i]);
            }
        }

        // Method to save player data
        public void SavePlayerData()
        {
            PlayerData data = new PlayerData
            {
                level = level,
                currentXP = currentXP,
                xpRequiredPerLevel = xpRequiredPerLevel,
                totalXP = totalXP
            };

            BinaryFormatter formatter = new BinaryFormatter();
            string path = Application.persistentDataPath + "/playerData.dat";
            FileStream stream = new FileStream(path, FileMode.Create);

            formatter.Serialize(stream, data);
            stream.Close();
        }
        public PlayerData LoadTotalXP()
        {
            string path = Application.persistentDataPath + "/playerData.dat";

            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                PlayerData data = formatter.Deserialize(stream) as PlayerData;
                stream.Close();

                level = data.level;
                currentXP = data.currentXP;
                totalXP = data.totalXP;
                return data;
            }
            else
            {
                CalculateXPRequirements();
                return null;
            }
        }
    }

   
}
[System.Serializable]
public class PlayerData
{
    public int level;
    public long currentXP;
    public long[] xpRequiredPerLevel;
    public long totalXP;
}
