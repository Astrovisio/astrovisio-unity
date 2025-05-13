using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astrovisio
{

    [System.Serializable]
    public class ColorMapEntry
    {
        public string name;
        public Sprite texture;
    }

    [CreateAssetMenu(fileName = "ColorMapSO", menuName = "Scriptable Objects/ColorMapSO")]
    public class ColorMapSO : ScriptableObject
    {
        [SerializeField]
        private List<ColorMapEntry> entries = new();

        private Dictionary<string, Sprite> _lookup;

        public Sprite GetTexture(string name)
        {
            if (_lookup == null)
            {
                _lookup = entries.ToDictionary(e => e.name, e => e.texture);
            }

            return _lookup.TryGetValue(name, out var texture) ? texture : null;
        }

        public IReadOnlyList<ColorMapEntry> GetAllEntries() => entries;

    }

}
