using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public AudioSource mainSong;

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
        mainSong = GameObject.Find("Music").GetComponent<AudioSource>();
        if (player0 == null || player == null)
        {

            player0 = player.GetComponent<PlayerMovement>();
            player = player0.gameObject;
            
        }
        if (player.transform.position.x >= transform.position.x && player != null)
        {
            player.transform.position = transform.position;
            player0.enabled = false;
            StartCoroutine(FadeOutVolume());
        }

            float destruction = player0.counter.score;
        float actualdest = (float)player0.counter.destructionPercentage;
            deadScore.text = "Your stats for this attempt: \nTier: " + scores.GetTier(actualdest) + "\nHighest Combo: " + player0.highestCombo.ToString() + string.Format("\nScore: {0} ({1})", destruction.ToString("N0"), scores.destroyedCubes);

        
    }

    private void SaveLevelData(string levelKey, string tierKey, string destructionKey, float destruction)
    {
        if (PlayerPrefs.HasKey(destructionKey))
        {
            float num = PlayerPrefs.GetFloat(destructionKey);
            if ((double)destruction <= (double)num)
                return;
            PlayerPrefs.SetString(levelKey, Guid.NewGuid().ToString("N"));
            PlayerPrefs.SetString(tierKey, scores.GetTier(destruction));
            PlayerPrefs.SetFloat(destructionKey, destruction);
        }
        else
        {
            PlayerPrefs.SetString(levelKey, Guid.NewGuid().ToString("N"));
            PlayerPrefs.SetString(tierKey, scores.GetTier(destruction));
            PlayerPrefs.SetFloat(destructionKey, destruction);
        }
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
        if (activeScene.buildIndex == 25)
            SaveLevelData("Explorers", "ExplorersTier", "destexplorer", actualdest);
        if (activeScene.buildIndex == 26)
            SaveLevelData("GeometricalDominator", "GeometricalDominatorTier", "destgd", actualdest);
        if (activeScene.buildIndex == 27)
            SaveLevelData("SkySoul", "SkySoulTier", "destsky", actualdest);
        if (activeScene.buildIndex == 28)
            SaveLevelData("RedHorizon", "RedHorizonTier", "destredhorizon", actualdest);
        if (activeScene.buildIndex == 29)
            SaveLevelData("Skystrike", "SkystrikeTier", "destskystrike", actualdest);
        PlayerPrefs.Save();
        yield return new WaitForSecondsRealtime(2f);
        finishMenu.SetActive(true);

        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        if (data.scoreType == 0)
            score.text = "Tier: " + scores.GetTier(actualdest) + "\nHighest Combo: " + player0.highestCombo.ToString() + string.Format("\nScore: {0} ({1})", destruction.ToString("N0"), scores.destroyedCubes) + string.Format("\nAccuracy: {0:00.00}%", (object)scores.destructionPercentage);
        else
            score.text = "Tier: " + scores.GetTier(actualdest) + "\nHighest Combo: " + player0.highestCombo.ToString() + string.Format("\nScore: {0}", scores.destroyedCubes) + string.Format("\nAccuracy: {0:00.00}%", (object)scores.destructionPercentage);
       
    }

    private IEnumerator FadeOutVolume()
    {
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        float duration = 2f;
        float elapsedTime = 0.0f;
        float startVolume = mainSong.volume;
        while (elapsedTime < duration)
        {
            mainSong.outputAudioMixerGroup.audioMixer.SetFloat("Lowpass", Mathf.Lerp(22000f, data.lowpassValue, elapsedTime / duration));
            
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }
        mainSong.outputAudioMixerGroup.audioMixer.SetFloat("Lowpass", data.lowpassValue);
        coroutineRunning = false;
    }
}
