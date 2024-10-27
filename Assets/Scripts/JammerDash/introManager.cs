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

        private bool sceneActivationAllowed = false;

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

            yield return new WaitForSecondsRealtime(17f);
            sceneActivationAllowed = true;
            operation.allowSceneActivation = true;
        }
    }

}
