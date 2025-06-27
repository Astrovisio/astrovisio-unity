using System.Threading.Tasks;
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

            successToastMessage = Root.Q<VisualElement>("ContainerSuccess");
            errorToastMessage = Root.Q<VisualElement>("ContainerError");
        }

        public void SetToastSuccessMessage(string message)
        {
            // Debug.Log($"SetToastSuccessMessage: {message}");
            Label successToastMessageLabel = successToastMessage.Q<Label>();
            successToastMessageLabel.text = message;
            successToastMessage.AddToClassList("active");
            RemoveClassAfterDelay(successToastMessage, 4);
        }

        public void SetToastErrorMessage(string message)
        {
            // Debug.Log($"SetToastErrorMessage: {message}");
            Label errorToastMessageLabel = errorToastMessage.Q<Label>();
            errorToastMessageLabel.text = message;
            errorToastMessage.AddToClassList("active");
            RemoveClassAfterDelay(errorToastMessage, 4);
        }

        private async void RemoveClassAfterDelay(VisualElement element, int seconds)
        {
            await Task.Delay(seconds * 1000);
            element.RemoveFromClassList("active");
        }

    }

}
