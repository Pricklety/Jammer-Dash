using JammerDash.Audio;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JammerDash
{
    public class introManager : MonoBehaviour
    {
        public Text introtext;
        public AudioSource source;

        void Awake() {
             bool doesPathExist = File.Exists(Path.Combine(Application.persistentDataPath, "path.txt"));
               
                if (doesPathExist) {
                     string path = File.ReadAllText(Path.Combine(Application.persistentDataPath, "path.txt"));
                     
                Main.gamePath = path;
                }
                else if (!doesPathExist)
                Main.gamePath = Application.persistentDataPath;
        }
        void Start()
        {

            if (!Directory.Exists(Path.Combine(Main.gamePath, "backgrounds")))
            {
                Directory.CreateDirectory(Path.Combine(Main.gamePath, "backgrounds"));
            }
            if (!File.Exists(Path.Combine(Main.gamePath, "scores.dat"))) {
                File.Create(Path.Combine(Main.gamePath, "scores.dat")).Dispose();
            }
            if (!Directory.Exists(Path.Combine(Main.gamePath, "scenes")))
            {
                Directory.CreateDirectory(Path.Combine(Main.gamePath, "scenes"));
            }
            if (!Directory.Exists(Path.Combine(Main.gamePath, "levels")))
            {
                Directory.CreateDirectory(Path.Combine(Main.gamePath, "levels"));
                Directory.CreateDirectory(Path.Combine(Main.gamePath, "levels", "extracted"));
            }
            if (!PlayerPrefs.HasKey("bootSafe031"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.SetInt("bootSafe031", 1);
            }
            StartCoroutine(LoadMusicAndMenu());
        }


        IEnumerator LoadMusicAndMenu()
        {
           
            AsyncOperation operation = SceneManager.LoadSceneAsync(1);
            operation.allowSceneActivation = false;
            AudioManager.Instance.source.Play();
           
            float elapsedTime = 0f;

            while (elapsedTime < 6f)
            {
                if (Input.GetKeyDown(KeyCode.Escape) && (Account.Instance.loggedIn || Application.isEditor))
                {
                    operation.allowSceneActivation = true;
                    yield break; 
                }

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            // If Escape wasn't pressed during the wait, activate the scene after 17 seconds
            operation.allowSceneActivation = true;
        }

    }

}
