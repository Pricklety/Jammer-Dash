using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using JammerDash.Difficulty;

namespace JammerDash.Editor.Screens
{

    public class LoadingScreen : MonoBehaviour
    {
        public GameObject loadingPanel;  // Reference to the loading panel
        public Text loadingText;          // Reference to the loading text
        public Slider loadingSlider;      // Reference to the loading slider

        private void Awake()
        {
            // Hide loading panel at start
            loadingPanel.SetActive(false);
        }

        // Call this method to show the loading screen
        public void ShowLoadingScreen()
        {
            loadingPanel.SetActive(true);
            loadingText.text = "Loading...";
            loadingSlider.value = 0f; // Reset slider
        }

        // Call this method to hide the loading screen
        public void HideLoadingScreen()
        {
            loadingPanel.SetActive(false);
        }

        // Call this method to update the loading screen text and slider value
        public void UpdateLoading(string message, float progress)
        {
                loadingText.text = message;
                loadingSlider.value = progress;
            
        }
    }

}