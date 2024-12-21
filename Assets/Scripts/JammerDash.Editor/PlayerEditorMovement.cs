using JammerDash.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
            if (Input.GetKeyDown(KeybindingManager.up) && transform.position.y < maxY)
            {
                transform.position += new Vector3(0f, jumpHeight, 0f);
            }
            else if (Input.GetKeyDown(KeybindingManager.boost) && transform.position.y < maxY - jumpHeight)
            {
                transform.position += new Vector3(0f, jumpHeight * 2f, 0f);
            }
            else if (Input.GetKeyDown(KeybindingManager.down) && transform.position.y > minY)
            {
                transform.position -= new Vector3(0f, jumpHeight, 0f);
            }

            if (Input.GetKeyDown(KeybindingManager.ground) && transform.position.y > -1)
            {
                transform.position = new Vector3(transform.position.x, -1, transform.position.z);

            }

            if (Input.GetKey(KeyCode.R))
            {
                Time.timeScale = 2f;
                AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

                foreach (AudioSource source in sources)
                {
                    source.pitch = 2f;
                }
            }
        }

        public AudioClip[] hitSounds;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Cubes"))
            {
                StartCoroutine(HandleTriggerEnter(collision));
            }
            if (collision.CompareTag("LongCube")) 
            {
                StartCoroutine(HandleTriggerEnter(collision));
                
            }
            if (collision.CompareTag("Beat"))
            {
                Debug.Log("hit");
                AudioClip hitSound = Resources.Load<AudioClip>("Audio/SFX/metronome");
                GameObject.Find("sfx").GetComponent<AudioSource>().PlayOneShot(hitSound, 1);

            }
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("LongCube"))
            {
               Debug.Log("hit");
               sfxS.PlayOneShot(hitSounds[6]);


            }
        }
        private IEnumerator HandleTriggerEnter(Collider2D collision)
        {
            // Wait for 0.07 seconds
            yield return new WaitForSeconds(0.07f);

            // Logic after delay
            Debug.Log("hit");
            switch (collision.gameObject.name)
            {
                case "hitter01(Clone)":
                    sfxS.PlayOneShot(hitSounds[0]);
                    break;
                case "hitter02(Clone)":
                    sfxS.PlayOneShot(hitSounds[1]);
                    break;
                case "hitter03(Clone)":
                    sfxS.PlayOneShot(hitSounds[2]);
                    break;
                case "hitter04(Clone)":
                    sfxS.PlayOneShot(hitSounds[3]);
                    break;
                case "hitter05(Clone)":
                    sfxS.PlayOneShot(hitSounds[4]);
                    break;
                case "hitter06(Clone)":
                    sfxS.PlayOneShot(hitSounds[5]);
                    break;
                default:
                    break;
            }
        }
    }

}