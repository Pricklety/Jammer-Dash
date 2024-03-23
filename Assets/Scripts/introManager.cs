using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class introManager : MonoBehaviour
{
    public Text introtext;
    public AudioSource source;
    public Slider load;

    private bool sceneActivationAllowed = false;

    void Start()
    {
        StartCoroutine(LoadMusicAndMenu()); 
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath + "backgrounds")))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath + "backgrounds"));
        }
        if (!PlayerPrefs.HasKey("bootSafe"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("bootSafe", 1);
        }
    }

    
    IEnumerator LoadMusicAndMenu()
    {
        

        while (!AudioManager.Instance.isMusicLoaded)
        {
            if (Directory.Exists(Path.Combine(Application.persistentDataPath + "music")))
            {
                float progress = AudioManager.Instance.GetLoadingProgress();
                Text percentage = load.GetComponentInChildren<Text>();
                percentage.text = $"Loaded {AudioManager.Instance.GetLoadedSongsCount()} / {AudioManager.Instance.GetTotalNumberOfSongs()} songs";
                load.value = AudioManager.Instance.GetLoadedSongsCount();
                load.maxValue = AudioManager.Instance.GetTotalNumberOfSongs();
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath + "music"));
                StartCoroutine(LoadMusicAndMenu());
            }
            

            yield return null;
        }

        if (AudioManager.Instance.isMusicLoaded)
        {
            load.value = 0;
            load.maxValue = 100;
            AsyncOperation operation = SceneManager.LoadSceneAsync(1);
            operation.allowSceneActivation = false;

            while (true)
            {
                Text percentage = load.GetComponentInChildren<Text>();
                load.value = operation.progress * 100;

                percentage.text = "Loading Game Resources: " + $"{load.value.ToString("0.00")}%";

                if (operation.progress >= 0.9f)
                {
                    introtext.gameObject.SetActive(true);
                    load.gameObject.SetActive(false);

                    if (!sceneActivationAllowed && Input.anyKeyDown)
                    {
                        sceneActivationAllowed = true;
                        operation.allowSceneActivation = true;
                        source.Play();
                        break;
                    }
                }

                yield return null;
            }

            
        }
    }
}

