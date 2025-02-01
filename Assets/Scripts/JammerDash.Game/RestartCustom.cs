using JammerDash.Audio;
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

        void Start()
        {
            Button loadButton = GetComponent<Button>();
        }

        public void LoadScene()
        {
            
            AudioManager.Instance.source.outputAudioMixerGroup.audioMixer.SetFloat("Lowpass", 22000);
            AudioManager.Instance.source.outputAudioMixerGroup.audioMixer.ClearFloat("Lowpass");
            


            if (SceneManager.GetActiveScene().name == "LevelDefault")
            {
                string levelName = CustomLevelDataManager.Instance.levelName;
                string fullName = CustomLevelDataManager.Instance.ID + " - " + CustomLevelDataManager.Instance.levelName;
               
                    string path = Path.Combine(Main.gamePath, "levels", "extracted", fullName, levelName + ".json");
                    string json = File.ReadAllText(path);
                    SceneData data = SceneData.FromJson(json);
                    CustomLevelDataManager.Instance.LoadLevelData(levelName, data.ID);
                
            }
            Time.timeScale = 1f;
        }

      
    }

}