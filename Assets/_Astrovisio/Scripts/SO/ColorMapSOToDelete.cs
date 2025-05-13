using UnityEngine;

// [CreateAssetMenu(fileName = "ColorMapSO", menuName = "Scriptable Objects/ColorMapSO")]
public class ColorMapSOToDelete : ScriptableObject
{
    [Header("Color Map Info")]
    public int colorMapID;
    public string colorMapName;
    
    [Header("Color Map Sprite")]
    public Sprite colorMapSprite;

    // Estrae la sotto-texture corrispondente alla sprite (assicurati che la texture sia Read/Write Enabled)
    public Texture2D GetExtractedTexture()
    {
        if (colorMapSprite == null) return null;
        
        Rect rect = colorMapSprite.rect;
        Texture2D originalTexture = colorMapSprite.texture;
        // Crea una nuova texture con le dimensioni della sotto-sprite
        Texture2D extractedTexture = new Texture2D((int)rect.width, (int)rect.height);
        extractedTexture.SetPixels(originalTexture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
        extractedTexture.Apply();
        return extractedTexture;
    }
    
}
