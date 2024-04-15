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

    private bool sceneActivationAllowed = false;

    void Start()
    {
        StartCoroutine(LoadMusicAndMenu()); 
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath,"backgrounds")))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "backgrounds"));
        }
        if (!PlayerPrefs.HasKey("bootSafe031"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("bootSafe031", 1);
        }
    }

    
    IEnumerator LoadMusicAndMenu()
    {
            AsyncOperation operation = SceneManager.LoadSceneAsync(1);
            operation.allowSceneActivation = false;

        new WaitForSecondsRealtime(5f);
        while (true)
        {
            if (operation.progress >= 0.9f)
                {
                    if (!sceneActivationAllowed && Input.anyKeyDown)
                    {
                        sceneActivationAllowed = true;
                        operation.allowSceneActivation = true;
                        break;
                    }
                }

                yield return null;
            }

            
        
    }
}

