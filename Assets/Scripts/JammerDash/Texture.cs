using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash
{
    public class Texture : MonoBehaviour
    {
        public string textureName;
        public Texture2D fallbackTexture;

        private SpriteRenderer objectRenderer;
        private Image uiImage;
        private Texture2D currentTexture;

        void Awake()
        {
            objectRenderer = GetComponent<SpriteRenderer>();
            uiImage = GetComponent<Image>();

            if (objectRenderer == null && uiImage == null)
            {
                Debug.LogError($"[TEXTURE SYSTEM] No Renderer or Image found on {gameObject.name}. This script requires one of them.");
            }
        }

        public void UpdateTexture(string texturePackPath)
        {
            if (this == null || string.IsNullOrEmpty(textureName)) return;

            string textureFilePath = Path.Combine(texturePackPath, textureName);
            if (!textureFilePath.EndsWith(".png")) textureFilePath += ".png";

            StartCoroutine(LoadTextureAsync(textureFilePath));
        }

        private IEnumerator LoadTextureAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                byte[] textureData = File.ReadAllBytes(filePath);
                Texture2D newTexture = new Texture2D(2, 2);
                bool success = newTexture.LoadImage(textureData);

                if (success)
                {
                    ApplyTexture(newTexture);
                    Debug.Log($"[TEXTURE SYSTEM] Texture applied: {textureName} from {filePath}");
                }
                else
                {
                    Debug.LogError($"[TEXTURE SYSTEM] Failed to load texture: {textureName}");
                    ApplyTexture(fallbackTexture);
                }
            }
            else
            {
                Debug.LogWarning($"[TEXTURE SYSTEM] Texture file not found: {filePath}. Applying fallback texture.");
                ApplyTexture(fallbackTexture);
            }
            yield return null;
        }

        public void ApplyTexture(Texture2D texture)
        {
            if (texture == null) return;
            if (currentTexture != null && currentTexture != fallbackTexture)
            {
                Destroy(currentTexture);
            }

            currentTexture = texture;

            if (objectRenderer != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                objectRenderer.GetPropertyBlock(block);
                block.SetTexture("_MainTex", texture);
                objectRenderer.SetPropertyBlock(block);
            }
            else if (uiImage != null)
            {
                uiImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }

        private void OnDestroy()
        {
            if (currentTexture != null && currentTexture != fallbackTexture)
            {
                Destroy(currentTexture);
            }
        }
    }
   
}
