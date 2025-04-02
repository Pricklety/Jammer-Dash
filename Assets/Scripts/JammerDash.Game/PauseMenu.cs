using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JammerDash.Tech;
using JammerDash.Game.Player;
using JammerDash.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.Video;

namespace JammerDash.Game
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject panel;

        public Text song;
        public Text creator;
        public Text info;
        public int attint;
        public GameObject canvas;
        public AudioSource music;
        public VideoPlayer video;
        public Slider dim;
        public RawImage image;
        // Start is called before the first frame update
        void Start()
        {
            music = AudioManager.Instance.source;
            SettingsData sd = SettingsFileHandler.LoadSettingsFromFile();
            dim.value = sd.dim;
           
                string levelName = CustomLevelDataManager.Instance.artist + " - " + CustomLevelDataManager.Instance.levelName;
                string creator = $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "mapped by")} {CustomLevelDataManager.Instance.creator}";
                this.creator.text = creator;
                song.text = levelName;
            
           video = FindFirstObjectByType<VideoPlayer>();
        }

      
      
        string FormatTime(float time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            return string.Format("{0:D2}:{0:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && (CustomLevelDataManager.Instance.modStates.ContainsKey(ModType.noDeath) ? CustomLevelDataManager.Instance.modStates.ContainsKey(ModType.noDeath) : GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().health > 0) && GameObject.FindGameObjectWithTag("Player").transform.position.x < FindFirstObjectByType<FinishLine>().transform.position.x && (GameObject.FindGameObjectWithTag("Player").transform.position != new Vector3(0, -1, 0)))
            {

                PlayerPrefs.SetInt("attempts", attint);
                PlayerPrefs.Save();
                if (Time.timeScale == 1f)
                {
                    panel.SetActive(true);
                }
                else if (Time.timeScale == 0f)
                {
                    music.time = GameObject.FindGameObjectWithTag("Player").transform.position.x / 7;
                    panel.SetActive(false);
                }

                if (panel.activeSelf && Time.timeScale == 1f)
                {

                    music.Pause();
                    video.Pause();
                    Time.timeScale = 0;
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().enabled = false;
                }
                else if (!panel.activeSelf)
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

        void FixedUpdate()
        {
            image.color = new(image.color.r, image.color.g, image.color.b, dim.value);
              if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>() != null && music == null)
                music = AudioManager.Instance.source;
        }

        void OnApplicationFocus(bool focus)
        {
            if (!focus && canvas.activeSelf && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().FindNearestCubeDistance() < 21) // Only execute when focus is lost
            {
                if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().health > 0 && GameObject.FindGameObjectWithTag("Player").transform.position.x < FindFirstObjectByType<FinishLine>().transform.position.x && (GameObject.FindGameObjectWithTag("Player").transform.position != new Vector3(0, -1, 0)))
                {

                    panel.SetActive(true);
                    music.time = GameObject.FindGameObjectWithTag("Player").transform.position.x / 7;
                    music.Pause();
                    Time.timeScale = 0;
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().enabled = false;
                    video.Pause();

                }
            }
        }


        public void Resume()
        {
            
            AudioSource music = AudioManager.Instance.source;

            panel.SetActive(false);
            GameObject.Find("loadingText").GetComponent<Text>().text = "Click anything to continue";

            StartCoroutine(ResumeAfterDelay(music));
        }
        IEnumerator ResumeAfterDelay(AudioSource music)
        {
            Debug.Log("hi");
            float startTime = Time.realtimeSinceStartup;
            float startPitch = music.pitch;
            float startScale = Time.timeScale;
            yield return new WaitUntil(() => Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape));
            GameObject.Find("loadingText").GetComponent<Text>().text = "";
            // Check if the player exists and is not at the starting position
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            music.UnPause();
            video.Play();
            player.GetComponent<PlayerMovement>().enabled = true;
            Time.timeScale = 1f;
            music.pitch = Time.timeScale;
            video.playbackSpeed = Time.timeScale;
            




        }


        public void Menu()
        {
            Time.timeScale = 1;
            music.UnPause();
            SceneManager.LoadSceneAsync(1);
            CustomLevelDataManager.Instance.enabled = false;
        }

        public void Restart()
        {
            if (SceneManager.GetActiveScene().name != "LevelDefault")
            {
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
                music.UnPause();
                Time.timeScale = 1;
            }
        }
    }
}