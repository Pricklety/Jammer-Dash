using JammerDash.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash.Editor
{

    public class PlayerEditorMovement : MonoBehaviour
    {
        public float moveSpeed = 1f;
        private float jumpHeight = 1f;
        private float minY = -1f;
        private float maxY = 4f;


        public Transform cam;

        public AudioSource music;
        public AudioSource sfxS;
        public AudioClip jump;
        public AudioClip impact;
        public Text text;

        private void Start()
        {
            music = AudioManager.Instance.source;
        }
        private void Update()
        {
            // Move player right
            transform.position = new Vector2(music.time * 7, transform.position.y);
            text.text = "X: " + transform.position.x.ToString("F1") + ", Y: " + transform.position.y.ToString("F0");
            cam.transform.position = new Vector3(transform.position.x + 6, 0.7f, -10);


            // Check for vertical movement
            if (Input.GetKeyDown(KeyCode.W) && transform.position.y < maxY || Input.GetKeyDown(KeyCode.UpArrow) && transform.position.y < maxY)
            {
                transform.position += new Vector3(0f, jumpHeight, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Space) && transform.position.y < maxY - jumpHeight)
            {
                transform.position += new Vector3(0f, jumpHeight * 2f, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.S) && transform.position.y > minY || Input.GetKeyDown(KeyCode.DownArrow) && transform.position.y > minY)
            {
                transform.position -= new Vector3(0f, jumpHeight, 0f);
            }

            if (Input.GetKeyDown(KeyCode.A) && transform.position.y > -1 || Input.GetKeyDown(KeyCode.LeftArrow) && transform.position.y > -1)
            {
                transform.position = new Vector3(transform.position.x, -1, transform.position.z);

            }

            if (Input.GetKey(KeyCode.R))
            {
                Time.timeScale = 2f;
                AudioSource[] sources = FindObjectsOfType<AudioSource>();

                foreach (AudioSource source in sources)
                {
                    source.pitch = 2f;
                }
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Cubes"))
            {
                new WaitForSecondsRealtime(0.07f);
                Debug.Log("hit");
                AudioClip hitSound = Resources.Load<AudioClip>("Audio/SFX/hit0");
                GameObject.Find("sfx").GetComponent<AudioSource>().PlayOneShot(hitSound, 1);

            }
            if (collision.CompareTag("Beat"))
            {
                Debug.Log("hit");
                AudioClip hitSound = Resources.Load<AudioClip>("Audio/SFX/metronome");
                GameObject.Find("sfx").GetComponent<AudioSource>().PlayOneShot(hitSound, 2);

            }
        }
    }

}