using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

[System.Serializable]
public class VisualizerLine
{
    public RectTransform rectTransform;
    public RectTransform beatTransform; // Reference to the beat's RectTransform
    public float minHeight = 1f;  // Adjust the minimum height of the visualizer bar
    public float maxHeight = 1000f;  // Adjust the maximum height of the visualizer bar
    public float scaleSpeed = 100f;  // Adjust the speed at which the visualizer bar scales
    public float beatThreshold = 0.001f;  // Adjust the beat detection threshold
    public float intensityMultiplier = 1050f;  // Adjust the multiplier for beat intensity
    public Color startColor = Color.blue;  // Adjust the start color
    public Color endColor = Color.red;
    public float delay = 0f; // Delay for this visualizer line
}

public class MusicVisualizer : MonoBehaviour
{
    public List<VisualizerLine> visualizerLines = new List<VisualizerLine>();

    private AudioSource musicAudioSource;
    public Camera cam;
    public PostProcessVolume vol;
    private DepthOfField depth;
    public Image customImage;

    void Start()
    {
        StartCoroutine(CalculateRMS());
        vol.profile.TryGetSettings(out depth);
        musicAudioSource = GameObject.Find("mainmenu").GetComponent<AudioSource>();

        if (musicAudioSource == null)
        {
            Debug.LogError("AudioSource not found in the scene.");
            enabled = false;
        }

        // Automatically add all objects with the name "beat" to the visualizerLines list
        GameObject[] beatObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject beatObject in beatObjects)
        {
            if (beatObject.name == "beat")
            {
                RectTransform rectTransform = beatObject.GetComponent<RectTransform>();

                if (rectTransform != null)
                {
                    VisualizerLine newLine = new VisualizerLine
                    {
                        rectTransform = rectTransform,
                        beatTransform = rectTransform
                        
                    };

                    visualizerLines.Add(newLine);
                }
                else
                {
                    Debug.LogWarning("Object named 'beat' does not have RectTransform component.");
                }
            }
        }
    }

    IEnumerator CalculateRMS()
    {
        while (true)
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (musicAudioSource != null && musicAudioSource.isPlaying)
            {
                foreach (var line in visualizerLines)
                {
                    if (line.rectTransform != null && line.beatTransform != null && data.lineVisualizer)
                    {
                        float beatScale = line.beatTransform.localScale.x;
                        line.delay = Mathf.Lerp(0.75f, 0.0f, beatScale / 0.75f);

                        float rms = GetRMS(musicAudioSource);

                        // Calculate scale multiplier based on position relative to the middle
                        float positionFactor = Mathf.Abs(line.rectTransform.localPosition.x) / (Screen.width / 2f);
                        float scaleMultiplier = Mathf.Lerp(1f, 0.5f, positionFactor);

                        float intensity = rms * line.intensityMultiplier;

                        // Apply scale multiplier to the intensity
                        intensity *= scaleMultiplier;

                        line.rectTransform.sizeDelta = new Vector3(intensity, 100, 1);

                        float num2 = 15f * rms;
                        Color targetColor = CalculateTargetColor(Mathf.Lerp(0.0f, intensity, (float)(Time.fixedDeltaTime * num2 * 5.0)), Mathf.Lerp(0.0f, rms, (float)(Time.fixedDeltaTime * num2 / 20.0)));
                        line.rectTransform.GetComponent<Image>().color = targetColor;
                    }
                }

                if (data.bgVisualizer)
                {

                    float rms = GetRMS(musicAudioSource);
                    float depthvalue = rms * 300;
                    FloatParameter par = new FloatParameter() { value = depthvalue };
                    depth.focalLength.value = par;

                }
                else
                {
                    depth.focalLength.value = 0;

                }
                // Apply the same effect to the customImage
                if (customImage != null && data.logoVisualizer)
                {
                    float rms = GetRMS(musicAudioSource);
                    float customTargetSize = Mathf.Lerp(1.25f, 1f, rms);
                    float customCurrentSize = customImage.rectTransform.localScale.x;
                    float customNewSize = Mathf.Lerp(customCurrentSize, customTargetSize, Time.unscaledDeltaTime * 10f);
                    customImage.rectTransform.localScale = new Vector3(customNewSize, customNewSize, 1f);
                }
                else if (customImage != null && !data.logoVisualizer)
                {
                    customImage.rectTransform.localScale = Vector3.one;
                }
            }

            yield return null; // Wait for the next frame
        }
    }


    float GetRMS(AudioSource audioSource)
    {
        int sampleSize = 1000;
        float[] samples = new float[sampleSize];
        audioSource.GetOutputData(samples, 1); // Get raw audio data

        float sum = 0f;
        for (int i = 0; i < sampleSize; i++)
        {
            sum += samples[i] * samples[i];
        }

        float rms = Mathf.Sqrt(sum / sampleSize);
        return rms;
    }

    Color CalculateTargetColor(float intensity, float rms)
    {
        float adjustedIntensity = intensity * rms * 2;
        float hue = Mathf.Lerp(360, Random.Range(0, 360), adjustedIntensity);
        return Color.HSVToRGB(hue / 360f, 1.0f, 1.0f);
    }
}
