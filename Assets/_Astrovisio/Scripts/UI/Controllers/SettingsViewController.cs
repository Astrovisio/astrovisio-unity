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
            public string Name { get; }
            public Button Button { get; }
            public VisualElement Container { get; }
            public VisualElement Icon { get; }
            public Action OnClick { get; }

            public bool State { get; private set; } = false;

            public ButtonAction(string name, Button button, VisualElement container = null, VisualElement icon = null, Action onClick = null)
            {
                Name = name;
                Button = button;
                Container = container;
                Icon = icon;
                OnClick = onClick;
            }

            public void SetState(bool state)
            {
                if (Container != null)
                {
                    Container.EnableInClassList("active", state);
                }

                SetIconState(state);

                if (Button != null)
                {
                    Button.EnableInClassList("active", state);
                }

                State = state;
            }

            public void SetIconState(bool state)
            {
                if (Icon != null)
                {
                    Icon.EnableInClassList("active", state);
                }
            }

            public void ToggleState()
            {
                State = !State;
                SetState(State);
            }

        }

        private List<ButtonAction> buttonActions = new List<ButtonAction>();


        // Controllers
        private InspectorSettingController inspectorSettingController;
        private NoiseSettingController noiseSettingController;
        private ScreenrecorderSettingController screenrecorderSettingController;


        public SettingsViewController(VisualElement root, UIManager uiManager)
        {
            Root = root;
            UIManager = uiManager;

            RenderManager.Instance.OnProjectRenderEnd += OnProjectRendered;

            Init();
            SetSettingsVisibility(false);
        }

        private void OnProjectRendered(Project project)
        {
            inspectorSettingController.Reset();
            noiseSettingController.Reset();
            // screenrecorderSettingController.Reset();
            DeactivateAllPanels();
        }

        private void Init()
        {
            Root.pickingMode = PickingMode.Ignore;
            Root[0].pickingMode = PickingMode.Ignore;
            Root.Q<VisualElement>("Container").pickingMode = PickingMode.Ignore;


            // Containers
            VisualElement inspectorContainer = Root.Q<VisualElement>("InspectorContainer");
            VisualElement noiseContainer = Root.Q<VisualElement>("NoiseContainer");
            VisualElement hideUIContainer = Root.Q<VisualElement>("HideUIContainer");
            VisualElement screenshotContainer = Root.Q<VisualElement>("ScreenshotContainer");
            VisualElement screenrecorderContainer = Root.Q<VisualElement>("ScreenrecorderContainer");

            // Buttons
            Button inspectorButton = inspectorContainer.Q<Button>("InspectorButton");
            Button noiseButton = noiseContainer.Q<Button>("NoiseButton");
            Button hideUIButton = hideUIContainer.Q<Button>("HideUIButton");
            Button screenshotButton = screenshotContainer.Q<Button>("ScreenshotButton");
            Button screenrecorderButton = screenrecorderContainer.Q<Button>("ScreenrecorderButton");

            // Icons
            VisualElement inspectorIcon = inspectorContainer.Q<VisualElement>("Icon");
            VisualElement noiseIcon = noiseContainer.Q<VisualElement>("Icon");
            VisualElement hideUIIcon = hideUIContainer.Q<VisualElement>("Icon");
            VisualElement screenshotIcon = screenshotContainer.Q<VisualElement>("Icon");
            VisualElement screenrecorderIcon = screenrecorderContainer.Q<VisualElement>("Icon");


            // ButtonActions

            // 1. Panels
            buttonActions.Add(new ButtonAction("Inspector", inspectorButton, inspectorContainer, inspectorIcon));
            buttonActions.Add(new ButtonAction("Noise", noiseButton, noiseContainer, noiseIcon));
            buttonActions.Add(new ButtonAction("Screenrecorder", screenrecorderButton, screenrecorderContainer, screenrecorderIcon));

            // 2. Action-only buttons
            buttonActions.Add(new ButtonAction(
                "Screenshot", screenshotButton, null, screenshotIcon,
                () =>
                {
                    UIManager.TakeScreenshot();
                }
            ));
            buttonActions.Add(new ButtonAction(
                "HideUI", hideUIButton, hideUIContainer, hideUIIcon,
                () =>
                {
                    bool uiVisibility = UIManager.GetUIVisibility();
                    UIManager.SetUIVisibility(!uiVisibility);
                }
            ));

            // Remove "active" from all at startup
            foreach (ButtonAction buttonAction in buttonActions)
            {
                SetPanelActive(buttonAction, false);
            }

            // Bind all button events
            foreach (ButtonAction buttonAction in buttonActions)
            {
                buttonAction.Button.clicked += () =>
                {
                    if (buttonAction.Container != null)
                    {
                        bool newState = !buttonAction.Button.ClassListContains("active");
                        if (newState && IsAnyPanelActive())
                        {
                            DeactivateAllPanels();
                        }

                        if (buttonAction.OnClick != null)
                        {
                            // Action-only button
                            buttonAction.OnClick?.Invoke();
                            // Optional: add highlight
                            buttonAction.ToggleState();
                        }

                        SetPanelActive(buttonAction, newState);

                        // Only for Inspector Button
                        if (buttonAction.Name == "Inspector")
                        {
                            bool selectionState = inspectorSettingController.GetState();
                            bool newSelectionState = !selectionState;
                            inspectorSettingController.SetInspectorState(newSelectionState);
                            // Debug.Log("buttonAction.Name == Inspector -> " + newSelectionState);
                        }

                        // Only for Noise Button
                        if (buttonAction.Name == "Noise" && buttonAction.State == false)
                        {
                            bool noiseState = noiseSettingController.GetState();
                            buttonAction.SetIconState(noiseState);
                        }
                    }
                    else
                    {
                        // Action-only button
                        buttonAction.OnClick?.Invoke();
                        // Optional: add highlight
                        buttonAction.SetState(true);
                        // Remove highlight after short delay for feedback
                        Root.schedule.Execute(() => buttonAction.SetState(false)).StartingIn(150);
                    }
                };
            }

            inspectorSettingController = new InspectorSettingController(inspectorContainer);
            noiseSettingController = new NoiseSettingController(noiseContainer);
            screenrecorderSettingController = new ScreenrecorderSettingController(screenrecorderContainer);
        }

        public void SetSettingsVisibility(bool visibility)
        {
            Root.style.display = visibility ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetPanelActive(ButtonAction buttonAction, bool active)
        {
            buttonAction.SetState(active);
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
            foreach (ButtonAction buttonAction in buttonActions)
            {
                if (buttonAction.Container != null)
                {
                    SetPanelActive(buttonAction, false);

                    // Only for Inspector Button
                    if (buttonAction.Name == "Inspector")
                    {
                        inspectorSettingController.SetInspectorState(false);
                        // Debug.Log("buttonAction.Name == Inspector -> " + newSelectionState);
                    }

                    // Only for Noise Button
                    if (buttonAction.Name == "Noise" && buttonAction.State == false)
                    {
                        bool noiseState = noiseSettingController.GetState();
                        buttonAction.SetIconState(noiseState);
                    }
                }
            }
        }

    }

}
