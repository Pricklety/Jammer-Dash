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
            gui.text = $"Debug v12 - Jammer Dash {Application.version} ({Application.unityVersion})\n\n";

            DisplayAudioInfo();
            DisplayInputInfo();
            DisplayOptionsInfo();
            DisplaySystemInfo();
            DisplayGraphicsInfo();
            DisplayVideoInfo();

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
            gui.text += "Audio Mixer: " + (audioMixer != null ? audioMixer.name : "N/A");

            if (musicSource != null && musicSource.name == "mainmenu")
            {
                gui.text += "\nMusic Clip: " + (musicSource.clip != null ? musicSource.clip.name : "N/A") +
                            "\nCurrent song index: " + musicSource.GetComponent<AudioManager>().currentClipIndex;
            }
            else
            {
                musicSource = GameObject.Find("mainmenu").GetComponent<AudioSource>();
            }
        }

        void DisplayOptionsInfo()
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
          

            string bg = "";
            switch (data.bgTime)
            {
                case 1:
                    bg = "On song change";
                    break;
                case 2:
                    bg = "Every 15s";
                    break;
                case 3:
                    bg = "Every 30s";
                    break;
            }
            string v = data.canvasOff ? "Off" : "On";

            gui.text += "\n\nResolution value: " + data.resolutionValue + $" ({Screen.width}x{Screen.height})" +
                        "\nArtistic Backgrounds: " + data.artBG + $" ({Resources.LoadAll<Sprite>("backgrounds").Length} bgs)" +
                        "\nCustom Backgrounds: " + data.customBG + $" ({Directory.GetFiles(Application.persistentDataPath + "/backgrounds", "*.png").Length} bgs)" +
                        "\nVideo Backgrounds: " + data.vidBG + $" ({Directory.GetFiles(Application.persistentDataPath + "/backgrounds", "*.mp4").Length} bgs)" +
                        "\nSFX: " + data.sfx +
                        "\nHit Notes: " + data.hitNotes +
                        "\nPlayer Type: " + data.playerType +
                        "\nCursor Trail: " + data.cursorTrail +
                        "\nNo focus volume: " + data.noFocusVolume +
                        "\nLowpass value: " + data.lowpassValue +
                        "\nMouse particle count: " + data.mouseParticles +
                        "\nShowing FPS: " + data.isShowingFPS +
                        "\nParallax: " + data.parallax +
                        "\nRandom hit sounds: " + data.randomSFX +
                        "\nConfined mouse: " + data.confinedMouse +
                        "\nBackground Time: " + bg +
                        "\nMouse wheel volume: " + data.wheelShortcut +
                        "\nIncreased game volume: " + data.volumeIncrease +
                        "\nSnow: " + data.snow +
                        "\nGameplay Canvas: " + v +
                        "\nBass: " + data.bass +
                        "\nBass Gain: " + data.bassgain + "Hz";


        }

        void DisplayVideoInfo()
        {
            gui.text += "\n\nScreen Full Screen: " + Screen.fullScreen +
                        "\nConnected Displays: " + Display.displays.Length;
        }


        void DisplayInputInfo()
        {
            gui.text += "\n\nTouch Support: " + Input.touchSupported +
                        "\nTouch count: " + Input.touchCount + "\n\n" +
                        "\nPolling frequency: " + InputSystem.pollingFrequency + "Hz" +
                        "\nButton press point: " + InputSystem.settings.defaultButtonPressPoint +
                        "\nInput update mode: " + InputSystem.settings.updateMode +
                        "\nInput processing time: " + InputSystem.metrics.averageProcessingTimePerEvent.ToString("f5") + " seconds";
        }


        

    }
}
