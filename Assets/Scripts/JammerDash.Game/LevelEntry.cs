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
using UnityEngine.Video;

namespace JammerDash.Game
{
    public class LevelEntry : MonoBehaviour
    {

        public PlayerMovement player;
        public AudioSource song;
        public VideoPlayer video;
        public GameObject player0;
        public GameObject player1;
        public GameObject player2;
        bool started = false;
        public TextMeshProUGUI infotext;

        public RawImage image;

        private void Start()
        {
            song = AudioManager.Instance.source;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }
            
            StartCoroutine(LateStart());
        }

        IEnumerator LateStart()
        {
            StartCoroutine(CustomLevelDataManager.Instance.LoadImage(Path.Combine(Main.gamePath, "levels", "extracted", $"{CustomLevelDataManager.Instance.ID} - {CustomLevelDataManager.Instance.levelName}", "bgImage.png"), image));
            
            if (player0 != null && player1 != null && player2 != null)
            {
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (data.playerType == 0)
                {
                    Destroy(player1);
                    Destroy(player2);
                    player0.SetActive(true);
                    player.enabled = false;
                }
                else if (data.playerType == 1)
                {
                    Destroy(player0);
                    Destroy(player2);
                    player1.SetActive(true);
                    player.enabled = false;
                }
                else if (data.playerType == 2)
                {
                    Destroy(player0);
                    Destroy(player1);
                    player2.SetActive(true);
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
            player.enabled = true;
            new WaitForSecondsRealtime(0.5f);
            
            yield return new WaitForSeconds(3f); // Delay for 4 seconds

            started = true;
            player.enabled = true; // Enable the player movement after the delay
            
            song.Play(); 
            if (File.Exists(Path.Combine(Main.gamePath, "levels", "extracted", $"{CustomLevelDataManager.Instance.ID} - {CustomLevelDataManager.Instance.levelName}", "backgroundVideo.mp4")))
            {
                image.gameObject.SetActive(true);
                image.texture = video.targetTexture;
                video.url = Path.Combine(Main.gamePath, "levels", "extracted", $"{CustomLevelDataManager.Instance.ID} - {CustomLevelDataManager.Instance.levelName}", "backgroundVideo.mp4");
                video.Play();
            }
            infotext.text = $"{CustomLevelDataManager.Instance.data.artist} - {CustomLevelDataManager.Instance.data.songName}";
            
            TexturePack.Instance.UpdateTexture();

        }
    }
}