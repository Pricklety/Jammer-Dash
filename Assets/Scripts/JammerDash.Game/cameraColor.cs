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
using JammerDash.Audio;

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
        public RawImage image;

        private void Start()
        {

            song = AudioManager.Instance.source;
            song.pitch = 0;
            player.enabled = false;
            StartCoroutine(LateStart());
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }

        }

        IEnumerator LateStart()
        {
            StartCoroutine(CustomLevelDataManager.Instance.LoadImage(Path.Combine(Application.persistentDataPath, "levels", "extracted", $"{CustomLevelDataManager.Instance.ID} - {CustomLevelDataManager.Instance.levelName}", "bgImage.png"), image));

            if (player0 != null && player1 != null)
            {
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (data.playerType == 0)
                {
                    Destroy(player1);
                    player0.SetActive(true);
                    player.enabled = false;
                }
                else if (data.playerType == 1)
                {
                    Destroy(player0);
                    player1.SetActive(true);
                    player.enabled = false;
                }
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
                song = AudioManager.Instance.source;
            }
            else
            {
                player0 = GameObject.FindGameObjectWithTag("Player");
            }
            song.Stop();
            player.enabled = false;
            new WaitForSecondsRealtime(0.5f);
            
            yield return new WaitForSeconds(4f); // Delay for 4 seconds

            started = true;
            player.enabled = true; // Enable the player movement after the delay
            GetComponent<Animator>().enabled = false;
            song.pitch = 1f;
            song.volume = 1f;
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
            song = AudioManager.Instance.source;

            if (player.health > 0 || player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
                song = AudioManager.Instance.source;

            }

            
        }

       
    }
}