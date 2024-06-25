using JammerDash.Tech;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JammerDash.Game
{

    public class RestartCustom : MonoBehaviour
    {
        string sceneAddress;

        void Start()
        {
            Button loadButton = GetComponent<Button>();
        }

        public void LoadScene()
        {
            AudioSource[] audios = FindObjectsOfType<AudioSource>();
            foreach (AudioSource audio in audios)
            {
                audio.outputAudioMixerGroup.audioMixer.SetFloat("Lowpass", 22000);
                audio.outputAudioMixerGroup.audioMixer.ClearFloat("Lowpass");
            }


            if (SceneManager.GetActiveScene().name == "LevelDefault")
            {
                string levelName = CustomLevelDataManager.Instance.levelName;
                if (levelName == null)
                {

                    levelName = LevelDataManager.Instance.levelName;
                    CheckSceneDataExists(levelName, "scenes");
                    LevelDataManager.Instance.LoadLevelData(levelName);
                    Debug.Log(levelName);
                }
                else
                {
                    CheckSceneDataExists(levelName, "levels\\extracted");
                    string path = Path.Combine(Application.persistentDataPath, "levels", "extracted", levelName, levelName + ".json");
                    string json = File.ReadAllText(path);
                    SceneData data = SceneData.FromJson(json);
                    CustomLevelDataManager.Instance.LoadLevelData(levelName, data.ID);
                }
            }
            Time.timeScale = 1f;
        }

        private void CheckSceneDataExists(string levelName, string folder)
        {
            string path = Path.Combine(Application.persistentDataPath, folder, levelName, levelName + ".json");

            if (!File.Exists(path))
            {
                Debug.LogError("Scene data for level " + levelName + " does not exist in folder " + folder);
                // You can handle the absence of scene data here, for example, you might want to create default data or display an error message to the user.
            }
        }

    }

}