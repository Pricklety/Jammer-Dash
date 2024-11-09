using Lachee.Discord;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JammerDash.Editor;
using JammerDash.Audio;
using JammerDash.Menus;
using System;
namespace JammerDash.Tech
{
    public class DiscordRPC : MonoBehaviour
    {
        Timestamp time;
        void Start()
        {
            time = DateTime.UnixEpoch;
            
        }

        private void FixedUpdate()
        {
            DiscordManager.current.UpdateStartTime(time);
            DiscordManager.current.client.UpdateLargeAsset("logo", $"{JammerDash.Account.Instance.username}: #N/A");
            DiscordManager.current.client.UpdateSmallAsset("shine", "-- sp");
            if (SceneManager.GetActiveScene().name == "LevelDefault")
            {
                DiscordManager.current.UpdateDetails($"▶ {CustomLevelDataManager.Instance.data.artist} - {CustomLevelDataManager.Instance.data.songName}");
                DiscordManager.current.UpdateState($"by {CustomLevelDataManager.Instance.creator}");
            }
            else if (SceneManager.GetActiveScene().name == "SampleScene")
            {
                DiscordManager.current.UpdateDetails($"✎ {FindObjectOfType<EditorManager>().songArtist.text} - {FindObjectOfType<EditorManager>().customSongName.text}");
                DiscordManager.current.UpdateState($"{FindObjectOfType<EditorManager>().objectCount.text}");
            }
            else if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                mainMenu menu = FindAnyObjectByType<mainMenu>();
                if (menu.mainPanel.activeSelf)
                {
                    DiscordManager.current.UpdateDetails($"Idle");
                }
                else if (menu.playPanel.activeSelf)
                {
                    DiscordManager.current.UpdateDetails($"Choosing a level to play");
                }
                else if (menu.settingsPanel.activeSelf)
                {
                    DiscordManager.current.UpdateDetails($"Changing options");
                }
                else if (menu.levelInfo.activeSelf)
                {
                    DiscordManager.current.UpdateDetails($"Choosing a level to edit");
                }
                DiscordManager.current.UpdateState($"♬ {AudioManager.Instance.source.clip.name}");
            }
            else if (SceneManager.GetActiveScene().name == "intro")
            {
                DiscordManager.current.UpdateDetails($"Idle");
                DiscordManager.current.UpdateState($"Loading up...");
            }
        }
    }
}