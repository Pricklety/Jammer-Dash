using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

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


    private void Update()
    {
        // Move player right
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
        cam.transform.position = new Vector3(transform.position.x + 6, 0.7f, -10);

        // Check for vertical movement
        // Check for vertical movement
        if (Input.GetKeyDown(KeyCode.W) && transform.position.y < maxY || Input.GetKeyDown(KeyCode.UpArrow) && transform.position.y < maxY)
        {
            transform.position += new Vector3(0f, jumpHeight, 0f);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && transform.position.y < maxY - jumpHeight)
        {
            transform.position += new Vector3(0f, jumpHeight * 2f, 0f);
            sfxS.clip = jump;
            sfxS.Play();
        }
        else if (Input.GetKeyDown(KeyCode.S) && transform.position.y > minY || Input.GetKeyDown(KeyCode.DownArrow) && transform.position.y > minY)
        {
            transform.position -= new Vector3(0f, jumpHeight, 0f);
        }

        if (Input.GetKeyDown(KeyCode.A) && transform.position.y > -1 || Input.GetKeyDown(KeyCode.LeftArrow) && transform.position.y > -1)
        {
            if (transform.position.y > 0)
            {
                sfxS.clip = impact;
                sfxS.Play();
            }

            transform.position = new Vector3(transform.position.x, -1, transform.position.z);


        }

    }

}
