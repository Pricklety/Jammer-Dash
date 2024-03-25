using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiscordPresence;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Linq;

public class DiscordRPC : MonoBehaviour
{

    public string detail;
    public string state;
    public string largeimagekey;
    public string largetext;
    public string smallimagekey;
    public string smalltext;
    public Int64 start;

    GameObject player;
    GameObject pause;


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        start = GetCurrentTimeAsLong();
    }
    private void FixedUpdate()
    {
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
       

        #region Large Text
        largeimagekey = "logo";
#if DEBUG
        state = "Development Build";
        detail = "Debugging";
#endif
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            state = "Loading up...";
            detail = $"{Application.version}";

        }
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            state = "Main Menu";
            smallimagekey = "shine";
            smallimagekey = "Total sn: N/A";
            largetext = "Guest (#N/A)";
            detail = "";
        }
        else if (SceneManager.GetActiveScene().buildIndex == 2)
        {

            player = GameObject.FindGameObjectWithTag("Player");
            state = SceneManager.GetActiveScene().name + " by Pricklety";
            detail = "";
            smallimagekey = "shine";
            smalltext = $"-- sn";
            largetext = "Guest (#N/A)";

        }
        else if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            state = "Learning";
            detail = "Tutorial";
        }

        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            state = "Editing " + GameObject.FindObjectOfType<EditorManager>().sceneNameInput.text;
            detail = GameObject.FindObjectOfType<EditorManager>().objectCount.text + " obj";
            largetext = "N/A jams";
        }

        if (SceneManager.GetActiveScene().buildIndex >= 6)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            state = SceneManager.GetActiveScene().name + " by Pricklety";
            detail = "";
            smallimagekey = "shine";
            smalltext = $"-- sn"; // Implement server connection to server.Level.difficulty
            largetext = "Guest (#N/A)";
        }

        if (SceneManager.GetActiveScene().name == "LevelDefault")
        {
            player = GameObject.FindGameObjectWithTag("Player");
            state = "";
            detail = $"{LevelDataManager.Instance.levelName} by {LevelDataManager.Instance.creator}";
            smallimagekey = "shine";
            smalltext = $"{LevelDataManager.Instance.diff} sn";
            largetext = "Guest (#N/A)";
        }
        
        
        #endregion

    }

    private long GetCurrentTimeAsLong()
    {
        DateTime currentTime = DateTime.UtcNow;
        long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
        return unixTime;
    }
}
