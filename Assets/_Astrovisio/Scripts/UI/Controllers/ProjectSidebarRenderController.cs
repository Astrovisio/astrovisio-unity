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
            settingsPanelController = new SettingsPanelController(Project, settingsPanel);

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

            // Debug.Log("Applied Param Name: " + appliedParamName);

            // Update project params
            switch (appliedMapping)
            {
                case MappingType.None:
                    // Debug.Log("OnApplySettings -> None: " + appliedParamName);
                    break;
                case MappingType.Opacity:
                    if (projectRenderSettings.OpacitySettingsController is not null)
                    {
                        // Debug.Log("Resetting -> Opacity: " + projectRenderSettings.OpacitySettingsController.ParamName);
                        projectRenderSettings.OpacitySettingsController.Reset();
                    }
                    // Debug.Log("OnApplySettings -> Opacity: " + appliedParamName);
                    projectRenderSettings.OpacitySettingsController = appliedParamRowSettingsController;
                    break;
                case MappingType.Colormap:
                    if (projectRenderSettings.ColorMapSettingsController is not null)
                    {
                        // Debug.Log("Resetting -> Colormap: " + projectRenderSettings.ColorMapSettingsController.ParamName);
                        projectRenderSettings.ColorMapSettingsController.Reset();
                    }
                    // Debug.Log("OnApplySettings -> Colormap: " + appliedParamName);
                    projectRenderSettings.ColorMapSettingsController = appliedParamRowSettingsController;
                    break;
            }

            SetParamRowSettingsController(appliedParamName, appliedParamRowSettingsController);
            CloseSettingsPanel();
            UpdateRenderManager();
        }

        private void UpdateRenderManager()
        {
            Debug.Log("UpdateRenderManager");
            if (projectRenderSettings.ColorMapSettingsController is null)
            {
                Debug.Log("Removing colormap");
                RenderManager.Instance.RemoveColorMap();
            }
            else
            {
                Debug.Log("Setting colormap");
                RenderManager.Instance.SetRenderSettings(projectRenderSettings.ColorMapSettingsController.RenderSettings);
            }

            if (projectRenderSettings.OpacitySettingsController is null)
            {
                Debug.Log("Removing opacity");
                RenderManager.Instance.RemoveOpacity();
            }
            else
            {
                Debug.Log("Setting opacity");
                RenderManager.Instance.SetRenderSettings(projectRenderSettings.OpacitySettingsController.RenderSettings);
            }
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
        }

    }

}
