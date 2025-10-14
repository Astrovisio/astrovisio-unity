using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using UnityEngine;

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
            public bool State { get; private set; }
            public Action OnClick { get; }


            public ButtonAction(string name, Button button, VisualElement container = null, VisualElement icon = null, bool state = false, Action onClick = null)
            {
                Name = name;
                Button = button;
                Container = container;
                Icon = icon;
                State = state;
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
        private HideUISettingController hideUIController;
        private AxesGizmoSettingController axesGizmoController;
        private ScreenrecorderSettingController screenrecorderSettingController;


        public SettingsViewController(VisualElement root, UIManager uiManager)
        {
            Root = root;
            UIManager = uiManager;

            RenderManager.Instance.OnFileRenderEnd += OnProjectRenderedEnd;

            Init();
            SetSettingsVisibility(false);
        }

        private void OnProjectRenderedEnd(Project project)
        {
            inspectorSettingController.Reset();
            noiseSettingController.Reset();
            hideUIController.Reset();
            axesGizmoController.Reset();
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
            VisualElement axesGizmoContainer = Root.Q<VisualElement>("AxesGizmoContainer");
            VisualElement screenshotContainer = Root.Q<VisualElement>("ScreenshotContainer");
            VisualElement screenrecorderContainer = Root.Q<VisualElement>("ScreenrecorderContainer");

            // Buttons
            Button inspectorButton = inspectorContainer.Q<Button>("InspectorButton");
            Button noiseButton = noiseContainer.Q<Button>("NoiseButton");
            Button hideUIButton = hideUIContainer.Q<Button>("HideUIButton");
            Button axesGizmoButton = axesGizmoContainer.Q<Button>("AxesGizmoButton");
            Button screenshotButton = screenshotContainer.Q<Button>("ScreenshotButton");
            Button screenrecorderButton = screenrecorderContainer.Q<Button>("ScreenrecorderButton");

            // Icons
            VisualElement inspectorIcon = inspectorContainer.Q<VisualElement>("Icon");
            VisualElement noiseIcon = noiseContainer.Q<VisualElement>("Icon");
            VisualElement hideUIIcon = hideUIContainer.Q<VisualElement>("Icon");
            VisualElement axesGizmoIcon = axesGizmoContainer.Q<VisualElement>("Icon");
            VisualElement screenshotIcon = screenshotContainer.Q<VisualElement>("Icon");
            VisualElement screenrecorderIcon = screenrecorderContainer.Q<VisualElement>("Icon");


            // ButtonActions

            // 1. Panels
            buttonActions.Add(new ButtonAction("Inspector", inspectorButton, inspectorContainer, inspectorIcon));
            buttonActions.Add(new ButtonAction("Noise", noiseButton, noiseContainer, noiseIcon));
            buttonActions.Add(new ButtonAction("Screenrecorder", screenrecorderButton, screenrecorderContainer, screenrecorderIcon));

            // 2. Action-only buttons
            buttonActions.Add(new ButtonAction(
                "Screenshot", screenshotButton, null, screenshotIcon, false,
                () =>
                {
                    UIManager.TakeScreenshot();
                }
            ));
            buttonActions.Add(new ButtonAction(
                "HideUI", hideUIButton, hideUIContainer, hideUIIcon, true,
                () =>
                {
                    bool hideUIState = hideUIController.GetState();
                    bool newHideUIState = !hideUIState;
                    hideUIController.SetState(newHideUIState);
                    UIManager.SetUIVisibility(newHideUIState);
                }
            ));
            buttonActions.Add(new ButtonAction(
                "AxesGizmo", axesGizmoButton, axesGizmoContainer, axesGizmoIcon, true,
                () =>
                {
                    bool axesGizmoState = axesGizmoController.GetState();
                    bool newAxesGizmoState = !axesGizmoState;
                    axesGizmoController.SetState(newAxesGizmoState);
                    SceneManager.Instance.SetAxesGizmoVisibility(newAxesGizmoState);
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

                        // Inspector Button
                        if (buttonAction.Name == "Inspector")
                        {
                            bool selectionState = inspectorSettingController.GetState();
                            bool newSelectionState = !selectionState;
                            inspectorSettingController.SetInspectorState(newSelectionState);
                            // Debug.Log("buttonAction.Name == Inspector -> " + newSelectionState);
                        }

                        // Noise Button
                        if (buttonAction.Name == "Noise" && buttonAction.State == false)
                        {
                            bool noiseState = noiseSettingController.GetState();
                            buttonAction.SetIconState(noiseState);
                        }

                        // HideUI Button
                        if (buttonAction.Name == "HideUI")
                        {
                            bool hideUIState = hideUIController.GetState();
                            buttonAction.SetIconState(hideUIState);
                            // Debug.Log("Setting -> " + hideUIState);
                        }

                        // AxesGizmo Button
                        if (buttonAction.Name == "AxesGizmo")
                        {
                            bool axesGizmoState = axesGizmoController.GetState();
                            buttonAction.SetIconState(axesGizmoState);
                            // Debug.Log("Setting -> " + axesGizmoState);
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
            hideUIController = new HideUISettingController(hideUIContainer);
            axesGizmoController = new AxesGizmoSettingController(axesGizmoContainer);
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

                    // Inspector Button
                    if (buttonAction.Name == "Inspector")
                    {
                        inspectorSettingController.SetInspectorState(false);
                        // Debug.Log("buttonAction.Name == Inspector -> " + newSelectionState);
                    }

                    // Noise Button
                    if (buttonAction.Name == "Noise" && buttonAction.State == false)
                    {
                        bool noiseState = noiseSettingController.GetState();
                        buttonAction.SetIconState(noiseState);
                    }

                    // HideUI Button
                    if (buttonAction.Name == "HideUI" && buttonAction.State == false)
                    {
                        bool hideUIState = hideUIController.GetState();
                        buttonAction.SetIconState(hideUIState);
                    }

                    // AxesGizmo Button
                    if (buttonAction.Name == "AxesGizmo" && buttonAction.State == false)
                    {
                        bool axesGizmoState = axesGizmoController.GetState();
                        buttonAction.SetIconState(axesGizmoState);
                    }
                }
            }
        }

    }

}
