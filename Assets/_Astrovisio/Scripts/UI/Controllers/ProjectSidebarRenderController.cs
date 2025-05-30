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
        private ScrollView paramSettingsScrollView;
        private Dictionary<string, ParamRow> paramSettingsDatas = new();
        private VisualElement settingsPanel;
        private DropdownField colorMapDropdown;
        private MinMaxSlider settingsPanelThresholdSlider;
        private DoubleField settingsPanelMinFloatField;
        private DoubleField settingsPanelMaxFloatField;
        private Button settingsPanelCancelButton;
        private Button settingsPanelApplyButton;
        private EventCallback<ChangeEvent<string>> settingsPanelDropdownCallback;
        private EventCallback<ChangeEvent<Vector2>> settingsPanelSliderCallback;
        private EventCallback<ChangeEvent<double>> settingsPanelMinFieldCallback;
        private EventCallback<ChangeEvent<double>> settingsPanelMaxFieldCallback;
        private Action settingsPanelCancelButtonCallback;
        private Action settingsPanelApplyButtonCallback;
        private bool isThresholdUpdating = false;


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

            InitSettingsPanel();
            Update();
        }

        private void InitSettingsPanel()
        {
            settingsPanel = renderSettingsContainer.Q<VisualElement>("SettingsPanel");
            settingsPanel.RemoveFromClassList("active");

            settingsPanelThresholdSlider = settingsPanel.Q<VisualElement>("ThresholdSlider")?.Q<MinMaxSlider>("MinMaxSlider");
            settingsPanelMinFloatField = settingsPanel.Q<VisualElement>("ThresholdSlider")?.Q<DoubleField>("MinFloatField");
            settingsPanelMaxFloatField = settingsPanel.Q<VisualElement>("ThresholdSlider")?.Q<DoubleField>("MaxFloatField");
            settingsPanelCancelButton = settingsPanel.Q<VisualElement>("CancelButton")?.Q<Button>();
            settingsPanelApplyButton = settingsPanel.Q<VisualElement>("ApplyButton")?.Q<Button>();
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
                    if (paramButton.ClassListContains("active"))
                    {
                        UnselectAllParamSettingButton();
                    }
                    else
                    {
                        UnselectAllParamSettingButton();
                        paramButton.ToggleInClassList("active");
                        settingsPanel.ToggleInClassList("active");
                        UpdateSettingsPanel(paramRow.ParamRowSettingsController);
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

            UnregisterSettingPanelEvents();
            CloseSettingsPanel();
        }


        // === Settings Panel ===
        private void UpdateSettingsPanel(ParamRowSettingsController paramRowSettingsController)
        {
            RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);

            ColorMapSettings colorMapSettings = paramRowSettingsController.RenderSettings.MappingSettings as ColorMapSettings;
            ColorMapEnum colorMap = colorMapSettings.ColorMap;

            colorMapDropdown = settingsPanel.Q<VisualElement>("ColorMapDropdown")?.Q<DropdownField>("DropdownField");
            if (colorMapDropdown != null)
            {
                colorMapDropdown.choices = Enum.GetNames(typeof(ColorMapEnum)).ToList();
                colorMapDropdown.value = colorMap.ToString();

                settingsPanelDropdownCallback = evt =>
                {
                    if (Enum.TryParse<ColorMapEnum>(evt.newValue, out var selectedColorMap))
                    {
                        ColorMapSettings colorMapSettings = paramRowSettingsController.RenderSettings.MappingSettings as ColorMapSettings;
                        if (colorMapSettings != null)
                        {
                            colorMapSettings.ColorMap = selectedColorMap;
                            RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);
                            Debug.Log("ColorMAP " + (paramRowSettingsController.RenderSettings.MappingSettings as ColorMapSettings).ColorMap);
                        }
                        else
                        {
                            Debug.LogError("MappingSettings is not a ColorMapSettings");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Not valid value for ColorMapEnum: {evt.newValue}");
                    }
                };

                colorMapDropdown.RegisterValueChangedCallback(settingsPanelDropdownCallback);
            }

            if (settingsPanelThresholdSlider != null)
            {
                settingsPanelThresholdSlider.highLimit = float.MaxValue;
                settingsPanelThresholdSlider.lowLimit = float.MinValue;
                settingsPanelThresholdSlider.highLimit = paramRowSettingsController.RenderSettings.ThresholdMax;
                settingsPanelThresholdSlider.lowLimit = paramRowSettingsController.RenderSettings.ThresholdMin;
                settingsPanelThresholdSlider.maxValue = paramRowSettingsController.RenderSettings.ThresholdMaxSelected;
                settingsPanelThresholdSlider.minValue = paramRowSettingsController.RenderSettings.ThresholdMinSelected;
                settingsPanelMaxFloatField.value = paramRowSettingsController.RenderSettings.ThresholdMaxSelected;
                settingsPanelMinFloatField.value = paramRowSettingsController.RenderSettings.ThresholdMinSelected;
            }

            isThresholdUpdating = false;

            settingsPanelSliderCallback = evt =>
            {
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                settingsPanelMinFloatField.value = evt.newValue.x;
                settingsPanelMaxFloatField.value = evt.newValue.y;
                paramRowSettingsController.RenderSettings.ThresholdMinSelected = settingsPanelThresholdSlider.minValue;
                paramRowSettingsController.RenderSettings.ThresholdMaxSelected = settingsPanelThresholdSlider.maxValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);
                isThresholdUpdating = false;
            };

            settingsPanelMinFieldCallback = evt =>
            {
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                settingsPanelThresholdSlider.minValue = (float)evt.newValue;
                paramRowSettingsController.RenderSettings.ThresholdMinSelected = settingsPanelThresholdSlider.minValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);
                isThresholdUpdating = false;
            };

            settingsPanelMaxFieldCallback = evt =>
            {
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                settingsPanelThresholdSlider.maxValue = (float)evt.newValue;
                paramRowSettingsController.RenderSettings.ThresholdMaxSelected = settingsPanelThresholdSlider.maxValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);
                isThresholdUpdating = false;
            };

            settingsPanelThresholdSlider?.RegisterValueChangedCallback(settingsPanelSliderCallback);
            settingsPanelMinFloatField?.RegisterValueChangedCallback(settingsPanelMinFieldCallback);
            settingsPanelMaxFloatField?.RegisterValueChangedCallback(settingsPanelMaxFieldCallback);

            settingsPanelCancelButton.clicked += () =>
            {
                // Debug.Log("CancelButton clicked");
                UnregisterSettingPanelEvents();
                CloseSettingsPanel();
                RenderManager.Instance.CancelRenderSettings();
            };
            settingsPanelApplyButton.clicked += () =>
            {
                // Debug.Log("ApplyButton clicked");
                RenderManager.Instance.ApplyRenderSettings();
                UnregisterSettingPanelEvents();
                CloseSettingsPanel();
            };
        }

        private void CloseSettingsPanel()
        {
            foreach (var paramSettingButton in paramSettingsScrollView.Children())
            {
                Button paramButton = paramSettingButton.Q<Button>();
                paramButton.RemoveFromClassList("active");
                paramSettingButton.RemoveFromClassList("active");
                settingsPanel.RemoveFromClassList("active");
            }
        }

        private void UnregisterSettingPanelEvents()
        {
            if (settingsPanelSliderCallback != null)
            {
                colorMapDropdown?.UnregisterValueChangedCallback(settingsPanelDropdownCallback);
            }
            if (settingsPanelSliderCallback != null)
            {
                settingsPanelThresholdSlider?.UnregisterValueChangedCallback(settingsPanelSliderCallback);
            }
            if (settingsPanelMinFieldCallback != null)
            {
                settingsPanelMinFloatField?.UnregisterValueChangedCallback(settingsPanelMinFieldCallback);
            }
            if (settingsPanelMaxFieldCallback != null)
            {
                settingsPanelMaxFloatField?.UnregisterValueChangedCallback(settingsPanelMaxFieldCallback);
            }
            if (settingsPanelCancelButtonCallback != null)
            {
                settingsPanelCancelButton.clicked -= settingsPanelCancelButtonCallback;
            }
            if (settingsPanelApplyButtonCallback != null)
            {
                settingsPanelApplyButton.clicked -= settingsPanelApplyButtonCallback;
            }
        }

    }

}
