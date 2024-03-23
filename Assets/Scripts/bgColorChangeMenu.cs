using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class bgColorChangeMenu : MonoBehaviour
{

    private Image backgroundImage;
    public AudioSource musicAudioSource;
    public float beatThreshold = 0.1f;  // Adjust the threshold for detecting a beat
    public float highBeatThreshold = 0.5f;  // Adjust the threshold for detecting a high beat
    public float colorPulseDuration = 0.1f;  // Adjust the duration of the color pulse
    public float colorLerpSpeed = 2f;  // Adjust the speed of lerping to black

    private int previousSample;
    private Color targetColor;
    private Color currentColor;
    SettingsData data = SettingsFileHandler.LoadSettingsFromFile();

    private void Start()
    {
        backgroundImage = this.GetComponent<Image>();  // Assuming this script is attached to the same GameObject as the Image

        if (backgroundImage == null)
        {
            Debug.LogError("Image component not found on the GameObject.");
            enabled = false;
            return;
        }

        if (musicAudioSource == null)
        {
            musicAudioSource = FindObjectOfType<AudioSource>();

            if (musicAudioSource == null)
            {
                Debug.LogError("AudioSource not found in the scene.");
                enabled = false;
                return;
            }
        }

        // Initialize the previousSample to the current sample
        previousSample = musicAudioSource.timeSamples;

        // Initialize colors
        targetColor = Color.black;
        currentColor = targetColor;

        // Start the color-changing coroutine
        StartCoroutine(ChangeColorCoroutine());
    }

    private void Update()
    {
            if (musicAudioSource != null && musicAudioSource.isPlaying)
            {
                float rms = GetRMS(musicAudioSource);

                // Check if there is a significant change in the audio waveform (beat)
                int currentSample = musicAudioSource.timeSamples;
                if (Mathf.Abs(currentSample - previousSample) > beatThreshold)
                {
                    // If there is a beat, check if it's a high beat
                    if (rms > highBeatThreshold)
                    {
                        // Pulse to a random color
                        targetColor = new Color(Random.value, Random.value, Random.value, 1f);

                    }
                }

                // Update the previous sample for the next frame
                previousSample = currentSample;
            }
        
       
    }

    private float GetRMS(AudioSource audioSource)
    {
        float[] samples = new float[1024];
        audioSource.GetOutputData(samples, 0);
        float sum = 0f;
        foreach (float sample in samples)
        {
            sum += sample * sample;
        }
        return Mathf.Sqrt(sum / samples.Length);
    }

    private IEnumerator LerpToBlack()
    {
        float elapsedTime = 0f;

        while (elapsedTime < colorPulseDuration)
        {
            currentColor = Color.Lerp(targetColor, Color.black, elapsedTime / colorPulseDuration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the final color is set to black
        currentColor = Color.black;
    }

    public IEnumerator ChangeColorCoroutine()
    {
        while (true)
        {
            // Wait for the next color pulse
            new WaitForSeconds(colorPulseDuration);

            // Pulse to a random color
            targetColor = new Color(Random.value, Random.value, Random.value, 1f);

            yield return targetColor;
        }
    }
}