using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Discord;
using JammerDash.Editor;
using JammerDash.Menus;
using JammerDash.Audio;
using NUnit.Framework.Constraints;
using JammerDash.Game.Player;
using JammerDash.Game;
using System.Linq;

namespace JammerDash.Tech
{

    public class DiscordRPC : MonoBehaviour
    {
        private Discord.Activity presence;
        private Discord.Discord discord;
        private Discord.ActivityManager manager;

        void Start()
        {
            DateTime currentDateTime = DateTime.UtcNow;
            DateTimeOffset dateTimeOffset = new DateTimeOffset(currentDateTime);
            try
            {
                SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                discord = new Discord.Discord(1127906222482391102, (ulong)Discord.CreateFlags.NoRequireDiscord);
                presence = new Discord.Activity
                {
                    Assets = new Discord.ActivityAssets()
                    {
                        LargeImage = "logo",
                        SmallImage = "shine",
                        LargeText = $"@{Account.Instance.username} | Rank: #N/A",
                        SmallText = $"{Mathf.RoundToInt(Difficulty.Calculator.CalculateSP("scores.dat"))}sp | {Difficulty.Calculator.CalculateAccuracy("scores.dat"):0.00}% | lv{Account.Instance.level}"
                    },
                    Timestamps = new Discord.ActivityTimestamps()
                    {

                        Start = dateTimeOffset.ToUnixTimeSeconds()
                    },
                    Instance = true
                };
                manager = discord.GetActivityManager();
                manager.UpdateActivity(presence, null);
                InvokeRepeating(nameof(UpdateDiscordPresence), 0, 2);

            }
            catch (Exception ex)
            {
                Debug.LogError("Error initializing Discord SDK: " + ex.Message);
            }
        }
        private void FixedUpdate()
        {
            discord?.RunCallbacks();
        }
        private void OnDisable()
        {
            discord.Dispose();
        }
        private void UpdateDiscordPresence()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
                if (!data.discordPlay)
                {
                    // Update presence based on the active scene
                    if (sceneName == "LevelDefault")
                    {
                        presence.Details = $"▶ {CustomLevelDataManager.Instance.data.artist} - {CustomLevelDataManager.Instance.data.songName}";
                        presence.State = $"by {CustomLevelDataManager.Instance.creator}";
                        presence.Assets.SmallImage = "note";
                        presence.Assets.SmallText = $"{FindFirstObjectByType<CubeCounter>().rank} | {FindFirstObjectByType<CubeCounter>().accCount / FindFirstObjectByType<PlayerMovement>().Total * 100:0.00}%";
                    }
                }
                if (!data.discordEdit)
                {
                    if (sceneName == "SampleScene")
                    {
                        var editorManager = FindFirstObjectByType<EditorManager>();
                        presence.Details = $"✎ {editorManager.customSongName.text}";
                        presence.State = $"{editorManager.songArtist.text}";
                    presence.Assets.SmallImage = "cube";
                    presence.Assets.SmallText = $"{editorManager.bpm.text} BPM | {editorManager.cubes.Count() + editorManager.longCubes.Count() + editorManager.saws.Count()} objects";
                    }
                }

                if (sceneName == "MainMenu")
                {
                    presence.Assets.SmallImage = "shine";
                    presence.Assets.SmallText = $"{Mathf.RoundToInt(Difficulty.Calculator.CalculateSP("scores.dat"))}sp | {Difficulty.Calculator.CalculateAccuracy("scores.dat"):0.00}% | lv{Account.Instance.level}";
                    var menu = FindAnyObjectByType<mainMenu>();
                    if (menu.afkTime < 10f)
                    {
                        presence.Type = ActivityType.Playing;
                        presence.Details = "Main Menu";
                        presence.State = $"ᶻ 𝗓 𐰁 Idle";
                        presence.Assets.SmallText = $"{Mathf.RoundToInt(Difficulty.Calculator.CalculateSP("scores.dat"))}sp | {Difficulty.Calculator.CalculateAccuracy("scores.dat"):0.00}% | lv{Account.Instance.level}";
                    
                    }
                    else if (menu.afkTime > 10f && !data.discordAFK)
                    {
                float Time = AudioManager.Instance.source.time;
                float length = AudioManager.Instance.source.clip.length;
                        presence.Type = ActivityType.Listening;
                        presence.Details = "AFK";
                    }

                }

                // Update the Discord activity with the modified presence
                manager.UpdateActivity(presence, (res) =>
                {

                });
            }
             public string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            // Ensure seconds don't go beyond 59
            seconds = Mathf.Clamp(seconds, 0, 59);

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        }
        
    }