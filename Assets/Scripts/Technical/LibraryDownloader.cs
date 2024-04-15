using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;

public class LibraryDownloader : MonoBehaviour
{

    // UI elements for progress tracking
    public Slider downloadSlider;
    public Text statusText;
    public GameObject statusPanel;
    public Button[] buttons;
    private Stopwatch stopwatch; // Stopwatch to measure download time
    private ulong downloadedBytesLastFrame; // Store downloaded bytes from previous frame

    public void StartDownload()
    {
        StartCoroutine(DownloadFileAsync("https://onedrive.live.com/download?resid=9B1B55231F56D5E9%219099&authkey=!AFmcye_0FGdPxhg"));
    }


    private string GetFileIdFromUrl(string url)
    {
        // Extract file ID from URL using regex
        Match match = Regex.Match(url, @"[A-Za-z0-9_-]{25,}");
        return match.Success ? match.Value : null;
    }
    public void OpenDownload()
    {
        statusPanel.SetActive(true);
    }
    public void CloseDownload()
    {
        statusPanel.SetActive(false);
    }
    private IEnumerator DownloadFileAsync(string url)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "Library.zip");

        // Check if the file already exists
        if (!File.Exists(filePath))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                downloadSlider.gameObject.SetActive(true);
                // Start stopwatch to measure download time
                stopwatch = Stopwatch.StartNew();
                www.downloadHandler = new DownloadHandlerFile(filePath);
                // Send request asynchronously
                var operation = www.SendWebRequest();

                foreach (Button button in buttons)
                {
                    button.interactable = false;
                }

                while (!operation.isDone)
                {
                    downloadSlider.value = www.downloadProgress;

                    // Calculate download speed
                    ulong downloadedBytes = www.downloadedBytes;
                    float deltaTime = stopwatch.ElapsedMilliseconds / 1000f; // Convert elapsed time to seconds
                    ulong bytesDelta = downloadedBytes - downloadedBytesLastFrame;
                    float downloadSpeed = bytesDelta / deltaTime; // Bytes per second
                    float downloadedMB = www.downloadedBytes / 1024768f;
                    string downloadSizeText;
                    if (downloadedMB >= 1024f)
                    {
                        float downloadedGB = downloadedMB / 1024f;
                        downloadSizeText = $"Downloading: {downloadedGB:F2} GB ({www.downloadProgress * 100:F2}%)  {downloadSpeed / 1024768:F2} MB/s";
                    }
                    else
                    {
                        downloadSizeText = $"Downloading: {downloadedMB:F2} MB ({www.downloadProgress * 100:F2}%)  {downloadSpeed / 1024768:F2} MB/s";
                    }
                    statusText.text = downloadSizeText;
                    yield return null;
                }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download file: " + www.error);
                    statusText.text = "Oops! Something went wrong.";
                    foreach (Button button in buttons)
                    {
                        button.interactable = true;
                    }
                    yield break;
                }



                ExtractZipFile(filePath, Path.Combine(Application.streamingAssetsPath, "music"));
            }
        }
        else
        {
            ExtractZipFile(filePath, Path.Combine(Application.streamingAssetsPath, "music"));
        }
    }


    private async void ExtractZipFile(string zipFilePath, string destinationFolder)
    {
        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        downloadSlider.gameObject.SetActive(true);
        statusText.text = "Extracting...";

        await Task.Run(() => {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    int totalEntries = archive.Entries.Count;
                    int extractedEntries = 0;

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryDestinationPath = Path.Combine(destinationFolder, entry.FullName);
                        entry.ExtractToFile(entryDestinationPath, true);

                        // Update progress
                        extractedEntries++;
                        float progress = (float)extractedEntries / totalEntries;
                        UpdateProgress(progress);
                    }
                }

                statusPanel.SetActive(false);
                downloadSlider.gameObject.SetActive(false);
                File.Delete(Path.Combine(Application.persistentDataPath, "Library.zip"));

                // Your code to handle editor vs. build mode
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = true;

#else
        string parentDirectory = System.IO.Directory.GetParent(Application.dataPath).FullName;
        Application.Quit();
        Process.Start(parentDirectory + "/Jammer Dash.exe");
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("Error extracting zip file: " + e.Message);
            }
        });

    }

    private void UpdateProgress(float progress)
    {
        // Ensure progress is within range [0, 1]
        progress = Mathf.Clamp01(progress);

        // Update slider value
        downloadSlider.value = progress;

        // Update status text
        statusText.text = string.Format("Extracting... {0}%", Mathf.RoundToInt(progress * 100));
    }
}
