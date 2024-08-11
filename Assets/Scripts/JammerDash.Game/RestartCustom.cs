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
               
                    string path = Path.Combine(Application.persistentDataPath, "levels", "extracted", levelName, levelName + ".json");
                    string json = File.ReadAllText(path);
                    SceneData data = SceneData.FromJson(json);
                    CustomLevelDataManager.Instance.LoadLevelData(levelName, data.ID);
                
            }
            Time.timeScale = 1f;
        }

      
    }

}