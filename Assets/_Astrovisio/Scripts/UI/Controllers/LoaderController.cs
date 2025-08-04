using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class LoaderController
    {
        public VisualElement Root { get; }
        private VisualElement spinner;

        private VisualElement loadingBarContainer;
        private MinMaxSlider loadingBar;
        private Label loadingBarMessage;

        public LoaderController(VisualElement root)
        {
            Root = root;

            VisualElement loaderView = root.Q<VisualElement>("LoaderView");

            spinner = loaderView.Q<VisualElement>("LoadingSpinner");

            loadingBarContainer = loaderView.Q<VisualElement>("LoadingBarContainer");
            loadingBarMessage = loadingBarContainer.Q<Label>("Message");
            loadingBar = loadingBarContainer.Q<VisualElement>("LoadingBar")?.Q<MinMaxSlider>("MinMaxSlider");
            loadingBar.lowLimit = 0f;
            loadingBar.highLimit = 1f;

            SetSpinnerVisibility(false);
            SetBarVisibility(false);
        }

        public void SetSpinnerVisibility(bool visibility)
        {
            if (visibility)
            {
                spinner.style.display = DisplayStyle.Flex;
            }
            else
            {
                spinner.style.display = DisplayStyle.None;
            }
        }

        public void SetBarVisibility(bool visibility)
        {
            if (visibility)
            {
                loadingBarContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                loadingBarContainer.style.display = DisplayStyle.None;
            }
        }

        public void SetBarProgress(float value, string text, bool visibility)
        {
            // Debug.Log("SetBarProgress");
            loadingBarMessage.text = text;
            loadingBar.maxValue = value;
            SetBarVisibility(visibility);
        }

    }

}
