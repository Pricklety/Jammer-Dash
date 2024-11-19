using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using JammerDash.Audio;

namespace JammerDash.Editor
{


    public class LineController : MonoBehaviour
    {
        public GameObject linePrefab; // Prefab of the line to spawn
        public GameObject bpmLine; // Prefab of the BPM line
        public AudioClip audioClip; // The single audio clip to be played
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
            audioSource = AudioManager.Instance.source;


        }

        private void Update()
        {
            if (isMoving && !isPaused)
            {
                if (currentLine != null)
                {
                    currentLine.transform.position = new Vector2(audioSource.time * 7f, 0);
                }


            }

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
                audioSource.Play();

            }
            else if (!isPaused)
            {
                StopLine();
                audioSource.Stop();
                isPaused = true;
                audioSource.time = 0;
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
}