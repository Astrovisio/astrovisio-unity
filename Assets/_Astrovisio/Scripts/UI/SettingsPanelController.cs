using System;
using System.Collections;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class SettingsPanelController
    {

        // === Dependencies ===
        public Project Project { private set; get; }
        public VisualElement Root { private set; get; }

        // === Other ===
        public ParamRowSettingsController ParamRowSettingsController { private set; get; }
        public ParamRowSettingsController TempParamRowSettingsController { private set; get; }
        private Label paramNameLabel;
        private DropdownField mappingDropdown;
        private MinMaxSlider thresholdSlider;
        private DoubleField thresholdSliderMinFloatField;
        private DoubleField thresholdSliderMaxFloatField;
        private DropdownField colorMapDropdown;
        private DropdownField scalingDropdown;
        private Toggle invertMappingToggle;
        private Button applyButton;
        private Button cancelButton;
        private bool isThresholdUpdating = false;

        // === Callbacks ===
        private EventCallback<ChangeEvent<string>> mappingDropdownCallback;
        private EventCallback<ChangeEvent<string>> colorMapDropdownCallback;
        private EventCallback<ChangeEvent<Vector2>> thresholdSliderCallback;
        private EventCallback<ChangeEvent<double>> thresholdMinFloatFieldCallback;
        private EventCallback<ChangeEvent<double>> thresholdMaxFloatFieldCallback;
        private Action applyButtonCallback;
        private Action cancelButtonCallback;

        // === Events ===
        public Action<ParamRowSettingsController> OnApplySetting;
        public Action OnCancelSetting;

        public SettingsPanelController(Project project, VisualElement root)
        {
            Project = project;
            Root = root;

            Init();
        }

        private void Init()
        {
            Root.RemoveFromClassList("active");

            paramNameLabel = Root.Q<Label>("ParamNameLabel");
            mappingDropdown = Root.Q<VisualElement>("MappingContainer")?.Q<DropdownField>("DropdownField");
            thresholdSlider = Root.Q<VisualElement>("ThresholdSlider")?.Q<MinMaxSlider>("MinMaxSlider");
            thresholdSliderMinFloatField = Root.Q<VisualElement>("ThresholdSlider")?.Q<DoubleField>("MinFloatField");
            thresholdSliderMaxFloatField = Root.Q<VisualElement>("ThresholdSlider")?.Q<DoubleField>("MaxFloatField");
            colorMapDropdown = Root.Q<VisualElement>("ColorMapDropdown")?.Q<DropdownField>("DropdownField");
            scalingDropdown = Root.Q<VisualElement>("ScalingContainer")?.Q<DropdownField>("DropdownField");
            invertMappingToggle = Root.Q<VisualElement>("InvertMappingToggleContainer")?.Q<Toggle>("CheckboxRoot");
            applyButton = Root.Q<VisualElement>("ApplyButton")?.Q<Button>();
            cancelButton = Root.Q<VisualElement>("CancelButton")?.Q<Button>();

            cancelButton.clicked += () =>
            {
                // Debug.Log("CancelButton clicked");
                RenderManager.Instance.SetRenderSettings(ParamRowSettingsController.RenderSettings);
                CloseSettingsPanel();
                OnCancelSetting.Invoke();
            };
            applyButton.clicked += () =>
            {
                // Debug.Log("ApplyButton clicked");
                ParamRowSettingsController = TempParamRowSettingsController;
                RenderManager.Instance.SetRenderSettings(ParamRowSettingsController.RenderSettings);
                CloseSettingsPanel();
                OnApplySetting.Invoke(ParamRowSettingsController);
            };
        }

        private void SetMappingDropdown(string value)
        {
            if (mappingDropdownCallback != null)
            {
                mappingDropdown.UnregisterValueChangedCallback(mappingDropdownCallback);
            }

            mappingDropdown.value = value;

            mappingDropdown.schedule.Execute(() =>
            {
                if (mappingDropdownCallback != null)
                {
                    mappingDropdown.RegisterValueChangedCallback(mappingDropdownCallback);
                }
            }).ExecuteLater(100);
        }


        public void InitSettingsPanel(ParamRowSettingsController paramRowSettingsController)
        {
            UnregisterSettingPanelEvents();

            ParamRowSettingsController = paramRowSettingsController;
            TempParamRowSettingsController = ParamRowSettingsController.Clone() as ParamRowSettingsController;

            paramNameLabel.text = TempParamRowSettingsController.ParamName;
            SetMappingDropdown(TempParamRowSettingsController);

            // RenderManager.Instance.SetRenderSettings(renderSettings);

            MappingType mappingType = TempParamRowSettingsController.RenderSettings.Mapping;
            switch (mappingType)
            {
                case MappingType.None:
                    // Debug.Log("MappingType.None");
                    InitNone(TempParamRowSettingsController);
                    break;

                case MappingType.Opacity:
                    // Debug.Log("MappingType.Opacity");
                    InitOpacity(TempParamRowSettingsController);
                    break;

                case MappingType.Colormap:
                    // Debug.Log("MappingType.Colormap");
                    InitColormap(TempParamRowSettingsController);
                    break;
            }

        }

        private void InitNone(ParamRowSettingsController paramRowSettingsController)
        {
            SetNoneDisplayStyle();
        }

        private void InitOpacity(ParamRowSettingsController paramRowSettingsController)
        {
            SetOpacityDisplayStyle();
            OpacitySettings opacitySettings = paramRowSettingsController.RenderSettings.MappingSettings as OpacitySettings;
        }

        private void InitColormap(ParamRowSettingsController paramRowSettingsController)
        {
            SetColorMapDisplayStyle();

            SetThresholdSlider(paramRowSettingsController);
            SetColorMapDropdown(paramRowSettingsController);
        }

        private void SetMappingDropdown(ParamRowSettingsController paramRowSettingsController)
        {
            if (mappingDropdown is null)
            {
                return;
            }

            if (mappingDropdownCallback is not null)
            {
                mappingDropdown?.UnregisterValueChangedCallback(mappingDropdownCallback);
            }

            mappingDropdownCallback = evt =>
            {
                string mappingTypeValue = evt.newValue;

                if (Enum.TryParse<MappingType>(mappingTypeValue, ignoreCase: true, out var mappingType))
                {
                    switch (mappingType)
                    {
                        case MappingType.None:
                            // Debug.Log("Mapping type: None");
                            paramRowSettingsController.RenderSettings.Mapping = MappingType.None;
                            paramRowSettingsController.RenderSettings.MappingSettings = null;
                            InitNone(paramRowSettingsController);
                            break;

                        case MappingType.Opacity:
                            // Debug.Log("Mapping type: Opacity");
                            paramRowSettingsController.RenderSettings.Mapping = MappingType.Opacity;
                            paramRowSettingsController.RenderSettings.MappingSettings = new OpacitySettings(
                            );
                            InitOpacity(paramRowSettingsController);
                            break;

                        case MappingType.Colormap:
                            // Debug.Log("Mapping type: ColorMap");
                            paramRowSettingsController.RenderSettings.Mapping = MappingType.Colormap;
                            paramRowSettingsController.RenderSettings.MappingSettings = new ColorMapSettings(
                                ColorMapEnum.Autumn,
                                ScalingType.Linear,
                                (float)paramRowSettingsController.Param.ThrMin,
                                (float)paramRowSettingsController.Param.ThrMax,
                                (float)paramRowSettingsController.Param.ThrMinSel,
                                (float)paramRowSettingsController.Param.ThrMaxSel,
                                false
                            );
                            // Debug.Log("Dropdown " + paramRowSettingsController.ParamName + " " + paramRowSettingsController.RenderSettings.Mapping);
                            InitColormap(paramRowSettingsController);
                            break;
                    }
                }
            };

            mappingDropdown.RegisterValueChangedCallback(mappingDropdownCallback);
        }

        private void SetThresholdSlider(ParamRowSettingsController paramRowSettingsController)
        {
            if (thresholdSlider is null || thresholdSliderMinFloatField is null || thresholdSliderMaxFloatField is null)
            {
                return;
            }

            if (thresholdSliderCallback is not null)
            {
                thresholdSlider?.UnregisterValueChangedCallback(thresholdSliderCallback);
            }
            if (thresholdMinFloatFieldCallback is not null)
            {
                thresholdSliderMinFloatField?.UnregisterValueChangedCallback(thresholdMinFloatFieldCallback);
            }
            if (thresholdMaxFloatFieldCallback is not null)
            {
                thresholdSliderMaxFloatField?.UnregisterValueChangedCallback(thresholdMaxFloatFieldCallback);
            }


            ColorMapSettings colorMapSettings = paramRowSettingsController.RenderSettings.MappingSettings as ColorMapSettings;

            thresholdSlider.lowLimit = float.MinValue;
            thresholdSlider.highLimit = float.MaxValue;
            thresholdSlider.lowLimit = colorMapSettings.ThresholdMin;
            thresholdSlider.highLimit = colorMapSettings.ThresholdMax;
            thresholdSlider.minValue = colorMapSettings.ThresholdMinSelected;
            thresholdSlider.maxValue = colorMapSettings.ThresholdMaxSelected;
            thresholdSliderMinFloatField.value = colorMapSettings.ThresholdMinSelected;
            thresholdSliderMaxFloatField.value = colorMapSettings.ThresholdMaxSelected;


            thresholdSliderCallback = evt =>
            {
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                thresholdSliderMinFloatField.value = evt.newValue.x;
                thresholdSliderMaxFloatField.value = evt.newValue.y;
                colorMapSettings.ThresholdMinSelected = thresholdSlider.minValue;
                colorMapSettings.ThresholdMaxSelected = thresholdSlider.maxValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);
                isThresholdUpdating = false;
            };

            thresholdMinFloatFieldCallback = evt =>
            {
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                thresholdSlider.minValue = (float)evt.newValue;
                colorMapSettings.ThresholdMinSelected = thresholdSlider.minValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);
                isThresholdUpdating = false;
            };

            thresholdMaxFloatFieldCallback = evt =>
            {
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                thresholdSlider.maxValue = (float)evt.newValue;
                colorMapSettings.ThresholdMaxSelected = thresholdSlider.maxValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);
                isThresholdUpdating = false;
            };

            thresholdSlider?.RegisterValueChangedCallback(thresholdSliderCallback);
            thresholdSliderMinFloatField?.RegisterValueChangedCallback(thresholdMinFloatFieldCallback);
            thresholdSliderMaxFloatField?.RegisterValueChangedCallback(thresholdMaxFloatFieldCallback);
        }

        private void SetColorMapDropdown(ParamRowSettingsController paramRowSettingsController)
        {
            if (colorMapDropdown is null)
            {
                return;
            }

            if (colorMapDropdownCallback is not null)
            {
                colorMapDropdown?.UnregisterValueChangedCallback(colorMapDropdownCallback);
            }

            ColorMapSettings colorMapSettings = paramRowSettingsController.RenderSettings.MappingSettings as ColorMapSettings;
            ColorMapEnum colorMap = colorMapSettings.ColorMap;

            colorMapDropdown.choices = Enum.GetNames(typeof(ColorMapEnum)).ToList();
            colorMapDropdown.value = colorMap.ToString();

            colorMapDropdownCallback = evt =>
            {
                if (Enum.TryParse<ColorMapEnum>(evt.newValue, out var selectedColorMap))
                {
                    colorMapSettings.ColorMap = selectedColorMap;
                    RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);
                }
            };

            colorMapDropdown.RegisterValueChangedCallback(colorMapDropdownCallback);
        }

        private void SetNoneDisplayStyle()
        {
            VisualElement lineSpacer = Root.Q<VisualElement>("LineSpacer");
            lineSpacer.style.display = DisplayStyle.None;

            VisualElement thresholdSlider = Root.Q<VisualElement>("ThresholdSlider");
            thresholdSlider.style.display = DisplayStyle.None;

            VisualElement colorMapContainer = Root.Q<VisualElement>("ColorMapContainer");
            colorMapContainer.style.display = DisplayStyle.None;

            VisualElement scalingContainer = Root.Q<VisualElement>("ScalingContainer");
            scalingContainer.style.display = DisplayStyle.None;

            VisualElement invertMappingToggleContainer = Root.Q<VisualElement>("InvertMappingToggleContainer");
            invertMappingToggleContainer.style.display = DisplayStyle.None;

            SetMappingDropdown(mappingDropdown.choices[0]);
        }

        private void SetOpacityDisplayStyle()
        {
            VisualElement lineSpacer = Root.Q<VisualElement>("LineSpacer");
            lineSpacer.style.display = DisplayStyle.Flex;

            VisualElement thresholdSlider = Root.Q<VisualElement>("ThresholdSlider");
            thresholdSlider.style.display = DisplayStyle.Flex;

            VisualElement colorMapContainer = Root.Q<VisualElement>("ColorMapContainer");
            colorMapContainer.style.display = DisplayStyle.None;

            VisualElement scalingContainer = Root.Q<VisualElement>("ScalingContainer");
            scalingContainer.style.display = DisplayStyle.Flex;

            VisualElement invertMappingToggleContainer = Root.Q<VisualElement>("InvertMappingToggleContainer");
            invertMappingToggleContainer.style.display = DisplayStyle.Flex;

            SetMappingDropdown(mappingDropdown.choices[1]);
        }

        private void SetColorMapDisplayStyle()
        {
            VisualElement lineSpacer = Root.Q<VisualElement>("LineSpacer");
            lineSpacer.style.display = DisplayStyle.Flex;

            VisualElement thresholdSlider = Root.Q<VisualElement>("ThresholdSlider");
            thresholdSlider.style.display = DisplayStyle.Flex;

            VisualElement colorMapContainer = Root.Q<VisualElement>("ColorMapContainer");
            colorMapContainer.style.display = DisplayStyle.Flex;

            VisualElement scalingContainer = Root.Q<VisualElement>("ScalingContainer");
            scalingContainer.style.display = DisplayStyle.Flex;

            VisualElement invertMappingToggleContainer = Root.Q<VisualElement>("InvertMappingToggleContainer");
            invertMappingToggleContainer.style.display = DisplayStyle.Flex;

            SetMappingDropdown(mappingDropdown.choices[2]);
        }

        public void CloseSettingsPanel()
        {
            UnregisterSettingPanelEvents();
            Root.RemoveFromClassList("active");
        }

        private void UnregisterSettingPanelEvents()
        {
            if (mappingDropdownCallback is not null)
            {
                mappingDropdown?.UnregisterValueChangedCallback(mappingDropdownCallback);
            }
            if (colorMapDropdownCallback is not null)
            {
                colorMapDropdown?.UnregisterValueChangedCallback(colorMapDropdownCallback);
            }
            if (thresholdSliderCallback is not null)
            {
                thresholdSlider?.UnregisterValueChangedCallback(thresholdSliderCallback);
            }
            if (thresholdMinFloatFieldCallback is not null)
            {
                thresholdSliderMinFloatField?.UnregisterValueChangedCallback(thresholdMinFloatFieldCallback);
            }
            if (thresholdMaxFloatFieldCallback is not null)
            {
                thresholdSliderMaxFloatField?.UnregisterValueChangedCallback(thresholdMaxFloatFieldCallback);
            }
            if (applyButtonCallback is not null)
            {
                applyButton.clicked -= applyButtonCallback;
            }
            if (cancelButtonCallback is not null)
            {
                cancelButton.clicked -= cancelButtonCallback;
            }
        }

    }

}