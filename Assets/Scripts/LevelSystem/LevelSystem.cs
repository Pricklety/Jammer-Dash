using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class PlayerData
{
    public int level;
    public long currentXP;
    public Dictionary<int, long> xpRequiredPerLevel;
    public long totalXP;
}

public class LevelSystem : MonoBehaviour
{
    public int level = 0;
    public long currentXP = 0;
    public double initialXPRequirement = 250000; // Changed to double for better precision
    public float xpGrowthRate = 1.1f;
    public Dictionary<int, long> xpRequiredPerLevel = new Dictionary<int, long>();
    public long totalXP = 0;

    public static LevelSystem Instance { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
    }

    void Start()
    {
        LoadPlayerData();
    }

    void CalculateXPRequirements()
    {
        double xpRequirement = initialXPRequirement;
        for (int i = 0; i < 10000; i++)
        {
            xpRequiredPerLevel[i] = Convert.ToInt64(xpRequirement); // Convert to long
            xpRequirement *= xpGrowthRate;
        }
    }

    public void GainXP(long amount)
    {
        currentXP += amount;
        totalXP += amount;
        CheckForLevelUp();
        SavePlayerData();
    }

    public void CheckForLevelUp()
    {
        while (xpRequiredPerLevel.ContainsKey(level) && currentXP >= xpRequiredPerLevel[level])
        {
            level++;
            if (xpRequiredPerLevel.ContainsKey(level))
            {
                initialXPRequirement = xpRequiredPerLevel[level];
            }
            else
            {
                initialXPRequirement = initialXPRequirement * xpGrowthRate;
                xpRequiredPerLevel[level] = Convert.ToInt64(initialXPRequirement);
            }
            currentXP -= xpRequiredPerLevel[level - 1];
        }
    }


    void SavePlayerData()
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

    public void LoadPlayerData()
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
            xpRequiredPerLevel = data.xpRequiredPerLevel;
            totalXP = data.totalXP;

            CheckForLevelUp();
        }
        else
        {
            CalculateXPRequirements();
        }
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
            xpRequiredPerLevel = data.xpRequiredPerLevel;
            return data;
        }
        else
        {
            CalculateXPRequirements();
            return null;
        }
    }
}
