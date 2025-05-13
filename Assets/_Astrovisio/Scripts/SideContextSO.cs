using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    [CreateAssetMenu(fileName = "SideContext", menuName = "Scriptable Objects/Side Context")]
    public class SideContextSO : ScriptableObject
    {
        public VisualTreeAsset projectSidebarTemplate;
        public VisualTreeAsset sidebarParamRowTemplate;
        public VisualTreeAsset paramRowSettingsTemplate;
        public ColorMapSO colorMapSO;
    }
}
