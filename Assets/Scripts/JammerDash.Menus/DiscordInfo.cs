using Discord;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace JammerDash.Menus.Options
{
    public class DiscordInfo : MonoBehaviour
    {
        public Text name;
        public Text user;
        public RawImage pfp;
        private Discord.User currentUser;
        private Discord.Discord discord;

        private void Awake()
        {
            // Initialize the Discord instance with your application ID.
            discord = new Discord.Discord(1127906222482391102, (ulong)Discord.CreateFlags.Default);
        }

        private void Start()
        {
            // Ensure callbacks run
            discord.RunCallbacks();
                // Get the UserManager from Discord
                var userManager = discord.GetUserManager();

                // Fetch the current user
                currentUser = userManager.GetCurrentUser();

                // Now that the user data is fetched, we can update the UI
                name.text = currentUser.Username;
                user.text = currentUser.Discriminator;

                // If the user has a custom avatar, download it
                if (!string.IsNullOrEmpty(currentUser.Avatar))
                {
                    StartCoroutine(DownloadAvatar(pfp, (ulong)currentUser.Id, currentUser.Avatar));
                }
            
        }


        void Update()
        {
            // Always run the callbacks in the Update method to keep Discord data fresh.
            discord.RunCallbacks();
        }

        // Coroutine to download the avatar image and set it as the texture
        IEnumerator DownloadAvatar(RawImage rawImage, ulong userId, string avatarHash)
        {
            // Construct the avatar URL (Discord CDN URL format)
            string avatarUrl = string.Format("https://cdn.discordapp.com/avatars/{0}/{1}.png", userId, avatarHash);

            // Make a web request to get the texture
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(avatarUrl);
            yield return request.SendWebRequest();

            // If the request is successful, set the downloaded texture
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                rawImage.texture = texture;
            }
            else
            {
                // Log an error if fetching the avatar failed
                Debug.LogError("Error fetching avatar: " + request.error);
            }
        }

        private void OnApplicationQuit()
        {
            // Ensure to properly shut down Discord when the application closes
            if (discord != null)
            {
                discord.Dispose();
            }
        }
    }
}
