using System;
using System.Collections;
using System.Collections.Generic;
using JammerDash.Audio; 
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
namespace JammerDash.Game.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private float click = 0.05f;
        public float moveSpeed = 1f;
        public Transform cam;
        public GameObject goodTextPrefab;
        public GameObject normalTextPrefab;
        public GameObject okTextPrefab;
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


        [Header("SP Calculator")]
        public float accuracyWeight = 0.56f;
        public float comboWeight = 0.25f;
        public float movementEfficiencyWeight = 0.14f;
        public float strategicDecisionMakingWeight = 0.13f;
        public float adaptabilityWeight = 0.15f;
        public float levelCompletionTimeWeight = 0.1f;

        [Header("Others")]
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
        private bool bufferActive = false;
        public float longcombo = 0;
        public int misses;
        public int five;
        public int three;
        public int one;
        public int SPInt;
        private void Start()
        {
            volume = FindObjectOfType<PostProcessVolume>();
            volume.profile.TryGetSettings(out vignette);
            initialIntensity = vignette.intensity.value;
            vignette.color.value = startColor;

            maxHealth = 300;
            InputSystem.pollingFrequency = 1000;
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (data.hitType == 0)
            {
                goodTextPrefab = Resources.Load<GameObject>("good");
                okTextPrefab = Resources.Load<GameObject>("crap");
                normalTextPrefab = Resources.Load<GameObject>("ok");
                badTextPrefab = Resources.Load<GameObject>("bad");
                UnityEngine.Debug.Log("asd");
            }
            else if (data.hitType == 1)
            {
                goodTextPrefab = Resources.Load<GameObject>("RinHit");
                badTextPrefab = Resources.Load<GameObject>("RinMiss");
                UnityEngine.Debug.Log("asd");
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
            if (data.randomSFX)
            {

                // Array to store all loaded audio clips
                hitSounds = new AudioClip[3];
                // Loop through each possible variation of the hit name
                for (int i = 0; i < hitSounds.Length; i++)
                {
                    // Load the audio clip for each variation
                    hitSounds[i] = Resources.Load<AudioClip>("Audio/SFX/hit" + i);
                }

                // Check if any hits were found
                if (hitSounds.Length > 0)
                {
                    // Choose a random hit from the array
                    AudioClip randomHit = hitSounds[Random.Range(0, hitSounds.Length)];

                    // Now you can play the random hit audio clip
                    hit = randomHit;
                }
                else
                {
                    UnityEngine.Debug.LogError("No hits found in the Resources/Audio/SFX folder.");
                }
            }
            else
            {
                hit = Resources.Load<AudioClip>("Audio/sfx/hit0");
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
            if (SceneManager.GetActiveScene().buildIndex != 2)
                health -= 0.25f;
            else
                health -= 0.1f;
            hpint = (int)health;
            // Move player right
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
            if (cam.transform.position.x < FindObjectOfType<FinishLine>().transform.position.x)
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

            UpdateVignette(hpint);

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
        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.H))
                Time.timeScale = 0.25f;
            else if (Input.GetKeyUp(KeyCode.H))
                Time.timeScale = 1f;
#endif
            // Check for vertical movement
            if (Input.GetKeyDown(KeyCode.W) && transform.position.y < maxY && !isDying || Input.GetKeyDown(KeyCode.UpArrow) && transform.position.y < maxY && !isDying)
            {
                transform.position += new Vector3(0f, jumpHeight, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Space) && transform.position.y < maxY - jumpHeight && !isDying)
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
            float maxAdaptability = 1f; // Max adaptability value
            float maxLevelCompletionTime = 1f; // Max level completion time value

            // Calculate individual skill components
            float accuracy = Mathf.Clamp01((float)counter.accCount / Total);
            float comboEfficiency = highestCombo != 0 ? Mathf.Clamp01(combo / highestCombo + 1) : 0;
            float adaptability = Mathf.Clamp01(CalculateAdaptability() / maxAdaptability);
            float levelCompletionTime = Mathf.Clamp01(CalculateLevelCompletionTime() / maxLevelCompletionTime);

            // Calculate skill performance point
            float skillPerformancePoint = Mathf.Max(
                Mathf.Pow(accuracy, 2) * accuracyWeight +
                comboEfficiency * comboWeight +
                adaptability * adaptabilityWeight +
                levelCompletionTime * levelCompletionTimeWeight,
                0f
            );

            SPInt = Mathf.RoundToInt(skillPerformancePoint);
            if (Total > 0)
            {
                float acc = (float)counter.accCount / Total * 100;
                this.acc.text = $"{acc:F2}% | {counter.GetTier(acc)} | {SPInt} sp";
            }
            else
            {
                acc.text = $"";
            }

            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.L)) && !isDying)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, cubeLayerMask);

                if (hit.collider != null)
                {
                    bufferActive = true;
                    if (hit.transform.position.y == transform.position.y)
                    {
                        if (hit.transform.name.Contains("hitter01"))
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
                                StartCoroutine(ChangeScore(Vector3.Distance(hit.collider.transform.position, transform.position)));
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
                        health -= 20;
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
                else if (passedCubes.Count > 0)
                {
                    if (AudioManager.Instance != null)
                    {
                        if (AudioManager.Instance.hits)
                        {
                            ShowBadText();
                        }
                    }

                    Total += 1;
                    health -= 20;
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

        private float CalculateAdaptability()
        {
            float adaptability = Mathf.Clamp((float)health / maxHealth * 100f, 0f, 100f);
            return adaptability;
        }

        private float CalculateLevelCompletionTime()
        {
            float levelCompletionTime = Mathf.Clamp(0, 0f, FindObjectOfType<FinishLine>().transform.position.x / 7);
            return levelCompletionTime;
        }


        IEnumerator ChangeTextCombo()
        {
            float lerpSpeed = 1f;
            float lerpTimer = 0f;
            misses++;
            while (lerpTimer < 1f)
            {
                lerpTimer += Time.fixedDeltaTime * lerpSpeed;
                combotext.color = Color.Lerp(Color.red, Color.white, lerpTimer);
                yield return null;
            }
            lerpTimer = 0f;
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (data.randomSFX)
            {

                // Array to store all loaded audio clips
                AudioClip[] allHits = new AudioClip[3]; // Adjust the size based on the number of hits you have

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
                    UnityEngine.Debug.LogError("No hits found in the Resources/Audio/SFX folder.");
                }
            }

        }
        IEnumerator ChangeScore(float playerDistance)
        {
            float factor;
            UnityEngine.Debug.Log(playerDistance);
            if (playerDistance <= 2.04f)
            {
                factor = 1f;
                five++;
                if (AudioManager.Instance != null)
                {
                    if (AudioManager.Instance.hits)
                    {
                        Instantiate(goodTextPrefab, transform.position, Quaternion.identity);
                    }
                }

            }
            else if (playerDistance <= 2.06 && playerDistance > 2.04f)
            {
                factor = 1f / 3f;
                three++;
                if (AudioManager.Instance != null)
                {
                    if (AudioManager.Instance.hits)
                    {
                        Instantiate(normalTextPrefab, transform.position, Quaternion.identity);
                    }
                }

            }
            else
            {
                factor = 1f / 5f;
                one++;
                if (AudioManager.Instance != null)
                {
                    if (AudioManager.Instance.hits)
                    {
                        Instantiate(okTextPrefab, transform.position, Quaternion.identity);
                    }
                }
            }

            counter.destroyedCubes += 50;

            // Calculate the new destroyed cubes based on the factor
            float newDestroyedCubes = counter.score + (Mathf.RoundToInt((float)counter.destructionPercentage) * ((int)counter.accCount / Total * 100) + combo * 20 * factor);
            newDestroyedCubes = Mathf.RoundToInt(newDestroyedCubes);
            float elapsedTime = 0f;
            float duration = 0.1f;
            counter.accCount += 1 * factor;

            while (elapsedTime < duration)
            {
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (data.scoreType == 0)
                {
                    counter.score = (int)Mathf.Lerp(counter.score, newDestroyedCubes, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    scoreText.text = counter.score.ToString("N0");
                    yield return null;
                }
                yield return null; // Wait for the next frame
            }

            // Ensure the score reaches the final value precisely
            counter.score = Mathf.RoundToInt(newDestroyedCubes);
            yield return new WaitForSeconds(0.2f);
        }
        public IEnumerator ChangeScoreLong()
        {
            counter.destroyedCubes += 50;
            five++;
            // Calculate the new destroyed cubes based on the factor
            int newDestroyedCubes = counter.score + Mathf.RoundToInt((float)counter.destructionPercentage) * ((int)counter.accCount / Total * 100 / 2) + combo * 20;
            newDestroyedCubes = Mathf.RoundToInt(newDestroyedCubes);
            if (AudioManager.Instance != null)
            {
                if (AudioManager.Instance.hits)
                {
                    Instantiate(goodTextPrefab, transform.position, Quaternion.identity);
                }
            }

            float elapsedTime = 0f;
            float duration = 0.1f;
            counter.accCount += 1;

            while (elapsedTime < duration)
            {
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (data.scoreType == 0)
                {
                    counter.score = (int)Mathf.Lerp(counter.score, newDestroyedCubes, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    scoreText.text = counter.score.ToString("N0");
                    yield return null;
                }
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
                Destroy(cube);
                activeCubes.Remove(cube);

                if (counter.destroyedCubes > counter.maxScore)
                {
                    counter.destroyedCubes = counter.maxScore;
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


            if (collision.tag == "Cubes" && collision.gameObject.name.Contains("hitter01"))
            {
                collision.GetComponent<Animation>().Play();
            }

            if (collision.tag == "Cubes" || collision.gameObject.name.Contains("hitter02"))
            {
                activeCubes.Add(collision.gameObject);
                UnityEngine.Debug.Log(collision.gameObject);
            }
            if (collision.tag == "LongCube" && collision.transform.position.y == transform.position.y)
            {
                activeCubes.Add(collision.gameObject);
                if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Return) || Input.GetMouseButton(1) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.L)) && !isDying)
                {
                    bufferActive = false;
                }

            }
        }


        private void OnTriggerExit2D(Collider2D collision)
        {
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
                    health -= 35; // Lower health due to passing the cube
                    combo = 0; // Reset combo

                    StartCoroutine(ChangeTextCombo());
                }


            }
            if (collision.gameObject.name.Contains("hitter02"))
            {
                if (!bufferActive)
                {
                    UnityEngine.Debug.Log("failed: " + bufferActive);
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
                else if (bufferActive)
                {
                    UnityEngine.Debug.Log("hit: " + bufferActive);
                    DestroyCube(collision.gameObject);

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
                        StartCoroutine(ChangeScoreLong());
                    }
                    else
                    {
                        StartCoroutine(ChangeScoreLong());
                        counter.destroyedCubes += 50;
                        scoreText.text = $"{counter.destroyedCubes}";
                    }
                    health += 20;
                    anim.Stop("comboanim");
                    anim.Play("comboanim");
                    comboTime = 0;
                }
            }

        }


        private IEnumerator OnTriggerEnter2DBuffer()
        {
            if (bufferActive)
            {
                float elapsedTime = 0f;
                float duration = 0.1f;
                elapsedTime += Time.deltaTime;
                int newDestroyedCubes = counter.score + combo + 1;
                counter.score = (int)Mathf.Lerp(counter.score, newDestroyedCubes, elapsedTime / duration);
                scoreText.text = counter.score.ToString("N0");
                // Ensure the score reaches the final value precisely
                counter.score = newDestroyedCubes;
                health += 0.25f;
                yield return null;
            }


            yield return null;
        }

        private void ProcessCollision(Collider2D collision)
        {
            StopCoroutine(OnTriggerEnter2DBuffer());
            bufferActive = false;
            passedCubes.Add(collision.gameObject);
            DestroyCube(collision.gameObject);

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
                StartCoroutine(ChangeScoreLong());
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


        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.name.Contains("hitter02") && collision.transform.position.y == transform.position.y)
            {
                UnityEngine.Debug.Log(bufferActive);
                if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.L)) && !isDying)
                {
                    combo++;
                    if (highestCombo == combo)
                    {
                        highestCombo++;
                    }
                    bufferActive = true;

                }
                else if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Return) || Input.GetMouseButton(1) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.L)) && !isDying)
                {
                    StartCoroutine(OnTriggerEnter2DBuffer());
                }

                else if (!(Input.GetMouseButton(0) || Input.GetKey(KeyCode.Return) || Input.GetMouseButton(1) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.L)) && !isDying)
                {
                    bufferActive = false;
                }

            }
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
}
