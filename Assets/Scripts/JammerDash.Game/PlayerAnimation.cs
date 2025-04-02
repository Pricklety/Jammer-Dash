using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JammerDash 
{

public class PlayerAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public string spriteName1;
    public string spriteName2;

    [Header("Fallback Sprites")]
    public Sprite fallbackSprite1;
    public Sprite fallbackSprite2; 

    public float swapInterval = 0.14f; 
    private bool isSwapped = false;
    private Sprite sprite1;
    private Sprite sprite2;

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
        Debug.LogWarning("[PlayerAnimation] No texture pack found, using fallback textures.");
        sprite1 = fallbackSprite1;
        sprite2 = fallbackSprite2;
        spriteRenderer.sprite = sprite1;
        return;
    }

    Sprite loadedSprite1 = LoadSpriteFromFile(texturePackPath, spriteName1);
    Sprite loadedSprite2 = LoadSpriteFromFile(texturePackPath, spriteName2);

    sprite1 = loadedSprite1 ?? fallbackSprite1;
    sprite2 = loadedSprite2 ?? fallbackSprite2;

    spriteRenderer.sprite = sprite1;
}



    Sprite LoadSpriteFromFile(string folderPath, string fileName)
    {
        string filePath = Path.Combine(folderPath, "player", fileName + ".png");

        if (File.Exists(filePath))
        {
            byte[] imageData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageData))
            {
                texture.filterMode = FilterMode.Bilinear;
                Debug.LogWarning($"[PlayerAnimation] Sprite changed: {filePath}.");
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }

        Debug.LogWarning($"[PlayerAnimation] Sprite file not found: {filePath}, using fallback.");
        return null;
    }

    void SwapSprite()
    {
        if (sprite1 == null || sprite2 == null) return;

        isSwapped = !isSwapped;
        spriteRenderer.sprite = isSwapped ? sprite2 : sprite1;
    }

    public void ReloadSprites()
    {
        LoadSprites();
    }
}
}