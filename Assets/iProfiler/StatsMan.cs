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
using UnityEngine.Networking.Types;
using Ping = UnityEngine.Ping;
using System.Collections;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;

public class StatsMan : MonoBehaviour
{
    public Color tx_Color = Color.white;
    public Text gui;
    Queue<float> fpsValues = new Queue<float>();
    float updateInterval = 0.1f;
    float lastInterval; // Last interval end time
    float frames = 0; // Frames over the current interval

    float framesavtick = 0;
    float framesav = 0.0f;
    float ping;

    [SerializeField]
    private AudioMixer audioMixer; // Reference to your AudioMixer
    private AudioSource musicSource;

    // Use this for initialization
    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
        framesav = 0;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        musicSource = FindObjectOfType<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        ++frames;
        var timeNow = Time.realtimeSinceStartup;

        if (timeNow > lastInterval + updateInterval)
        {
            float fps = frames / (timeNow - lastInterval);
            float ms = 1000.0f / Mathf.Max(fps, 0f);


            gui.text = $"Debug 9 - FPS: {fps:f0} ({ms:f1}ms)\n";

            DisplayApplicationInfo();
            DisplayAudioInfo();
            DisplayInputInfo();
            DisplayNetworkInfo();
            DisplayOptionsInfo();
            DisplaySystemInfo();
            DisplayGraphicsInfo();
            DisplayVideoInfo();

            frames = 0;
            lastInterval = timeNow;
        }
    } 
    
    void DisplaySystemInfo()
    {
        gui.text += "\n\n" + "System Memory: " + (SystemInfo.systemMemorySize / 1000).ToString("f2") + "GB" +
                    "\nProcessor Type: " + SystemInfo.processorType +
                    "\nProcessor Count: " + SystemInfo.processorCount +
                    "\nDevice Model: " + SystemInfo.deviceModel +
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
                    "\nShader Level: " + SystemInfo.graphicsShaderLevel +
                    "\nRender Texture Formats: " + string.Join(", ", SystemInfo.supportedRenderTargetCount);
    }

    void DisplayAudioInfo()
    {
        gui.text += "\n\n" + "Audio Mixer: " + (audioMixer != null ? audioMixer.name : "N/A");

        if (musicSource != null && musicSource.name == "mainmenu")
        {
            gui.text += "\nMusic Clip: " + (musicSource.clip != null ? musicSource.clip.name : "N/A") + 
                        "\nCurrent song index: " + musicSource.GetComponent<AudioManager>().currentClipIndex;
        }
    }

    void DisplayOptionsInfo()
    {
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();

        gui.text += "Resolution value: " + data.resolutionValue + $" ({Screen.width}x{Screen.height})"+
                    "\nScreen Mode: " + data.windowMode + $" ({Screen.fullScreenMode})"+
                    "\nQuality Level: " + data.qualitySettingsLevel + $" ({GetQualityLevelName()})" +
                    "\nArtistic Backgrounds: " + data.artBG + $" ({Resources.LoadAll<Sprite>("backgrounds").Length} bgs)" +
                    "\nCustom Backgrounds: " + data.customBG + $" ({Directory.GetFiles(Application.persistentDataPath + "/backgrounds", "*.png").Length} bgs)" +
                    "\nVideo Backgrounds: " + data.vidBG + $" ({Directory.GetFiles(Application.persistentDataPath + "/backgrounds", "*.mp4").Length} bgs)" +
                    "\nSFX: " + data.sfx + 
                    "\nHit Notes: " + data.hitNotes + 
                    "\nPlayer Type: " + data.playerType + 
                    "\nAntialiasing: " + data.antialiasing + 
                    "\nCursor Trail: " + data.cursorTrail + 
                    "\nVisualizers: {" + data.allVisualizers + "," + data.lineVisualizer + "," + data.logoVisualizer + "," + data.bgVisualizer + "}";

    }
        
    void DisplayVideoInfo()
    {
        gui.text += "\n\n" + "Current Resolution: " + Screen.currentResolution.width + "x" + Screen.currentResolution.height +
                    "\nScreen DPI: " + Screen.dpi +
                    "\nScreen Orientation: " + Screen.orientation +
                    "\nScreen Full Screen: " + Screen.fullScreen +
                    "\nScreen Sleep Timeout: " + Screen.sleepTimeout +
                    "\nConnected Displays: " + Display.displays.Length;
    }


    void DisplayInputInfo()
    {
        gui.text += "\n\n" + "Touch Support: " + Input.touchSupported +
                    "\nInput count: " + Input.touchCount +
                    "\nInput Acceleration: " + $"{Input.GetAxisRaw("Mouse X")},  {Input.GetAxisRaw("Mouse Y")}";
    }

    void DisplayNetworkInfo()
    {
        

        string info = "\n\n";

        // Internet reachability
        info += "Internet Reachability: " + Application.internetReachability + "\n\n";

        // Network interface information
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface iface in interfaces)
        {
            info += "Network: " + iface.Name + "\n";
            info += "Status: " + iface.OperationalStatus + "\n";
            info += "Speed: " + (iface.Speed / 1000000) + " Mbps\n";
            info += "Multicast: " + iface.SupportsMulticast + "\n\n";

        }
        
        StartCoroutine(GetPingToGoogle((pingTime) =>
        {
            ping = pingTime;
        }));
        info += "Ping: " + ping + "ms\n\n";
        gui.text += info;
    }

    void DisplayApplicationInfo()
    {
        gui.text += "\n\n" + "Made in Unity " + Application.unityVersion +
                    "\nTarget Frame Rate: " + Application.targetFrameRate +
                    "\nGame version: " + Application.version +
                    "\nVSync: " + QualitySettings.vSyncCount;
    }

    private string[] qualityLevelNames = new string[]
    {
       "Low", "Medium", "High"
    };
    public string GetQualityLevelName()
    {
        int qualityLevelIndex = QualitySettings.GetQualityLevel();
        if (qualityLevelIndex >= 0 && qualityLevelIndex <= qualityLevelNames.Length)
        {
            return qualityLevelNames[qualityLevelIndex];
        }
        else
        {
            return "Unknown";
        }
    }

    IEnumerator GetPingToGoogle(Action<float> callback)
    {
        const float trackingDuration = 1; // Duration to track ping times (in seconds)
        List<float> pingTimes = new List<float>();

        // Track ping times for the specified duration
        float startTime = Time.time;
        while (Time.time - startTime < trackingDuration)
        {
            Ping ping = new Ping("jammerdash.com");
            while (!ping.isDone)
            {
                yield return null;
            }
            pingTimes.Add(ping.time);
            yield return new WaitForSeconds(1f); // Wait for 1 second before checking again
        }

        // Calculate average ping time
        float totalPingTime = 0f;
        foreach (float time in pingTimes)
        {
            totalPingTime += time;
        }
        float averagePingTime = pingTimes.Count > 0 ? totalPingTime / pingTimes.Count : -1f;

        callback(averagePingTime);
    }

}
