using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    public GameObject panel;

    public Text song;
    public Text info;
    public int attint;
    private float startTime;
    private Animator anim;
    private SceneData data;
    public AudioSource music;
    // Start is called before the first frame update
    void Start()
    {
        
        GameObject animA = GameObject.Find("Main Camera");
        anim = animA.GetComponent<Animator>();
        startTime = Time.time;
        if (SceneManager.GetActiveScene().buildIndex < 25 && SceneManager.GetActiveScene().name != "LevelDefault")
        {

            song.text = GameObject.Find("Music").GetComponent<AudioSource>().clip.name;
        }
        else if (SceneManager.GetActiveScene().name == "LevelDefault")
        {

            LoadSceneData(LevelDataManager.Instance.levelName);
        }
        if (PlayerPrefs.HasKey("attempts"))
        {
            attint = PlayerPrefs.GetInt("attempts", 0); // Set attint from PlayerPrefs
            if (attint < 1)
            {
                attint = 1;
            }
        }
    }

    // Function to load SceneData for a specific level
    private SceneData LoadSceneData(string levelName)
    {
        SceneData loadedData = new SceneData();

        // Construct the path to the level data file
        string filePath = Path.Combine(Application.persistentDataPath, "scenes", levelName, levelName + ".json");

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the JSON file
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON string to SceneData
            loadedData = SceneData.FromJson(json);
        }
        song.text = loadedData.levelName;
        if (info != null)
        {

            info.text = $"Difficulty: {(int)loadedData.calculatedDifficulty}sn\nLevel-Specific BPM: {loadedData.bpm}\nLength: {FormatTime(loadedData.levelLength)}";
        }
        else
        {

        }
        return loadedData;
    }
    string FormatTime(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        return string.Format("{0:D2}:{0:D2}", timeSpan.Minutes, timeSpan.Seconds);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>() != null)
            music = GameObject.Find("Music").GetComponent<AudioSource>();


        if (Input.GetKeyDown(KeyCode.Escape) && GameObject.Find("loadingText").GetComponent<Text>().text == "" && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().health > 0 && GameObject.FindGameObjectWithTag("Player").transform.position.x < FindObjectOfType<FinishLine>().transform.position.x && (GameObject.FindGameObjectWithTag("Player").transform.position != new Vector3(0, -1, 0)))
        {
            
            PlayerPrefs.SetInt("attempts", attint);
            PlayerPrefs.Save();
            panel.SetActive(!panel.activeSelf);

            if (panel.active && Time.timeScale == 1f)
            {
                music.time = GameObject.FindGameObjectWithTag("Player").transform.position.x / 7;
                music.pitch = 0;
                Time.timeScale = 0;
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().enabled = false;
            }
            else if (!panel.active)
            {
                Resume();

            }
            
        }
        bool focus = Application.isFocused;
        if (!focus)
        {
            OnApplicationFocus(focus);
        }
        
        if (Time.timeScale > 0f)
        {
            GameObject.Find("loadingText").GetComponent<Text>().text = "";
        }
    }

    void OnApplicationFocus(bool focus)
    {
        if (!focus) // Only execute when focus is lost
        {
            if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().health > 0 && GameObject.FindGameObjectWithTag("Player").transform.position.x < FindObjectOfType<FinishLine>().transform.position.x && (GameObject.FindGameObjectWithTag("Player").transform.position != new Vector3(0, -1, 0)))
            {
               
                panel.SetActive(true);
                music.time = GameObject.FindGameObjectWithTag("Player").transform.position.x / 7;
                    music.pitch = 0;
                    Time.timeScale = 0;
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().enabled = false;
                
            }
        }
    }


    public void Resume()
    {
        GameObject obj01 = GameObject.Find("Music");
        AudioSource music = obj01.GetComponent<AudioSource>();

        panel.SetActive(false);
        GameObject.Find("loadingText").GetComponent<Text>().text = "Click the left mouse to continue";

        StartCoroutine(ResumeAfterDelay(music));
    }
    IEnumerator ResumeAfterDelay(AudioSource music)
    {
        Debug.Log("hi");
        float startTime = Time.realtimeSinceStartup;
        float duration = 1f; // Duration for the time scale transition
        float startPitch = music.pitch;
        float startScale = Time.timeScale;
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        GameObject.Find("loadingText").GetComponent<Text>().text = "";
            // Check if the player exists and is not at the starting position
            GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && (player.transform.position != new Vector3(0, -1, 0) || player.transform.position != FindObjectOfType<FinishLine>().transform.position))
        {
            if (music.pitch < 1)
            {
                player.GetComponent<PlayerMovement>().enabled = true;
                while (Time.realtimeSinceStartup - startTime < duration + 0.01f)
                {
                    float t = (Time.realtimeSinceStartup - startTime) / duration;
                    music.pitch = Mathf.Lerp(startPitch, 1f, t);
                    Time.timeScale = Mathf.Lerp(startScale, 1f, t);
                    yield return null; // Wait for the next frame
                }
            }
        }
        else
        {
            Time.timeScale = 1f;
        }

        // Ensure that time scale and pitch are set to 1
        music.pitch = 1f;
        Time.timeScale = 1f;

        // Wait for 1 real-time second before exiting the coroutine 
        yield return new WaitForSecondsRealtime(1f);
        

        
    }


    public void Menu()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(1);
    }

    public void Restart()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }
}
