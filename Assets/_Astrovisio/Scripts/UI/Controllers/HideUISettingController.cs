using UnityEngine.UIElements;

namespace Astrovisio
{
    public class HideUISettingController
    {
        public VisualElement Root { get; }

        private bool hideUIState = true;

        public HideUISettingController(VisualElement root)
        {
            Root = root;
        }

        public bool GetState()
        {
            return hideUIState;
        }

        public void SetState(bool state)
        {
            hideUIState = state;
        }

        public void Reset()
        {
            hideUIState = true;
        }

    }

}
