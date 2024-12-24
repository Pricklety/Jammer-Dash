using UnityEngine;
using System.IO;
using System;
using System.Collections;
using UnityEngine.Events;
using JammerDash.Audio;

namespace JammerDash
{

    public class ScreenshotHandler : MonoBehaviour
    {
        public AudioClip shutter;
        private bool takeScreenshotOnNextFrame;

        private void Update()
        {
            DontDestroyOnLoad(this);
            // Check if F12 key is pressed
            if (Input.GetKeyDown(KeybindingManager.screenshot))
            {
                takeScreenshotOnNextFrame = true;
            }
        }

        private void LateUpdate()
        {
            if (takeScreenshotOnNextFrame)
            {
                takeScreenshotOnNextFrame = false;
                StartCoroutine(TakeScreenshot());
            }
        }

        private IEnumerator TakeScreenshot()
        {
            yield return new WaitForEndOfFrame();

            // Generate file name with current date and time
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string screenshotFileName = $"Screenshot_{timestamp}.png";
            string screenshotFolder = Path.Combine(Application.persistentDataPath, "screenshots");
            string screenshotPath = Path.Combine(screenshotFolder, screenshotFileName);
            if (!Directory.Exists(screenshotFolder))
                Directory.CreateDirectory(screenshotFolder);

            // Capture screenshot
            ScreenCapture.CaptureScreenshot(screenshotPath);
            AudioManager.Instance.source.PlayOneShot(shutter);

            // Wait for a short period of time before checking if the file exists
            yield return new WaitForSeconds(1f);

            // Wait until the file is created
            yield return new WaitUntil(() => File.Exists(screenshotPath));

            // Load the image from file
            Texture2D texture = new Texture2D(2, 2);
            byte[] fileData = File.ReadAllBytes(screenshotPath);
            texture.LoadImage(fileData);

           
                UnityAction action = () => OpenScreenshotFolder();
                Notifications.instance.Notify("Screenshot taken. \nClick to open folder.", action);
            

            Debug.Log("Screenshot captured: " + screenshotPath);
        }

        public void OpenScreenshotFolder()
        {
            string screenshotFolderPath = Path.Combine(Application.persistentDataPath, "screenshots");
            
            if (Directory.Exists(screenshotFolderPath))
            {
                // Open the folder using platform-specific code
                OpenFolder(screenshotFolderPath);
            }
            else
            {
                Debug.LogWarning("Screenshot folder does not exist.");
            }
        }

        private void OpenFolder(string folderPath)
        {
            folderPath = folderPath.Replace("/", "\\");
            // Check the operating system to determine the appropriate command
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
            {
                // For Windows, use the specific folder path to open the explorer
                System.Diagnostics.Process.Start("explorer.exe", "/e," + folderPath);
            }
            else if (Application.platform == RuntimePlatform.OSXEditor ||
                     Application.platform == RuntimePlatform.OSXPlayer)
            {
                // For macOS, use "open" with the folder path to open the Finder
                System.Diagnostics.Process.Start("open", folderPath);
            }
            else
            {
                // For other platforms, log a warning
                Debug.LogWarning("Opening folder is not supported on this platform.");
            }
        }
    }

}