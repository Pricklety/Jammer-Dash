using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Discord;
using JammerDash.Editor;
using JammerDash.Menus;
using JammerDash.Audio;
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
        private SettingsData settings;
        private mainMenu menu;
        private EditorManager editorManager;
        private AudioSource audioSource;
        private CubeCounter cubeCounter;
        private PlayerMovement playerMovement;
        private bool isInitialized = false;

        private void Start()
        {
            try
            {
                settings = SettingsFileHandler.LoadSettingsFromFile(); // Cache settings
                discord = new Discord.Discord(1127906222482391102, (ulong)Discord.CreateFlags.NoRequireDiscord);
                manager = discord.GetActivityManager();
                menu = FindAnyObjectByType<mainMenu>();
                editorManager = FindFirstObjectByType<EditorManager>();
                audioSource = AudioManager.Instance.source;
                cubeCounter = FindFirstObjectByType<CubeCounter>();
                playerMovement = FindFirstObjectByType<PlayerMovement>();

                // Initialize Presence
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
                        Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    },
                    Instance = true
                };

                // Initial Presence Update
                UpdateDiscordPresence();
                manager.UpdateActivity(presence, null);

                // Schedule Updates Every 5 Seconds Instead of Every Frame
                InvokeRepeating(nameof(UpdateDiscordPresence), 5f, 5f);
                isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error initializing Discord SDK: " + ex.Message);
            }
        }

        private void Update()
        {
            if (isInitialized)
            {
                discord.RunCallbacks();
            }
        }

        private void OnDisable()
        {
            if (discord != null)
            {
                discord.Dispose();
            }
        }

        private void UpdateDiscordPresence()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (!settings.discordPlay && sceneName == "LevelDefault")
            {
                presence.Details = $"▶ {CustomLevelDataManager.Instance.data.artist} - {CustomLevelDataManager.Instance.data.songName}";
                presence.State = $"by {CustomLevelDataManager.Instance.creator}";
                presence.Assets.SmallImage = "note";

                if (cubeCounter != null && playerMovement != null)
                {
                    float accuracy = (cubeCounter.accCount / (float)playerMovement.Total) * 100;
                    presence.Assets.SmallText = $"{cubeCounter.rank} | {accuracy:0.00}%";
                }
            }

            if (!settings.discordEdit && sceneName == "SampleScene" && editorManager != null)
            {
                presence.Details = $"✎ {editorManager.customSongName.text}";
                presence.State = $"{editorManager.songArtist.text}";
                presence.Assets.SmallImage = "cube";
                presence.Assets.SmallText = $"{editorManager.bpm.text} BPM | {editorManager.cubes.Count() + editorManager.longCubes.Count() + editorManager.saws.Count()} objects";
            }

            if (sceneName == "MainMenu")
            {
                presence.Assets.SmallImage = "shine";
                presence.Assets.SmallText = $"{Mathf.RoundToInt(Difficulty.Calculator.CalculateSP("scores.dat"))}sp | {Difficulty.Calculator.CalculateAccuracy("scores.dat"):0.00}% | lv{Account.Instance.level}";

                if (menu != null)
                {
                    if (menu.playPanel.activeSelf)
                    {
                        presence.State = "▶ Choosing a level to play";
                    }
                    else if (menu.settingsPanel.activeSelf)
                    {
                        presence.State = "⚙︎ Changing options";
                    }
                    else if (menu.levelInfo.activeSelf)
                    {
                        presence.State = "✎ Choosing a level to edit";
                    }
                    else if (menu.afkTime < 10f)
                    {
                        presence.Type = ActivityType.Playing;
                        presence.Details = "Main Menu";
                        presence.State = "ᶻ 𝗓 𐰁 Idle";
                    }
                    else if (menu.afkTime > 10f && !settings.discordAFK)
                    {
                        presence.Type = ActivityType.Listening;
                        presence.Details = "AFK";

                        if (audioSource != null && audioSource.clip != null)
                        {
                            presence.State = $"♪ {audioSource.clip.name}";
                        }
                    }
                }
            }

            manager.UpdateActivity(presence, null);
        }

        
    }
}
