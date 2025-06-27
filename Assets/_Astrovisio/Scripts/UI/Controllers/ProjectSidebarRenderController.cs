using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
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
        private SettingsPanelController settingsPanelController;
        private VisualElement settingsPanel;
        private ScrollView paramSettingsScrollView;
        private Dictionary<string, ParamRow> paramSettingsDatas = new();
        private ProjectRenderSettings projectRenderSettings = new();


        public ProjectSidebarRenderController(ProjectSidebarController projectSidebarController, UIManager uiManager, ProjectManager projectManager, UIContextSO uiContextSO, Project project, VisualElement root)
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

            settingsPanel = renderSettingsContainer.Q<VisualElement>("SettingsPanel");
            settingsPanelController = new SettingsPanelController(Project, settingsPanel, UIContextSO);

            settingsPanelController.OnApplySetting = OnApplySettings;
            settingsPanelController.OnCancelSetting = OnCancelSettings;

            Update();
        }

        public void Update()
        {
            UpdateXYZLabels();
            UpdateParamsScrollView();
        }

        private void UpdateXYZLabels()
        {
            paramSettingsDatas.Clear();

            var xLabel = renderSettingsContainer.Q<Label>("XParamLabel");
            var yLabel = renderSettingsContainer.Q<Label>("YParamLabel");
            var zLabel = renderSettingsContainer.Q<Label>("ZParamLabel");
            xLabel.text = Project.ConfigProcess.Params.FirstOrDefault(p => p.Value.XAxis).Key ?? "";
            yLabel.text = Project.ConfigProcess.Params.FirstOrDefault(p => p.Value.YAxis).Key ?? "";
            zLabel.text = Project.ConfigProcess.Params.FirstOrDefault(p => p.Value.ZAxis).Key ?? "";
        }

        private void UpdateParamsScrollView()
        {
            paramSettingsScrollView = renderSettingsContainer.Q<ScrollView>("ParamSettingsScrollView");
            paramSettingsScrollView.Clear();

            foreach (var kvp in Project.ConfigProcess.Params)
            {
                string paramName = kvp.Key;
                ConfigParam configParam = kvp.Value;


                if (!configParam.Selected || configParam.XAxis || configParam.YAxis || configParam.ZAxis)
                {
                    continue;
                }

                VisualElement paramRowSettings = UIContextSO.paramRowSettingsTemplate.CloneTree();
                paramRowSettings.style.marginBottom = 8;

                var nameLabel = paramRowSettings.Q<Label>("ParamLabel");
                if (nameLabel != null)
                {
                    nameLabel.text = paramName;
                }

                ParamRow paramRow = new ParamRow
                {
                    VisualElement = paramRowSettings,
                    ParamRowSettingsController = new ParamRowSettingsController(paramName, Project)
                };

                Button paramButton = paramRowSettings.Q<Button>("Root");
                paramButton.RemoveFromClassList("active");
                paramButton.clicked += () =>
                {
                    ParamRowSettingsController paramRowSettingsController = GetParamRowSettingsController(paramName);

                    if (paramButton.ClassListContains("active"))
                    {
                        UnselectAllParamSettingButton();
                        CloseSettingsPanel();
                    }
                    else
                    {
                        UnselectAllParamSettingButton();
                        paramButton.AddToClassList("active");
                        settingsPanel.AddToClassList("active");
                        settingsPanelController.InitSettingsPanel(paramRowSettingsController);
                        UpdateRenderManager();
                    }

                };

                paramSettingsDatas.Add(paramName, paramRow);
                paramSettingsScrollView.Add(paramRowSettings);
            }
        }

        private void UnselectAllParamSettingButton()
        {
            foreach (var paramSettingsData in paramSettingsDatas.Values)
            {
                var button = paramSettingsData.VisualElement.Q<Button>("Root");
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

        private void OnApplySettings(ParamRowSettingsController appliedParamRowSettingsController)
        {
            string appliedParamName = appliedParamRowSettingsController.ParamName;
            MappingType appliedMapping = appliedParamRowSettingsController.RenderSettings.Mapping;

            // Debug.Log("Applied Param Name: " + appliedParamName + " " + appliedMapping);
            // PrintAllMappings();

            // Update project params
            switch (appliedMapping)
            {
                case MappingType.None:
                    // Debug.Log("OnApplySettings -> None: " + appliedParamName);
                    if (
                        projectRenderSettings.OpacitySettingsController != null &&
                        projectRenderSettings.OpacitySettingsController.ParamName == appliedParamName)
                    {
                        // Debug.Log("Resetting opacity...");
                        projectRenderSettings.OpacitySettingsController.Reset();
                        projectRenderSettings.OpacitySettingsController = null;
                    }
                    else if (
                        projectRenderSettings.ColorMapSettingsController != null &&
                        projectRenderSettings.ColorMapSettingsController.ParamName == appliedParamName)
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
                MappingType paramMappingType = paramRowSettingsController.RenderSettings.Mapping;

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
                MappingType paramMappingType = paramRowSettingsController.RenderSettings.Mapping;
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
                RenderManager.Instance.SetRenderSettings(projectRenderSettings.ColorMapSettingsController.RenderSettings);
            }

            if (projectRenderSettings.OpacitySettingsController is null)
            {
                // Debug.Log("Removing opacity");
                RenderManager.Instance.RemoveOpacity();
            }
            else
            {
                // Debug.Log("Setting opacity " + projectRenderSettings.OpacitySettingsController.ParamName);
                RenderManager.Instance.SetRenderSettings(projectRenderSettings.OpacitySettingsController.RenderSettings);
            }
        }

        private void UpdateMappingIcons()
        {
            string colormapParamName = projectRenderSettings.ColorMapSettingsController != null ? projectRenderSettings.ColorMapSettingsController.ParamName : "";
            string opacityParamName = projectRenderSettings.OpacitySettingsController != null ? projectRenderSettings.OpacitySettingsController.ParamName : "";

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
            string opacityParamName = (projectRenderSettings.OpacitySettingsController != null) ? projectRenderSettings.OpacitySettingsController.ParamName : "null";
            string colormapParamName = (projectRenderSettings.ColorMapSettingsController != null) ? projectRenderSettings.ColorMapSettingsController.ParamName : "null";

            Debug.Log("PrintAllMapping");
            Debug.Log("Opacity: " + opacityParamName);
            Debug.Log("Colormap: " + colormapParamName);
        }

        private void SetParamRowSettingsController(string paramName, ParamRowSettingsController paramRowSettingsController)
        {
            if (paramSettingsDatas.TryGetValue(paramName, out ParamRow paramRow))
            {
                paramRow.ParamRowSettingsController = paramRowSettingsController;
                // Debug.Log("SetParamRowSettingsController: " + paramName + " -> " + paramRowSettingsController.RenderSettings.Mapping);

                // foreach (var kvp in paramSettingsDatas)
                // {
                //     string key = kvp.Key;
                //     ParamRow value = kvp.Value;
                //     Debug.Log($"Parametro: {key}, Mapping: {value.ParamRowSettingsController.RenderSettings.Mapping}");
                // }
            }
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
            PrintAllMappings();
        }

    }

}
