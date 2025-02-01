using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash {

        public class Texture : MonoBehaviour
{
    public string textureName; // The name of the texture file (e.g., "exampleTexture.png")
    public Texture2D fallbackTexture; // Fallback texture to use if the texture is missing
    private SpriteRenderer objectRenderer; // Renderer for 2D objects
    private Image uiImage; // Image component for UI elements

    void Awake()
    {
        // Check if this object has a Renderer (2D objects)
        objectRenderer = GetComponent<SpriteRenderer>();

        // Check if this object has an Image (UI)
        uiImage = GetComponent<Image>();

        if (objectRenderer == null && uiImage == null)
        {
            Debug.LogError($"No Renderer or Image found on {gameObject.name}. This script requires one of them.");
        }
    }

    public void UpdateTexture(string texturePackPath)
    {
        if (string.IsNullOrEmpty(textureName)) return;

        string textureFilePath = Path.Combine(texturePackPath, textureName + ".png");

        if (File.Exists(textureFilePath))
        {
            byte[] textureData = File.ReadAllBytes(textureFilePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(textureData))
            {
                ApplyTexture(texture);
                Debug.Log($"Texture applied: {textureName} from {texturePackPath}");
            }
            else
            {
                Debug.LogError($"Failed to load texture: {textureName}");
            }
        }
        else
        {
            Debug.LogWarning($"Texture file not found: {textureFilePath}. Applying fallback texture.");
            ApplyTexture(fallbackTexture);
        }
    }

    private void ApplyTexture(Texture2D texture)
    {
        if (texture == null) return;

        if (objectRenderer != null) // Apply to 3D objects
        {
            objectRenderer.material.mainTexture = texture;
        }
        else if (uiImage != null) // Apply to UI elements
        {
            uiImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
}