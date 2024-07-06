using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

namespace JammerDash.Audio 
{
   
    public class MusicVisualizer : MonoBehaviour
    {
        public List<VisualizerLine> visualizerLines = new List<VisualizerLine>();

        private AudioSource musicAudioSource;
        public Camera cam;
        public PostProcessVolume vol;
        public Image customImage;
        public ParticleSystem[] particles;
        public Animation anim; 

        void Start()
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            musicAudioSource = GameObject.Find("mainmenu").GetComponent<AudioSource>();

            if (musicAudioSource == null)
            {
                UnityEngine.Debug.LogError("AudioSource not found in the scene.");
                enabled = false;
            }

            // Automatically add all objects with the name "beat" to the visualizerLines list
            GameObject[] beatObjects = (GameObject[])FindObjectsOfTypeAll(typeof(GameObject));

            foreach (GameObject beatObject in beatObjects)
            {
                if (beatObject.CompareTag("Beat") && (data.allVisualizers || data.lineVisualizer))
                {
                    RectTransform rectTransform = beatObject.GetComponent<RectTransform>();

                    if (rectTransform != null)
                    {
                        VisualizerLine newLine = new VisualizerLine
                        {
                            rectTransform = rectTransform
                            // You can set other default parameters here if needed
                        };

                        visualizerLines.Add(newLine);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("Object named 'beat' does not have RectTransform component.");
                    }
                }
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

                    foreach (var line in visualizerLines)
                    {
                        if (line.rectTransform != null && data.lineVisualizer)
                        {
                            foreach (ParticleSystem particle in particles)
                            {

                               
                                if (particle.gameObject.name == "particleLaser1")
                                {
                                    var emission = particle.emission;
                                    emission.rateOverTime = rms * 2800;
                                    var shape = particle.shape;
                                    shape.arcSpeed = rms * 1.6f;
                                }
                                if (particle.gameObject.name == "particleLaser2")
                                {
                                    var emission = particle.emission;
                                    emission.rateOverTime = rms * 3800;
                                    var shape = particle.shape;
                                    shape.arcSpeed = rms * 0.8f;
                                }
                                if (particle.gameObject.name == "overall")
                                {
                                    var main = particle.main;
                                    var emission = particle.emission;
                                    emission.rateOverTime = rms * 4600;
                                    main.simulationSpeed = rms * 10f;
                                }

                            }

                            float intensity = rms * line.intensitymultiplier;
                            line.rectTransform.sizeDelta = new Vector2(line.rectTransform.sizeDelta.x, intensity);
                            float num2 = 15f * rms;
                            if (data.visualizerColor)
                            {
                                Color targetColor = CalculateTargetColor(Mathf.Lerp(0.0f, intensity, (float)(Time.fixedDeltaTime * num2 * 5.0)), Mathf.Lerp(0.0f, rms, (float)(Time.fixedDeltaTime * num2 / 20.0)));
                                line.rectTransform.GetComponent<Image>().color = new Color(targetColor.r, targetColor.g, targetColor.b, 155);

                            }
                            else
                            {
                                line.rectTransform.GetComponent<Image>().color = new Color(255, 255, 255, 150);
                            }

                        }
                        else if (line.rectTransform != null && !data.lineVisualizer)
                        {
                            foreach (ParticleSystem particle in particles)
                            {
                                var main = particle.main;
                                main.simulationSpeed = 0.25f;
                            }

                            line.rectTransform.sizeDelta = new Vector2(line.rectTransform.sizeDelta.x, 0);
                            line.rectTransform.GetComponent<Image>().color = Color.white;
                        }
                    }


                    // Apply the same effect to the customImage
                    if (customImage != null && data.logoVisualizer)
                    {
                        float customTargetSize = Mathf.Lerp(1.25f, 0.8f, rms);
                        float customCurrentSize = customImage.rectTransform.localScale.x;
                        float customNewSize = Mathf.Lerp(customCurrentSize, customTargetSize, Time.unscaledDeltaTime * 50f);
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
