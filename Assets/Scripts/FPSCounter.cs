using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public Text text;
    public GameObject obj;
    public float smoothing = 0.5f;

    private float fps;
    private float ms;

    private float lastFps;
    private float lastMs;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        InvokeRepeating("UpdateFPS", 0f, 0.1f); // Call UpdateFPS every 0.1 seconds
    }

    void UpdateFPS()
    {
        float currentFps = 1.0f / Time.deltaTime;
        fps = Mathf.Lerp(lastFps, currentFps, smoothing);
        lastFps = fps;

        ms = 1000.0f / Mathf.Max(fps, 0.00001f); // Avoid division by zero
    }

    void Update() 
    {
        // Calculate color based on FPS
        Color color;
        if (fps >= 55)
        {
            color = Color.green;
        }
        else if (fps >= 30)
        {
            color = Color.Lerp(Color.red, Color.green, (fps - 30) / 25f);
        }
        else
        {
            color = Color.Lerp(Color.red, Color.yellow, (fps - 5) / 25f);
        }

        // Update text color and content
        text.color = color;
        text.text = $"FPS: {fps:F0}\n{ms:F2} ms";

        // Apply settings
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        obj.SetActive(data.isShowingFPS);
    }
}
