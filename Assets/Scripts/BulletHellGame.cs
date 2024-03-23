using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BulletHellGame : MonoBehaviour
{
    public GameObject player;
    public GameObject notePrefab;
    public float playerSpeed = 5f;
    public float noteSpeed = 7f;
    public int health = 25;
    public int score = 0;

    public Text healthText;
    public Text scoreText;
    public Text feedbackText; // New UI Text for feedback

    public AudioClip harmoniousSFX;
    public AudioClip dissonantSFX;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        UpdateUI();
    }

    private void Update()
    {
        // Player movement
        float horizontalInput = Input.GetAxis("Horizontal");
        player.transform.Translate(Vector3.right * horizontalInput * playerSpeed * Time.deltaTime);

        // Restrict player movement to the screen boundaries
        float playerX = Mathf.Clamp(player.transform.position.x, -4.5f, 4.5f);
        player.transform.position = new Vector3(playerX, player.transform.position.y, 0f);
        UpdateUI();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        NoteController noteController = collision.gameObject.GetComponent<NoteController>();

        if (noteController != null)
        {
            // Check the note type and adjust score and health accordingly
            if (noteController.noteType == NoteType.Harmonious)
            {
                // Play harmonious SFX
                audioSource.PlayOneShot(harmoniousSFX);
                score += 500;

                // Display "Good" feedback
                ShowFeedback("Good!");
            }
            else
            {
                // Play dissonant SFX
                audioSource.PlayOneShot(dissonantSFX);
                health--;
                score -= 200;

                // Display "Bad" feedback
                ShowFeedback("Bad!");
            }

            UpdateUI();

            // Destroy the note
            Destroy(collision.gameObject);
        }
    }

    void UpdateUI()
    {
        healthText.text = health + "/25";
        scoreText.text = score + "x";
    }

    void ShowFeedback(string message)
    {
        // Display feedback text for a short duration
        feedbackText.text = message;
        StartCoroutine(HideFeedback());
    }

    IEnumerator HideFeedback()
    {
        yield return new WaitForSeconds(1.0f); // Adjust duration as needed
        feedbackText.text = "";
    }
}
