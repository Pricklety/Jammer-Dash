using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomLevelDataManager : MonoBehaviour
{
    // Singleton instance
    public static CustomLevelDataManager Instance;
    public string levelName;
    public string creator;
    public int diff;
    bool loaded;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Function to load SceneData based on the scene name
    public SceneData LoadLevelData(string sceneName)
    {
        levelName = sceneName;
        string filePath = Path.Combine(Application.persistentDataPath, "levels", sceneName + ".jdl");

        if (File.Exists(filePath))
        {
            filePath = Path.Combine(Application.persistentDataPath, "levels", "extracted", levelName, $"{levelName}.json");
            string json = File.ReadAllText(filePath);
            Debug.LogWarning(filePath);
            SceneData sceneData = SceneData.FromJson(json);
            levelName = sceneData.levelName;
            creator = sceneData.creator;
            diff = (int)sceneData.calculatedDifficulty;
            loaded = false;
            Addressables.LoadScene("Assets/" + SceneManager.GetActiveScene().name + ".unity", LoadSceneMode.Single);

        }
        else
        {
            Debug.LogWarning("JDL file not found: " + filePath);
        }

        return null;
    }


    IEnumerator LoadImage(string url)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Enable the GameObject with the RawImage component
                RawImage rawImage = FindInactiveObjectOfType<RawImage>();
                if (rawImage != null)
                {
                    // Enable the GameObject with the RawImage component
                    rawImage.gameObject.SetActive(true);
                }

                // Create a Sprite from the downloaded texture and set it to the Image component
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                if (rawImage != null)
                {
                    // Enable the GameObject with the RawImage component
                    rawImage.texture = texture;
                }
            }
            else
            {
                Debug.Log("Failed to load image: " + www.error);
            }
        }
    }
    // Function to find an inactive object of a specific type
    private T FindInactiveObjectOfType<T>() where T : Component
    {
        // Find all objects of type T in the scene
        T[] components = Resources.FindObjectsOfTypeAll<T>();

        // Iterate through the found components
        foreach (T component in components)
        {
            // Check if the GameObject of the component is inactive
            if (!component.gameObject.activeSelf)
            {
                // Return the component if it's inactive
                return component;
            }
        }

        // If no inactive object of the specified type is found, return null
        return null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LevelDefault")
        {
            StartCoroutine(ProcessAfterSceneLoaded());
        }
    }

    private IEnumerator ProcessAfterSceneLoaded()
    {
        new WaitForEndOfFrame();
        string filePath = Path.Combine(Application.persistentDataPath, "levels", "extracted", levelName, $"{levelName}.json");
        filePath = filePath.Replace("/", "\\");
        Debug.Log(filePath);
        string json = File.ReadAllText(filePath);
        SceneData sceneData = SceneData.FromJson(json);
        Debug.Log(sceneData.levelName);

        Debug.Log(sceneData.cubePositions.Count);

        foreach (Vector3 cubePos in sceneData.cubePositions)
        {
            GameObject cubeObject = Instantiate(Resources.Load<GameObject>("hitter01"), cubePos, Quaternion.identity);
            Debug.Log("Cube instantiated: " + cubeObject);

        }

        foreach (Vector3 sawPos in sceneData.sawPositions)
        {
            Instantiate(Resources.Load<GameObject>("Saws and Spikes/rotateSaw01"), sawPos, Quaternion.identity);
        }
        if (sceneData.picLocation != null)
        {
            StartCoroutine(LoadImage(sceneData.picLocation));
        }

        Camera[] cams = FindObjectsOfType<Camera>();

        foreach (Camera cam in cams)
        {
            cam.backgroundColor = sceneData.defBGColor;
        }



        levelName = sceneData.levelName;
        creator = sceneData.creator;
        diff = (int)sceneData.calculatedDifficulty;

        FindObjectOfType<CubeCounter>().maxScore = FindObjectOfType<CubeCounter>().cubes.Length * 50;
        GameObject.Find("Cube").SetActive(sceneData.ground);
        GameObject.Find("elevator").SetActive(sceneData.ground);
        GameObject.Find("elevator").SetActive(sceneData.ground);

        FindObjectOfType<Camera>().backgroundColor = sceneData.defBGColor;
        levelName = sceneData.levelName;
        creator = sceneData.creator;
        diff = (int)sceneData.calculatedDifficulty;
       
        yield return null;
    }
}
