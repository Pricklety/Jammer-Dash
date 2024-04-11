using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class PlayerMovement : MonoBehaviour
{
    private float click = 0.05f;
    public float moveSpeed = 1f;
    public Transform cam;
    public GameObject goodTextPrefab;
    public GameObject badTextPrefab;
    public GameObject destroyParticlesPrefab;
    public LayerMask cubeLayerMask;
    public LayerMask longcubeLayerMask;
    public AudioClip[] hitSounds;
    public AudioSource sfxS;
    public AudioClip jump;
    public AudioClip hit;
    public AudioClip impact;
    public AudioClip fail;
    private int hpint;
    public float health = 300;
    public float maxHealth;
    public Text hpText;
    public Slider hpSlider;
    public CubeCounter counter;
    public Text combotext;
    public Animation cubeanim;
    public Animation godmode;
    private Animation dash;
    public AudioSource music;
    public HashSet<GameObject> passedCubes = new HashSet<GameObject>();    
    private HashSet<GameObject> activeCubes = new HashSet<GameObject>();
    public List<GameObject> cubes = new();
    public Text scoreText;
    public Text keyText;
    float k, l, y, x, z, enter, m1, m2;
    public Text acc;
    private PostProcessVolume volume;
    private Vignette vignette;
    private float initialIntensity;
    public float vignetteStartHealthPercentage = 0.5f;
    public UnityEngine.Color startColor = UnityEngine.Color.red;

    [Header("Tutorial Stuff")]
    public AudioClip clip01;
    public AudioClip clip02;
    public AudioClip clip03;
    public AudioClip clip04;
    public AudioClip clip05;
    public AudioClip clip06;

    [Header("UI")]
    public GameObject deathPanel;

    private float jumpHeight = 1f;
    private float minY = -1f;
    private float maxY = 4f;
    public int combo;
    public int highestCombo;
    private float comboTime;
    private float combominimize = .75f;
    bool isDying = false;
    bool invincible = false;
    public int Total = 0;

    private void Start()
    {
        volume = FindObjectOfType<PostProcessVolume>();
        volume.profile.TryGetSettings(out vignette);
        initialIntensity = vignette.intensity.value;
        vignette.color.value = startColor;
           
        maxHealth = 300;

        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        if (data.hitType == 0)
        {
            goodTextPrefab = Resources.Load<GameObject>("good");
            badTextPrefab = Resources.Load<GameObject>("bad");
            Debug.Log("asd");
        }
        else if (data.hitType == 1)
        {
            goodTextPrefab = Resources.Load<GameObject>("RinHit");
            badTextPrefab = Resources.Load<GameObject>("RinMiss");
            Debug.Log("asd");
        }
        GameObject[] deathObjects = FindObjectsOfType<GameObject>();
        FindObjectOfType<cameraColor>().enabled = true;

        foreach (GameObject obj in deathObjects)
        {
            if (obj.name == "death")
            {
                deathPanel = obj;
                break;
            }
        }
        acc = GameObject.Find("acc").GetComponent<Text>();
        // Array to store all loaded audio clips
        AudioClip[] allHits = new AudioClip[2];

        // Loop through each possible variation of the hit name
        for (int i = 0; i < allHits.Length; i++)
        {
            // Load the audio clip for each variation
            allHits[i] = Resources.Load<AudioClip>("Audio/SFX/hit" + (i + 1));
        }

        // Check if any hits were found
        if (allHits.Length > 0)
        {
            // Choose a random hit from the array
            AudioClip randomHit = allHits[Random.Range(0, allHits.Length)];

            // Now you can play the random hit audio clip
            hit = randomHit;
        }
        else
        {
            Debug.LogError("No hits found in the Resources/Audio/SFX folder.");
        }
    }


    private void UpdateVignette(int currentHealth)
    {
        vignette.color.Override(Color.red);
        float healthPercentage = 1f - ((float)currentHealth / (maxHealth / 2f));

        if (healthPercentage < 0)
        {
            healthPercentage = 0;
        }
        else if (healthPercentage > 1)
        {
            healthPercentage = 1;
        }
        vignette.center.Override(new Vector2(0.5f, 0.5f));
        float vignetteIntensity = Mathf.Lerp(0, 0.25f, healthPercentage);
        vignette.intensity.Override(vignetteIntensity);
    }

    private void FixedUpdate()
    {
        
        hpint = (int)health;
        hpText.text = hpint.ToString() + "/" + maxHealth.ToString("0");
        // Move player right
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
        if(cam.transform.position.x < FindObjectOfType<FinishLine>().transform.position.x)
        {
            float distanceToFinishLine = Mathf.Abs(cam.transform.position.x - FindObjectOfType<FinishLine>().transform.position.x);

            if (distanceToFinishLine < 3)
            {
                // Calculate the target position to move the camera
                Vector3 targetPos = new Vector3(FindObjectOfType<FinishLine>().transform.position.x, 0.7f, -10);

                // Smoothly move the camera towards the target position
                Vector3 smoothPosition = Vector3.Lerp(cam.transform.position, targetPos, 10f * Time.deltaTime);

                // Update the camera's position
                cam.transform.position = smoothPosition;
            }
            else
            {
                Vector3 targetPosition = new Vector3(transform.position.x + 6, 0.7f, -10);
                Vector3 smoothedPosition = Vector3.Lerp(cam.transform.position, targetPosition, 10f * Time.deltaTime);
                cam.transform.position = smoothedPosition;

            }
        }
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        hpSlider.value = health;
        hpSlider.maxValue = maxHealth;
            if (IsPlayerCloseToNextCube() && !IsPlayerInLongSeries())
            {

                UpdateVignette(hpint);
                health -= 0.25f;
            }
            else if (!IsPlayerCloseToNextCube() && !IsPlayerInLongSeries())
            { 
                vignette.color.Override(Color.green);
                vignette.intensity.Override(0.35f);
            }
        
        combotext.text = combo.ToString() + "x";

        comboTime += Time.deltaTime;


        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        if (combo < 0)
        {
            combo = 0;
        }
        if (data.scoreType == 0)
        {
            scoreText.text = $"{counter.score:N0}";
        }
        else if (data.scoreType == 1)
        {
            scoreText.text = $"{counter.destroyedCubes}";
        }
        if (Total > 0)
        {
            float acc = (float)counter.accCount / Total * 100;
            this.acc.text = $"{acc:F2}%\n{counter.GetTier(acc)}";
        }
        else
        {
            acc.text = $"100.00%\nS+";
        }
        float newVolume = 1.0f - (combo / 2) * 0.01f;
        newVolume = Mathf.Clamp(newVolume, 0.25f, 1.0f);

        // Set the calculated volume to the SFX AudioSource
        sfxS.volume = newVolume;

        if (health <= 0 && Time.timeScale > 0)
        {
            Time.timeScale -= 0.05f;
            music.pitch = Time.timeScale;
            health = 0;
            isDying = true;

        }

        if (health <= 0 && Time.timeScale <= 0.1f)
        {
            EndLife();
            Time.timeScale = 0f;
            health = 0;
        }
        if (IsPlayerCloseToNextCube())
        {
            float playerPositionInSeconds = transform.position.x / 7;
            float finishLinePositionInSeconds = FindObjectOfType<FinishLine>().transform.position.x / 7;

            // Calculate time in minutes and seconds
            int playerMinutes = Mathf.FloorToInt(playerPositionInSeconds / 60);
            int playerSeconds = Mathf.FloorToInt(playerPositionInSeconds % 60);

            int finishLineMinutes = Mathf.FloorToInt(finishLinePositionInSeconds / 60);
            int finishLineSeconds = Mathf.FloorToInt(finishLinePositionInSeconds % 60);

            // Format time strings
            string playerTime = string.Format("{0}:{1:00}", playerMinutes, playerSeconds);
            string finishLineTime = string.Format("{0}:{1:00}", finishLineMinutes, finishLineSeconds);

            keyText.text = $"{playerTime} / {finishLineTime}";
        }
    }

    public bool IsPlayerCloseToNextCube()
    {
        itemUnused[] cubes = FindObjectsOfType<itemUnused>();
        // If there are no cubes, return false
        if (cubes.Length == 0)
        {
            return true;
        }

        // Find cubes that are above the player's x position
        List<itemUnused> cubesAbovePlayerX = new List<itemUnused>();
        foreach (itemUnused cube in cubes)
        {
            if (cube.transform.position.x > transform.position.x)
            {
                cubesAbovePlayerX.Add(cube);
            }
        }

        // If there are no cubes above the player's x position, return false
        if (cubesAbovePlayerX.Count == 0)
        {
            return true;
        }

        // Find the nearest cube among the cubes above the player's x position
        itemUnused nearestCube = cubesAbovePlayerX[0];
        float nearestDistance = Vector3.Distance(transform.position, nearestCube.transform.position);

        foreach (itemUnused cube in cubesAbovePlayerX)
        {
            float distance = Vector3.Distance(transform.position, cube.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCube = cube;
            }
        }

        if (nearestDistance > 21)
        {
            // Calculate the distance to the point 21 units away from the cube along the player's direction
            Vector3 point21UnitsAway = nearestCube.transform.position - transform.forward * 21;
            float distanceToPlayer = Vector3.Distance(transform.position, point21UnitsAway);

            // Calculate the time to reach the distance of 21 units
            float timeToReachSeconds = distanceToPlayer / 7; // Assuming player's speed is 7 units per second
            if (timeToReachSeconds > 10)
            {
                // Format the time as minutes and seconds
                TimeSpan timeSpan = TimeSpan.FromSeconds(timeToReachSeconds - 4);
                string formattedTime = string.Format("{0:D2}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);// Update the text
                keyText.text = $"Break ends in {formattedTime}";
                Debug.Log("Text updated successfully.");
            }
            else
            {
                keyText.text = $"Break ends in {timeToReachSeconds - 4f:0.00}s";
            }
            
        }
        else
        {
            // If the player is between two cubes vertically or if the nearest distance is less than or equal to 21, reset the text
            keyText.text = "";
        }

        // Check if the nearest cube is within the proximity threshold
        return nearestDistance <= 21;
    }
    public bool IsPlayerInLongSeries()
    {
    // Find the cubes in between
    List<GameObject> cubesInBetween = new List<GameObject>();
    foreach (GameObject cube in cubes)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, cube.transform.position);
        if (distanceToPlayer <= 70)
        {
            cubesInBetween.Add(cube);
        }
    }

    // Calculate the total distance between cubes in between
    float totalDistance = 0f;
    for (int i = 0; i < cubesInBetween.Count - 1; i++)
    {
        totalDistance += Vector3.Distance(cubesInBetween[i].transform.position, cubesInBetween[i + 1].transform.position);
    }

    // Check if the total distance between cubes in between is longer than maxDistanceBetweenCubes
    return totalDistance > 3 * 7;
}

    private void Update()
    {
       
        // Check for vertical movement
        if (Input.GetKeyDown(KeyCode.W) && transform.position.y < maxY && !isDying || Input.GetKeyDown(KeyCode.UpArrow) && transform.position.y < maxY && !isDying)
        {
            transform.position += new Vector3(0f, jumpHeight, 0f);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && transform.position.y < maxY - jumpHeight && !isDying )
        {
            transform.position += new Vector3(0f, jumpHeight * 2f, 0f);
            sfxS.PlayOneShot(jump);
        }
        else if (Input.GetKeyDown(KeyCode.S) && transform.position.y > minY && !isDying || Input.GetKeyDown(KeyCode.DownArrow) && transform.position.y > minY && !isDying)
        {
            transform.position -= new Vector3(0f, jumpHeight, 0f);
        }

        if (Input.GetKeyDown(KeyCode.A) && transform.position.y > -1 || Input.GetKeyDown(KeyCode.LeftArrow) && transform.position.y > -1)
        {
            if (transform.position.y > 0)
            {
                sfxS.PlayOneShot(impact);
            }

            transform.position = new Vector3(transform.position.x, -1, transform.position.z);

            
        }

       
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.K)|| Input.GetKeyDown(KeyCode.L)) && !isDying && IsPlayerCloseToNextCube() && !IsPlayerInLongSeries())
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, cubeLayerMask);

            if (hit.collider != null)
            {
                if (hit.transform.position.y == transform.position.y)
                {
                    passedCubes.Add(hit.collider.gameObject);
                    DestroyCube(hit.collider.gameObject);

                    Total += 1;

                    Animation anim = combotext.GetComponent<Animation>();
                    if (highestCombo == combo)
                    {
                        highestCombo++;
                    }
                    combo++;

                    SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                    if (data.scoreType == 0)
                    {
                        StartCoroutine(ChangeScore());
                    }
                    else
                    {
                        counter.destroyedCubes += 50;
                        scoreText.text = $"{counter.destroyedCubes}";
                    }
                    health += 20;
                    anim.Stop("comboanim");
                    anim.Play("comboanim");
                    comboTime = 0;
                }
                else if (passedCubes.Count > 0)
                {
                    if (AudioManager.Instance != null)
                    {
                        if (AudioManager.Instance.hits)
                        {
                            ShowBadText();
                           
                        }
                    }
                    health -= 50;
                    counter.destroyedCubes -= 100 - combo;
                    Total += 1;
                    if (combo >= 100)
                    {
                        sfxS.PlayOneShot(fail);
                    }
                    combo = 0;
                    StartCoroutine(ChangeTextCombo());
                }
            }
            else if(passedCubes.Count > 0)
            {
                if (AudioManager.Instance != null)
                {
                    if (AudioManager.Instance.hits)
                    {
                        ShowBadText();
                    }
                }

                Total += 1;
                health -= 50;
                if (combo >= 100)
                {
                    sfxS.PlayOneShot(fail);
                    counter.destroyedCubes -= 250;
                }
                counter.destroyedCubes -= 100 - combo;

                combo = 0;
                StartCoroutine(ChangeTextCombo());
            }
        }
       
       
       
    }

    IEnumerator ChangeTextCombo()
    {
        float lerpSpeed = 1f;
        float lerpTimer = 0f;

        while (lerpTimer < 1f)
        {
            lerpTimer += Time.fixedDeltaTime * lerpSpeed;
            combotext.color = Color.Lerp(Color.red, Color.white, lerpTimer);
            yield return null;
        }
        lerpTimer = 0f;
        // Array to store all loaded audio clips
        AudioClip[] allHits = new AudioClip[2]; // Adjust the size based on the number of hits you have

        // Loop through each possible variation of the hit name
        for (int i = 0; i < allHits.Length; i++)
        {
            // Load the audio clip for each variation
            allHits[i] = Resources.Load<AudioClip>("Audio/SFX/hit" + (i + 1));
        }

        // Check if any hits were found
        if (allHits.Length > 0)
        {
            // Choose a random hit from the array
            AudioClip randomHit = allHits[Random.Range(0, allHits.Length)];

            hit = randomHit;
        }
        else
        {
            Debug.LogError("No hits found in the Resources/Audio/SFX folder.");
        }
    
    }
    IEnumerator ChangeScore()
    {
        counter.destroyedCubes += 50;
        int newDestroyedCubes = counter.score + Mathf.RoundToInt((float)counter.destructionPercentage) * (counter.accCount / Total * 100 / 2) + hpint + combo * 10;
        float elapsedTime = 0f;
        float duration = 0.1f; 
        counter.accCount++;
        while (elapsedTime < duration)
        {
            counter.score = (int)Mathf.Lerp(counter.score, newDestroyedCubes, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            scoreText.text = counter.score.ToString("N0");
            yield return null; // Wait for the next frame
        }

        // Ensure the score reaches the final value precisely
        counter.score = newDestroyedCubes;
        yield return new WaitForSeconds(0.2f);
    }

    private void DestroyCube(GameObject cube)
    {
        if (Time.timeScale > 0)
        {
            
            sfxS.PlayOneShot(hit);
            Vector3 cubePosition = cube.transform.position;
            Destroy(cube);
            activeCubes.Remove(cube);
            if (AudioManager.Instance != null)
            {
                if (AudioManager.Instance.hits)
                {
                    Instantiate(goodTextPrefab, cubePosition, Quaternion.identity);
                }
            }
            
        }
    }

    private void ShowBadText()
    {
        if (Time.timeScale > 0)
        {
            Instantiate(badTextPrefab, transform.position, Quaternion.identity);
            
            
        }

    }


    private void EndLife()
    {
        deathPanel.SetActive(true); 
        music.pitch = 0f;
        gameObject.SetActive(false);
        enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
            if (GetComponent<BoxCollider2D>().IsTouching(collision))
            {
                if (collision.tag == "Saw" && !invincible)
                {
                    health -= 150;
                }
            }
            

        if (collision.tag == "Cubes")
        {
            collision.GetComponent<Animation>().Play();
        }

        if (collision.tag == "Cubes")
        {
            activeCubes.Add(collision.gameObject);
        }


    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "SloMoTutorial")
        {
            GameObject tutg = GameObject.Find("TutorialText");
        }

        if (collision.tag == "Cubes" && activeCubes.Contains(collision.gameObject) && health > 0)
        {
            activeCubes.Remove(collision.gameObject);

            if (!passedCubes.Contains(collision.gameObject))
            {
                if (AudioManager.Instance != null)
                {

                    if (AudioManager.Instance.hits)
                    {
                        ShowBadText();
                    }
                }

                Total += 1;
                activeCubes.Remove(collision.gameObject);
                health -= 75; // Lower health due to passing the cube
                combo = 0; // Reset combo

                StartCoroutine(ChangeTextCombo());
            }
        }
    }
    

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "SloMoTutorial")
        {
            GameObject tutg = GameObject.Find("TutorialText");
            Text tuttext = tutg.GetComponent<Text>();
            health -= 0.05f;

            if (transform.position.x < 25)
            {
                tuttext.text = "Welcome to Jammer Dash!";
                health -= 0f;
            }
            if (transform.position.x > 25 && transform.position.x < 56)
            {
                tuttext.text = "Click the left mouse button to destroy the cube and continue (Enter and right mouse button work too)";
            }
            if (transform.position.x > 78 && transform.position.x < 100)
            {
                tuttext.text = "Click W to go upwards and destroy the cube";
            }
            if (transform.position.x > 120 && transform.position.x < 150)
            {
                tuttext.text = "Click S to go downwards and destroy the cube";
            }
            if (transform.position.x > 165 && transform.position.x < 200)
            {
                tuttext.text = "Click Space to dash upwards and destroy the cube";
            }
            if (transform.position.x > 210 && transform.position.x < 260)
            {
                tuttext.text = "Click Space again";
            }
            if (transform.position.x > 270 && transform.position.x < 320)
            {
                tuttext.text = "Click A to go back to the ground";
            }
            if (transform.position.x > 330 && transform.position.x < 360)
            {
                tuttext.text = "Now try to do this level to the finish with what you learned.";
            }



        }

        if (collision.name == "finishline")
        {
            health -= 0f;
        }
    }
}

