using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using JammerDash.Difficulty;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net;
using Texture = UnityEngine.Texture2D;
namespace JammerDash
{
    public class Account : MonoBehaviour
    {
        [Header("Account info")]
        public string uuid;
        public string nickname;
        public string username;
        public string user;
        public string email;
        public string cc;
        public string url;
        public string token;
        public string ip;
        public UnityEngine.Texture pfp;
        public bool isBanned;

        public string role;
        public string country_name;
        public string region;
    

        [Header("Level")]
        public int level = 0;
        public long[] xpRequiredPerLevel;
        public long totalXP = 0;

        [Header("Local data")]
        public PlayerData p;

        [Header("Internet Check")]
        public GameObject checkInternet;

        [Header("Playtime")]
        public float playtime;
        public static Account Instance { get; private set; }

        public bool checkRegister = false;

        public bool loggedIn;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("Account instance found.");
                
            }
            else
            {
                Debug.LogWarning("Duplicate instance of Account found. Destroying the new one.");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoginData(); // Try to load login data only once at the start
            LoadData();
            CalculateXPRequirements();
            InvokeRepeating(nameof(SavePlaytime), 1f, 1);
        }

        private bool IsFileInUse(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // File is available for read/write
                    return false;
                }
            }
            catch (IOException)
            {
                // File is in use
                return true;
            }
        }
        private IEnumerator DownloadProfilePicture(string url)
                {
                    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
                    {
                        yield return request.SendWebRequest();
        
                        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                        {
                            Debug.LogError($"Error downloading profile picture: {request.error}");
                        }
                        else
                        {
                            pfp = ((DownloadHandlerTexture)request.downloadHandler).texture;
                        }
                    }
                }
        private static readonly object fileLock = new object();
        private void SaveLocalData()
        {
            SavePlaytime();

            if (!File.Exists(Path.Combine(Main.gamePath, "playerData.dat"))) {
                File.Create(Path.Combine(Main.gamePath, "playerData.dat"));
            }
            lock (fileLock) // Ensure thread safety
            {
                try
                {  string loginDataPath = Path.Combine(Main.gamePath, "loginData.dat");
                    if (!IsFileInUse(loginDataPath))
                    {
                        LoginData login = new LoginData
                        {
                            uuid = uuid,
                            username = username.ToLower(),
                            nickname = nickname,
                            password = user,
                            token = token,
                            hardware_id = SystemInfo.deviceUniqueIdentifier
                        };

                        XmlSerializer formatter1 = new XmlSerializer(typeof(LoginData));
                        using (FileStream stream1 = new FileStream(loginDataPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            formatter1.Serialize(stream1, login);
                        }
                    }
                    string playerDataPath = Path.Combine(Main.gamePath, "playerData.dat");
                    if (!IsFileInUse(playerDataPath))
                    {
                        PlayerData data = new PlayerData
                        {
                            username = username.ToLower(),
                            level = level,
                            country = cc,
                            isLocal = true,
                            isOnline = true,
                            sp = Calculator.CalculateSP("scores.dat"),
                            playCount = Calculator.CalculateOtherPlayerInfo("scores.dat").TotalPlays
                        };

                        XmlSerializer formatter = new XmlSerializer(typeof(PlayerData));
                        using (FileStream stream = new FileStream(playerDataPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            formatter.Serialize(stream, data);
                        }
                    }

                  
                }
                catch (IOException ex)
                {
                    Debug.LogError($"File access error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected error: {ex.Message}");
                }
            }
        }

       


        public void Apply(string nickname, string username, string user, string email, string cc)
        {
            this.nickname = nickname;
            this.username = username;
            this.user = user;
            this.cc = cc;
            this.email = email;
            SavePlayerData(user, email);
        }
public static String sha256_hash(String value) {
  StringBuilder Sb = new StringBuilder();

  using (SHA256 hash = SHA256Managed.Create()) {
    Encoding enc = Encoding.UTF8;
    Byte[] result = hash.ComputeHash(enc.GetBytes(value));

    foreach (Byte b in result)
      Sb.Append(b.ToString("x2"));
  }

  return Sb.ToString();
}
public IEnumerator ApplyLogin(string username, string user)
{
    SaveLocalData();

    LoginData loginData = new LoginData
    {
        username = username.ToLower(),
        password = user
    };

    string json = JsonConvert.SerializeObject(loginData, Formatting.None, new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore
    });

    using (UnityWebRequest request = new UnityWebRequest(url + "/v1/account/login", "POST"))
    {
        request.SetRequestHeader("content-type", "application/json");
        request.SetRequestHeader("User-Agent", Secret.UserAgent);
        request.SetRequestHeader("Referer", "https://api.jammerdash.com");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
                request.SetRequestHeader("x-client", "Jammer-Dash");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Ensure HTTPS
        if (!url.StartsWith("https"))
        {
            Notifications.instance.Notify("Login failed: insecure connection.", null);
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            HandleErrorResponse(request);
        }
        else
        {
            try
            {
                var successResponse = JObject.Parse(request.downloadHandler.text);
                string[] welcomeMessages = new string[]
                {
                    "Welcome back, {0}!",
                    "Hey there, {0}!",
                    "Successfully logged in as {0}!",
                    "{0} has joined the game.",
                    "こんにちは、{0}さん！"
                };

                System.Random random = new System.Random();
                int index = random.Next(welcomeMessages.Length);
                string welcomeMessage = string.Format(welcomeMessages[index], loginData.username);
                Notifications.instance.Notify(welcomeMessage, null);
                #if UNITY_EDITOR
                Debug.Log(successResponse);
                #endif
                // Extract basic information
                // Assign the fetched details
                string token = successResponse["token"].ToString();
                string nickname = successResponse["user"]["nickname"]?.ToString();
                string _username = successResponse["user"]["username"]?.ToString();
                string uuid = successResponse["user"]["id"].ToString();
                string cc = successResponse["user"]["cc"]?.ToString();
                string cn = successResponse["user"]["country"]?.ToString();
                string rg = successResponse["user"]["region"]?.ToString();
                string totalscore = successResponse["user"]["totalscore"]?.ToString();
                this.nickname = nickname;
                this.totalXP = long.Parse(totalscore);
                this.uuid = uuid;
                this.token = token;
                this.username = _username;
                this.user = user;
                this.cc = cc;
                this.country_name = cn;
                this.region = rg;
                // Fetch additional user details using the UUID
                StartCoroutine(FetchUserDetails(uuid, token));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing success response or fetching additional data: {ex}");
                Notifications.instance.Notify("Login succeeded, but an error occurred while fetching additional data.", null);
                SaveLocalData();
                loggedIn = true;
            }
        }
    }
}

public IEnumerator EditUser(string username, string mail, string nickname, string pfpLink) {
   
    LoginData loginData = new LoginData
    {
        uuid = uuid,
        username = username,
        email = mail,
        nickname = nickname,
        profile_picture = pfpLink
    };

    string json = JsonConvert.SerializeObject(loginData, Formatting.None, new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore
    });

    using (UnityWebRequest request = new UnityWebRequest(url + $"/v1/account/{uuid}/edit-user", "POST"))
    {
        request.SetRequestHeader("content-type", "application/json");
        request.SetRequestHeader("User-Agent", Secret.UserAgent);
        request.SetRequestHeader("Referer", "https://api.jammerdash.com");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
                request.SetRequestHeader("x-client", "Jammer-Dash");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Ensure HTTPS
        if (!url.StartsWith("https"))
        {
            Notifications.instance.Notify("Login failed: insecure connection.", null);
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            HandleErrorResponse(request);
        }
        else
        {
            try
            {
                var successResponse = JObject.Parse(request.downloadHandler.text);
                Notifications.instance.Notify($"Successfully edited {loginData.username}! Welcome back!", null);
                #if UNITY_EDITOR
                Debug.Log(successResponse);
                #endif
                // Extract basic information
                this.username = username;
                this.nickname = nickname;
                // Fetch additional user details using the UUID
                StartCoroutine(FetchUserDetails(uuid, token));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing success response or fetching additional data: {ex}");
                Notifications.instance.Notify("Login succeeded, but an error occurred while fetching additional data.", null);
                SaveLocalData();
                loggedIn = true;
            }
        }
    }
}

private void HandleErrorResponse(UnityWebRequest request)
{
    try
    {
        #if UNITY_EDITOR
        Debug.LogError($"Request Error: {request.error}");
        Debug.LogError($"Response Code: {request.responseCode}");
        Debug.LogError($"SSL/TLS Handshake Error: {request.downloadHandler.text}");
        #endif

        var errorResponse = JObject.Parse(request.downloadHandler.text);
        var errorMessage = errorResponse["error"]?.ToString(); // Extract error message safely

        if (!string.IsNullOrEmpty(errorMessage))
        {
            Notifications.instance.Notify($"Error: {errorMessage}.", null);
        }
        else
        {
            Notifications.instance.Notify("An unknown error occurred during login.", null);
        }
    }
    catch (Exception ex)
    {
        Notifications.instance.Notify($"An unexpected error occurred: {ex.Message}", null);
        Debug.LogError($"Request Error: {request.error}");
        Debug.LogError($"Response Code: {request.responseCode}");
        Debug.LogError($"SSL/TLS Handshake Error: {request.downloadHandler.text}");
        Debug.LogError($"Exception: {ex}");
    }
}

private IEnumerator FetchUserDetails(string uuid, string token)
{
    string apiUrl = $"https://api.jammerdash.com/v1/account/{uuid}";
    using (UnityWebRequest userRequest = UnityWebRequest.Get(apiUrl))
    {
        userRequest.SetRequestHeader("Authorization", $"Bearer {token}");
        userRequest.SetRequestHeader("User-Agent", Secret.UserAgent);
        userRequest.SetRequestHeader("Referer", "https://api.jammerdash.com");
        yield return userRequest.SendWebRequest();

        if (userRequest.result == UnityWebRequest.Result.ConnectionError || userRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Notifications.instance.Notify("Failed to fetch user details.", null);
        }
        else
        {
            try
            {
                var accountData = JObject.Parse(userRequest.downloadHandler.text);

                this.role = accountData["role_perms"]?.ToString();
                if (!string.IsNullOrEmpty(accountData["pfp_link"]?.ToString()))
                {
                    string pfpLink = accountData["pfp_link"].ToString();
                    StartCoroutine(DownloadProfilePicture(pfpLink));
                }
                else {
                    this.pfp = Resources.Load<UnityEngine.Texture>("defaultPFP");
                }

               string[] words = role.Split('_');

for (int i = 0; i < words.Length; i++)
{
    // Check if the word contains "jd", "Jd", or "JD"
    if (words[i].Equals("jd", StringComparison.OrdinalIgnoreCase) || words[i].Equals("Jd", StringComparison.OrdinalIgnoreCase))
    {
        words[i] = "JD";
    }
    else
    {
        // Capitalize the word
        words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i]);
    }

    if (words[i].Equals("Hizuru") && words[i].Equals("Chan"))
    {
        words[i] = "ひずるちゃん";
}
}

role = string.Join(" ", words);

                // Save local data and mark user as logged in
                SaveLocalData();
                loggedIn = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing additional user data: {ex}");
                Notifications.instance.Notify("Error processing user data. Please try again later.", null);
            }
        }
    }
}


        public void CalculateXPRequirements()
        {
            long initialXP = 100000L;
            float growthRate = 1.40f;
            xpRequiredPerLevel = new long[300];

            xpRequiredPerLevel[0] = initialXP;
            for (int i = 1; i < xpRequiredPerLevel.Length; i++)
            {
                xpRequiredPerLevel[i] = (long)(xpRequiredPerLevel[i - 1] * growthRate);
            }
        }

        public void SavePlayerData(string pass, string email)
        {
                        pass = sha256_hash(pass);
                       string ip = new WebClient().DownloadString("http://ipv4.icanhazip.com");
            LoginData loginData = new LoginData
            {
                nickname = nickname,
                username = username.ToLower(),
                email = email,
                password = pass,
                hardware_id = SystemInfo.deviceUniqueIdentifier,
                signup_ip = ip
            };

            // Register player
            StartCoroutine(Register(url + "/v1/account/signup", loginData, pass));
        }

        public IEnumerator Register(string url, LoginData bodyJsonObject, string inputPassword)
        {
            string bodyJsonString = JsonUtility.ToJson(bodyJsonObject); 
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.SetRequestHeader("content-type", "application/json");
                request.SetRequestHeader("Referer", "https://api.jammerdash.com");
                request.SetRequestHeader("Authorization", $"Bearer {token}");
                request.SetRequestHeader("User-Agent", Secret.UserAgent);
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJsonString);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                // Ensure HTTPS
                if (!url.StartsWith("https"))
                {
                    Debug.LogError("Insecure connection detected. HTTPS is required.");
                    Notifications.instance.Notify("Registration failed: insecure connection.", null);
                    yield break;
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    


                    try
                    {
                        var errorResponse = JObject.Parse(request.downloadHandler.text);
                        var errors = errorResponse["errors"];
                        Notifications.instance.Notify($"{errors.Count()} error(s) occurred. More info in the player logs (click).\nYour data has been saved locally.", () => Process.Start($@"{Path.Combine(Application.persistentDataPath, "Player.log")}"));
                        Debug.LogError(errors);
                    }
                    catch (Exception ex)
                    {
                        Notifications.instance.Notify("An unknown error occurred. Please try again.\nHowever, we've set up a local account for you.", null);
                        Debug.LogError($"Request Error: {request.error}");
                        Debug.LogError($"Response Code: {request.responseCode}");
                        Debug.LogError($"SSL/TLS Handshake Error: {request.downloadHandler.text}");
                        Debug.LogError(ex.Message);
                    }
                }
                else
                {

                    try
                    {
                        var successResponse = JObject.Parse(request.downloadHandler.text);
                        Notifications.instance.Notify($"Successfully registered as {bodyJsonObject.username}", null);

                        // Safely pass the password for immediate login if needed
                        StartCoroutine(ApplyLogin(bodyJsonObject.username, inputPassword));

                        // Mark user as logged in
                        loggedIn = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error parsing success response: {ex}");
                        Notifications.instance.Notify("Registration succeeded, but a response error occurred.", null);
                    }
                }

                // Clear sensitive data from memory
                inputPassword = null;
            }
        }

        void LoginData()
        {
            string path = Main.gamePath + "/loginData.dat"; // No file extension here

            if (File.Exists(path))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(LoginData));

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    LoginData login = (LoginData)formatter.Deserialize(stream);

                    // Apply login data and avoid calling login multiple times
                    if (!loggedIn)
                    {Debug.Log($"Deserialized password: {login.password}");
                        StartCoroutine(ApplyLogin(login.username, login.password));
                    }
                }
            }
            else
            {
                File.Create(path);
                Debug.LogError("Login data file not found.");
            }
        }

       public void Logout()
{
    StartCoroutine(CallLogout(url));
}

private IEnumerator CallLogout(string url)
{
    UnityWebRequest request = new UnityWebRequest(url, "POST");
    yield return request.SendWebRequest();
    string path = Path.Combine(Main.gamePath, "loginData.dat");
    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    {
        Debug.Log("Logged out.");
        Notifications.instance.Notify("Logged out.", null);
        loggedIn = false;
        File.Delete(path);
    }
    else
    {
        Debug.LogError("Error logging out: " + request.error);
    }
}        public PlayerData LoadData()
        {
            string path = Main.gamePath + "/playerData.dat";
            string playtime = Main.gamePath + "/playtime.dat";
            string play = File.ReadAllText(playtime);
            this.playtime = float.Parse(play);

            if (File.Exists(playtime)) {

            InvokeRepeating(nameof(SavePlaytime), 1, 1);
            }
            else {
                File.Create(playtime);
                SavePlaytime();
            }
            
            if (File.Exists(path))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(PlayerData));
                FileStream stream = new FileStream(path, FileMode.Open);

                PlayerData data = formatter.Deserialize(stream) as PlayerData;
                stream.Close();

                username = data.username;
                level = data.level;
                return data;
            }
            else
            {
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
            string playtimePath = Path.Combine(Main.gamePath, "playtime.dat");
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

    }
    [System.Serializable]
    public class PlayerData
    {
        [Header("Stats")]
        public int level;
        public long[] xpRequiredPerLevel;
        public long totalXP;
        public float playtime;
        public float sp;
        public int playCount;

        [Header("Login data")]
        public string username;
        public string nickname;
        public string password;
        public string email;
        public string loginToken;

        [Header("Profile info")]
        public string country;
        public bool isLocal;
        public bool isOnline;
        public string id;
        public string token;
    }

    public class LoginData
    {
        public string uuid;
        public string nickname;
        public string username;
        public string email;
        public string password;
        public string token;

        public string role_perms;

        public bool is_suspended;
        public string hardware_id;

        public string signup_ip;

        public string profile_picture;
    }

}
