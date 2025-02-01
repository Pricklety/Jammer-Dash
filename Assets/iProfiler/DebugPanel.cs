using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;
using JammerDash.Audio;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.IO;

namespace JammerDash.Tech
{
    public class DebugPanel : MonoBehaviour
    {
        public Text gui;

        [SerializeField]
        private AudioMixer audioMixer;
        private AudioSource musicSource;
        void FixedUpdate()
        {
            gui.text = $"Debug v14 - Jammer Dash {Application.version}\n\n";

            DisplayAccountInfo();
            DisplayAudioInfo();
            DisplaySystemInfo();
            DisplayGraphicsInfo();

            
        }
        void DisplayAccountInfo()
        {
            // Display account information and ping status
            gui.text += "Username: " + Account.Instance.username +
                        "\nExperience: " + Account.Instance.totalXP +
                        "\nLogged in: " + Account.Instance.loggedIn +
                        "\nPlaytime: " + Account.Instance.playtime +
                        "\nScores saved: " + File.ReadAllLines(Main.gamePath + "/scores.dat").Length;
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
