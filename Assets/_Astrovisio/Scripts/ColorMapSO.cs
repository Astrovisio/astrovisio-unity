using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astrovisio
{

    [System.Serializable]
    public class ColorMapEntry
    {
        public ColorMapEnum colorMap;
        public Sprite texture;
    }

    [CreateAssetMenu(fileName = "ColorMapSO", menuName = "Scriptable Objects/ColorMapSO")]
    public class ColorMapSO : ScriptableObject
    {
        [SerializeField]
        private List<ColorMapEntry> entries = new();
        private Dictionary<ColorMapEnum, Sprite> _lookup;

        private void EnsureLookup()
        {
            if (_lookup == null)
                _lookup = entries.ToDictionary(e => e.colorMap, e => e.texture);
        }

        public Sprite GetSprite(ColorMapEnum colorMap)
        {
            EnsureLookup();
            return _lookup.TryGetValue(colorMap, out var texture) ? texture : null;
        }

        public Texture2D GetTexture2D(ColorMapEnum colorMap)
        {
            EnsureLookup();

            if (_lookup.TryGetValue(colorMap, out var sprite) && sprite != null)
            {
                return SpriteToTexture2D(sprite);
            }

            return null;
        }

        private Texture2D SpriteToTexture2D(Sprite sprite)
        {
            Rect rect = sprite.rect;
            Texture2D source = sprite.texture;

            Color[] pixels = source.GetPixels(
                Mathf.FloorToInt(rect.x),
                Mathf.FloorToInt(rect.y),
                Mathf.FloorToInt(rect.width),
                Mathf.FloorToInt(rect.height)
            );

            Texture2D result = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            result.SetPixels(pixels);
            result.Apply();

            return result;
        }

        public IReadOnlyList<ColorMapEntry> GetAllEntries() => entries;
    }

}
