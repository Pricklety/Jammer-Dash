using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestartCustom : MonoBehaviour
{
    string sceneAddress;

    void Start()
    {
        Button loadButton = GetComponent<Button>();
        loadButton.onClick.AddListener(LoadScene);
    }

    public void LoadScene()
    {
        AudioSource[] audios = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in audios)
        {
            audio.outputAudioMixerGroup.audioMixer.SetFloat("Lowpass", 22000);
            audio.outputAudioMixerGroup.audioMixer.ClearFloat("Lowpass");
        }

        string levelName = LevelDataManager.Instance.levelName;

        // Check if the level exists in the "levels" folder
        string levelsFolderPath = Path.Combine(Application.persistentDataPath, "levels");
        string levelPathInLevelsFolder = Path.Combine(levelsFolderPath, levelName + ".jdl");
        bool levelExistsInLevelsFolder = File.Exists(levelPathInLevelsFolder);

        // Load the level based on its existence in the "levels" folder
        if (levelExistsInLevelsFolder)
        {
            // Load the level from the "levels" folder
           CustomLevelDataManager.Instance.LoadLevelData(levelName);
        }
        else
        {
            // Load the level from the "scenes" folder
            LevelDataManager.Instance.LoadLevelData(levelName);
        }

        Time.timeScale = 1f;
    }

}
