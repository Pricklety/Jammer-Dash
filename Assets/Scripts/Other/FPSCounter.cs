using System;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash.Tech
{
    public class FPSCounter : MonoBehaviour
    {
        public Text Text;
        public GameObject panel;
        private int[] _frameRateSamples;
        private int _cacheNumbersAmount = 300;
        private int _averageFromAmount = 30;
        private int _averageCounter = 0;
        private int _currentAveraged;
        private Color _currentColor;
        private Color _targetColor;
        private float _smoothTime = 0.3f; // Smoothing time in seconds

        void Awake()
        {
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
            if (data.isShowingFPS)
            {
                panel.SetActive(true);
            }
            else
            {
                panel.SetActive(false);
            }
            DontDestroyOnLoad(gameObject);
            _frameRateSamples = new int[_averageFromAmount];
            _currentColor = GetColorForFPS(0);
            _targetColor = _currentColor;
        }

        void Update()
        {
            // Sample FPS
            {
                var currentFrame = (int)Math.Round(1f / Time.unscaledDeltaTime);
                _frameRateSamples[_averageCounter] = currentFrame;
            }

            // Average FPS
            {
                var average = 0f;

                foreach (var frameRate in _frameRateSamples)
                {
                    average += frameRate;
                }

                _currentAveraged = (int)Math.Round(average / _averageFromAmount);
                _averageCounter = (_averageCounter + 1) % _averageFromAmount;
            }

            // Update color smoothly
            _targetColor = GetColorForFPS(_currentAveraged);
            _currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime / _smoothTime);

            // Calculate drawing time in milliseconds
            float drawingTimeMs = 1000f / Mathf.Max(_currentAveraged, 0.00001f); // Avoid division by zero

            // Assign to UI
            {
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (data.isShowingFPS)
                {
                    panel.SetActive(true);
                }
                else
                {
                    panel.SetActive(false);
                }
                if (QualitySettings.vSyncCount == 1)
                {
                    Text.text = $"FPS: {_currentAveraged} / {Screen.currentResolution.refreshRate} \n{drawingTimeMs:F2} ms";
                    Text.color = _currentColor;
                }
                else
                {
                    Text.text = $"FPS: {_currentAveraged} / {Application.targetFrameRate} \n{drawingTimeMs:F2} ms";
                    Text.color = _currentColor;
                }

            }
        }


        Color GetColorForFPS(int fps)
        {
            if (fps < 30)
                return Color.red;
            else if (fps < 90)
                return Color.yellow;
            else
                return Color.green;
        }
    }
}