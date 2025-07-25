using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

namespace Astrovisio
{
    public class SettingsViewController
    {
        public VisualElement Root { get; }
        public UIManager UIManager { get; }

        private class ButtonAction
        {
            public string Name;
            public Button Button;
            public VisualElement Container;
            public Action OnClick;

            public ButtonAction(string name, Button button, VisualElement container = null, Action onClick = null)
            {
                Name = name;
                Button = button;
                Container = container;
                OnClick = onClick;
            }
        }

        private List<ButtonAction> buttonActions = new List<ButtonAction>();


        // Controllers
        private ScreenrecorderSettingController screenrecorderSettingController;
        private NoiseSettingController noiseSettingController;


        public SettingsViewController(VisualElement root, UIManager uiManager)
        {
            Root = root;
            UIManager = uiManager;
            Init();
            SetSettingsVisibility(false);
        }

        private void Init()
        {
            Root.pickingMode = PickingMode.Ignore;
            Root[0].pickingMode = PickingMode.Ignore;
            Root.Q<VisualElement>("Container").pickingMode = PickingMode.Ignore;

            // Containers
            var inspectorContainer = Root.Q<VisualElement>("InspectorContainer");
            var noiseContainer = Root.Q<VisualElement>("NoiseContainer");
            var hideUIContainer = Root.Q<VisualElement>("HideUIContainer");
            var screenshotContainer = Root.Q<VisualElement>("ScreenshotContainer");
            var screenrecorderContainer = Root.Q<VisualElement>("ScreenrecorderContainer");

            // Buttons
            var inspectorButton = inspectorContainer.Q<Button>("InspectorButton");
            var noiseButton = noiseContainer.Q<Button>("NoiseButton");
            var hideUIButton = hideUIContainer.Q<Button>("HideUIButton");
            var screenshotButton = screenshotContainer.Q<Button>("ScreenshotButton");
            var screenrecorderButton = screenrecorderContainer.Q<Button>("ScreenrecorderButton");


            // ButtonActions

            // 1. Panels
            buttonActions.Add(new ButtonAction("Inspector", inspectorButton, inspectorContainer));
            buttonActions.Add(new ButtonAction("Noise", noiseButton, noiseContainer));
            buttonActions.Add(new ButtonAction("Screenrecorder", screenrecorderButton, screenrecorderContainer));

            // 2. Action-only buttons
            buttonActions.Add(new ButtonAction(
                "Screenshot", screenshotButton, null,
                () =>
                {
                    UIManager.TakeScreenshot();
                }
            ));
            buttonActions.Add(new ButtonAction(
                "HideUI", hideUIButton, null,
                () =>
                {
                    bool uiVisibility = UIManager.GetUIVisibility();
                    UIManager.SetUIVisibility(!uiVisibility);
                }
            ));

            // Remove "active" from all at startup
            foreach (var action in buttonActions)
            {
                action.Button.RemoveFromClassList("active");
                if (action.Container != null)
                {
                    action.Container.RemoveFromClassList("active");
                }
            }

            // Bind all button events
            foreach (ButtonAction buttonAction in buttonActions)
            {
                buttonAction.Button.clicked += () =>
                {
                    if (buttonAction.Container != null)
                    {
                        // Panel logic: toggle this tab, deactivate others if activating
                        bool newState = !buttonAction.Button.ClassListContains("active");
                        if (newState && IsAnyPanelActive())
                        {
                            DeactivateAllPanels();
                        }

                        SetPanelActive(buttonAction, newState);
                    }
                    else
                    {
                        // Action-only button
                        buttonAction.OnClick?.Invoke();
                        // Optional: add highlight
                        buttonAction.Button.AddToClassList("active");
                        // Remove highlight after short delay for feedback
                        Root.schedule.Execute(() => buttonAction.Button.RemoveFromClassList("active")).StartingIn(150);
                    }
                };
            }

            screenrecorderSettingController = new ScreenrecorderSettingController(screenrecorderContainer);
            noiseSettingController = new NoiseSettingController(noiseContainer);
        }

        public void SetSettingsVisibility(bool visibility)
        {
            Root.style.display = visibility ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetPanelActive(ButtonAction action, bool active)
        {
            if (active)
            {
                action.Button.AddToClassList("active");
                if (action.Container != null)
                {
                    action.Container.AddToClassList("active");
                }
            }
            else
            {
                action.Button.RemoveFromClassList("active");
                if (action.Container != null)
                {
                    action.Container.RemoveFromClassList("active");
                }
            }
        }

        private bool IsAnyPanelActive()
        {
            foreach (var action in buttonActions)
            {
                if (action.Container != null && action.Button.ClassListContains("active"))
                {
                    return true;
                }
            }
            return false;
        }

        private void DeactivateAllPanels()
        {
            foreach (var action in buttonActions)
            {
                if (action.Container != null)
                {
                    SetPanelActive(action, false);
                }
            }
        }

    }

}
