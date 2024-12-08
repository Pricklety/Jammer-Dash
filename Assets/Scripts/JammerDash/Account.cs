using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

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

        [Header("Playtime")]
        public float playtime;
        public static Account Instance { get; private set; }

        public bool loggedIn;

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
            StartCoroutine(SavePlaytimeEverySecond());
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
                SavePlayerData(user);
            }
            else
            {
                SavePlayerData(user);
            }
        }

        // Method to calculate totalXP by summing every 5th entry from scores.dat
        private long CalculateTotalXPFromFile()
        {
            long sum = 0;
            string filePath = Path.Combine(Application.persistentDataPath, "scores.dat");

            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"File does not exist at path: {filePath}");
                    return 0;
                }

                var lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var parts = line.Split(',');
                    if (parts.Length > 4 && long.TryParse(parts[4], out long score))
                    {
                        sum += score;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading the file: {ex.Message}");
            }

            return sum;
        }

        // Method to level up
        private void LevelUp()
        {
            if (currentXP >= xpRequiredPerLevel[level] && level <= 299)
            {
                currentXP -= xpRequiredPerLevel[level];
                level++;
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
            SavePlayerData(user);
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

        public static string sha256_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        // Method to save player data
        public void SavePlayerData(string pass)
        {
            string save = sha256_hash(user);
            PlayerData data = new PlayerData
            {
                level = level,
                currentXP = currentXP,
                username = username,
                password = save,
                isLocal = true,
                isOnline = false,
                country = cc,
                id = SystemInfo.deviceUniqueIdentifier,
                sp = Difficulty.Calculator.CalculateSP("scores.dat"),
                playtime = playtime
            };

            // Prepare data for registration
            PlayerData accountPost = new PlayerData
            {
                username = username,
                password = save,
                email = email,
                country = cc,
                id = SystemInfo.deviceUniqueIdentifier
            };

            // Register player
            StartCoroutine(Register(url, accountPost, save)); // passing the hashed password
        }

        public IEnumerator Register(string url, PlayerData bodyJsonObject, string inputPassword)
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

                JObject jsonObject = JObject.Parse(request.downloadHandler.text);
                var errors = jsonObject["errors"];
                Notifications.instance.Notify($"An error happened.\n{errors}", null);

                // Check if email or username are taken
                if (errors != null && errors["email"] != null && errors["email"].ToString() == "Email Taken!" &&
                    errors["username"] != null && errors["username"].ToString() == "Username Taken!")
                {
                    // Perform login check since the user already exists
                    StartCoroutine(HandleLogin(bodyJsonObject.email, inputPassword)); // pass plain password
                }
            }
            else
            {
                Debug.Log("Status Code: " + request.responseCode);
                Debug.Log("Response Body: " + request.downloadHandler.text);
                Notifications.instance.Notify($"Successfully registered as {bodyJsonObject.username}", null);
                JObject jsonObject = JObject.Parse(request.downloadHandler.text);
                StartCoroutine(Login(this.url, bodyJsonObject, jsonObject["token"]));
                loggedIn = true;
            }
        }

        public IEnumerator HandleLogin(string email, string inputPassword)
        {
            // Assuming you have an endpoint or method to get user data by email
            string loginUrl = "https://yourapi.com/getUser"; // Replace with your actual URL
            UnityWebRequest request = UnityWebRequest.Get(loginUrl + "?email=" + email);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                yield break;
            }

            JObject jsonObject = JObject.Parse(request.downloadHandler.text);
            string hashedPasswordFromDb = jsonObject["password"].ToString(); // Assuming the password is hashed in the database

            // add checking passwords soon
        }

        public IEnumerator Login(string url, PlayerData loginData, JToken token)
        {
            // Perform the login with the received token or user data
            // Save the login state, and continue with game logic
            Debug.Log("User logged in successfully with token: " + token.ToString());
            loggedIn = true;
            yield return null;
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
            playtime += Time.deltaTime;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                checkInternet.SetActive(true);
            }
            else
            {
                checkInternet.SetActive(false);
            }
        }
        public string ConvertPlaytimeToReadableFormat()
        {
            int totalSeconds = Mathf.FloorToInt(playtime);

            int days = totalSeconds / 86400; // 86400 seconds in a day
            if (days > 0)
            {
                int hours = (totalSeconds % 86400) / 3600; // Remaining hours within the day
                return $"{days}d {hours}h";
            }

            int hours1 = totalSeconds / 3600; // 3600 seconds in an hour
            if (hours1 > 0)
            {
                int minutes = (totalSeconds % 3600) / 60; // Remaining minutes within the hour
                return $"{hours1}h {minutes}m";
            }

            int minutes1 = totalSeconds / 60; // 60 seconds in a minute
            int seconds = totalSeconds % 60;
            return $"{minutes1}m {seconds}s";
        }
        private void SavePlaytime()
        {
            string playtimePath = Path.Combine(Application.persistentDataPath, "playtime.dat");
            try
            {
                using (StreamWriter writer = new StreamWriter(playtimePath, false))
                {
                    writer.WriteLine(playtime); // Save the playtime
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error saving playtime: " + ex.Message);
            }
        }

        private IEnumerator SavePlaytimeEverySecond()
        {
            while (true)
            {
                SavePlaytime();
                yield return new WaitForSeconds(1f); // Wait for 1 second
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
    public string id;
    public float playtime;
    public float sp;
    public int playCount;
    public string token;
}
