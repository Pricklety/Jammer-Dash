using System.Collections;
using System.IO;
using JammerDash.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JammerDash {
    public class TexturePack : MonoBehaviour
    {
        public static TexturePack Instance; // Singleton for global access
        private static string activeTexturePackPath;
        private string defaultTexturePackPath;
        private Texture[] textureableObjects;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Set up the default texture pack path
            defaultTexturePackPath = Path.Combine(Main.gamePath, "textures", "default");
            if (!Directory.Exists(defaultTexturePackPath))
            {
                Debug.LogWarning($"Default texture pack not found at: {defaultTexturePackPath}");
            }

            // Listen for scene changes
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            UpdateTexture();
        }

        /// <summary>
        /// Sets the active texture pack.
        /// </summary>
        /// <param name="texturePackPath">The path to the texture pack folder.</param>
        public void SetActiveTexturePack(string texturePackPath)
        {
            if (!Directory.Exists(texturePackPath))
            {
                Debug.LogError($"Texture pack path not found: {texturePackPath}");
                return;
            }

            activeTexturePackPath = texturePackPath;
            Debug.Log($"Active texture pack set to: {activeTexturePackPath}");
        }

        /// <summary>
        /// Gets the path of the currently active texture pack.
        /// </summary>
        public static string GetActiveTexturePackPath()
        {
            string path = activeTexturePackPath;
            return path;
        }

        public void UpdateTexture()
        {
            if (Directory.Exists(defaultTexturePackPath)) {
                 if (SceneManager.GetActiveScene().buildIndex == 1 && FindAnyObjectByType<Options>().textures.options.Count != 0) {

                    
                ConfigLoader.Instance.LoadConfig();
                }
                
                ConfigLoader.Instance.LoadConfig();
            }

            textureableObjects = FindObjectsByType<Texture>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Texture obj in textureableObjects)
            {
                if (!string.IsNullOrEmpty(activeTexturePackPath))
                {
                    StartCoroutine(LoadTextureAsync(obj, activeTexturePackPath));
                }
                else if (!string.IsNullOrEmpty(defaultTexturePackPath))
                {
                    StartCoroutine(LoadTextureAsync(obj, defaultTexturePackPath));
                }
                else
                {
                    Debug.LogWarning($"No valid texture pack path set for {obj.gameObject.name}");
                }
            }
        }

        /// <summary>
        /// Loads texture asynchronously to reduce lag.
        /// </summary>
        private IEnumerator LoadTextureAsync(Texture obj, string path)
        {
            // Assume textures are stored as PNG files with their names matching the object name
            string texturePath = Path.Combine(path, obj.textureName + ".png");

            if (!File.Exists(texturePath))
            {
                Debug.LogWarning($"[TEXTURE SYSTEM] Texture not found: {texturePath}");
                obj.ApplyTexture(obj.fallbackTexture);
                yield break;
            }

            byte[] fileData = File.ReadAllBytes(texturePath);
           Texture2D newTexture = new Texture2D(2, 2);
            if (newTexture.LoadImage(fileData))
            {
                Debug.Log($"[TEXTURE SYSTEM] Applied {texturePath} to {obj}");
                obj.ApplyTexture(newTexture);
            }



            yield return null; // Allow Unity to continue rendering
        }

        /// <summary>
        /// Loads the default texture pack if no other texture pack is active.
        /// </summary>
        public void LoadDefaultTexturePack()
        {
            if (Directory.Exists(defaultTexturePackPath))
            {
                SetActiveTexturePack(defaultTexturePackPath);
            }
            else
            {
                Debug.LogError($"Default texture pack not found: {defaultTexturePackPath}");
            }
        }

        public void Update() {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F7)) {
                UpdateTexture();
                KeybindPanel.ToggleFunction("Reload textures", "Shift+F7");
            }
        }
    }
}
