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

    void SaveLevelData(float actualdest, float destruction)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "LevelDefault")
        {
            SaveLevelDataForLevelDefault(actualdest, destruction);
        }
        else
        {
            SaveLevelDataDef(SceneManager.GetActiveScene().buildIndex, scores.GetTier(actualdest), "dest" + sceneName, actualdest, "scores", destruction);
        }
    }
    void SaveLevelDataForLevelDefault(float actualdest, float destruction)
    {
        // Construct the path based on conditions
        string levelsPath = Path.Combine(Application.persistentDataPath,
            string.IsNullOrEmpty(CustomLevelDataManager.Instance.levelName)
                ? Path.Combine("scenes", LevelDataManager.Instance.levelName)
                : Path.Combine("levels", "extracted", CustomLevelDataManager.Instance.levelName));

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
            SaveLevelDataDef(sceneData.ID, scores.GetTier(actualdest), "dest" + sceneData.levelName, actualdest, "scores", destruction);
        }
        else
        {
            Debug.LogWarning("No valid level found in the directory: " + levelsPath);
        }
    }
    void SaveLevelDataDef(int levelID, string tierName, string destName, float actualdest, string fileName, float destruction)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName + ".dat");
        using (StreamWriter writer = File.AppendText(filePath))
        {
            writer.WriteLine($"{levelID},{tierName},{destName},{actualdest},{destruction}");
        }
    }
    float LoadLevelData(int levelID, string fileName, float destruction)
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
    private IEnumerator End()
    {
        coroutineRunning = true;
        PlayerMovement objectOfType = FindObjectOfType<PlayerMovement>();
        float destruction = objectOfType.counter.score;
        float actualdest = (float)player0.counter.destructionPercentage;
        if (SceneManager.GetActiveScene().name != "LevelDefault")
        {
            Scene activeScene = SceneManager.GetActiveScene(); 
            float lastScore = LoadLevelData(activeScene.buildIndex, "scores", destruction);
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
        } 
        else
        {

            int id = CustomLevelDataManager.Instance.ID;
            if (CustomLevelDataManager.Instance.levelName == null)
            {
                id = LevelDataManager.Instance.ID;
            }
            float lastScore = LoadLevelData(id, "scores", destruction); 
            float currentScore = player0.counter.score;
            float scoreDifference = currentScore - lastScore;
            // Add the score difference as XP
            if (scoreDifference > 0)
            {
                float xpToAdd = scoreDifference;
                LevelSystem.Instance.GainXP(xpToAdd);
            }
            else if (lastScore == 0)
            {
                LevelSystem.Instance.GainXP(scoreDifference);
            }
        }
        player.transform.localScale = Vector3.zero;
        objectOfType.enabled = false;

       
        SaveLevelData(actualdest, destruction);
        

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
