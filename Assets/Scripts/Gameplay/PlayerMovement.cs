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
using JammerDash.Tech;
using JammerDash.Editor.Basics;
using System.Linq;
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
        public float health;
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
        public AudioClip shortCube;
        public AudioClip longCube;

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

        public GameObject canvas;
        private void Start()
        {
            volume = FindObjectOfType<PostProcessVolume>();
            volume.profile.TryGetSettings(out vignette);
            initialIntensity = vignette.intensity.value;
            vignette.color.value = startColor;
            if (LevelDataManager.Instance.playerhp != 0)
            {
                maxHealth = LevelDataManager.Instance.playerhp;
                hpSlider.maxValue = LevelDataManager.Instance.playerhp;
            }
            else if (CustomLevelDataManager.Instance.playerhp != 0)
            {
                maxHealth = CustomLevelDataManager.Instance.playerhp;
                hpSlider.maxValue = CustomLevelDataManager.Instance.playerhp;
            }
            else
                maxHealth = 300;
            InputSystem.pollingFrequency = 1000;
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            shortCube = Resources.Load<AudioClip>("Audio/SFX/pause");
            sfxS.clip = shortCube;
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
            if (data.randomSFX && !UAP_AccessibilityManager.IsActive())
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
            health = maxHealth;
            CustomLevelDataManager.Instance.sceneLoaded = false;
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

            GetComponentInChildren<AudioSource>().time = transform.position.x / 7f;
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
                scoreText.text = $"{counter.score}";
           
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
            if (UAP_AccessibilityManager.IsActive())
            {

                foreach (itemUnused cube in FindObjectsByType(typeof(itemUnused), FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<itemUnused>())
                {
                    float distance = Vector3.Distance(transform.position, cube.transform.position);

                    if (cube.transform.position.x < 3)
                    {
                        Destroy(cube.gameObject);
                        health = maxHealth;
                    }
                    if (distance <= 2.25f)
                    { 
                        sfxS.PlayOneShot(shortCube);
                    }

                        
                    // Check if the cube's name contains "hitter01(Clone)"
                    if (cube.transform.gameObject.name.Contains("hitter01(Clone)"))
                    {
                        cube.transform.position = new Vector3(cube.transform.position.x, -1, cube.transform.position.z);
                    }
                    else
                    {
                        Debug.Log("Destroying cube: " + cube.transform.gameObject.name);
                        Destroy(cube.gameObject, 0);
                    }


                }

            }

        }
        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.H))
                Time.timeScale = 0.25f;
            else if (Input.GetKeyUp(KeyCode.H))
                Time.timeScale = 1f;
#endif
            if (!UAP_AccessibilityManager.IsActive())
            {
                float newVolume = 1.0f - (combo / 2) * 0.01f;
                newVolume = Mathf.Clamp(newVolume, 0.25f, 1.0f);

                // Set the calculated volume to the SFX AudioSource
                sfxS.volume = newVolume;

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
            else
            {
                sfxS.volume = 1f;
               
                    keyText.text = "Accessibility mode\nGameplay changed.";
            }
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F1))
            {
                canvas.SetActive(!canvas.activeSelf);
                data.canvasOff = !data.canvasOff;
                SettingsFileHandler.SaveSettingsToFile(data);
            }
            if (data.canvasOff)
            {
                StartCoroutine(FindObjectOfType<PauseMenu>().CheckUI());
            }
           if (!UAP_AccessibilityManager.IsActive()) // Check for vertical movement
          {  if (Input.GetKeyDown(KeyCode.W) && transform.position.y < maxY && !isDying || Input.GetKeyDown(KeyCode.UpArrow) && transform.position.y < maxY && !isDying)
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
            }

            // Calculate individual skill components
            float accuracy = Mathf.Clamp(counter.accCount / Total, 0, 1);
            float comboEfficiency = highestCombo != 0 ? highestCombo / 150 : 0;
            float adaptability = five * 0.05f + three + 0.025f + one * -0.1f + misses * -0.5f;
            float levelCompletionTime = Mathf.Clamp01(CalculateLevelCompletionTime() / 1);

            // Calculate skill performance point
            float skillPerformancePoint = Mathf.Max((
                Mathf.Pow(accuracy, 2) * accuracyWeight +
                comboEfficiency +
                adaptability +
                levelCompletionTime +
                CustomLevelDataManager.Instance.diff * 0.35f),
                0f
            );
            SPInt = Mathf.RoundToInt(skillPerformancePoint);
            if (Total > 0)
            {
                float acc = (float)counter.accCount / Total * 100;
                this.acc.text = $"{acc:F2}% | {counter.GetTier(acc)} | {skillPerformancePoint:F2} sp";
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
                            if (highestCombo <= combo)
                            {
                                highestCombo++;
                            }
                            combo++;

                           
                                StartCoroutine(ChangeScore(Vector3.Distance(hit.collider.transform.position, transform.position)));
                            
                           
                                counter.destroyedCubes += 50;
                                
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
                        sfxS.PlayOneShot(fail);
                        
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
                        sfxS.PlayOneShot(fail);
                        counter.destroyedCubes -= 250;
                    counter.destroyedCubes -= 100 - combo;

                    combo = 0;
                    StartCoroutine(ChangeTextCombo());
                }
            }
           

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
            if (data.randomSFX && !UAP_AccessibilityManager.IsActive())
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

            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            
                float formula = Mathf.RoundToInt(((LevelDataManager.Instance.diff + CustomLevelDataManager.Instance.diff) * 10) + health / 100 + ((int)counter.accCount / Total * 100) + combo * 3 * factor * 2) * 3;
                if (UAP_AccessibilityManager.IsActive())
                    formula = formula * 2.5f;
                float newDestroyedCubes = counter.score + formula;
                newDestroyedCubes = Mathf.RoundToInt(newDestroyedCubes);
                float elapsedTime = 0f;
                float duration = 0.1f;
                while (elapsedTime < duration)
                {
                   
                        counter.score = (int)Mathf.Lerp(counter.score, newDestroyedCubes, elapsedTime / duration);
                        elapsedTime += Time.deltaTime;
                        scoreText.text = $"{counter.score}\n<color=lime>(+{formula})</color>";
                        yield return null;
                    
                    yield return null; // Wait for the next frame
                }


            
            counter.accCount += 1 * factor;

            counter.destroyedCubes += 50;

            yield return new WaitForSeconds(0.2f);
            // Ensure the score reaches the final value precisely
            counter.score = Mathf.RoundToInt(newDestroyedCubes);
        }
        public IEnumerator ChangeScoreLong()
        {
            counter.destroyedCubes += 50;
            five++;
            int formula = Mathf.RoundToInt((float)counter.destructionPercentage) * ((int)counter.accCount / Total * 100 / 2) + combo * 20;
            int newDestroyedCubes = counter.score + formula;
            health += 30f;
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
                
                    counter.score = (int)Mathf.Lerp(counter.score, newDestroyedCubes, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    scoreText.text = $"{counter.score}\n<color=lime>(+{formula})</color>";
                
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
            transform.localScale = Vector3.zero;
            Account.Instance.GainXP(counter.score);
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
                    if (highestCombo <= combo)
                    {
                        highestCombo++;
                    }
                    if (collision.offset.x >= 5)
                        combo += Mathf.RoundToInt(collision.offset.x / 5);
                    else
                        combo += 1;

                    SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                    
                        StartCoroutine(ChangeScoreLong());
                   
                        StartCoroutine(ChangeScoreLong());
                        counter.destroyedCubes += 50;
                    
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
                int formula = (combo + 1) * 10;
                int newDestroyedCubes = counter.score + formula;
                scoreText.text = $"{counter.score}\n<color=lime>(+{formula})</color>";
                // Ensure the score reaches the final value precisely
                counter.score = newDestroyedCubes;
                health += 0.175f;
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
            if (highestCombo <= combo)
            {
                highestCombo++;
            }
            combo++;

            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                StartCoroutine(ChangeScoreLong());
          
                counter.destroyedCubes += 50;
                scoreText.text = $"{counter.destroyedCubes}";
            
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
                    if (highestCombo <= combo)
                    {
                        highestCombo++;
                    }
                    bufferActive = true;
                    sfxS.PlayOneShot(hit);
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
