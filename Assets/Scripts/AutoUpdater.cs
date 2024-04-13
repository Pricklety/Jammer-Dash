using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using static System.Net.WebRequestMethods;

public class AutoUpdater : MonoBehaviour
{
    public string currentVersion = "v0.32"; // Placeholder for the current version of your game
    public Text updateButtonText;
    public GameObject updatePanel;
    public Button updateButton;
    private string downloadUrl;

    private void Start()
    {
        StartCoroutine(CheckForUpdate());
    }

    private IEnumerator CheckForUpdate()
    {
        using (WebClient client = new WebClient())
        {
            string apiUrl = $"https://api.github.com/repos/Pricklety/Jammer-Dash/releases/latest";
            client.Headers.Add("User-Agent", "Jammer-Dash");

            try
            {
                string jsonString = client.DownloadString(apiUrl);
                UnityEngine.Debug.Log(jsonString);
                ReleaseInfo latestRelease = JsonUtility.FromJson<ReleaseInfo>(jsonString);

                if (latestRelease.tag_name != currentVersion)
                {
                    // Show update panel
                    updatePanel.SetActive(true);
                    // Enable update button
                    updateButton.interactable = true;
                    // Set button text
                    updateButtonText.text = "Update Available!";
                    // Set download URL
                    downloadUrl = $"https://api.github.com/repos/Pricklety/Jammer-Dash/zipball/{latestRelease.tag_name}";

                }
            }
            catch (WebException ex)
            {
                updateButtonText.text = $"Update download failed: {ex.Message}";
            }
        }
        yield return null;
    }

    public void UpdateGame()
    {
        StartCoroutine(DownloadAndExtractUpdate());
    }

    private IEnumerator DownloadAndExtractUpdate()
    {
        using (WebClient client = new WebClient())
        {
            string tempZipPath = Path.Combine(Application.persistentDataPath, "update.zip");
            try
            {
                // Download the zip file
                client.DownloadFile(new Uri(downloadUrl), tempZipPath);

                // Extract the zip file
                ZipFile.ExtractToDirectory(tempZipPath, Application.persistentDataPath);

                // Clean up the temporary zip file
                File.Delete(tempZipPath);

                // Run the update script
                RunUpdateScript();
            }
            catch (WebException ex)
            {
                updateButtonText.text = $"Update download failed: {ex.Message}";
            }
        }
        yield return null;
    }

    void RunUpdateScript()
    {
        string executablePath = Path.Combine(Application.dataPath, "..", "Jammer Dash.exe");
        Process.Start(executablePath);
        Application.Quit();
    }

    [Serializable]
    public class ReleaseInfo
    {
        public string tag_name;
        public string zipball_url;
    }
}
