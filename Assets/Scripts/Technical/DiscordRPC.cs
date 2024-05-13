using Lachee.Discord;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JammerDash.Editor;

namespace JammerDash.Tech
{
    public class DiscordRPC : MonoBehaviour
    {
        void Start()
        {
            DiscordManager.current.UpdateStartTime();
        }

        private void FixedUpdate()
        {
            DiscordManager.current.client.UpdateLargeAsset("logo", $"(No account found): #N/A");
            DiscordManager.current.client.UpdateSmallAsset("shine", "-- sp");
            if (SceneManager.GetActiveScene().buildIndex > 2 && SceneManager.GetActiveScene().name != "LevelDefault" && SceneManager.GetActiveScene().name != "SampleScene")
            {
                DiscordManager.current.UpdateDetails(SceneManager.GetActiveScene().name);
                DiscordManager.current.UpdateState("Clicking boxes and evading saws");
            }
            else if (SceneManager.GetActiveScene().name == "LevelDefault")
            {
                DiscordManager.current.UpdateDetails($"{CustomLevelDataManager.Instance.levelName}{LevelDataManager.Instance.levelName} by {LevelDataManager.Instance.creator}{CustomLevelDataManager.Instance.creator}");
                DiscordManager.current.UpdateState("Clicking boxes and evading saws");
            }
            else if (SceneManager.GetActiveScene().name == "SampleScene")
            {
                DiscordManager.current.UpdateDetails($"{LevelDataManager.Instance.levelName} - {FindObjectOfType<EditorManager>().objectCount.text}");
                DiscordManager.current.UpdateState("Editing a level");
            }
            else if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                DiscordManager.current.UpdateDetails($"Idle");
                DiscordManager.current.UpdateState($"{DiscordManager.current.CurrentUser.username}; Level {LevelSystem.Instance.level} - {LevelSystem.Instance.totalXP:N1}xp");
            }
            else if (SceneManager.GetActiveScene().name == "intro")
            {
                DiscordManager.current.UpdateDetails($"Idle");
                DiscordManager.current.UpdateState($"Loading up...");
            }

        }
    }
}