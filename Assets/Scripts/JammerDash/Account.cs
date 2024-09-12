using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Data;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Xml.Serialization;

namespace JammerDash
{

    public class Account : MonoBehaviour
    {
        [Header("Username")]
        public string username;
        public string password;
        public string email;
        public string cc;
        public string url;
        [Header("Level")]
        public int level = 0;
        public long currentXP = 0;
        public long[] xpRequiredPerLevel;
        public long totalXP = 0;


        [Header("Internet Check")]
        public GameObject checkInternet;

        public static Account Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Duplicate instance of Account found. Destroying the new one.");
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            CalculateXPRequirements();
            LoadData();
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

            if (currentXP >= xpRequiredPerLevel[level] && level <= 249)
            {
                LevelUp();
            }
            Debug.Log("Level Up! You are now level " + level);
        }
        public void Apply(string username, string password, string email, string cc)
        {
            this.username = username;
            this.password = password;
            this.cc = cc;
            this.email = email;
            SavePlayerData();
        }
        public void CalculateXPRequirements()
        {
            long initialXP = 250000L;
            float growthRate = 1.05f;
            xpRequiredPerLevel = new long[251];

            xpRequiredPerLevel[0] = initialXP;
            for (int i = 1; i < xpRequiredPerLevel.Length; i++)
            {
                Debug.Log((long)(xpRequiredPerLevel[i - 1] * growthRate));
                xpRequiredPerLevel[i] = (long)(xpRequiredPerLevel[i - 1] * growthRate);
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
                totalXP = totalXP,
                username = username,
                isLocal = true,
                isOnline = false
            };
            PlayerData accountPost = new PlayerData
            {
                username = username,
                password = password,
                email = email,
                country = cc
            };
            Debug.Log(accountPost.country);
            StartCoroutine(Post(url, accountPost));
            XmlSerializer formatter = new XmlSerializer(typeof(PlayerData));
            string path = Application.persistentDataPath + "/playerData.dat";
            FileStream stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
        }

        public IEnumerator Post(string url, PlayerData bodyJsonObject)
        {
            string bodyJsonString = JsonUtility.ToJson(bodyJsonObject);

            UnityWebRequest request = new UnityWebRequest(url, "POST");

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("accept-encoding", "application/json");

            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(bodyJsonString);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                Debug.Log("Status Code: " + request.responseCode);
                Debug.Log("Response Body: " + request.downloadHandler.text);
            }
        }
        public PlayerData LoadData()
        {
            string path = Application.persistentDataPath + "/playerData.dat";

            if (File.Exists(path))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(PlayerData));
                FileStream stream = new FileStream(path, FileMode.Open);

                PlayerData data = formatter.Deserialize(stream) as PlayerData;
                stream.Close();

                level = data.level;
                currentXP = data.currentXP;
                totalXP = data.totalXP;
                username = data.username;
                return data;
            } 
            else
            {
                CalculateXPRequirements();
                return null;
            }
        }
        void Update()
        {
            

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                checkInternet.SetActive(true);
            }
            else
            {
                checkInternet.SetActive(false);
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
    public string username;
    public string password;
    public string email;
    public string country;
    public bool isLocal;
    public bool isOnline;
}
