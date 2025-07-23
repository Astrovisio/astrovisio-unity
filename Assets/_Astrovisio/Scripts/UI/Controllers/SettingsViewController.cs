using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class SettingsViewController
    {
        public VisualElement Root { get; }

        private Button inspectorButton;
        private Button screenshotButton;
        private Button noiseButton;
        private Button hideUIButton;

        private ScreenshotSettingController screenshotSettingController;
        private NoiseSettingController noiseSettingController;

        public SettingsViewController(VisualElement root)
        {
            Root = root;
            Init();

            SetSettingsVisibility(false);
        }

        private void Init()
        {
            Root.pickingMode = PickingMode.Ignore;
            Root[0].pickingMode = PickingMode.Ignore;
            Root.Q<VisualElement>("Container").pickingMode = PickingMode.Ignore;

            inspectorButton = Root.Q<Button>("InspectorButton");
            screenshotButton = Root.Q<Button>("ScreenshotButton");
            noiseButton = Root.Q<Button>("NoiseButton");
            hideUIButton = Root.Q<Button>("HideUIButton");

            // Debug.Log(inspectorButton);
            // Debug.Log(screenshotButton);
            // Debug.Log(noiseButton);
            // Debug.Log(hideUIButton);

            SetInspectorState(false);
            SetScreenshotState(false);
            SetNoiseState(false);
            SetNoiseState(false);

            inspectorButton.clicked += () =>
            {
                Debug.Log("inspectorButton");

                bool newPanelState = !GetInspectorState();
                if (newPanelState && IsAnyStateActive())
                {
                    DeactivateAllStates();
                }
                SetInspectorState(newPanelState);
            };

            screenshotButton.clicked += () =>
            {
                Debug.Log("screenshotButton");

                bool newPanelState = !GetScreenshotState();
                if (newPanelState && IsAnyStateActive())
                {
                    DeactivateAllStates();
                }
                SetScreenshotState(newPanelState);
            };

            noiseButton.clicked += () =>
            {
                // Debug.Log("noiseButton");

                bool newPanelState = !GetNoiseState();
                if (newPanelState && IsAnyStateActive())
                {
                    DeactivateAllStates();
                }
                SetNoiseState(newPanelState);
            };

            hideUIButton.clicked += () =>
            {
                Debug.Log("hideUIButton");

                // bool newPanelState = !GetHideUIState();
                // SetHideUIState(newPanelState);

                Debug.Log(Root.pickingMode);
                Debug.Log(Root.Q<VisualElement>("SettingsView").pickingMode);
                Debug.Log(Root.Q<VisualElement>("Container").pickingMode);
            };

            screenshotSettingController = new ScreenshotSettingController(screenshotButton);
            noiseSettingController = new NoiseSettingController(noiseButton);
        }

        public void SetSettingsVisibility(bool visibility)
        {
            Root.style.display = visibility ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log(Root.style.display);
        }

        private void SetInspectorState(bool state)
        {
            if (state)
            {
                inspectorButton.AddToClassList("active");
            }
            else
            {
                inspectorButton.RemoveFromClassList("active");
            }
        }

        private void SetScreenshotState(bool state)
        {
            if (state)
            {
                screenshotButton.AddToClassList("active");
            }
            else
            {
                screenshotButton.RemoveFromClassList("active");
            }
        }

        private void SetNoiseState(bool state)
        {
            if (state)
            {
                noiseButton.AddToClassList("active");
            }
            else
            {
                noiseButton.RemoveFromClassList("active");
            }
        }

        private void SetHideUIState(bool state)
        {
            if (state)
            {
                hideUIButton.AddToClassList("active");
            }
            else
            {
                hideUIButton.RemoveFromClassList("active");
            }
        }

        private bool GetInspectorState()
        {
            return inspectorButton.ClassListContains("active");
        }

        private bool GetScreenshotState()
        {
            return screenshotButton.ClassListContains("active");
        }

        private bool GetNoiseState()
        {
            return noiseButton.ClassListContains("active");
        }

        private bool GetHideUIState()
        {
            return hideUIButton.ClassListContains("active");
        }

        private bool IsAnyStateActive()
        {
            return GetInspectorState() || GetScreenshotState() || GetNoiseState() || GetHideUIState();
        }

        private void DeactivateAllStates()
        {
            inspectorButton.RemoveFromClassList("active");
            screenshotButton.RemoveFromClassList("active");
            noiseButton.RemoveFromClassList("active");
            hideUIButton.RemoveFromClassList("active");
        }

    }

}
