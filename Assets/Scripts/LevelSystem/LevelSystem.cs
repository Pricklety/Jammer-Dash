using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class PlayerData
{
    public int level;
    public float currentXP;
    public Dictionary<int, float> xpRequiredPerLevel;
}

public class LevelSystem : MonoBehaviour
{
    public int level = 0;
    public float currentXP = 0;
    public float initialXPRequirement = 200000;
    public float xpGrowthRate = 1.2f;
    public Dictionary<int, float> xpRequiredPerLevel = new Dictionary<int, float>();

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
        float xpRequirement = initialXPRequirement;
        for (int i = 0; i < 10000; i++)
        {
            xpRequiredPerLevel[i] = xpRequirement;
            xpRequirement *= xpGrowthRate;
        }
    }

    public void GainXP(float amount)
    {
        currentXP += amount;
        CheckForLevelUp();
        SavePlayerData();
    }

    public void CheckForLevelUp()
    {
        while (xpRequiredPerLevel.ContainsKey(level) && currentXP >= initialXPRequirement)
        {
            level++;
            currentXP -= initialXPRequirement;
            initialXPRequirement *= xpGrowthRate;
        }
    }

    void SavePlayerData()
    {
        PlayerData data = new PlayerData
        {
            level = level,
            currentXP = currentXP,
            xpRequiredPerLevel = xpRequiredPerLevel
        };

        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/playerData.dat";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    void LoadPlayerData()
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
        }
        else
        {
            CalculateXPRequirements();
        }
    }
}
