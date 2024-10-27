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
using JammerDash.Game.Player;
using UnityEngine.InputSystem.Controls;
using JammerDash.Difficulty;
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
        public Text debug;
        public Text acc;
        private PostProcessVolume volume;
        private Vignette vignette;
        private float initialIntensity;
        public float vignetteStartHealthPercentage = 0.5f;
        public UnityEngine.Color startColor = UnityEngine.Color.red;
        public int k;
        public int l;
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
        public float _performanceScore;

        private float _gameDifficulty;

        [Header("Others")]
        private float jumpHeight = 1f;
        private float minY = -1f;
        private float maxY = 4f;
        public int combo;
        public int highestCombo;
        bool isDying = false;
        bool invincible = false;
        public int Total = 0;
        private bool bufferActive = false;
        public int maxScore = 1000000;
        public float factor;
        public int misses;
        public int five;
        public int three;
        public int one;
        public int SPInt;
        private bool isBufferRunning = false;

        private void Awake()
        {
            music = AudioManager.Instance.source;
        }
        private void Start()
        {
            CustomLevelDataManager.Instance.sceneLoaded = false;
            volume = FindObjectOfType<PostProcessVolume>();
            volume.profile.TryGetSettings(out vignette);
            initialIntensity = vignette.intensity.value;
            vignette.color.value = startColor;
           if (CustomLevelDataManager.Instance.playerhp != 0)
            {
                maxHealth = CustomLevelDataManager.Instance.playerhp;
                hpSlider.maxValue = CustomLevelDataManager.Instance.playerhp;
            }
            else
                maxHealth = 300;
            InputSystem.pollingFrequency = 1000;
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (data.hitType == 0)
            {
                goodTextPrefab = Resources.Load<GameObject>("good");
                okTextPrefab = Resources.Load<GameObject>("crap");
                normalTextPrefab = Resources.Load<GameObject>("ok");
                badTextPrefab = Resources.Load<GameObject>("bad");
            }
            else if (data.hitType == 1)
            {
                goodTextPrefab = Resources.Load<GameObject>("RinHit");
                okTextPrefab = Resources.Load<GameObject>("RinOK");
                normalTextPrefab = Resources.Load<GameObject>("RinNormal");
                badTextPrefab = Resources.Load<GameObject>("RinMiss");
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
            health = maxHealth;
        }
      

        private void OnJump()
        {
            if (transform.position.y < maxY)
              transform.position += new Vector3(0f, jumpHeight, 0f);
            
        }
        private void OnBoost()
        {
            if (transform.position.y < maxY - 1)
            {
                transform.position += new Vector3(0f, jumpHeight * 2f, 0f);
                sfxS.PlayOneShot(jump);

            }

        }
        private void OnCrouch()
        {
            if (transform.position.y > minY && !isDying)
            {
                transform.position -= new Vector3(0f, jumpHeight, 0f);
            }
        }

        private void OnHit()
        {
            if (Input.GetKey(KeybindingManager.hit1))
                k++;
            else if (Input.GetKey(KeybindingManager.hit2))
                l++;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, cubeLayerMask);

            if (hit.collider != null)
            {
                HandleHit(hit);
            }
            else
            {
                HandleMiss();
            }
        }

        private void OnResetPosition()
        {
            if (transform.position.y > -1)
            {
                if (transform.position.y > 0)
                {
                    sfxS.PlayOneShot(impact);
                }
                transform.position = new Vector3(transform.position.x, -1, transform.position.z);
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
            if (FindNearestCubeDistance() < 21)
                health -= 0.25f;
           
            hpint = (int)health;
           
           

            // Debug text area
            {
               
                
                debug.text = $"<b>KEYS</b>\r" +
                    $"\nKey1: {k}\r\n" +
                    $"Key2: {l}\r\n" +
                    $"\r\n" +
                    $"<b>POSITIONING</b>\r\n" +
                    $"Pos: {transform.position.x},{transform.position.y}\r\n" +
                    $"MusicTime: {music.time}\r\n" +
                    $"\r\n<b>SCORING</b>\r\n" +
                    $"five: {five}\r\n" +
                    $"three: {three}\r\n" +
                    $"one: {one}\r\n" +
                    $"miss: {misses}\r\n" +
                    $"score: {counter.score}\r\n" +
                    $"combo: {combo}x\r\n" +
                    $"health: {hpint}\r\n" +
                    $"accuracy: {counter.accCount / Total * 100:000.00}% ({counter.accCount} / {Total})\r\n" +
                    $"sp: {_performanceScore:0.00}";
            }
            if (health > maxHealth)
            {
                health = maxHealth;
            }
            hpSlider.value = health;
            hpSlider.maxValue = maxHealth;

            UpdateVignette(hpint);

            combotext.text = combo.ToString() + "x";



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
            float playerPositionInSeconds = transform.position.x / 7;
            float finishLinePositionInSeconds = FindObjectOfType<FinishLine>().transform.position.x / 7 - playerPositionInSeconds;

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
            if (!isDying)
            {
                if (Input.GetKeyDown(KeybindingManager.up))
                {
                    OnJump();
                }
                if (Input.GetKeyDown(KeybindingManager.down))
                {
                    OnCrouch();
                }
                if (Input.GetKeyDown(KeybindingManager.boost))
                {
                    OnBoost();
                }
                if (Input.GetKeyDown(KeybindingManager.ground))
                {
                    OnResetPosition();
                }
                if (Input.GetKeyDown(KeybindingManager.hit1) || Input.GetKeyDown(KeybindingManager.hit2))
                {
                    OnHit();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.F5))
            {
                var parentGameObject = debug.transform.parent.gameObject;
                parentGameObject.SetActive(!parentGameObject.activeSelf);


            }
            if (music.isPlaying)
                transform.position = Vector2.Lerp(transform.position, new Vector2(music.time * moveSpeed, transform.position.y), 1); 
            else
                transform.Translate(moveSpeed * Time.deltaTime * Vector2.right);

            float distanceToFinishLine = Mathf.Abs(cam.transform.position.x - FindObjectOfType<FinishLine>().transform.position.x);
            if (cam.transform.position.x < FindObjectOfType<FinishLine>().transform.position.x)
            {;

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
                    Vector3 smoothedPosition = Vector3.Lerp(cam.transform.position, targetPosition, 1000f * Time.deltaTime);
                    cam.transform.position = smoothedPosition;

                }
            }   
           

            // Calculate skill performance point
            float distanceInPercent = Mathf.Abs(cam.transform.position.x / FindObjectOfType<FinishLine>().transform.position.x);
            ShinePerformance calc = new ShinePerformance(five, three, one, misses, combo, highestCombo, CustomLevelDataManager.Instance.diff, distanceInPercent);
            _performanceScore = calc.PerformanceScore;
            SPInt = Mathf.RoundToInt(_performanceScore);
            if (Total > 0)
            {
                
                float acc = (float)counter.accCount / Total * 100;
                if (float.IsNaN(acc))
                {
                    acc = 0;
                }
                else if (float.IsNaN(_performanceScore))
                {
                    _performanceScore = 0;
                }
                this.acc.text = $"{acc:F2}% | {counter.GetTier(acc)} | {~~SPInt:F0} sp";
            }
            else
            {
                acc.text = $"";
            }


            if (FindNearestCubeDistance() > 21)
            {
                keyText.text = "Break!";
            }
        }

        void HandleHit(RaycastHit2D hit)
        {
            bufferActive = true;

            if (hit.transform.position.y == transform.position.y)
            {
                if (hit.transform.name.Contains("hitter01"))
                {
                    passedCubes.Add(hit.collider.gameObject);
                    DestroyCube(hit.collider.gameObject);

                    Animation anim = combotext.GetComponent<Animation>();
                    if (highestCombo <= combo)
                    {
                        highestCombo++;
                    }
                    combo++;

                    StartCoroutine(ChangeScore(Vector3.Distance(hit.collider.transform.position, transform.position)));

                    anim.Stop("comboanim");
                    anim.Play("comboanim");
                }
            }
            else if (passedCubes.Count > 0 && FindNearestCubeDistance() < 2)
            {
                HandleBadHit();
            }
        }

        void HandleMiss()
        {
            float nearestDistance = FindNearestCubeDistance();

            if (passedCubes.Count > 0 && nearestDistance < 2)
            {
                HandleBadHit();
            }
        }

        public float FindNearestCubeDistance()
        {
            float nearestDistance = Mathf.Infinity;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10, cubeLayerMask);

            foreach (Collider2D collider in colliders)
            {
                if (IsWithinColliderBounds(collider))
                {
                    // If within bounds, distance is effectively 0
                    return 0;
                }

                float distance = Vector2.Distance(transform.position, collider.transform.position);

                BoxCollider2D boxCollider = collider as BoxCollider2D;
                if (boxCollider != null)
                {
                    float colliderWidth = boxCollider.size.x;
                    distance += colliderWidth / 2 - boxCollider.offset.x;
                }

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                }
            }

            return nearestDistance;
        }

        bool IsWithinColliderBounds(Collider2D collider)
        {
            if (collider is BoxCollider2D boxCollider)
            {
                Bounds bounds = boxCollider.bounds;
                return bounds.Contains(transform.position);
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                Vector2 circleCenter = (Vector2)circleCollider.transform.position + circleCollider.offset;
                float radius = circleCollider.radius;
                return Vector2.Distance(transform.position, circleCenter) <= radius;
            }

            // Add more collider type checks as needed

            return false;
        }


        void HandleBadHit()
        {
            if (AudioManager.Instance != null && AudioManager.Instance.hits)
            {
                ShowBadText();
            }

            counter.destroyedCubes -= 100 - combo;
            sfxS.PlayOneShot(fail);

            combo = 0;
            StartCoroutine(ChangeTextCombo());
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
            health -= 30;
            counter.score -= 350;
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
            UnityEngine.Debug.Log(playerDistance);
            if (playerDistance <= 0.29f)
            {
                factor = 1f;
                five++;
                counter.accCount += 5;
                Total += 5;
                if (AudioManager.Instance != null)
                {
                    if (AudioManager.Instance.hits)
                    {
                        Instantiate(goodTextPrefab, transform.position, Quaternion.identity);
                    }
                }
                 
            }
            else if (playerDistance <= 0.45 && playerDistance > 0.29f)
            {
                factor = 1f / 3f;
                three++;
                counter.accCount += 3;
                Total += 5;
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
                counter.accCount += 1;
                Total += 5;
                if (AudioManager.Instance != null)
                {
                    if (AudioManager.Instance.hits)
                    {
                        Instantiate(okTextPrefab, transform.position, Quaternion.identity);
                    }
                }
            }

            counter.destroyedCubes += 50;
            float formula = (maxScore * factor) / counter.cubes.Count;
            float newDestroyedCubes = counter.score + formula;
            newDestroyedCubes = Mathf.RoundToInt(newDestroyedCubes);

            float elapsedTime = 0f;
            float duration = 0.1f;

            health += maxHealth / (20f * factor); 

            while (elapsedTime < duration)
            {
                counter.score = (int)Mathf.Lerp(counter.score, newDestroyedCubes, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                scoreText.text = $"{counter.score}\n<color=lime>(+{formula:0})</color>";
                yield return null;
            }

            // Ensure final score is precisely updated
            counter.score = Mathf.RoundToInt(newDestroyedCubes);
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

                Total += 5;

            }

        }


        private void EndLife()
        {
            deathPanel.SetActive(true);
            music.pitch = 0f;
            transform.localScale = Vector3.zero;
            JammerDash.Account.Instance.GainXP(counter.score);
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
                if ((Input.GetKey(KeybindingManager.hit1) ||Input.GetKey(KeybindingManager.hit2)) && !isDying)
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

                    health -= 30;
                    Total += 5;
                    activeCubes.Remove(collision.gameObject);
                }


            }
            if (collision.gameObject.name.Contains("hitter02"))
            {
                if (!bufferActive)
                {
                    activeCubes.Remove(collision.gameObject);


                    health -= 30;
                    bufferActive = false;

                }
                else if (bufferActive)
                {
                    UnityEngine.Debug.Log("hit: " + bufferActive);
                    DestroyCube(collision.gameObject);

                    bufferActive = false;
                    health += 30;
                }
            }


        }


        private IEnumerator OnTriggerEnter2DBuffer()
        {
            if (bufferActive)
            {
                int formula = 1;
                int newDestroyedCubes = counter.score + formula;
                scoreText.text = $"{counter.score}\n<color=lime>(+{formula})</color>";
                // Ensure the score reaches the final value precisely
                counter.score = newDestroyedCubes;
                
                health += 0.175f;
                yield return null;
            }


            yield return null;
        }



        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.name.Contains("hitter02") && collision.transform.position.y == transform.position.y)
            {

                if ((Input.GetKeyDown(KeybindingManager.hit1) || Input.GetKeyDown(KeybindingManager.hit2)) && !isDying)
                {
                    if (!bufferActive)
                    {

                        sfxS.PlayOneShot(hit);
                    }
                    bufferActive = true;

                    float distance = Vector2.Distance(collision.transform.position, transform.position);
                    float middle = Mathf.Abs(collision.offset.x);
                    Debug.Log(distance);
                    if (distance < 1)
                    {
                        combo++;
                        if (highestCombo < combo)
                        {
                            highestCombo++;
                        }

                        StartCoroutine(ChangeScore(0f));
                        if (AudioManager.Instance != null && AudioManager.Instance.hits)
                        {
                            Instantiate(goodTextPrefab, transform.position, Quaternion.identity);
                        }

                        Animation anim = combotext.GetComponent<Animation>();

                        anim.Stop("comboanim");
                        anim.Play("comboanim");
                    }
                    else if (distance < middle && distance >= 1)
                    {
                        StartCoroutine(ChangeScore(0.30f));
                        if (AudioManager.Instance != null && AudioManager.Instance.hits)
                        {
                            Instantiate(normalTextPrefab, transform.position, Quaternion.identity);
                        }
                    }
                    else if (distance >= middle)
                    {
                        health -= 35;
                        combo = 0;
                        StartCoroutine(ChangeScore(0.48f));
                        if (AudioManager.Instance != null && AudioManager.Instance.hits)
                        {
                            Instantiate(okTextPrefab, transform.position, Quaternion.identity);
                        }
                    }
                    
                }
                if ((Input.GetKeyUp(KeybindingManager.hit1) || Input.GetKeyUp(KeybindingManager.hit2)) && !isDying && collision.transform.position.y == transform.position.y) 
                {
                    counter.score -= Mathf.RoundToInt((maxScore * factor) / counter.cubes.Count);
                    bufferActive = false;
                }
                if ((Input.GetKey(KeybindingManager.hit1) || Input.GetKey(KeybindingManager.hit2)) && !isDying)
                {
                    if (!isBufferRunning && bufferActive)
                    {
                        StartCoroutine(OnTriggerEnter2DBufferLoop());
                    }
                    else
                    {
                        isBufferRunning = false;
                    }
                }

                else if (!(Input.GetKey(KeybindingManager.hit1) || Input.GetKey(KeybindingManager.hit2)) && !isDying)
                {
                    bufferActive = false;
                }
            }


            if (collision.name == "finishline")
            {
                health -= 0f;
            }
        }

        IEnumerator OnTriggerEnter2DBufferLoop()
        {
            isBufferRunning = true;
            while ((Input.GetKey(KeybindingManager.hit1) || Input.GetKey(KeybindingManager.hit2)) && !isDying && bufferActive)
            {
                StartCoroutine(OnTriggerEnter2DBuffer());
                yield return new WaitForSeconds(0.03f);    
            }
            isBufferRunning = false;
        }
    }

   
}
