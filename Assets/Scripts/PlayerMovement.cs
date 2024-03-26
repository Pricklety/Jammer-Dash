using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        if ((SceneManager.GetActiveScene().name != "LevelDefault" && SceneManager.GetActiveScene().buildIndex < 25) || scoreText == null)
        {
            jump = Resources.Load<AudioClip>("Audio/SFX/boost");
            hit = Resources.Load<AudioClip>("Audio/SFX/hit");
            impact = Resources.Load<AudioClip>("Audio/SFX/impact");
            fail = Resources.Load<AudioClip>("Audio/SFX/fail");
            cubes = GameObject.FindGameObjectsWithTag("Cubes").ToList();
            combo = 0;
            highestCombo = 0;
            Scene scene = SceneManager.GetActiveScene();
            SettingsData adata = SettingsFileHandler.LoadSettingsFromFile();
            if (adata.hitType == 0)
            {
                goodTextPrefab = Resources.Load<GameObject>("good");
                badTextPrefab = Resources.Load<GameObject>("bad");
            }
            else if (adata.hitType == 1)
            {
                goodTextPrefab = Resources.Load<GameObject>("RinHit");
                badTextPrefab = Resources.Load<GameObject>("RinMiss");
            }
            hpSlider = GameObject.Find("health").GetComponent<Slider>();
            if (hpSlider = null)
            {
                hpSlider = GameObject.Find("health").GetComponent<Slider>();
            }
            hpText = GameObject.Find("hp").GetComponent<Text>();
            scoreText = GameObject.Find("score").GetComponent<Text>();
            keyText = GameObject.Find("key").GetComponent<Text>();
            combotext = GameObject.Find("combo").GetComponent<Text>();
        }
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
      
    }

    private void UpdateVignette(int currentHealth)
    {
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
        
        if ((SceneManager.GetActiveScene().name != "LevelDefault" && SceneManager.GetActiveScene().buildIndex < 25) || scoreText == null)
        {
            jump = Resources.Load<AudioClip>("Audio/SFX/boost");
            hit = Resources.Load<AudioClip>("Audio/SFX/hit");
            impact = Resources.Load<AudioClip>("Audio/SFX/impact");
            fail = Resources.Load<AudioClip>("Audio/SFX/fail");
            cubes = GameObject.FindGameObjectsWithTag("Cubes").ToList();
            combo = 0;
            highestCombo = 0;
            Scene scene = SceneManager.GetActiveScene();
            SettingsData adata = SettingsFileHandler.LoadSettingsFromFile();
            if (adata.hitType == 0)
            {
                goodTextPrefab = Resources.Load<GameObject>("good");
                badTextPrefab = Resources.Load<GameObject>("bad");
            }
            else if (adata.hitType == 1)
            {
                goodTextPrefab = Resources.Load<GameObject>("RinHit");
                badTextPrefab = Resources.Load<GameObject>("RinMiss");
            }
            hpSlider = GameObject.Find("health").GetComponent<Slider>();
            if (hpSlider = null)
            {
                hpSlider = GameObject.Find("Canvas/default/health").GetComponent<Slider>();
            }
            hpText = GameObject.Find("hp").GetComponent<Text>();
            scoreText = GameObject.Find("score").GetComponent<Text>();
            keyText = GameObject.Find("key").GetComponent<Text>();
            combotext = GameObject.Find("combo").GetComponent<Text>();

        }

        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        if (data.hitType == 0)
        {
            goodTextPrefab = Resources.Load<GameObject>("good");
            badTextPrefab = Resources.Load<GameObject>("bad");
        }
        else if (data.hitType == 1)
        {
            goodTextPrefab = Resources.Load<GameObject>("RinHit");
            badTextPrefab = Resources.Load<GameObject>("RinMiss");
        }
        hpint = ((int)health);
        UpdateVignette(hpint);
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
        if (SceneManager.GetActiveScene().name != "Tutorial" && passedCubes.Count > 0)
        {
            health -= 0.25f;
        }

        combotext.text = combo.ToString() + "x";

        comboTime += Time.deltaTime;

       
    }

    private void Update()
    {
        
        if (combo < 0)
        {
            combo = 0;
        }

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


        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        if (Input.GetMouseButtonDown(0) && !isDying|| Input.GetKeyDown(KeyCode.Return) && !isDying || Input.GetMouseButtonDown(1) && !isDying || Input.GetKeyDown(KeyCode.X) && !isDying || Input.GetKeyDown(KeyCode.Y) && !isDying || Input.GetKeyDown(KeyCode.Z) && !isDying || Input.GetKeyDown(KeyCode.K) && !isDying || Input.GetKeyDown(KeyCode.L) && !isDying)
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


        if (Input.GetKeyDown(KeyCode.K))
        {
            k++;
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            l++;
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            y++;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            x++;
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            z++;
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            enter++;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            m1++;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            m2++;
        }

        keyText.text = $"| {k} | {l} | {y} | {x} | {z} | {enter} | {m1} | {m2} |";


        if (health <= 0 && Time.timeScale > 0)
        {
            Time.timeScale -= 0.01f;
            music.pitch = Time.timeScale;
            health = 0;
            isDying = true;

        }

        if (health <= 0 && Time.timeScale <= 0.01f)
        {
            EndLife();
            Time.timeScale = 0f;
            health = 0;
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
    }
    IEnumerator ChangeScore()
    {
        counter.destroyedCubes += 50;
        int newDestroyedCubes = counter.score + Mathf.RoundToInt((float)counter.destructionPercentage) * combo * (counter.accCount / Total * 100 / 2) + hpint;
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

        if (collision.tag == "Cubes" && activeCubes.Contains(collision.gameObject))
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

