using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class LoaderController
    {
        public VisualElement Root { get; }
        private VisualElement spinner;
        private VisualElement bar;
        private Label barLabel;
        private VisualElement barFill;

        public LoaderController(VisualElement root)
        {
            Root = root;

            VisualElement loaderView = root.Q<VisualElement>("LoaderView");
            spinner = loaderView.Q<VisualElement>("LoadingSpinner");
            bar = loaderView.Q<VisualElement>("LoadingBar");
            barLabel = bar.Q<Label>("Message");
            barFill = bar.Q<VisualElement>("BarFill");

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
                bar.style.display = DisplayStyle.Flex;
            }
            else
            {
                bar.style.display = DisplayStyle.None;
            }
        }

        public void SetBarProgress(float value, string text, bool visibility)
        {
            // Debug.Log("SetBarProgress");

            barLabel.text = text;

            Vector3 scale = barFill.transform.scale;
            scale.x = value;
            barFill.transform.scale = scale;

            SetBarVisibility(visibility);
        }

    }

}
