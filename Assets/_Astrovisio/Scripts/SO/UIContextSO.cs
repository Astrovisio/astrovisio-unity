using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    [CreateAssetMenu(fileName = "UIContext", menuName = "Astrovisio SO/UI Context")]
    public class UIContextSO : ScriptableObject
    {


        [Header("Header")]
        public VisualTreeAsset projectButtonTemplate;


        [Space(3)]
        [Header("Side")]

        [Space(1)]
        [Header("- Home")]
        public VisualTreeAsset favouriteProjectButton;
        public VisualTreeAsset projectSidebarTemplate;

        [Space(1)]
        [Header("- Project")]
        public VisualTreeAsset sidebarParamRowTemplate;
        public VisualTreeAsset paramRowSettingsTemplate;
        public ColorMapSO colorMapSO;


        [Header("Content")]

        [Space(1)]
        [Header("- Home")]
        public VisualTreeAsset projectRowHeaderTemplate;
        public VisualTreeAsset projectRowTemplate;

        [Space(1)]
        [Header("- Project")]
        public VisualTreeAsset projectViewTemplate;
        public VisualTreeAsset paramRowTemplate;
        public VisualTreeAsset listItemFileStateTemplate;

        [Space(1)]
        [Header("- New Project")]
        public VisualTreeAsset listItemFileTemplate;

        [Header("Cursor")]
        public Texture2D linkCursor;

    }
}
