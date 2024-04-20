using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class cameraColor : MonoBehaviour
{
    [Range(0.35f, 3f)]
    public float duration = 1f;

    public PlayerMovement player;
    private float t = 0f;
    private Color startColor;
    private Color startColorG;
    private Color targetColor;
    private Color targetColorG;
    public AudioSource song;
    public GameObject player0;
    public GameObject player1;
    bool started = false;

    public Transform ground;

    private void Start()
    {
        UpdateBackgroundColor();
       
        song.pitch = 0;
        player.enabled = false;
        StartCoroutine(LateStart());
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        if (player0 != null && player1 != null)
        {
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
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        if (data.gameplayDir == 1)
        {
            Matrix4x4 mat = Camera.main.projectionMatrix; mat *= Matrix4x4.Scale(new Vector3(-1, 1, 10)); Camera.main.projectionMatrix = mat;
        }
    }

    IEnumerator LateStart()
    {
        
        song.pitch = 0;
        player.enabled = false; 
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
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        song = GameObject.Find("Music").GetComponent<AudioSource>();

        if (player.health > 0 || player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            song = GameObject.Find("Music").GetComponent<AudioSource>();

        }
        
        if (player.gameObject.activeSelf)
        {

            t += Time.deltaTime;
            UpdateBackgroundColor();
        }
    }

    private void SetRandomTargetColor()
    {
        if (SceneManager.GetActiveScene().name == "LevelDefault")
        {

            startColorG = ground.GetComponent<SpriteRenderer>().color;
            targetColor = player.combo < 250 ? startColor : Random.ColorHSV();
            targetColorG = player.combo < 250 ? Color.white : Random.ColorHSV();
            t = 0f; // Reset time
        }
        else if (SceneManager.GetActiveScene().buildIndex < 25)
        {
            startColor = Camera.main.backgroundColor;
            startColorG = ground.GetComponent<SpriteRenderer>().color;
            targetColor = player.combo < 250 ? startColor : Random.ColorHSV();
            targetColorG = player.combo < 250 ? Color.white : Random.ColorHSV();
            t = 0f; // Reset time
        }
        else
        {
            startColor = Camera.main.backgroundColor;
            startColorG = ground.GetComponent<SpriteRenderer>().color;
            targetColor = player.combo < 250 ? startColor : Random.ColorHSV();
            targetColorG = player.combo < 250 ? Color.white : Random.ColorHSV();
            t = 0f; // Reset time
        }

    }

    private void UpdateBackgroundColor()
    {
        Camera.main.backgroundColor = Color.Lerp(startColor, targetColor, t / duration);

        if (player.combo >= 0 && t >= duration)
        {
            SetRandomTargetColor();

            ground.GetComponent<SpriteRenderer>().color = Color.Lerp(startColorG, targetColorG, t / duration);
        }
        
    }
}
