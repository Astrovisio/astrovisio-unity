using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ToastMessageController
    {

        public VisualElement Root { get; }

        public ToastMessageController(VisualElement root)
        {
            Root = root;

            Debug.Log(Root);
            Root.pickingMode = PickingMode.Ignore;
        }

    }

}
