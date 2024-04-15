using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using UnityEngine.Networking;

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
                    //updatePanel.SetActive(true);
                    // Enable update button
                    updateButton.interactable = true;
                    // Set button text
                    updateButtonText.text = "Update Available!";
                    // Set download URL
                    downloadUrl = $"https://github.com/Pricklety/Jammer-Dash/releases/download/release/Jammer.Dash.{latestRelease.tag_name}.zip";
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
        UnityEngine.Debug.Log(downloadUrl);
    }

    private IEnumerator DownloadAndExtractUpdate()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(downloadUrl))
        {
            string tempZipPath = Path.Combine(Application.persistentDataPath, "update.zip");
            www.downloadHandler = new DownloadHandlerFile(tempZipPath);

            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                updateButtonText.text = "Downloading: " + (www.downloadProgress * 100).ToString("F0") + "%";
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                string extractionPath = Path.Combine(Application.persistentDataPath, "update");
                Directory.CreateDirectory(extractionPath); // Ensure the extraction directory exists

                using (ZipArchive archive = ZipFile.OpenRead(tempZipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryDestinationPath = Path.Combine(extractionPath, entry.FullName);
                        entry.ExtractToFile(entryDestinationPath, true);
                    }
                }

                // Clean up the temporary zip file
                System.IO.File.Delete(tempZipPath);

                // Run the update script
                RunUpdateScript();
            }
            else
            {
                updateButtonText.text = "Update download failed: " + www.error;
            }
        }
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
