using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

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
    private bool coroutineRunning;

    private void Start()
    {
        
        float num1 = float.NegativeInfinity;
        itemUnused itemUnused1 = null;
        foreach (itemUnused itemUnused2 in FindObjectsOfType<itemUnused>())
        {
            float num2 = itemUnused2.transform.position.x - transform.position.x;
            if (num2 > num1)
            {
                num1 = num2;
                itemUnused1 = itemUnused2;
            }
        }
        if (itemUnused1 != null)
        {
            transform.position = new Vector3((itemUnused1.transform.position + new Vector3(5f, 0f, 0.0f)).x, 0.0f, 0.0f);

        }
       
    }

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

    private void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player0 = player.GetComponent<PlayerMovement>();
        scores = player.GetComponent<CubeCounter>();
        float num1 = float.NegativeInfinity;
        itemUnused itemUnused1 = null;
        foreach (itemUnused itemUnused2 in FindObjectsOfType<itemUnused>())
        {
            float num2 = itemUnused2.transform.position.x - transform.position.x;
            if (num2 > num1)
            {
                num1 = num2;
                itemUnused1 = itemUnused2;
            }
        }
        if (itemUnused1 != null && transform.position.x < itemUnused1.transform.position.x)
        {
            transform.position = new Vector3((itemUnused1.transform.position + new Vector3(5f, 0f, 0.0f)).x, 0.0f, 0.0f);

        }

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

            float destruction = player0.counter.score;
        float actualdest = (float)player0.counter.destructionPercentage;
            deadScore.text = "Your stats for this attempt: \nTier: " + scores.GetTier(actualdest) + "\nHighest Combo: " + player0.highestCombo.ToString() + string.Format("\nScore: {0} ({1})", destruction.ToString("N0"), scores.destroyedCubes);

        
    }

    private void SaveLevelData(string levelKey, string tierKey, string destructionKey, float destruction, string actualDestructionKey, float actualDestruction)
    {
        // Save destruction and actual destruction values
        PlayerPrefs.SetFloat(destructionKey, destruction);
        PlayerPrefs.SetFloat(actualDestructionKey, actualDestruction);

        // Save other level data if needed
        PlayerPrefs.SetString(levelKey, Guid.NewGuid().ToString("N"));
        PlayerPrefs.SetString(tierKey, scores.GetTier(destruction));
    }

    private float LoadLevelData(string destructionKey, float actualDestructionKey)
    {
        float destruction = PlayerPrefs.GetFloat(destructionKey, 0f);

        // Return destruction or actual destruction based on your requirements
        return destruction;
    }


    private IEnumerator End()
    {
        coroutineRunning = true;
        PlayerMovement objectOfType = FindObjectOfType<PlayerMovement>();
        float destruction = objectOfType.counter.score;
        float actualdest = (float)player0.counter.destructionPercentage;
        Scene activeScene = SceneManager.GetActiveScene();
        player.transform.localScale = Vector3.zero;
        objectOfType.enabled = false;
        float lastScore = 0f;
        if (activeScene.buildIndex == 25)
        {

            lastScore = LoadLevelData("destexplorer1", destruction);
            SaveLevelData("Explorers", "ExplorersTier", "destexplorer", actualdest, "destexplorer1", destruction);
        }

        if (activeScene.buildIndex == 26)
        {
            lastScore = LoadLevelData("destgd1", destruction);
            SaveLevelData("GeometricalDominator", "GeometricalDominatorTier", "destgd", actualdest, "destgd1", destruction);

        }

        if (activeScene.buildIndex == 27)
        {
            lastScore = LoadLevelData("destsky1", destruction);
            SaveLevelData("SkySoul", "SkySoulTier", "destsky", actualdest, "destsky1", destruction);
        }

        if (activeScene.buildIndex == 28)
        {
            lastScore = LoadLevelData("destredhorizon1", destruction);
            SaveLevelData("RedHorizon", "RedHorizonTier", "destredhorizon", actualdest, "destredhorizon1", destruction);
        }

        if (activeScene.buildIndex == 29)
        {
            lastScore = LoadLevelData("destskystrike1", destruction);
            SaveLevelData("Skystrike", "SkystrikeTier", "destskystrike", actualdest, "destskystrike1", destruction);
        }

        PlayerPrefs.Save();
        yield return new WaitForSecondsRealtime(2f);
        AudioSource[] audios = FindObjectsOfType<AudioSource>();
        Debug.Log(audios);
        float startVolume = 1f;
        float targetVolume = 0f;
        float currentTime = 0f;
        finishMenu.SetActive(true);
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        if (data.scoreType == 0)
            score.text = "Tier: " + scores.GetTier(actualdest) + "\nHighest Combo: " + player0.highestCombo.ToString() + string.Format("\nScore: {0} ({1})", destruction.ToString("N0"), scores.destroyedCubes) + string.Format("\nAccuracy: {0:00.00}%", (object)scores.destructionPercentage);
        else
            score.text = "Tier: " + scores.GetTier(actualdest) + "\nHighest Combo: " + player0.highestCombo.ToString() + string.Format("\nScore: {0}", scores.destroyedCubes) + string.Format("\nAccuracy: {0:00.00}%", (object)scores.destructionPercentage);
        
        float currentScore = player0.counter.score;
        float scoreDifference = currentScore - lastScore;
        // Add the score difference as XP
        if (scoreDifference > 0)
        {
            float xpToAdd = scoreDifference;
            LevelSystem.Instance.GainXP(xpToAdd);
        }
        else if (lastScore == 0 && scoreDifference <= 0)
        {
            LevelSystem.Instance.GainXP(scoreDifference);
        }

        while (currentTime < 3f)
        {

            currentTime += Time.deltaTime;
            foreach (AudioSource audio in audios)
            {
                audio.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / 3f);
            }
            yield return null;


        }
        // Stop playing the audio after 10 seconds
        foreach (AudioSource audio in audios)
        {
            audio.Stop();
        }

    }

}
