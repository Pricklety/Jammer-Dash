using System.Collections;
using System.Collections.Generic;
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
        Time.timeScale = 1f;
        LevelDataManager.Instance.LoadLevelData(LevelDataManager.Instance.levelName);
    }

}