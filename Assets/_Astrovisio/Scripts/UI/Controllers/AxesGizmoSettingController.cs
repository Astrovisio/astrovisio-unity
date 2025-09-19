using UnityEngine.UIElements;

namespace Astrovisio
{
    public class AxesGizmoSettingController
    {
        public VisualElement Root { get; }

        private bool axesGizmoState = true;

        public AxesGizmoSettingController(VisualElement root)
        {
            Root = root;
        }

        public bool GetState()
        {
            return axesGizmoState;
        }

        public void SetState(bool state)
        {
            axesGizmoState = state;
        }

        public void Reset()
        {
            axesGizmoState = true;
            SceneManager.Instance.SetAxesGizmoVisibility(axesGizmoState);
        }

    }

}
