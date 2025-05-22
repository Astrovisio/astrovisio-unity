using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ErrorVRViewController
    {

        public VisualElement Root { get; }

        private Button closeButton;

        public ErrorVRViewController(VisualElement root)
        {
            Root = root;

            closeButton = Root.Q<Button>("CloseButton");
            closeButton.clicked += Close;
        }

        public void Open()
        {
            Root.AddToClassList("active");
        }

        public void Close()
        {
            Root.RemoveFromClassList("active");
        }

    }

}