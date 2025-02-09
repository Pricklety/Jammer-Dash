using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JammerDash 
{
    public class PadAnimation : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public string[] spriteNames = {
    "padTexture0", "padTexture1", "padTexture2", "padTexture3", 
    "padTexture4", "padTexture5", "padTexture6", "padTexture7",
    "padTexture8", "padTexture9"
};


        [Header("Fallback Sprites")]
        public Sprite[] fallbackSprites;

        public float swapInterval = 0.10f; 
        private int currentSpriteIndex = 0;
        private Sprite[] sprites;

        void Start()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            LoadSprites();
            InvokeRepeating(nameof(SwapSprite), swapInterval, swapInterval);
        }

       void LoadSprites()
{
    string texturePackPath = TexturePack.GetActiveTexturePackPath();

    if (string.IsNullOrEmpty(texturePackPath))
    {
        Debug.LogWarning("[PadAnimation] No texture pack found, using fallback textures.");
        sprites = fallbackSprites;
    }
    else
    {
        List<Sprite> loadedSprites = new List<Sprite>();

        foreach (string spriteName in spriteNames)
        {
            Sprite loadedSprite = LoadSpriteFromFile(texturePackPath, spriteName);
            if (loadedSprite != null)
                loadedSprites.Add(loadedSprite);
        }

        sprites = loadedSprites.Count > 0 ? loadedSprites.ToArray() : fallbackSprites;
    }

    if (sprites.Length > 0)
        spriteRenderer.sprite = sprites[0];
}


        Sprite LoadSpriteFromFile(string folderPath, string fileName)
        {
            string filePath = Path.Combine(folderPath, fileName + ".png");

            if (File.Exists(filePath))
            {
                byte[] imageData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(imageData))
                {
                    texture.filterMode = FilterMode.Point;
                    Debug.Log($"[PlayerAnimation] Sprite loaded: {filePath}.");
                    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }

            Debug.LogWarning($"[PlayerAnimation] Sprite file not found: {filePath}, using fallback.");
            return null;
        }

        void SwapSprite()
        {
            if (sprites == null || sprites.Length == 0) return;

            currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Length;
            spriteRenderer.sprite = sprites[currentSpriteIndex];
        }

        public void ReloadSprites()
        {
            LoadSprites();
        }
    }
}
