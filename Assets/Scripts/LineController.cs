using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LineController : MonoBehaviour
{
    public GameObject linePrefab; // Prefab of the line to spawn
    public GameObject bpmLine; // Prefab of the BPM line
    public AudioClip audioClip; // The single audio clip to be played
    public float audioPitch = 1.0f; // Adjust this value to slow down or speed up the audio
    public Text artistText;
    public Text nameText;

    private GameObject currentLine;
    public AudioSource audioSource;
    private bool isMoving = true;
    private bool isPaused = false;
    private float initialXPosition;
    private float audioStartTime;
    private float savedXPosition; // Saved X position for restoring

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.pitch = audioPitch; // Set the initial pitch
        audioStartTime = 0;

        
    }

    private void Update()
    {
        if (isMoving && !isPaused)
        {
            Vector3 movement = Vector3.right * Time.deltaTime * 7f; // Adjust the speed as needed
            if (currentLine != null)
            {
                currentLine.transform.Translate(movement);
            }


        }

        // Check for 'G' key press
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z))
        {
            SaveLinePosition();
        }
    }


    public void StopLine()
    {
        Destroy(currentLine);
    }

    public void ToggleLine()
    {
        if (currentLine == null && !audioSource.isPlaying)
        {
            Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
            if (spawnPosition.x < 0)
            {
                spawnPosition = Vector3.zero;
            }
            currentLine = Instantiate(linePrefab, spawnPosition, Quaternion.identity);
            isMoving = true;
            isPaused = false;
            initialXPosition = currentLine.transform.position.x;

            audioSource.time = spawnPosition.x / 7;
            audioSource.pitch = audioPitch; // Reset the pitch
            audioSource.Play();

            // Load saved position from PlayerPrefs
            savedXPosition = PlayerPrefs.GetFloat("SavedXPosition", 0f);
        }
        else if (!isPaused)
        {
            StopLine();
            isPaused = true;
            audioSource.Stop();
            audioSource.time = 0;
        }
    }


    public IEnumerator LoadAudioClip(string songName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "music", songName);
        filePath = filePath.Replace("\\", "/");
        using (UnityWebRequest www = UnityWebRequest.Get("file://" + filePath))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to load audio clip: " + www.error);
                Debug.Log(filePath);
            }
            else
            {
                AudioClip audioClip = AudioClip.Create(songName, ((int)www.downloadedBytes) / 2, 2, 44100, false);

                // Convert byte array to float array
                float[] floatData = new float[audioClip.samples * audioClip.channels];
                for (int i = 0; i < floatData.Length; i++)
                {
                    floatData[i] = BitConverter.ToInt16(www.downloadHandler.data, i * 2) / 32768.0f;
                }

                audioClip.SetData(floatData, 0);

                if (audioClip != null)
                {
                    this.audioClip = audioClip;
                    audioSource.clip = audioClip;
                    Debug.Log("Audio clip loaded successfully");
                }
                else
                {
                    Debug.LogError("Failed to convert MP3 to AudioClip");
                }
            }
        }
    }

    public void UpdateArtistAndNameText(string songName)
    {
        // Extract artist and name information from the songName
        // Assuming the songName is in the format "Artist - SongName.mp3"
        string[] nameComponents = songName.Split('-');

        if (nameComponents.Length == 2)
        {
            artistText.text = nameComponents[0].Trim();
            nameText.text = nameComponents[1].Trim();
        }
        else
        {
            // Handle the case where the name format is different
            artistText.text = "Unknown Artist";
            nameText.text = songName;
        }
    }



    public void SaveLinePosition()
    {
        if (currentLine != null)
        {
            Instantiate(bpmLine, new Vector3(currentLine.transform.position.x, 0, 0), Quaternion.identity);
            savedXPosition = currentLine.transform.position.x;
            PlayerPrefs.SetFloat("SavedXPosition", savedXPosition);
        }
        else if (FindObjectOfType<PlayerEditorMovement>().enabled)
        {
            Instantiate(bpmLine, new Vector3(FindObjectOfType<PlayerEditorMovement>().transform.position.x, 0, 0), Quaternion.identity);
            savedXPosition = FindObjectOfType<PlayerEditorMovement>().transform.position.x;
        }
    }
}
