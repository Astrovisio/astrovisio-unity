using System;
using System.Collections;
using System.Collections.Generic;
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
        private enum SettingMode { Axis, Param }

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
        private DropdownField colormapDropdown;
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
        public event Action<Setting> OnApplyAxisSetting;
        public event Action<Setting> OnApplyParamSetting;
        public event Action OnCancelSetting;



        // === NEW ===
        private File file;
        private Axis axis;
        private Setting originalSetting;
        private Setting tempSetting;
        private SettingMode settingMode;




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
            colormapDropdown = Root.Q<VisualElement>("ColorMapDropdown")?.Q<DropdownField>("DropdownField");
            scalingDropdown = Root.Q<VisualElement>("ScalingContainer")?.Q<DropdownField>("DropdownField");
            invertToggle = Root.Q<VisualElement>("InvertMappingToggleContainer")?.Q<Toggle>("CheckboxRoot");
            applyButton = Root.Q<VisualElement>("ApplyButton")?.Q<Button>();
            cancelButton = Root.Q<VisualElement>("CancelButton")?.Q<Button>();

            List<string> mappingChoices = new List<string> { "None", "Opacity", "Colormap" };
            mappingDropdown.choices = mappingChoices;

            cancelButton.clicked += () =>
            {
                // Debug.Log("CancelButton clicked");
                if (settingMode == SettingMode.Axis)
                {
                    SettingsManager.Instance.SetAxisSetting(axis, originalSetting);
                }
                else
                {
                    SettingsManager.Instance.SetParamSetting(originalSetting);
                }
                // RenderManager.Instance.SetRenderSettings(originalSetting);
                CloseSettingsPanel();

                OnCancelSetting.Invoke();
            };
            applyButton.clicked += () =>
            {
                // Debug.Log("ApplyButton clicked");
                // ParamRowSettingsController = TempParamRowSettingsController;
                // SettingsManager.Instance.SetSetting();
                // RenderManager.Instance.SetRenderSettings(ParamRowSettingsController.RenderSettings);

                // TODO: Make API CALL...


                file = null;
                originalSetting = null;
                tempSetting = null;
                CloseSettingsPanel();
                OnApplyAxisSetting.Invoke(tempSetting);
            };
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

        // public void InitAxisSettingsPanel(AxisRowSettingsController axisRowSettingsController)
        // {
        //     UnregisterSettingPanelEvents();

        //     AxisRowSettingsController = axisRowSettingsController;
        //     TempAxisRowSettingsController = AxisRowSettingsController.Clone() as AxisRowSettingsController;

        //     SetAxisDisplayStyle();

        //     paramNameLabel.text = TempAxisRowSettingsController.Variable.Name;

        //     // Debug.Log($"Get 2: {TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected} {TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected}");

        //     // THRESHOLD
        //     thresholdSlider.lowLimit = float.MinValue;
        //     thresholdSlider.highLimit = float.MaxValue;
        //     thresholdSlider.lowLimit = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMin;
        //     thresholdSlider.highLimit = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMax;
        //     thresholdSlider.minValue = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected;
        //     thresholdSlider.maxValue = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected;
        //     thresholdSliderMinFloatField.value = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected;
        //     thresholdSliderMaxFloatField.value = TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected;

        //     thresholdSliderCallback = evt =>
        //     {
        //         // Debug.Log("Changing THRESHOLD " + evt.newValue);
        //         if (isThresholdUpdating)
        //         {
        //             return;
        //         }
        //         isThresholdUpdating = true;
        //         thresholdSliderMinFloatField.value = evt.newValue.x;
        //         thresholdSliderMaxFloatField.value = evt.newValue.y;
        //         TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected = thresholdSlider.minValue;
        //         TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected = thresholdSlider.maxValue;
        //         RenderManager.Instance.RenderSettingsController.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
        //         isThresholdUpdating = false;
        //     };

        //     thresholdMinFloatFieldCallback = evt =>
        //     {
        //         if (isThresholdUpdating)
        //         {
        //             return;
        //         }
        //         isThresholdUpdating = true;
        //         thresholdSlider.minValue = (float)evt.newValue;
        //         TempAxisRowSettingsController.AxisRenderSettings.ThresholdMinSelected = thresholdSlider.minValue;
        //         RenderManager.Instance.RenderSettingsController.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
        //         isThresholdUpdating = false;
        //     };

        //     thresholdMaxFloatFieldCallback = evt =>
        //     {
        //         if (isThresholdUpdating)
        //         {
        //             return;
        //         }
        //         isThresholdUpdating = true;
        //         thresholdSlider.maxValue = (float)evt.newValue;
        //         TempAxisRowSettingsController.AxisRenderSettings.ThresholdMaxSelected = thresholdSlider.maxValue;
        //         RenderManager.Instance.RenderSettingsController.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
        //         isThresholdUpdating = false;
        //     };

        //     thresholdSlider?.RegisterValueChangedCallback(thresholdSliderCallback);
        //     thresholdSliderMinFloatField?.RegisterValueChangedCallback(thresholdMinFloatFieldCallback);
        //     thresholdSliderMaxFloatField?.RegisterValueChangedCallback(thresholdMaxFloatFieldCallback);


        //     // SCALING
        //     scalingDropdown.value = TempAxisRowSettingsController.AxisRenderSettings.ScalingType.ToString();
        //     scalingDropdownCallback = evt =>
        //     {
        //         // Debug.Log("Changing SCALING " + evt.newValue);
        //         if (Enum.TryParse<ScalingType>(evt.newValue, out var selectedType))
        //         {
        //             TempAxisRowSettingsController.AxisRenderSettings.ScalingType = selectedType;

        //             RenderManager.Instance.RenderSettingsController.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
        //         }
        //         else
        //         {
        //             Debug.LogWarning($"Not valid ScalingType: {evt.newValue}");
        //         }
        //     };
        //     scalingDropdown?.RegisterCallback(scalingDropdownCallback);


        //     EventCallback<ClickEvent> onApply = evt =>
        //     {
        //         AxisRowSettingsController = TempAxisRowSettingsController;
        //         RenderManager.Instance.RenderSettingsController.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
        //         CloseSettingsPanel();
        //         OnApplyAxisSetting?.Invoke(AxisRowSettingsController);
        //     };
        //     EventCallback<ClickEvent> onCancel = evt =>
        //     {
        //         RenderManager.Instance.RenderSettingsController.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
        //         CloseSettingsPanel();
        //         OnCancelSetting?.Invoke();
        //     };

        //     RegisterButtonCallbacks(onApply, onCancel);
        // }

        // public void InitParamSettingsPanel(Setting setting)
        // {
        //     UnregisterSettingPanelEvents();

        //     ParamRowSettingsController = paramRowSettingsController;
        //     TempParamRowSettingsController = ParamRowSettingsController.Clone() as ParamRowSettingsController;

        //     paramNameLabel.text = TempParamRowSettingsController.Variable.Name;
        //     SetMappingDropdown(TempParamRowSettingsController);

        //     // RenderManager.Instance.SetRenderSettings(renderSettings);

        //     MappingType mappingType = TempParamRowSettingsController.ParamRenderSettings.Mapping;
        //     switch (mappingType)
        //     {
        //         case MappingType.None:
        //             // Debug.Log("MappingType.None");
        //             InitNone(TempParamRowSettingsController);
        //             break;

        //         case MappingType.Opacity:
        //             // Debug.Log("MappingType.Opacity " + TempParamRowSettingsController.RenderSettings.MappingSettings.ScalingType);
        //             InitOpacity(TempParamRowSettingsController);
        //             break;

        //         case MappingType.Colormap:
        //             // Debug.Log("MappingType.Colormap");
        //             InitColormap(TempParamRowSettingsController);
        //             break;
        //     }

        //     EventCallback<ClickEvent> onApply = evt =>
        //     {
        //         ParamRowSettingsController = TempParamRowSettingsController;
        //         RenderManager.Instance.RenderSettingsController.SetRenderSettings(ParamRowSettingsController.ParamRenderSettings);
        //         CloseSettingsPanel();
        //         OnApplyParamSetting?.Invoke(ParamRowSettingsController);
        //     };
        //     EventCallback<ClickEvent> onCancel = evt =>
        //     {
        //         RenderManager.Instance.RenderSettingsController.SetRenderSettings(ParamRowSettingsController.ParamRenderSettings);
        //         CloseSettingsPanel();
        //         OnCancelSetting?.Invoke();
        //     };

        //     RegisterButtonCallbacks(onApply, onCancel);
        // }


        public void InitAxisSettingsPanel(File file, Axis axis, Setting setting)
        {
            settingMode = SettingMode.Axis;
            this.file = file;
            this.axis = axis;

            UnregisterSettingPanelEvents();
            SetAxisDisplayStyle();


            originalSetting = setting;
            tempSetting = originalSetting.Clone();
            paramNameLabel.text = setting.Name;

            // THRESHOLD
            thresholdSlider.lowLimit = float.MinValue;
            thresholdSlider.highLimit = float.MaxValue;
            thresholdSlider.lowLimit = (float)tempSetting.ThrMin;
            thresholdSlider.highLimit = (float)tempSetting.ThrMax;
            thresholdSlider.minValue = (float)(tempSetting.ThrMinSel ?? tempSetting.ThrMin);
            thresholdSlider.maxValue = (float)(tempSetting.ThrMaxSel ?? tempSetting.ThrMax);
            thresholdSliderMinFloatField.value = thresholdSlider.minValue;
            thresholdSliderMaxFloatField.value = thresholdSlider.maxValue;

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
                tempSetting.ThrMinSel = thresholdSlider.minValue;
                tempSetting.ThrMaxSel = thresholdSlider.maxValue;
                RenderManager.Instance.RenderSettingsController.SetAxisSettings(axis, tempSetting);
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
                tempSetting.ThrMinSel = thresholdSlider.minValue;
                RenderManager.Instance.RenderSettingsController.SetAxisSettings(axis, tempSetting);
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
                tempSetting.ThrMaxSel = thresholdSlider.maxValue;
                RenderManager.Instance.RenderSettingsController.SetAxisSettings(axis, tempSetting);
                isThresholdUpdating = false;
            };

            thresholdSlider?.RegisterValueChangedCallback(thresholdSliderCallback);
            thresholdSliderMinFloatField?.RegisterValueChangedCallback(thresholdMinFloatFieldCallback);
            thresholdSliderMaxFloatField?.RegisterValueChangedCallback(thresholdMaxFloatFieldCallback);


            // SCALING
            List<string> scalingOptions = Enum.GetNames(typeof(ScalingType)).ToList();
            scalingDropdown.choices = scalingOptions;
            scalingDropdown.value = tempSetting.Scaling;
            scalingDropdownCallback = evt =>
            {
                // Debug.Log("Changing SCALING " + evt.newValue);
                if (Enum.TryParse<ScalingType>(evt.newValue, out var selectedType))
                {
                    tempSetting.Scaling = selectedType.ToString();
                    RenderManager.Instance.RenderSettingsController.SetAxisSettings(axis, tempSetting);
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
                RenderManager.Instance.RenderSettingsController.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
                CloseSettingsPanel();
                OnApplyAxisSetting?.Invoke(tempSetting);
            };
            EventCallback<ClickEvent> onCancel = evt =>
            {
                RenderManager.Instance.RenderSettingsController.SetAxisSettings(TempAxisRowSettingsController.AxisRenderSettings);
                CloseSettingsPanel();
                OnCancelSetting?.Invoke();
            };

            RegisterButtonCallbacks(onApply, onCancel);
        }

        public void InitParamSettingsPanel(File file, Setting setting)
        {
            settingMode = SettingMode.Param;
            this.file = file;

            UnregisterSettingPanelEvents();


            originalSetting = setting;
            tempSetting = originalSetting.Clone();
            paramNameLabel.text = setting.Name;

            switch (setting.Mapping)
            {
                case null:
                    InitNone();
                    break;

                case "Opacity":
                    InitOpacity(tempSetting);
                    break;

                case "Colormap":
                    InitColormap(tempSetting);
                    break;
            }

            EventCallback<ClickEvent> onApply = evt =>
            {
                ParamRowSettingsController = TempParamRowSettingsController;
                RenderManager.Instance.RenderSettingsController.SetRenderSettings(ParamRowSettingsController.ParamRenderSettings);
                CloseSettingsPanel();
                OnApplyParamSetting?.Invoke(tempSetting);
            };
            EventCallback<ClickEvent> onCancel = evt =>
            {
                RenderManager.Instance.RenderSettingsController.SetRenderSettings(ParamRowSettingsController.ParamRenderSettings);
                CloseSettingsPanel();
                OnCancelSetting?.Invoke();
            };

            RegisterButtonCallbacks(onApply, onCancel);
        }

        private void InitNone()
        {
            SetNoneDisplayStyle();

            tempSetting.Mapping = null;
            SettingsManager.Instance.SetParamSetting(tempSetting);
        }

        private void InitOpacity(Setting setting)
        {
            SetOpacityDisplayStyle();

            // RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);

            SetThresholdSlider();
            SetRangeSlider();
            SetScalingDropdown();
            SetInverseToggle();
        }

        private void InitColormap(Setting setting)
        {
            SetColorMapDisplayStyle();

            // RenderManager.Instance.SetRenderSettings(paramRowSettingsController.RenderSettings);

            SetThresholdSlider();
            SetColormapDropdown();
            SetScalingDropdown();
            SetInverseToggle();
        }






        private void SetMappingDropdown()
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
                string prevMappingValue = evt.previousValue;
                string newMappingValue = evt.newValue;

                switch (prevMappingValue)
                {
                    case "None":
                        break;
                    case "Opacity":
                        SettingsManager.Instance.RemoveOpacity();
                        break;
                    case "Colormap":
                        SettingsManager.Instance.RemoveColormap();
                        break;
                    default:
                        break;
                }

                switch (newMappingValue)
                {
                    case "None":
                        // Debug.Log("Mapping type: None");
                        InitNone();
                        break;
                    case "Opacity":
                        // Debug.Log("Mapping type: Opacity");
                        InitOpacity(tempSetting);
                        break;
                    case "Colormap":
                        // Debug.Log("Mapping type: Colormap");
                        InitColormap(tempSetting);
                        break;
                    default:
                        break;
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

        private void SetThresholdSlider()
        {
            if (tempSetting is null)
            {
                return;
            }
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

            // THRESHOLD
            thresholdSlider.lowLimit = float.MinValue;
            thresholdSlider.highLimit = float.MaxValue;
            thresholdSlider.lowLimit = (float)tempSetting.ThrMin;
            thresholdSlider.highLimit = (float)tempSetting.ThrMax;
            thresholdSlider.minValue = (float)(tempSetting.ThrMinSel ?? tempSetting.ThrMin);
            thresholdSlider.maxValue = (float)(tempSetting.ThrMaxSel ?? tempSetting.ThrMax);
            thresholdSliderMinFloatField.value = thresholdSlider.minValue;
            thresholdSliderMaxFloatField.value = thresholdSlider.maxValue;

            thresholdSliderCallback = evt =>
            {
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                thresholdSliderMinFloatField.value = evt.newValue.x;
                thresholdSliderMaxFloatField.value = evt.newValue.y;
                tempSetting.ThrMinSel = thresholdSlider.minValue;
                tempSetting.ThrMaxSel = thresholdSlider.maxValue;
                SettingsManager.Instance.SetParamSetting(tempSetting);
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
                tempSetting.ThrMinSel = thresholdSlider.minValue;
                SettingsManager.Instance.SetParamSetting(tempSetting);
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
                tempSetting.ThrMaxSel = thresholdSlider.maxValue;
                SettingsManager.Instance.SetParamSetting(tempSetting);
                isThresholdUpdating = false;
            };

            thresholdSlider?.RegisterValueChangedCallback(thresholdSliderCallback);
            thresholdSliderMinFloatField?.RegisterValueChangedCallback(thresholdMinFloatFieldCallback);
            thresholdSliderMaxFloatField?.RegisterValueChangedCallback(thresholdMaxFloatFieldCallback);
        }

        private void SetRangeSlider()
        {
            //
        }

        private void SetColormapDropdown()
        {
            if (tempSetting is null)
            {
                return;
            }
            if (colormapDropdown is null)
            {
                return;
            }
            if (colorMapDropdownCallback is not null)
            {
                colormapDropdown?.UnregisterValueChangedCallback(colorMapDropdownCallback);
            }


            if (!Enum.TryParse<ColorMapEnum>(tempSetting.Colormap, true, out ColorMapEnum colormap))
            {
                colormap = ColorMapEnum.Accent;
                Debug.LogWarning($"[SetColormapDropdown] Invalid colormap '{tempSetting?.Colormap}'. Using default: {colormap}");
            }

            UpdateColormapPreview(colormap);

            colormapDropdown.choices = Enum.GetNames(typeof(ColorMapEnum)).ToList();
            colormapDropdown.SetValueWithoutNotify(colormap.ToString());
            colorMapDropdownCallback = evt =>
            {
                if (Enum.TryParse<ColorMapEnum>(evt.newValue, out var selectedColormap))
                {
                    UpdateColormapPreview(selectedColormap);
                    tempSetting.Colormap = selectedColormap.ToString();
                    SettingsManager.Instance.SetParamSetting(tempSetting);
                }
            };

            colormapDropdown.RegisterValueChangedCallback(colorMapDropdownCallback);
        }

        private void UpdateColormapPreview(ColorMapEnum colorMapEnum)
        {
            if (UIContextSO == null || UIContextSO.colorMapSO == null)
            {
                Debug.LogWarning("[UI] UIContextSO or ColorMapSO not assigned.");
                return;
            }

            Texture2D texture = UIContextSO.colorMapSO.GetTexture2D(colorMapEnum);

            if (texture != null && colorMapVisualPreview != null)
            {
                colorMapVisualPreview.style.backgroundImage = new StyleBackground(texture);
            }
            else
            {
                Debug.LogWarning($"[UI] No texture found for colormap: {colorMapEnum}");
            }
        }

        private void SetScalingDropdown()
        {
            if (tempSetting is null)
            {
                return;
            }
            if (scalingDropdown == null)
            {
                return;
            }
            if (scalingDropdownCallback != null)
            {
                scalingDropdown.UnregisterValueChangedCallback(scalingDropdownCallback);
            }

            List<string> scalingOptions = Enum.GetNames(typeof(ScalingType)).ToList();
            scalingDropdown.choices = scalingOptions;
            scalingDropdown.value = tempSetting.ToString();
            scalingDropdownCallback = evt =>
            {
                // Debug.Log("Changing SCALING " + evt.newValue);
                if (Enum.TryParse<ScalingType>(evt.newValue, out var selectedType))
                {
                    tempSetting.Scaling = selectedType.ToString();
                    RenderManager.Instance.RenderSettingsController.SetAxisSettings(axis, tempSetting);
                }
                else
                {
                    Debug.LogWarning($"Not valid ScalingType: {evt.newValue}");
                }
            };
            scalingDropdown.RegisterValueChangedCallback(scalingDropdownCallback);
        }

        private void SetInverseToggle()
        {
            if (tempSetting is null)
            {
                return;
            }
            if (invertToggle is null)
            {
                return;
            }
            if (invertToggleCallback is not null)
            {
                invertToggle?.UnregisterValueChangedCallback(invertToggleCallback);
            }

            invertToggle.value = tempSetting.InvertMapping;

            invertToggleCallback = evt =>
            {
                Debug.Log($"Invert toggled: {evt.newValue}");
                tempSetting.InvertMapping = evt.newValue;
                SettingsManager.Instance.SetParamSetting(tempSetting);
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
            scalingContainer.style.display = DisplayStyle.Flex;

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
                colormapDropdown?.UnregisterValueChangedCallback(colorMapDropdownCallback);
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