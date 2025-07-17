using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class GizmoTransformController
    {
        public VisualElement Root { get; }

        public GizmoTransformController(VisualElement root)
        {
            Root = root;

            SetPanelVisibility(false);
        }

        public void SetPanelVisibility(bool visibility)
        {
            if (visibility)
            {
                Root.style.display = DisplayStyle.Flex;
            }
            else
            {
                Root.style.display = DisplayStyle.None;
            }
        }

    }

}