using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections;

public class RandomTextDisplay : MonoBehaviour
{
    public Text textComponent;
    public string filePath = "texts.txt";
    float time;

    void Start()
    {
        if (textComponent == null)
        {
            Debug.LogError("Text component not assigned.");
            return;
        }

        string fullPath;

        // Check if the application is in the Unity Editor or a build
        if (Application.isEditor)
        {
            fullPath = Path.Combine(Application.streamingAssetsPath, filePath);
        }
        else
        {
            // If in a build, use the StreamingAssets folder
            fullPath = Path.Combine(Application.streamingAssetsPath, filePath);
        }

        try
        {
            string[] lines = File.ReadAllLines(fullPath);
            if (lines.Length > 0)
            {
                string randomLine = lines[UnityEngine.Random.Range(0, lines.Length)];
                textComponent.text = randomLine;

            }
            else
            {
                Debug.LogWarning("Text file is empty.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading text file: " + e.Message);
        }
    }


    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (Input.GetKeyDown(KeyCode.H) || time > 10f)
        {
            time = 0f;
            StartCoroutine(Change());
        }
    }

    IEnumerator Change()
    {
        string fullPath;
        if (Application.isEditor)
        {
            fullPath = Path.Combine(Application.streamingAssetsPath, filePath);
        }
        else
        {
            // If in a build, use the StreamingAssets folder
            fullPath = Path.Combine(Application.streamingAssetsPath, filePath);
        }
        string[] lines = File.ReadAllLines(fullPath);
        string randomLine = lines[UnityEngine.Random.Range(0, lines.Length)];
        yield return textComponent.text = randomLine;
    }
}
