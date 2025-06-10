using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ToastMessageController
    {

        public VisualElement Root { get; }

        private VisualElement successToastMessage;
        private VisualElement errorToastMessage;

        public ToastMessageController(VisualElement root)
        {
            Root = root;
            Root.pickingMode = PickingMode.Ignore;

            successToastMessage = Root.Q<VisualElement>("");
            errorToastMessage = Root.Q<VisualElement>("");
        }

        public void SetToastSuccessMessage(string message)
        {
            Debug.Log($"SetToastSuccessMessage: {message}");
        }

        public void SetToastErrorMessage(string message)
        {
            Debug.Log($"SetToastSuccessMessage: {message}");
        }

    }

}
