using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class AxisRow
    {
        public VisualElement VisualElement { get; set; }
        public AxisRowSettingsController AxisRowSettingsController { get; set; }
        public EventCallback<ClickEvent> ClickHandler;
    }

    public class ParamRow
    {
        public VisualElement VisualElement { get; set; }
        public ParamRowSettingsController ParamRowSettingsController { get; set; }
    }

    public class ProjectSidebarRenderController
    {

        // === Dependencies ===
        public ProjectSidebarController ProjectSidebarController { private set; get; }
        public UIManager UIManager { private set; get; }
        public ProjectManager ProjectManager { private set; get; }
        public UIContextSO UIContextSO { private set; get; }
        public Project Project { private set; get; }
        public VisualElement Root { private set; get; }

        // === Other ===
        private VisualElement renderSettingsContainer;
        private Button xButton;
        private Button yButton;
        private Button zButton;
        private Dictionary<string, AxisRow> axisSettingsData = new();
        private ScrollView paramSettingsScrollView;
        private Dictionary<string, ParamRow> paramSettingsDatas = new();
        private VisualElement settingsPanel;
        private SettingsPanelController settingsPanelController;

        private ProjectRenderSettings projectRenderSettings = new();


        public ProjectSidebarRenderController(
            ProjectSidebarController projectSidebarController,
            UIManager uiManager,
            ProjectManager projectManager,
            UIContextSO uiContextSO,
            Project project,
            VisualElement root
            )
        {
            ProjectSidebarController = projectSidebarController;
            UIManager = uiManager;
            ProjectManager = projectManager;
            UIContextSO = uiContextSO;
            Project = project;
            Root = root;

            Init();
        }

        private void Init()
        {
            renderSettingsContainer = Root.Q<VisualElement>("RenderSettingsContainer");

            VisualElement axisContainer = renderSettingsContainer.Q<VisualElement>("AxisContainer");
            xButton = axisContainer.Q<VisualElement>("XLabel")?.Q<Button>("Root");
            yButton = axisContainer.Q<VisualElement>("YLabel")?.Q<Button>("Root");
            zButton = axisContainer.Q<VisualElement>("ZLabel")?.Q<Button>("Root");

            paramSettingsScrollView = renderSettingsContainer.Q<ScrollView>("ParamSettingsScrollView");

            settingsPanel = renderSettingsContainer.Q<VisualElement>("SettingsPanel");
            settingsPanelController = new SettingsPanelController(Project, settingsPanel, UIContextSO);

            settingsPanelController.OnApplyAxisSetting += OnApplyAxisSettings;
            settingsPanelController.OnApplyParamSetting += OnApplyParamSettings;
            settingsPanelController.OnCancelSetting += OnCancelSettings;
        }

        public void Update()
        {
            paramSettingsDatas.Clear();
            UpdateAxisButtons();
            UpdateParamsScrollView();
            UnselectAllSettingsButton();
        }

        private void UpdateAxisButtons()
        {
            Label xLabel = xButton.Q<Label>("ParamLabel");
            Label yLabel = yButton.Q<Label>("ParamLabel");
            Label zLabel = zButton.Q<Label>("ParamLabel");

            File file = Project.Files?.FirstOrDefault(); // GB
            xLabel.text = file?.Variables.FirstOrDefault(v => v.XAxis)?.Name ?? "";
            yLabel.text = file?.Variables.FirstOrDefault(v => v.YAxis)?.Name ?? "";
            zLabel.text = file?.Variables.FirstOrDefault(v => v.ZAxis)?.Name ?? "";

            VisualElement axisContainer = renderSettingsContainer.Q<VisualElement>("AxisContainer");
            VisualElement xVisualElement = axisContainer.Q<VisualElement>("XLabel");
            VisualElement yVisualElement = axisContainer.Q<VisualElement>("YLabel");
            VisualElement zVisualElement = axisContainer.Q<VisualElement>("ZLabel");

            foreach (Variable variable in file.Variables)
            {
                if (!variable.Selected)
                {
                    continue;
                }

                VisualElement axisVisualElement = null;

                if (variable.XAxis)
                    axisVisualElement = xVisualElement;
                else if (variable.YAxis)
                    axisVisualElement = yVisualElement;
                else if (variable.ZAxis)
                    axisVisualElement = zVisualElement;


                if (axisVisualElement != null)
                {
                    // Debug.Log("Axis " + paramName);

                    Button axisButton = axisVisualElement.Q<Button>("Root");
                    axisButton.RemoveFromClassList("active");

                    if (axisSettingsData.ContainsKey(variable.Name))
                    {
                        var existingRow = axisSettingsData[variable.Name];
                        axisButton.UnregisterCallback(existingRow.ClickHandler);
                    }

                    EventCallback<ClickEvent> clickHandler = evt =>
                    {
                        AxisRowSettingsController axisRowSettingsController = GetAxisRowSettingsController(variable.Name);

                        if (axisButton.ClassListContains("active"))
                        {
                            UnselectAllSettingsButton();
                            CloseSettingsPanel();
                        }
                        else
                        {
                            UnselectAllSettingsButton();
                            axisButton.AddToClassList("active");
                            settingsPanel.AddToClassList("active");
                            settingsPanelController.InitAxisSettingsPanel(axisRowSettingsController);
                        }
                    };

                    axisButton.RegisterCallback(clickHandler);

                    AxisRow axisRow = new AxisRow
                    {
                        VisualElement = axisVisualElement,
                        AxisRowSettingsController = new AxisRowSettingsController(variable),
                        ClickHandler = clickHandler
                    };

                    axisSettingsData[variable.Name] = axisRow;
                }
            }
        }

        private void UpdateParamsScrollView()
        {
            paramSettingsScrollView.Clear();

            File file = Project.Files?.FirstOrDefault(); // GB

            foreach (Variable variable in file.Variables)
            {
                if (!variable.Selected || variable.XAxis || variable.YAxis || variable.ZAxis)
                {
                    continue;
                }

                VisualElement paramRowSettings = UIContextSO.paramRowSettingsTemplate.CloneTree();
                paramRowSettings.style.marginBottom = 8;

                Label nameLabel = paramRowSettings.Q<Label>("ParamLabel");
                if (nameLabel != null)
                {
                    nameLabel.text = variable.Name;
                }

                ParamRow paramRow = new ParamRow
                {
                    VisualElement = paramRowSettings,
                    ParamRowSettingsController = new ParamRowSettingsController(variable)
                };

                Button paramButton = paramRowSettings.Q<Button>("Root");
                paramButton.RemoveFromClassList("active");
                paramButton.clicked += () =>
                {
                    // Debug.Log("Clicked");
                    ParamRowSettingsController paramRowSettingsController = GetParamRowSettingsController(variable.Name);

                    if (paramButton.ClassListContains("active"))
                    {
                        UnselectAllSettingsButton();
                        CloseSettingsPanel();
                    }
                    else
                    {
                        UnselectAllSettingsButton();
                        paramButton.AddToClassList("active");
                        settingsPanel.AddToClassList("active");
                        settingsPanelController.InitParamSettingsPanel(paramRowSettingsController);
                        UpdateRenderManager();
                    }

                };

                paramSettingsDatas.Add(variable.Name, paramRow);
                paramSettingsScrollView.Add(paramRowSettings);
            }
        }

        private void UnselectAllSettingsButton()
        {
            xButton.RemoveFromClassList("active");
            yButton.RemoveFromClassList("active");
            zButton.RemoveFromClassList("active");

            foreach (ParamRow paramSettingsData in paramSettingsDatas.Values)
            {
                Button button = paramSettingsData.VisualElement.Q<Button>("Root");
                if (button != null)
                {
                    button.RemoveFromClassList("active");
                    settingsPanel.RemoveFromClassList("active");
                }
            }

            CloseSettingsPanel();
        }

        private void CloseSettingsPanel()
        {
            // settingsPanelController.UnregisterSettingPanelEvents();
            settingsPanelController.CloseSettingsPanel();

            foreach (var paramSettingButton in paramSettingsScrollView.Children())
            {
                Button paramButton = paramSettingButton.Q<Button>();
                paramButton.RemoveFromClassList("active");
                paramSettingButton.RemoveFromClassList("active");
                // settingsPanel.RemoveFromClassList("active");
            }
        }

        private void OnApplyAxisSettings(AxisRowSettingsController appliedAxisRowSettingsController)
        {
            string appliedParamName = appliedAxisRowSettingsController.Variable.Name;

            SetAxisRowSettingsController(appliedParamName, appliedAxisRowSettingsController);

            CloseSettingsPanel();
            xButton.RemoveFromClassList("active");
            yButton.RemoveFromClassList("active");
            zButton.RemoveFromClassList("active");
        }

        private void OnApplyParamSettings(ParamRowSettingsController appliedParamRowSettingsController)
        {
            string appliedParamName = appliedParamRowSettingsController.Variable.Name;
            MappingType appliedMapping = appliedParamRowSettingsController.ParamRenderSettings.Mapping;

            // Debug.Log("Applied Param Name: " + appliedParamName + " " + appliedMapping);
            // PrintAllMappings();

            // Update project params
            switch (appliedMapping)
            {
                case MappingType.None:
                    // Debug.Log("OnApplySettings -> None: " + appliedParamName);
                    if (
                        projectRenderSettings.OpacitySettingsController != null &&
                        projectRenderSettings.OpacitySettingsController.Variable.Name == appliedParamName)
                    {
                        // Debug.Log("Resetting opacity...");
                        projectRenderSettings.OpacitySettingsController.Reset();
                        projectRenderSettings.OpacitySettingsController = null;
                    }
                    else if (
                        projectRenderSettings.ColorMapSettingsController != null &&
                        projectRenderSettings.ColorMapSettingsController.Variable.Name == appliedParamName)
                    {
                        // Debug.Log("Resetting colormap...");
                        projectRenderSettings.ColorMapSettingsController.Reset();
                        projectRenderSettings.ColorMapSettingsController = null;
                    }
                    break;
                case MappingType.Opacity:
                    // Debug.Log("OnApplySettings -> Opacity: " + appliedParamName + " " + appliedParamRowSettingsController.RenderSettings.MappingSettings.ScalingType);
                    ResetParamsByMappingType(appliedParamName, MappingType.Opacity);
                    projectRenderSettings.OpacitySettingsController = appliedParamRowSettingsController;
                    break;
                case MappingType.Colormap:
                    // Debug.Log("OnApplySettings -> Colormap: " + appliedParamName);
                    ResetParamsByMappingType(appliedParamName, MappingType.Colormap);
                    projectRenderSettings.ColorMapSettingsController = appliedParamRowSettingsController;
                    break;
            }

            SetParamRowSettingsController(appliedParamName, appliedParamRowSettingsController);
            CloseSettingsPanel();
            UpdateRenderManager();
            UpdateMappingIcons();
            // PrintAllMappings();
        }

        private void ResetParamsByMappingType(string appliedParamName, MappingType appliedMappingType)
        {
            foreach (var paramSettings in paramSettingsDatas)
            {
                string paramName = paramSettings.Key;
                ParamRow paramRow = paramSettings.Value;
                ParamRowSettingsController paramRowSettingsController = paramRow.ParamRowSettingsController;
                MappingType paramMappingType = paramRowSettingsController.ParamRenderSettings.Mapping;

                // Debug.Log(paramName + " <-> " + appliedParamName);
                if (paramName == appliedParamName)
                {
                    if (paramMappingType == MappingType.Opacity)
                    {
                        projectRenderSettings.OpacitySettingsController = null;
                        // Debug.Log(paramName + " opacity null");
                    }
                    else if (paramMappingType == MappingType.Colormap)
                    {
                        projectRenderSettings.ColorMapSettingsController = null;
                        // Debug.Log(paramName + " colormap null");
                    }
                }
            }

            foreach (var paramSettings in paramSettingsDatas)
            {
                ParamRow paramRow = paramSettings.Value;
                ParamRowSettingsController paramRowSettingsController = paramRow.ParamRowSettingsController;
                MappingType paramMappingType = paramRowSettingsController.ParamRenderSettings.Mapping;
                if (paramMappingType == appliedMappingType)
                {
                    paramRowSettingsController.Reset();
                }
            }
        }

        private void UpdateRenderManager()
        {
            if (projectRenderSettings.ColorMapSettingsController is null)
            {
                // Debug.Log("Removing colormap");
                RenderManager.Instance.RemoveColorMap();
            }
            else
            {
                // Debug.Log("Setting colormap " + projectRenderSettings.ColorMapSettingsController.ParamName);
                RenderManager.Instance.SetRenderSettings(projectRenderSettings.ColorMapSettingsController.ParamRenderSettings);
            }

            if (projectRenderSettings.OpacitySettingsController is null)
            {
                // Debug.Log("Removing opacity");
                RenderManager.Instance.RemoveOpacity();
            }
            else
            {
                // Debug.Log("Setting opacity " + projectRenderSettings.OpacitySettingsController.ParamName);
                RenderManager.Instance.SetRenderSettings(projectRenderSettings.OpacitySettingsController.ParamRenderSettings);
            }
        }

        private void UpdateMappingIcons()
        {
            string colormapParamName = projectRenderSettings.ColorMapSettingsController != null ? projectRenderSettings.ColorMapSettingsController.Variable.Name : "";
            string opacityParamName = projectRenderSettings.OpacitySettingsController != null ? projectRenderSettings.OpacitySettingsController.Variable.Name : "";

            // Debug.Log("colormapParamName " + colormapParamName);
            // Debug.Log("opacityParamName " + opacityParamName);

            foreach (var paramSettings in paramSettingsDatas)
            {
                string paramName = paramSettings.Key;
                ParamRow paramRow = paramSettings.Value;

                VisualElement paramVisualElement = paramRow.VisualElement.Q<VisualElement>("Root");
                paramVisualElement.RemoveFromClassList("colormap");
                paramVisualElement.RemoveFromClassList("opacity");
                paramVisualElement.RemoveFromClassList("haptics");
                paramVisualElement.RemoveFromClassList("sound");
                // Debug.Log("Remove all from " + paramName);

                if (paramName == colormapParamName)
                {
                    paramVisualElement.AddToClassList("colormap");
                    // Debug.Log("Add colormap to " + paramName);
                }
                else if (paramName == opacityParamName)
                {
                    paramVisualElement.AddToClassList("opacity");
                    // Debug.Log("Add opacity to " + paramName);
                }
            }

        }

        private void PrintAllMappings()
        {
            string opacityParamName = (projectRenderSettings.OpacitySettingsController != null) ? projectRenderSettings.OpacitySettingsController.Variable.Name : "null";
            string colormapParamName = (projectRenderSettings.ColorMapSettingsController != null) ? projectRenderSettings.ColorMapSettingsController.Variable.Name : "null";

            Debug.Log("PrintAllMapping");
            Debug.Log("Opacity: " + opacityParamName);
            Debug.Log("Colormap: " + colormapParamName);
        }

        private void SetAxisRowSettingsController(string paramName, AxisRowSettingsController axisRowSettingsController)
        {
            if (axisSettingsData.TryGetValue(paramName, out AxisRow axisRow))
            {
                // Debug.Log($"Set: {axisRowSettingsController.AxisRenderSettings.ThresholdMinSelected} {axisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected}");
                axisRow.AxisRowSettingsController = axisRowSettingsController;
            }
        }

        private void SetParamRowSettingsController(string paramName, ParamRowSettingsController paramRowSettingsController)
        {
            if (paramSettingsDatas.TryGetValue(paramName, out ParamRow paramRow))
            {
                paramRow.ParamRowSettingsController = paramRowSettingsController;
            }
        }

        private AxisRowSettingsController GetAxisRowSettingsController(string paramName)
        {
            if (axisSettingsData.TryGetValue(paramName, out AxisRow axisRow))
            {
                // Debug.Log($"Get: {axisRow.AxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected} {axisRow.AxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected}");
                return axisRow.AxisRowSettingsController;
            }

            return null;
        }

        private ParamRowSettingsController GetParamRowSettingsController(string paramName)
        {
            if (paramSettingsDatas.TryGetValue(paramName, out ParamRow paramRow))
            {
                // Debug.Log("GetParamRowSettingsController: " + paramName + " -> " + paramRow.ParamRowSettingsController.RenderSettings.Mapping);
                return paramRow.ParamRowSettingsController;
            }

            return null;
        }

        private void OnCancelSettings()
        {
            CloseSettingsPanel();
            UpdateRenderManager();
            UpdateMappingIcons();
            // PrintAllMappings();
        }

    }

}
