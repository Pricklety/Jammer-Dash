using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

namespace JammerDash.Audio 
{
   
    public class MusicVisualizer : MonoBehaviour
    {

        private AudioSource musicAudioSource;
        public Camera cam;
        public PostProcessVolume vol;
        public Image customImage;
        public SimpleSpectrum spectrum;

        void Start()
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            musicAudioSource = GameObject.Find("mainmenu").GetComponent<AudioSource>();

            if (musicAudioSource == null)
            {
                UnityEngine.Debug.LogError("AudioSource not found in the scene.");
                enabled = false;
            }

           
            StartCoroutine(CalculateRMS());
        }

        void OnEnable()
        {
            GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, 1);
            StartCoroutine(CalculateRMS()); 
        }

        public IEnumerator CalculateRMS()
        {
            while (true)
            {
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (musicAudioSource != null && musicAudioSource.isPlaying)
                {
                    float rms = GetRMS(musicAudioSource);

                       
                    // Apply the same effect to the customImage
                    if (customImage != null)
                    {
                        float customTargetSize = Mathf.Lerp(1.25f, 0.8f, rms);
                        float customCurrentSize = customImage.rectTransform.localScale.x;
                        float customNewSize = Mathf.Lerp(customCurrentSize, customTargetSize, Time.unscaledDeltaTime * 50f);
                        customImage.rectTransform.localScale = new Vector3(customNewSize, customNewSize, 1f);
                    }
                }

                yield return null; // Wait for the next frame

            }

        }

        float GetRMS(AudioSource audioSource)
        {
            int sampleSize = 512;
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
            float hue = Mathf.Lerp(rms / intensity, Random.Range(0, 360), adjustedIntensity);
            return Color.HSVToRGB(hue / 360f, hue / 90f, 1f, true);
        }

    }
    public class VisualizerLine
    {
        public RectTransform rectTransform;
        public float minHeight = 1f;  // Adjust the minimum height of the visualizer bar
        public float maxHeight = 1000f;  // Adjust the maximum height of the visualizer bar
        public float scaleSpeed = 100f;  // Adjust the speed at which the visualizer bar scales
        public float beatThreshold = 0.001f;  // Adjust the beat detection threshold
        public float intensitymultiplier = 1050f;  // Adjust the multiplier for beat intensity
        public Color startColor = Color.blue;  // Adjust the start color
        public Color endColor = Color.red;
    }

}
