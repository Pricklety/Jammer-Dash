using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Net.NetworkInformation;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using Ping = UnityEngine.Ping;
using System.Collections;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using JammerDash.Audio;

namespace JammerDash.Tech
{
    public class DebugPanel : MonoBehaviour
    {
        public Text gui;


        [SerializeField]
        private AudioMixer audioMixer; // Reference to your AudioMixer
        private AudioSource musicSource;

        // Update is called once per frame
        void FixedUpdate()
        {
            gui.text = $"Debug v13 - Jammer Dash {Application.version}\n\n";

            DisplayAccountInfo();
            DisplayAudioInfo();
            DisplaySystemInfo();
            DisplayGraphicsInfo();

        }

        void DisplayAccountInfo()
        {
            gui.text += "Username: " + Account.Instance.username +
                        "\nExperience: " + Account.Instance.totalXP +
                        "\nLogged in: " + Account.Instance.LoadData();
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
