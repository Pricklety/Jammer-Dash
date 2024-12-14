using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JammerDash.Game.Player;
using JammerDash.Tech;
using JammerDash.Editor.Basics;
using JammerDash.Audio;
using UnityEngine.Localization.Settings;
using JammerDash.Difficulty;

namespace JammerDash.Game
{
    public class FinishLine : MonoBehaviour
    {
        private Rigidbody2D rb;
        public PlayerMovement player0;
        public Text score;
        public Text deadScore;
        public CubeCounter scores;
        public GameObject finishParticles;
        public GameObject player;
        public AudioSource finishSound;
        public GameObject finishMenu;
        public Image scoreRank;
        public Animator anim;
        public SceneData data;
        public float maxPos;
        public RawImage img;
        public Text total;
        public Text combo;
        public Text acc;
        [Header("Scores")]
        public Text five;
        public Text three;
        public Text one;
        public Text miss;

        [Header("Level Info + more")]
        public Text main;
        public Slider level;
        public Text lvl;


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                StartCoroutine(End());
                player.transform.position = transform.position;
                finishParticles.transform.position = player.transform.position;
                Instantiate(finishParticles, new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z), Quaternion.identity);
                finishSound.Play();
            }

        }

        private void SetLinePos()
        {
            float maxDistance = float.NegativeInfinity;
            itemUnused farthestItem = null;

            // Iterate over all itemUnused objects in the scene
            foreach (itemUnused currentItem in FindObjectsOfType<itemUnused>())
            {
                float distance = currentItem.transform.position.x - transform.position.x;

                // Find the itemUnused with the greatest x distance
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestItem = currentItem;
                }
            }

            // If a farthestItem is found, update the position
            if (farthestItem != null)
            {
                float newXPosition = farthestItem.transform.position.x + 5f;

                // If the SpriteRenderer's size in the x direction is greater than 1, add its size to the position
                SpriteRenderer spriteRenderer = farthestItem.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.size.x > 1)
                {
                    newXPosition += spriteRenderer.size.x;
                }

                // Update the current object's position
                maxPos = newXPosition;
            }
        }
        private void FixedUpdate()
        {
            if (maxPos < 5)
            {
                SetLinePos();
                return;
            }
            transform.position = new Vector3(maxPos, 0, 0);
            player = GameObject.FindGameObjectWithTag("Player");
            player0 = player.GetComponent<PlayerMovement>();
            scores = player.GetComponent<CubeCounter>();

            if (player0 == null || player == null)
            {
                player0 = player.GetComponent<PlayerMovement>();
                player = player0.gameObject;
            }
            if (player.transform.position.x >= transform.position.x && player != null)
            {
                player.transform.position = new Vector2(transform.position.x, transform.position.y + 0.5f);
                player0.enabled = false;
            }

            deadScore.text = "There's always another time! Maybe it's after you restart?";
            SimpleSpectrum[] spectrums = FindObjectsByType<SimpleSpectrum>(FindObjectsSortMode.None);
            foreach (SimpleSpectrum spectrum in spectrums)
            {
                if (spectrum.audioSource == null)
                {
                    SetSpectrum();
                }
            }
        }

        void SaveLevelData(float actualdest, float destruction)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "LevelDefault")
            {
                SaveLevelDataForLevelDefault(actualdest, destruction);
            }
           
        }
        void SaveLevelDataForLevelDefault(float actualdest, float destruction)
        {
            // Construct the path based on conditions
            string levelsPath = Path.Combine(Application.persistentDataPath, "levels", "extracted", CustomLevelDataManager.Instance.ID + " - " + CustomLevelDataManager.Instance.levelName);

            string[] levelFiles = Directory.GetFiles(levelsPath, "*.json", SearchOption.AllDirectories);
            string levelName = "";

            foreach (string file in levelFiles)
            {
                if (Path.GetFileName(file).Equals("LevelDefault.jdl"))
                {
                    continue; // Skip LevelDefault.jdl
                }
                levelName = Path.GetFileNameWithoutExtension(file);
                break; // Stop after finding the first valid level file
            }

            if (!string.IsNullOrEmpty(levelName))
            {
                string json = File.ReadAllText(Path.Combine(levelsPath, levelName + ".json"));
                SceneData sceneData = SceneData.FromJson(json);
                Debug.Log(sceneData.levelName);
                data = sceneData;
                SaveLevelDataDef(sceneData.ID, scores.GetTier(actualdest), player0._performanceScore, actualdest, "scores", destruction, player0.five, player0.three, player0.one, player0.misses, Account.Instance.username, player0.highestCombo);
            }
            else
            {
                Debug.LogWarning("No valid level found in the directory: " + levelsPath);
            }
        }

        void SaveLevelDataDef(int levelID, string tierName, float sp, float actualdest, string fileName, float destruction, int five, int three, int one, int miss, string username, int combo)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName + ".dat");
            using (StreamWriter writer = File.AppendText(filePath))
            {
                string formattedActualDest = actualdest.ToString("0.#################");
                string formattedDestruction = destruction.ToString("0.#################");

                writer.WriteLine($"{levelID},{tierName}, {sp},{formattedActualDest},{formattedDestruction},{five},{three},{one},{miss},{combo},{username}");
            }
        }
        public void SetSpectrum()
        {
            SimpleSpectrum[] spectrums = FindObjectsByType<SimpleSpectrum>(FindObjectsSortMode.None);

            foreach (SimpleSpectrum spectrum in spectrums)
            {
                spectrum.audioSource = AudioManager.Instance.source;
            }
        }

        long LoadLevelData(int levelID, string fileName, long destruction)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName + ".dat");

            if (File.Exists(filePath))
            {
                // Read the file and parse data as needed
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line == SceneManager.GetActiveScene().name)
                    {
                        string[] data = line.Split(',');
                        // Assuming the file format is consistent with the data you're saving
                        float actualdest = int.Parse(data[3]);
                        destruction = int.Parse(data[4]);
                        // Process the loaded data as needed
                        return destruction;
                    }
                    else
                    {
                        string[] data = line.Split(',');
                        if (int.Parse(data[0]) == levelID)
                        {
                            float actualdest = float.Parse(data[3]);
                            destruction = int.Parse(data[4]);
                            // Process the loaded data as needed
                            Debug.LogError(destruction);
                            return destruction;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("File does not exist: " + filePath);
            }

            return 0;
        }
        public string FormatNumber(long number)
        {
            string formattedNumber;

            if (number < 1000)
            {
                formattedNumber = number.ToString();
                return formattedNumber;
            }
            else if (number >= 1000 && number < 1000000)
            {
                formattedNumber = (number / 1000f).ToString("F1") + "K";
                return formattedNumber;
            }
            else if (number >= 1000000 && number < 1000000000)
            {
                formattedNumber = (number / 1000000f).ToString("F2") + "M";
                return formattedNumber;
            }
            else if (number >= 1000000000 && number < 1000000000000)
            {
                formattedNumber = (number / 1000000000f).ToString("F2") + "B";
                return formattedNumber;
            }
            else if (number >= 1000000000000 && number <= 1000000000000000)
            {
                formattedNumber = (number / 1000000000000f).ToString("F2") + "T";
                return formattedNumber;
            }
            else
            {
                formattedNumber = (number / 1000000000000f).ToString("F3") + "Q";
                return formattedNumber;
            }
        }
        private IEnumerator End()
        {
            StartCoroutine(CustomLevelDataManager.Instance.LoadImage(Path.Combine(Application.persistentDataPath, "levels", "extracted", $"{CustomLevelDataManager.Instance.ID} - {CustomLevelDataManager.Instance.levelName}", "bgImage.png"), img));
            // Play the animation if available
            if (anim != null)
            {
                anim.Play("finish");
            }

            PlayerMovement objectOfType = FindObjectOfType<PlayerMovement>();
            long destruction = objectOfType.counter.score;
            float actualdest = (float)player0.counter.destructionPercentage;
            Account.Instance.GainXP(destruction);

            score.text = $"Level: {Account.Instance.level}\n" +
                $"XP: {FormatNumber(Account.Instance.totalXP)}\n" +
                $"\n" +
                $"SP: {player0.SPInt:N0}\n" +
                $"Ranking: \n" +
                $"Total SP: {Mathf.RoundToInt(Calculator.CalculateSP("scores.dat"))}\n" +
                $"\n" +
                $"{LocalizationSettings.StringDatabase.GetLocalizedString("lang", "played by")} {Account.Instance.username}";
            acc.text = $"Accuracy: {player0.counter.accCount / player0.Total * 100}%\n" +
                $"Score: {player0.counter.score}\n" +
                $"Combo: {player0.highestCombo}x"; //total and combo texts are free
            total.text = $"Leaderboard: \n" +
                $"Total level score: \n" +
                $"Jams: \n" +
                $"Level shines: \n";

            if (scoreRank != null)
            {
                scoreRank.sprite = Resources.Load<Sprite>($"ranking/{player0.counter.GetTier(player0.counter.accCount / player0.Total * 100)}");
            }

            player.transform.localScale = Vector3.zero;
            objectOfType.enabled = false;

            SaveLevelData(actualdest, destruction);

            AudioSource[] audios = FindObjectsOfType<AudioSource>();
            Debug.Log(audios);
            finishMenu.SetActive(true);

            // Update score breakdown
            five.text = $"{player0.five}";
            three.text = $"{player0.three}";
            one.text = $"{player0.one}";
            miss.text = $"{player0.misses}";

            main.text = $"{data.artist}\r\n{data.songName}";


            // Play the exponential progress animation with SFX
            float targetAccuracy = player0.counter.accCount / player0.Total;
            float progress = 0;
            float duration = 2f;
            float elapsedTime = 0f;

            // Play the long SFX
            AudioManager.Instance.source.PlayOneShot(Resources.Load<AudioClip>("Audio/SFX/progress"));

            // Gradually update the progress slider value
            while (progress < targetAccuracy)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                progress = Mathf.Pow(t, 2) * targetAccuracy; // Exponential interpolation
                level.value = Mathf.Clamp(progress, 0, 1);

                yield return null;
            }



            // Play rank SFX after progress completes
            AudioManager.Instance.source.PlayOneShot(Resources.Load<AudioClip>($"Audio/SFX/ranking/{player0.counter.GetTier(targetAccuracy * 100)} Rank"));
        }


    }


}