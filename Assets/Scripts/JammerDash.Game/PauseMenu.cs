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
        public Slider dim;
        public RawImage image;
        // Start is called before the first frame update
        void Start()
        {

            SettingsData sd = SettingsFileHandler.LoadSettingsFromFile();
            dim.value = sd.dim;
            GameObject animA = GameObject.Find("Main Camera");
           
                string levelName = CustomLevelDataManager.Instance.levelName;
                string creator = $"by {CustomLevelDataManager.Instance.creator}";
                this.creator.text = creator;
                song.text = levelName;
            
           
        }

        public IEnumerator CheckUI()
        {
            SettingsData sd = SettingsFileHandler.LoadSettingsFromFile();
            if (sd.canvasOff)
            {
                canvas.SetActive(false);
                Notifications.instance.Notify($"Playfield UI is off.\nClick Shift+{KeybindingManager.toggleUI.ToString()} to turn it back on.", null);
                yield return null;
            }
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

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeybindingManager.toggleUI))
            {
                SettingsData sd = SettingsFileHandler.LoadSettingsFromFile();
                sd.canvasOff = true;
                SettingsFileHandler.SaveSettingsToFile(sd);
                CheckUI();
            }
            image.color = new(image.color.r, image.color.g, image.color.b, dim.value);
            if (Input.GetKeyDown(KeyCode.Escape) && canvas.activeSelf && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().health > 0 && GameObject.FindGameObjectWithTag("Player").transform.position.x < FindObjectOfType<FinishLine>().transform.position.x && (GameObject.FindGameObjectWithTag("Player").transform.position != new Vector3(0, -1, 0)))
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

                if (panel.active && Time.timeScale == 1f)
                {

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
            if (!focus && canvas.activeSelf && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().FindNearestCubeDistance() < 21) // Only execute when focus is lost
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
            GameObject.Find("loadingText").GetComponent<Text>().text = "Click anything to continue";

            StartCoroutine(ResumeAfterDelay(music));
        }
        IEnumerator ResumeAfterDelay(AudioSource music)
        {
            Debug.Log("hi");
            float startTime = Time.realtimeSinceStartup;
            float duration = 1f; // Duration for the time scale transition
            float startPitch = music.pitch;
            float startScale = Time.timeScale;
            yield return new WaitUntil(() => Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape));
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
            CustomLevelDataManager.Instance.enabled = false;
        }

        public void Restart()
        {
            if (SceneManager.GetActiveScene().name != "LevelDefault")
            {
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
                Time.timeScale = 1;
            }
        }
    }
}