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
using System.Security.Cryptography;
using System.Text;

namespace JammerDash
{

    public class Account : MonoBehaviour
    {
        [Header("Username")]
        public string username;
        public string user;
        public string email;
        public string cc;
        public string url;
        [Header("Level")]
        public int level = 0;
        public long currentXP = 0;
        public long[] xpRequiredPerLevel;
        public long totalXP = 0;

        [Header("Local data")]
        public PlayerData p;
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

            // Update totalXP by reading the scores.dat file and summing every 5th entry
            totalXP = CalculateTotalXPFromFile();

            // Check if the player has enough XP to level up
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

        // Method to calculate totalXP by summing every 5th entry from scores.dat
        private long CalculateTotalXPFromFile()
        {
            long sum = 0;
            string filePath = Path.Combine(Application.persistentDataPath, "scores.dat");

            try
            {
                // Read all lines from the file
                var lines = File.ReadAllLines(filePath);

                // Iterate through every 5th line (entries are 0-indexed, so we start from index 4 for the 5th entry)
                for (int i = 4; i < lines.Length; i += 5)
                {
                    if (long.TryParse(lines[i], out long score))
                    {
                        sum += score;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the file: {ex.Message}");
            }

            return sum;
        }

        // Method to level up
        private void LevelUp()
        {
            currentXP -= xpRequiredPerLevel[level];
            level++;

            if (currentXP >= xpRequiredPerLevel[level] && level <= 299)
            {
                LevelUp();
            }
            Debug.Log("Level Up! You are now level " + level);
        }
        public void Apply(string username, string user, string email, string cc)
        {
            this.username = username;
            this.user = user;
            this.cc = cc;
            this.email = email;
            SavePlayerData();
        }
        public void CalculateXPRequirements()
        {
            long initialXP = 10000000L;
            float growthRate = 1.03f;
            xpRequiredPerLevel = new long[300];

            xpRequiredPerLevel[0] = initialXP;
            for (int i = 1; i < xpRequiredPerLevel.Length; i++)
            {
                xpRequiredPerLevel[i] = (long)(xpRequiredPerLevel[i - 1] * growthRate);
            }

        }
        public static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
        // Method to save player data
        public void SavePlayerData()
        {
            string save = sha256_hash(user);
            PlayerData data = new PlayerData
            {
                level = level,
                currentXP = currentXP,
                username = username,
                user = save,
                isLocal = true,
                isOnline = false,
                country = cc,
                id = SystemInfo.deviceUniqueIdentifier
            };
            PlayerData accountPost = new PlayerData
            {
                username = username,
                user = user,
                email = email,
                country = cc,
                id = SystemInfo.deviceUniqueIdentifier
            };
            Debug.Log(accountPost.country);
            StartCoroutine(Register(url, accountPost));
            XmlSerializer formatter = new XmlSerializer(typeof(PlayerData));
            string path = Application.persistentDataPath + "/playerData.dat";
            FileStream stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
        }

       
        public IEnumerator Register(string url, PlayerData bodyJsonObject)
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
                Debug.LogError("Response Body: " + request.downloadHandler.text);
                Notifications.instance.Notify($"An error happened.\n{request.error}", null);
            }
            else
            {
                Debug.Log("Status Code: " + request.responseCode);
                Debug.Log("Response Body: " + request.downloadHandler.text);
                Notifications.instance.Notify($"Successfully registered as {bodyJsonObject.username}", null);
                StartCoroutine(Login(this.url, bodyJsonObject));
            }
        }

        public IEnumerator Login(string url, PlayerData loginData)
        {
            // Login to account, save data locally and call this every time if there is a loginData.txt saved (hashed);
            return null;
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

                username = data.username;
                cc = data.country;
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
    public string user;
    public string email;
    public string country;
    public bool isLocal;
    public bool isOnline;
    public string id;
}
