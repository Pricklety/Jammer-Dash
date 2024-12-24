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


        void Start()
        {
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "backgrounds")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "backgrounds"));
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

           
            float elapsedTime = 0f;

            while (elapsedTime < 17f)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
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
