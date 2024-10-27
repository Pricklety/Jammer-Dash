using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using JammerDash.Game.Player;
using UnityEngine.UI;
using JammerDash.Tech;
using TMPro;

namespace JammerDash.Game
{
    public class cameraColor : MonoBehaviour
    {
        [Range(0.35f, 3f)]
        public float duration = 1f;

        public PlayerMovement player;
        public AudioSource song;
        public GameObject player0;
        public GameObject player1;
        bool started = false;
        public TextMeshProUGUI infotext;

        public Transform ground;

        private void Start()
        {

            song.pitch = 0;
            player.enabled = false;
            StartCoroutine(LateStart());
           
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }

        }

        IEnumerator LateStart()
        {
            if (player0 != null && player1 != null)
            {
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (data.playerType == 0)
                {
                    Destroy(player1);
                    player0.SetActive(true);
                    song.pitch = 0;
                    player.enabled = false;
                }
                else if (data.playerType == 1)
                {
                    Destroy(player0);
                    player1.SetActive(true);
                    song.pitch = 0;
                    player.enabled = false;
                }
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
                song = GameObject.Find("Music").GetComponent<AudioSource>();
            }
            else
            {
                player0 = GameObject.FindGameObjectWithTag("Player");
            }
            song.pitch = 0;
            player.enabled = false;
            new WaitForSecondsRealtime(0.5f);
            
            yield return new WaitForSeconds(4f); // Delay for 4 seconds

            started = true;
            player.enabled = true; // Enable the player movement after the delay
            GetComponent<Animator>().enabled = false;
            song.volume = 1f;
            song.pitch = 1f;
            song.Play();
        }

        private void Update()
        {
            if (CustomLevelDataManager.Instance.data.bpm != 0)
            {
                duration = 60 / CustomLevelDataManager.Instance.data.bpm;

            }
            infotext.text = $"{CustomLevelDataManager.Instance.levelName} by {CustomLevelDataManager.Instance.creator}\n" +
                $"? {CustomLevelDataManager.Instance.data.artist} - {CustomLevelDataManager.Instance.data.songName}";
           
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            song = GameObject.Find("Music").GetComponent<AudioSource>();

            if (player.health > 0 || player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
                song = GameObject.Find("Music").GetComponent<AudioSource>();

            }

            
        }

       
    }
}