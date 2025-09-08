using System;
using System.Collections;
using System.Data;
using System.Linq;
using CatalogData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class SettingsPanelController
    {

        // === Dependencies ===
        public Project Project { private set; get; }
        public VisualElement Root { private set; get; }
        public UIContextSO UIContextSO { private set; get; }

        // === Other ===
        public AxisRowSettingsController AxisRowSettingsController { private set; get; }
        public AxisRowSettingsController TempAxisRowSettingsController { private set; get; }
        public ParamRowSettingsController ParamRowSettingsController { private set; get; }
        public ParamRowSettingsController TempParamRowSettingsController { private set; get; }
        private Label paramNameLabel;
        private DropdownField mappingDropdown;
        private MinMaxSlider thresholdSlider;
        private DoubleField thresholdSliderMinFloatField;
        private DoubleField thresholdSliderMaxFloatField;
        private MinMaxSlider rangeSlider;
        private VisualElement colorMapVisualPreview;
        private DropdownField colorMapDropdown;
        private DropdownField scalingDropdown;
        private Toggle invertToggle;
        private Button applyButton;
        private Button cancelButton;
        private EventCallback<ClickEvent> applyCallback;
        private EventCallback<ClickEvent> cancelCallback;
        private bool isThresholdUpdating = false;

        // === Callbacks ===
        private EventCallback<ChangeEvent<string>> mappingDropdownCallback;
        private EventCallback<ChangeEvent<string>> colorMapDropdownCallback;
        private EventCallback<ChangeEvent<string>> scalingDropdownCallback;
        private EventCallback<ChangeEvent<bool>> invertToggleCallback;
        private EventCallback<ChangeEvent<Vector2>> thresholdSliderCallback;
        private EventCallback<ChangeEvent<double>> thresholdMinFloatFieldCallback;
        private EventCallback<ChangeEvent<double>> thresholdMaxFloatFieldCallback;
        private Action applyButtonCallback;
        private Action cancelButtonCallback;

        // === Events ===
        public event Action<AxisRowSettingsController> OnApplyAxisSetting;
        public event Action<ParamRowSettingsController> OnApplyParamSetting;
        public event Action OnCancelSetting;

        public SettingsPanelController(Project project, VisualElement root, UIContextSO uiContextSO)
        {
            Project = project;
            Root = root;
            UIContextSO = uiContextSO;

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
            rangeSlider = Root.Q<VisualElement>("RangeSlider")?.Q<MinMaxSlider>("MinMaxSlider");
            colorMapVisualPreview = Root.Q<VisualElement>("ColorMapContainer")?.Q<VisualElement>("Preview");
            colorMapDropdown = Root.Q<VisualElement>("ColorMapDropdown")?.Q<DropdownField>("DropdownField");
            scalingDropdown = Root.Q<VisualElement>("ScalingContainer")?.Q<DropdownField>("DropdownField");
            invertToggle = Root.Q<VisualElement>("InvertMappingToggleContainer")?.Q<Toggle>("CheckboxRoot");
            applyButton = Root.Q<VisualElement>("ApplyButton")?.Q<Button>();
            cancelButton = Root.Q<VisualElement>("CancelButton")?.Q<Button>();

            // cancelButton.clicked += () =>
            // {
            //     // Debug.Log("CancelButton clicked");
            //     RenderManager.Instance.SetRenderSettings(ParamRowSettingsController.RenderSettings);
            //     CloseSettingsPanel();
            //     OnCancelSetting.Invoke();
            // };
            // applyButton.clicked += () =>
            // {
            //     // Debug.Log("ApplyButton clicked");
            //     ParamRowSettingsController = TempParamRowSettingsController;
            //     RenderManager.Instance.SetRenderSettings(ParamRowSettingsController.RenderSettings);
            //     CloseSettingsPanel();
            //     OnApplySetting.Invoke(ParamRowSettingsController);
            // };

        }


        private void RegisterButtonCallbacks(EventCallback<ClickEvent> newApplyCallback, EventCallback<ClickEvent> newCancelCallback)
        {
            UnregisterButtonCallbacks();

            cancelCallback = newCancelCallback;
            applyCallback = newApplyCallback;

            cancelButton.RegisterCallback(newCancelCallback);
            applyButton.RegisterCallback(newApplyCallback);
        }

        private void UnregisterButtonCallbacks()
        {
            if (cancelCallback != null)
            {
                cancelButton.UnregisterCallback(cancelCallback);
            }

            if (applyCallback != null)
            {
                applyButton.UnregisterCallback(applyCallback);
            }
        }

        public void InitAxisSettingsPanel(AxisRowSettingsController axisRowSettingsController)
        {
            UnregisterSettingPanelEvents();

            AxisRowSettingsController = axisRowSettingsController;
            TempAxisRowSettingsController = AxisRowSettingsController.Clone() as AxisRowSettingsController;

            SetAxisDisplayStyle();

            paramNameLabel.text = TempAxisRowSettingsController.ParamName;

            // Debug.Log($"Get 2: {TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected} {TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected}");

            // THRESHOLD
            thresholdSlider.lowLimit = float.MinValue;
            thresholdSlider.highLimit = float.MaxValue;
            thresholdSlider.lowLimit = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMin;
            thresholdSlider.highLimit = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMax;
            thresholdSlider.minValue = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected;
            thresholdSlider.maxValue = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected;
            thresholdSliderMinFloatField.value = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected;
            thresholdSliderMaxFloatField.value = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected;

            thresholdSliderCallback = evt =>
            {
                // Debug.Log("Changing THRESHOLD " + evt.newValue);
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                thresholdSliderMinFloatField.value = evt.newValue.x;
                thresholdSliderMaxFloatField.value = evt.newValue.y;
                TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected = thresholdSlider.minValue;
                TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected = thresholdSlider.maxValue;
                RenderManager.Instance.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
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
                TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected = thresholdSlider.minValue;
                RenderManager.Instance.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
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
                TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected = thresholdSlider.maxValue;
                RenderManager.Instance.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
                isThresholdUpdating = false;
            };

            thresholdSlider?.RegisterValueChangedCallback(thresholdSliderCallback);
            thresholdSliderMinFloatField?.RegisterValueChangedCallback(thresholdMinFloatFieldCallback);
            thresholdSliderMaxFloatField?.RegisterValueChangedCallback(thresholdMaxFloatFieldCallback);


            // SCALING
            scalingDropdown.value = TempAxisRowSettingsController.AxisRenderSettings.ScalingType.ToString();
            scalingDropdownCallback = evt =>
            {
                // Debug.Log("Changing SCALING " + evt.newValue);
                if (Enum.TryParse<ScalingType>(evt.newValue, out var selectedType))
                {
                    TempAxisRowSettingsController.AxisRenderSettings.ScalingType = selectedType;

                    RenderManager.Instance.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
                }
                else
                {
                    Debug.LogWarning($"Not valid ScalingType: {evt.newValue}");
                }
            };
            scalingDropdown?.RegisterCallback(scalingDropdownCallback);


            EventCallback<ClickEvent> onApply = evt =>
            {
                AxisRowSettingsController = TempAxisRowSettingsController;
                RenderManager.Instance.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
                CloseSettingsPanel();
                OnApplyAxisSetting?.Invoke(AxisRowSettingsController);
            };
            EventCallback<ClickEvent> onCancel = evt =>
            {
                RenderManager.Instance.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
                CloseSettingsPanel();
                OnCancelSetting?.Invoke();
            };

            RegisterButtonCallbacks(onApply, onCancel);
        }

        public void InitParamSettingsPanel(ParamRowSettingsController paramRowSettingsController)
        {
            UnregisterSettingPanelEvents();

            ParamRowSettingsController = paramRowSettingsController;
            TempParamRowSettingsController = ParamRowSettingsController.Clone() as ParamRowSettingsController;

            paramNameLabel.text = TempParamRowSettingsController.ParamName;
            SetMappingDropdown(TempParamRowSettingsController);

            // RenderManager.Instance.SetRenderSettings(renderSettings);

            MappingType mappingType = TempParamRowSettingsController.ParamRenderSettings.Mapping;
            switch (mappingType)
            {
                case MappingType.None:
                    // Debug.Log("MappingType.None");
                    InitNone(TempParamRowSettingsController);
                    break;

                case MappingType.Opacity:
                    // Debug.Log("MappingType.Opacity " + TempParamRowSettingsController.RenderSettings.MappingSettings.ScalingType);
                    InitOpacity(TempParamRowSettingsController);
                    break;

                case MappingType.Colormap:
                    // Debug.Log("MappingType.Colormap");
                    InitColormap(TempParamRowSettingsController);
                    break;
            }

            EventCallback<ClickEvent> onApply = evt =>
            {
                ParamRowSettingsController = TempParamRowSettingsController;
                RenderManager.Instance.SetRenderSettings(ParamRowSettingsController.ParamRenderSettings);
                CloseSettingsPanel();
                OnApplyParamSetting?.Invoke(ParamRowSettingsController);
            };
            EventCallback<ClickEvent> onCancel = evt =>
            {
                RenderManager.Instance.SetRenderSettings(ParamRowSettingsController.ParamRenderSettings);
                CloseSettingsPanel();
                OnCancelSetting?.Invoke();
            };

            RegisterButtonCallbacks(onApply, onCancel);
        }

        private void InitNone(ParamRowSettingsController paramRowSettingsController)
        {
            SetNoneDisplayStyle();

            // RenderSettings renderSettings = paramRowSettingsController.RenderSettings;

            paramRowSettingsController.ParamRenderSettings.Mapping = MappingType.None;
            paramRowSettingsController.ParamRenderSettings.MappingSettings = null;
            RenderManager.Instance.SetRenderSettings(paramRowSettingsController.ParamRenderSettings);

            // switch (renderSettings.Mapping)
            // {
            //     case MappingType.Opacity:
            //         paramRowSettingsController.RenderSettings.Mapping = MappingType.None;
            //         paramRowSettingsController.RenderSettings.MappingSettings = null;
            //         RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);
            //         break;
            //     case MappingType.Colormap:
            //         RenderManager.Instance.re
            //         break;
            // }
        }

        private void InitOpacity(ParamRowSettingsController paramRowSettingsController)
        {
            SetOpacityDisplayStyle();

            // RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);

            SetThresholdSlider(paramRowSettingsController);
            SetRangeSlider(paramRowSettingsController);
            SetScalingDropdown(paramRowSettingsController);
            SetInverseToggle(paramRowSettingsController);
        }

        private void InitColormap(ParamRowSettingsController paramRowSettingsController)
        {
            SetColorMapDisplayStyle();

            // RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);

            SetThresholdSlider(paramRowSettingsController);
            SetColorMapDropdown(paramRowSettingsController);
            SetScalingDropdown(paramRowSettingsController);
            SetInverseToggle(paramRowSettingsController);
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
                string prevValue = evt.previousValue;

                if (Enum.TryParse<MappingType>(prevValue, ignoreCase: true, out var prevMappingType))
                {
                    switch (prevMappingType)
                    {
                        case MappingType.Opacity:
                            // Debug.Log("Removing previous opacity...");
                            RenderManager.Instance.RemoveOpacity();
                            break;
                        case MappingType.Colormap:
                            // Debug.Log("Removing previous colormap...");
                            RenderManager.Instance.RemoveColorMap();
                            break;
                    }
                }

                if (Enum.TryParse<MappingType>(mappingTypeValue, ignoreCase: true, out var mappingType))
                {
                    switch (mappingType)
                    {
                        case MappingType.None:
                            // Debug.Log("Mapping type: None");
                            paramRowSettingsController.ParamRenderSettings.Mapping = MappingType.None;
                            paramRowSettingsController.ParamRenderSettings.MappingSettings = null;
                            InitNone(paramRowSettingsController);
                            break;

                        case MappingType.Opacity:
                            // Debug.Log("Mapping type: Opacity");
                            paramRowSettingsController.ParamRenderSettings.Mapping = MappingType.Opacity;
                            paramRowSettingsController.ParamRenderSettings.MappingSettings = new OpacitySettings(
                                (float)paramRowSettingsController.Param.ThrMin,
                                (float)paramRowSettingsController.Param.ThrMax,
                                (float)paramRowSettingsController.Param.ThrMinSel,
                                (float)paramRowSettingsController.Param.ThrMaxSel,
                                ScalingType.Linear,
                                false
                            );
                            InitOpacity(paramRowSettingsController);
                            break;

                        case MappingType.Colormap:
                            // Debug.Log("Mapping type: ColorMap");
                            paramRowSettingsController.ParamRenderSettings.Mapping = MappingType.Colormap;
                            paramRowSettingsController.ParamRenderSettings.MappingSettings = new ColorMapSettings(
                                ColorMapEnum.Autumn,
                                (float)paramRowSettingsController.Param.ThrMin,
                                (float)paramRowSettingsController.Param.ThrMax,
                                (float)paramRowSettingsController.Param.ThrMinSel,
                                (float)paramRowSettingsController.Param.ThrMaxSel,
                                ScalingType.Linear,
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

        private void SetMappingDropdownValue(string value)
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


            IMappingSettings mappingSettings = paramRowSettingsController.ParamRenderSettings.MappingSettings;

            thresholdSlider.lowLimit = float.MinValue;
            thresholdSlider.highLimit = float.MaxValue;
            thresholdSlider.lowLimit = mappingSettings.ThresholdMin;
            thresholdSlider.highLimit = mappingSettings.ThresholdMax;
            thresholdSlider.minValue = mappingSettings.ThresholdMinSelected;
            thresholdSlider.maxValue = mappingSettings.ThresholdMaxSelected;
            thresholdSliderMinFloatField.value = mappingSettings.ThresholdMinSelected;
            thresholdSliderMaxFloatField.value = mappingSettings.ThresholdMaxSelected;


            thresholdSliderCallback = evt =>
            {
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                thresholdSliderMinFloatField.value = evt.newValue.x;
                thresholdSliderMaxFloatField.value = evt.newValue.y;
                mappingSettings.ThresholdMinSelected = thresholdSlider.minValue;
                mappingSettings.ThresholdMaxSelected = thresholdSlider.maxValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.ParamRenderSettings);
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
                mappingSettings.ThresholdMinSelected = thresholdSlider.minValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.ParamRenderSettings);
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
                mappingSettings.ThresholdMaxSelected = thresholdSlider.maxValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.ParamRenderSettings);
                isThresholdUpdating = false;
            };

            thresholdSlider?.RegisterValueChangedCallback(thresholdSliderCallback);
            thresholdSliderMinFloatField?.RegisterValueChangedCallback(thresholdMinFloatFieldCallback);
            thresholdSliderMaxFloatField?.RegisterValueChangedCallback(thresholdMaxFloatFieldCallback);
        }

        private void SetRangeSlider(ParamRowSettingsController paramRowSettingsController)
        {
            //
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

            ColorMapSettings colorMapSettings = paramRowSettingsController.ParamRenderSettings.MappingSettings as ColorMapSettings;
            ColorMapEnum colorMap = colorMapSettings.ColorMap;

            UpdateColormapPreview(colorMap);

            colorMapDropdown.choices = Enum.GetNames(typeof(ColorMapEnum)).ToList();
            colorMapDropdown.value = colorMap.ToString();

            colorMapDropdownCallback = evt =>
            {
                if (Enum.TryParse<ColorMapEnum>(evt.newValue, out var selectedColorMap))
                {
                    UpdateColormapPreview(selectedColorMap);
                    colorMapSettings.ColorMap = selectedColorMap;
                    RenderManager.Instance.SetRenderSettings(paramRowSettingsController.ParamRenderSettings);
                }
            };

            colorMapDropdown.RegisterValueChangedCallback(colorMapDropdownCallback);
        }

        private void UpdateColormapPreview(ColorMapEnum colorMapEnum)
        {
            if (UIContextSO == null || UIContextSO.colorMapSO == null)
            {
                Debug.LogWarning("[UI] UIContextSO o ColorMapSO non assegnato.");
                return;
            }

            Texture2D texture = UIContextSO.colorMapSO.GetTexture2D(colorMapEnum);

            if (texture != null && colorMapVisualPreview != null)
            {
                colorMapVisualPreview.style.backgroundImage = new StyleBackground(texture);
            }
            else
            {
                Debug.LogWarning($"[UI] Nessuna texture trovata per la colormap: {colorMapEnum}");
            }
        }



        private void SetScalingDropdown(ParamRowSettingsController paramRowSettingsController)
        {
            if (scalingDropdown == null)
                return;

            if (scalingDropdownCallback != null)
            {
                scalingDropdown.UnregisterValueChangedCallback(scalingDropdownCallback);
            }

            IMappingSettings mappingSettings = paramRowSettingsController.ParamRenderSettings.MappingSettings;

            var scalingOptions = Enum.GetNames(typeof(ScalingType)).ToList();
            scalingDropdown.choices = scalingOptions;
            scalingDropdown.value = mappingSettings.ScalingType.ToString();

            scalingDropdownCallback = evt =>
            {
                // Debug.Log($"Scaling type changed: {evt.newValue}");
                if (Enum.TryParse<ScalingType>(evt.newValue, out var newScalingType))
                {
                    mappingSettings.ScalingType = newScalingType;
                    RenderManager.Instance.SetRenderSettings(paramRowSettingsController.ParamRenderSettings);
                }
            };

            scalingDropdown.RegisterValueChangedCallback(scalingDropdownCallback);
        }

        private void SetInverseToggle(ParamRowSettingsController paramRowSettingsController)
        {
            if (invertToggle is null)
            {
                return;
            }

            if (invertToggleCallback is not null)
            {
                invertToggle?.UnregisterValueChangedCallback(invertToggleCallback);
            }


            IMappingSettings mappingSettings = paramRowSettingsController.ParamRenderSettings.MappingSettings;

            invertToggle.value = mappingSettings.Invert;

            invertToggleCallback = evt =>
            {
                Debug.Log($"Invert toggled: {evt.newValue}");
                mappingSettings.Invert = evt.newValue;
                RenderManager.Instance.SetRenderSettings(paramRowSettingsController.ParamRenderSettings);
            };

            invertToggle.RegisterValueChangedCallback(invertToggleCallback);
        }

        private void SetAxisDisplayStyle()
        {
            VisualElement mappingContainer = Root.Q<VisualElement>("MappingContainer");
            mappingContainer.style.display = DisplayStyle.None;

            VisualElement lineSpacer = Root.Q<VisualElement>("LineSpacer");
            lineSpacer.style.display = DisplayStyle.None;

            VisualElement thresholdSlider = Root.Q<VisualElement>("ThresholdSlider");
            thresholdSlider.style.display = DisplayStyle.Flex;

            VisualElement rangeSlider = Root.Q<VisualElement>("RangeSlider");
            rangeSlider.style.display = DisplayStyle.None;

            VisualElement colorMapContainer = Root.Q<VisualElement>("ColorMapContainer");
            colorMapContainer.style.display = DisplayStyle.None;

            VisualElement scalingContainer = Root.Q<VisualElement>("ScalingContainer");
            scalingContainer.style.display = DisplayStyle.None;

            VisualElement invertMappingToggleContainer = Root.Q<VisualElement>("InvertMappingToggleContainer");
            invertMappingToggleContainer.style.display = DisplayStyle.None;
            
        }

        private void SetNoneDisplayStyle()
        {
            VisualElement mappingContainer = Root.Q<VisualElement>("MappingContainer");
            mappingContainer.style.display = DisplayStyle.Flex;

            VisualElement lineSpacer = Root.Q<VisualElement>("LineSpacer");
            lineSpacer.style.display = DisplayStyle.None;

            VisualElement thresholdSlider = Root.Q<VisualElement>("ThresholdSlider");
            thresholdSlider.style.display = DisplayStyle.None;

            VisualElement rangeSlider = Root.Q<VisualElement>("RangeSlider");
            rangeSlider.style.display = DisplayStyle.None;

            VisualElement colorMapContainer = Root.Q<VisualElement>("ColorMapContainer");
            colorMapContainer.style.display = DisplayStyle.None;

            VisualElement scalingContainer = Root.Q<VisualElement>("ScalingContainer");
            scalingContainer.style.display = DisplayStyle.None;

            VisualElement invertMappingToggleContainer = Root.Q<VisualElement>("InvertMappingToggleContainer");
            invertMappingToggleContainer.style.display = DisplayStyle.None;

            SetMappingDropdownValue(mappingDropdown.choices[0]);
        }

        private void SetOpacityDisplayStyle()
        {
            VisualElement mappingContainer = Root.Q<VisualElement>("MappingContainer");
            mappingContainer.style.display = DisplayStyle.Flex;

            VisualElement lineSpacer = Root.Q<VisualElement>("LineSpacer");
            lineSpacer.style.display = DisplayStyle.Flex;

            VisualElement thresholdSlider = Root.Q<VisualElement>("ThresholdSlider");
            thresholdSlider.style.display = DisplayStyle.Flex;

            VisualElement rangeSlider = Root.Q<VisualElement>("RangeSlider");
            rangeSlider.style.display = DisplayStyle.None;

            VisualElement colorMapContainer = Root.Q<VisualElement>("ColorMapContainer");
            colorMapContainer.style.display = DisplayStyle.None;

            VisualElement scalingContainer = Root.Q<VisualElement>("ScalingContainer");
            scalingContainer.style.display = DisplayStyle.Flex;

            VisualElement invertMappingToggleContainer = Root.Q<VisualElement>("InvertMappingToggleContainer");
            invertMappingToggleContainer.style.display = DisplayStyle.Flex;

            SetMappingDropdownValue(mappingDropdown.choices[1]);
        }

        private void SetColorMapDisplayStyle()
        {
            VisualElement mappingContainer = Root.Q<VisualElement>("MappingContainer");
            mappingContainer.style.display = DisplayStyle.Flex;

            VisualElement lineSpacer = Root.Q<VisualElement>("LineSpacer");
            lineSpacer.style.display = DisplayStyle.Flex;

            VisualElement thresholdSlider = Root.Q<VisualElement>("ThresholdSlider");
            thresholdSlider.style.display = DisplayStyle.Flex;

            VisualElement rangeSlider = Root.Q<VisualElement>("RangeSlider");
            rangeSlider.style.display = DisplayStyle.None;

            VisualElement colorMapContainer = Root.Q<VisualElement>("ColorMapContainer");
            colorMapContainer.style.display = DisplayStyle.Flex;

            VisualElement scalingContainer = Root.Q<VisualElement>("ScalingContainer");
            scalingContainer.style.display = DisplayStyle.Flex;

            VisualElement invertMappingToggleContainer = Root.Q<VisualElement>("InvertMappingToggleContainer");
            invertMappingToggleContainer.style.display = DisplayStyle.Flex;

            SetMappingDropdownValue(mappingDropdown.choices[2]);
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
                // Debug.Log("Unregistered thresholdSliderCallback");
                thresholdSlider?.UnregisterValueChangedCallback(thresholdSliderCallback);
            }
            if (thresholdMinFloatFieldCallback is not null)
            {
                // Debug.Log("Unregistered thresholdMinFloatFieldCallback");
                thresholdSliderMinFloatField?.UnregisterValueChangedCallback(thresholdMinFloatFieldCallback);
            }
            if (thresholdMaxFloatFieldCallback is not null)
            {
                // Debug.Log("Unregistered thresholdMaxFloatFieldCallback");
                thresholdSliderMaxFloatField?.UnregisterValueChangedCallback(thresholdMaxFloatFieldCallback);
            }
            if (scalingDropdownCallback is not null)
            {
                scalingDropdown?.UnregisterValueChangedCallback(scalingDropdownCallback);
            }
            if (invertToggleCallback is not null)
            {
                invertToggle?.UnregisterValueChangedCallback(invertToggleCallback);
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