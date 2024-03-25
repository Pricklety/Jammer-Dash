using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
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
        LevelDataManager.Instance.LoadLevelData(LevelDataManager.Instance.levelName);

        Time.timeScale = 1f;

    }

}