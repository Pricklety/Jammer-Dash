using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Discord;
using JammerDash.Editor;
using JammerDash.Menus;
using JammerDash.Audio;

namespace JammerDash.Tech
{
    public class DiscordRPC : MonoBehaviour
    {
        private Discord.Activity presence;
        private Discord.Discord discord;
        private Discord.ActivityManager manager;
        private float lastUpdateTime = 0f;
        private const float UpdateInterval = 5f;

        void Start()
        {
            DateTime currentDateTime = DateTime.UtcNow;
            DateTimeOffset dateTimeOffset = new DateTimeOffset(currentDateTime);
            try
            {
                discord = new Discord.Discord(1127906222482391102, (ulong)Discord.CreateFlags.NoRequireDiscord);
                presence = new Discord.Activity
                {
                    Details = "Enter Sequence",
                    Assets = new Discord.ActivityAssets()
                    {
                        LargeImage = "logo",
                        SmallImage = "shine",
                        LargeText = $"{Account.Instance.username} | #0",
                        SmallText = $"{Mathf.RoundToInt(Difficulty.Calculator.CalculateSP("scores.dat"))}sp | {Difficulty.Calculator.CalculateAccuracy("scores.dat"):0.00}%"
                    },
                    Timestamps = new Discord.ActivityTimestamps()
                    {
                         
                         Start = dateTimeOffset.ToUnixTimeSeconds()
                    },
                    Instance = true
                };
                manager = discord.GetActivityManager();
                manager.UpdateActivity(presence, null);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error initializing Discord SDK: " + ex.Message);
            }
        }
        private void Update()
        {
            discord.RunCallbacks();
            UpdateDiscordPresence();
        }
        private void OnDisable()
        {
            discord.Dispose();
        }
        private void UpdateDiscordPresence()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            // Update presence based on the active scene
            if (sceneName == "LevelDefault")
            {
                presence.Details = $"▶ {CustomLevelDataManager.Instance.data.artist} - {CustomLevelDataManager.Instance.data.songName}";
                presence.State = $"by {CustomLevelDataManager.Instance.creator}";
            }
            else if (sceneName == "SampleScene")
            {
                var editorManager = FindObjectOfType<EditorManager>();
                presence.Details = $"✎ {editorManager.songArtist.text} - {editorManager.customSongName.text}";
                presence.State = $"{editorManager.objectCount.text}";
            }
            else if (sceneName == "MainMenu")
            {
                var menu = FindAnyObjectByType<mainMenu>();

                if (menu.playPanel.activeSelf)
                {
                    presence.State = $"▶ Choosing a level to play";
                }
                else if (menu.settingsPanel.activeSelf)
                {
                    presence.State = $"⚙︎ Changing options";
                }
                else if (menu.levelInfo.activeSelf)
                {
                    presence.State = $"✎ Choosing a level to edit";
                }
                else if (menu.afkTime < 25f)
                {
                    presence.Type = ActivityType.Playing;
                    presence.Details = "Main Menu";
                    presence.State = $"ᶻ 𝗓 𐰁 Idle";
                }
                else if (menu.afkTime > 25f)
                {
                    presence.Type = ActivityType.Listening; 
                    presence.Details = "AFK";
                    presence.State = $"♪ {AudioManager.Instance.source.clip.name}";
                }

            }

            // Update the Discord activity with the modified presence
            manager.UpdateActivity(presence, (res) =>
            {
                
            });
        }
    }
}

