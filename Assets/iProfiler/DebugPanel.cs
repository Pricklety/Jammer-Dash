using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;
using JammerDash.Audio;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

namespace JammerDash.Tech
{
    public class DebugPanel : MonoBehaviour
    {
        public Text gui;

        [SerializeField]
        private AudioMixer audioMixer;
        private AudioSource musicSource;
        private float pingTime = -1f;

        private float pingCooldown = 60f; 
        private float nextPingTime = 0f;
        // Update is called once per frame
        void FixedUpdate()
        {
            gui.text = $"Debug v13 - Jammer Dash {Application.version}\n\n";

            DisplayAccountInfo();
            DisplayAudioInfo();
            DisplaySystemInfo();
            DisplayGraphicsInfo();

            // Send a ping request every cooldown period
            if (Time.time >= nextPingTime)
            {
                nextPingTime = Time.time + pingCooldown;  // Reset the next ping time
                StartPing();  // Start the ping request
            }
        }

        void DisplayAccountInfo()
        {
            // Display account information and ping status
            gui.text += "Username: " + Account.Instance.username +
                        "\nExperience: " + Account.Instance.totalXP +
                        "\nLogged in: " + Account.Instance.loggedIn;

            // Display the ping time if it's available, else show "Calculating..."
            if (pingTime >= 0)
            {
                gui.text += "\nPing: " + Mathf.RoundToInt(pingTime) + " ms";
            }
            else
            {
                gui.text += "\nPing: Calculating...";
            }
        }

        // Start the coroutine to ping the server
        public void StartPing()
        {
            StartCoroutine(CallAPI());
        }

        // Coroutine to call the API and measure the ping
        IEnumerator CallAPI()
        {
            string pingUrl = $"https://api.jammerdash.com/v1/account/{Account.Instance.uuid}";
            using (UnityWebRequest www = UnityWebRequest.Get(pingUrl))
            {
                float startTime = Time.time;
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    float endTime = Time.time;
                    pingTime = (endTime - startTime) * 1000f;  // Convert to milliseconds
                    Debug.Log("API Response: " + www.downloadHandler.text);
                    var r = JObject.Parse(www.downloadHandler.text);
                    Account.Instance.nickname = r["nickname"].ToString();
                }
                else
                {
                    Debug.LogError("Ping failed: " + www.error);
                    pingTime = -1f;  // Set ping to -1 in case of an error
                }
            }
        }

        void DisplaySystemInfo()
        {
            gui.text += "\n\nSystem Memory: " + (SystemInfo.systemMemorySize / 1000).ToString("f2") + "GB" +
                        "\nProcessor Type: " + SystemInfo.processorType +
                        "\nProcessor Count: " + SystemInfo.processorCount +
                        "\nDevice Type: " + SystemInfo.deviceType +
                        "\nOperating System: " + SystemInfo.operatingSystem +
                        "\nCPU Speed: " + SystemInfo.processorFrequency + "MHz" +
                        "\nSystem Language: " + Application.systemLanguage;
        }

        void DisplayGraphicsInfo()
        {
            gui.text += "\n\n" + "GPU: " + SystemInfo.graphicsDeviceName +
                        "\nGPU Type: " + SystemInfo.graphicsDeviceType +
                        "\nGPU Version: " + SystemInfo.graphicsDeviceVersion +
                        "\nGPU Memory: " + SystemInfo.graphicsMemorySize + "MB" +
                        "\nMultithreading: " + SystemInfo.graphicsMultiThreaded;
        }

        void DisplayAudioInfo()
        {
            gui.text += "\n\nAudio Mixer: " + (audioMixer != null ? audioMixer.name : "N/A");

            if (musicSource != null && musicSource.name == "mainmenu")
            {
                gui.text += "\nMusic Clip: " + (musicSource.clip != null ? musicSource.clip.name : "N/A") +
                            "\nCurrent song index: " + musicSource.GetComponent<AudioManager>().currentClipIndex;
            }
            else
            {
                musicSource = AudioManager.Instance.source;
            }
        }
    }
}
