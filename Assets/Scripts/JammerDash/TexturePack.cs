using System.IO;
using UnityEngine;

namespace JammerDash {
    public class TexturePack : MonoBehaviour
{
    public static TexturePack Instance; // Singleton for global access

    private string activeTexturePackPath;
    private string defaultTexturePackPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Set up the default texture pack path
        defaultTexturePackPath = Path.Combine(Main.gamePath, "textures", "Default");
        if (!Directory.Exists(defaultTexturePackPath))
        {
            Debug.LogWarning($"Default texture pack not found at: {defaultTexturePackPath}");
        }
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
    /// <returns>Path to the active texture pack folder.</returns>
    public string GetActiveTexturePackPath()
    {
        return activeTexturePackPath;
    }

    /// <summary>
    /// Notifies all Texturet instances to update their textures.
    /// </summary>
    private void Update()
    {
        Texture[] textureableObjects = FindObjectsByType<Texture>(FindObjectsSortMode.None);

        foreach (var obj in textureableObjects)
        {
            if (!string.IsNullOrEmpty(activeTexturePackPath))
            {
                obj.UpdateTexture(activeTexturePackPath);
            }
            else if (!string.IsNullOrEmpty(defaultTexturePackPath))
            {
                obj.UpdateTexture(defaultTexturePackPath);
            }
            else
            {
                Debug.LogWarning($"No valid texture pack path set for {obj.gameObject.name}");
            }
        }
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
}
}
