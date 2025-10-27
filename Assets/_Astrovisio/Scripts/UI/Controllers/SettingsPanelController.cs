/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatalogData;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class SettingsPanelController
    {
        public enum SettingMode { Axis, Param }

        private const string NONE = "None";
        private const string COLORMAP = "Colormap";
        private const string OPACITY = "Opacity";

        // === Dependencies ===
        public Project Project { private set; get; }
        public VisualElement Root { private set; get; }
        public UIContextSO UIContextSO { private set; get; }

        // === Other ===
        private Label paramNameLabel;
        private DropdownField mappingDropdown;
        private MinMaxSlider thresholdSlider;
        private DoubleField thresholdSliderMinFloatField;
        private DoubleField thresholdSliderMaxFloatField;
        private Label thresholdSliderMinWarningLabel;
        private Label thresholdSliderMaxWarningLabel;
        private MinMaxSlider rangeSlider;
        private VisualElement colorMapVisualPreview;
        private DropdownField colormapDropdown;
        private DropdownField scalingDropdown;
        private Toggle invertToggle;
        private Button applyButton;
        private Button cancelButton;
        private bool isThresholdUpdating = false;

        // === Callbacks ===
        private EventCallback<ChangeEvent<string>> mappingDropdownCallback;
        private EventCallback<ChangeEvent<string>> colorMapDropdownCallback;
        private EventCallback<ChangeEvent<string>> scalingDropdownCallback;
        private EventCallback<ChangeEvent<bool>> invertToggleCallback;
        private EventCallback<ChangeEvent<Vector2>> thresholdSliderCallback;
        private EventCallback<ChangeEvent<double>> thresholdMinFloatFieldCallback;
        private EventCallback<ChangeEvent<double>> thresholdMaxFloatFieldCallback;
        private Func<Setting, Task> applyButtonCallback;
        private Action cancelButtonCallback;

        // === Local ===
        private File file;
        private Axis axis;
        private Setting originalSetting;
        private Setting tempSetting;
        private SettingMode settingMode;


        public SettingsPanelController(
            Project project,
            VisualElement root,
            UIContextSO uiContextSO,
            Func<Setting, Task> onApplySetting,
            Action onCancelSetting)
        {
            Project = project;
            Root = root;
            UIContextSO = uiContextSO;

            cancelButtonCallback = onCancelSetting;
            applyButtonCallback = onApplySetting;

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
            thresholdSliderMinWarningLabel = Root.Q<VisualElement>("ThresholdSlider")?.Q<VisualElement>("MinContainer")?.Q<Label>("MinWarningLabel");
            thresholdSliderMinWarningLabel.style.visibility = Visibility.Hidden;
            thresholdSliderMaxWarningLabel = Root.Q<VisualElement>("ThresholdSlider")?.Q<VisualElement>("MaxContainer")?.Q<Label>("MaxWarningLabel");
            thresholdSliderMaxWarningLabel.style.visibility = Visibility.Hidden;
            rangeSlider = Root.Q<VisualElement>("RangeSlider")?.Q<MinMaxSlider>("MinMaxSlider");
            colorMapVisualPreview = Root.Q<VisualElement>("ColorMapContainer")?.Q<VisualElement>("Preview");
            colormapDropdown = Root.Q<VisualElement>("ColorMapDropdown")?.Q<DropdownField>("DropdownField");
            scalingDropdown = Root.Q<VisualElement>("ScalingContainer")?.Q<DropdownField>("DropdownField");
            invertToggle = Root.Q<VisualElement>("InvertMappingToggleContainer")?.Q<Toggle>("CheckboxRoot");
            applyButton = Root.Q<VisualElement>("ApplyButton")?.Q<Button>();
            cancelButton = Root.Q<VisualElement>("CancelButton")?.Q<Button>();

            List<string> mappingChoices = new List<string> { "None", "Opacity", "Colormap" };
            mappingDropdown.choices = mappingChoices;

            cancelButton.clicked += () => cancelButtonCallback();
            applyButton.clicked += () => applyButtonCallback?.Invoke(tempSetting);
        }

        public SettingMode GetSettingMode()
        {
            return settingMode;
        }

        public void InitAxisSettingsPanel(File file, Axis axis, Setting setting)
        {
            SettingsManager.Instance.SetSettings(Project.Id, file.Id);

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
            thresholdSliderMinFloatField.value = (float)(tempSetting.ThrMinSel ?? tempSetting.ThrMin);
            thresholdSliderMaxFloatField.value = (float)(tempSetting.ThrMaxSel ?? tempSetting.ThrMax);

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
                SettingsManager.Instance.SetAxisSetting(axis, tempSetting);
                isThresholdUpdating = false;
            };

            thresholdMinFloatFieldCallback = evt =>
            {
                // Debug.Log("Changing THRESHOLD Min " + evt.newValue);
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                // Debug.Log($"{thresholdSlider.minValue} - {(float)evt.newValue}");
                thresholdSlider.minValue = (float)evt.newValue;
                tempSetting.ThrMinSel = thresholdSlider.minValue;
                // Debug.LogWarning($"{thresholdSlider.minValue} - {tempSetting.ThrMinSel}");

                // Debug.Log($"{(float)evt.newValue} et {thresholdSlider.minValue} = {(float)evt.newValue < thresholdSlider.minValue}");
                if ((float)evt.newValue < thresholdSlider.minValue)
                {
                    thresholdSliderMinWarningLabel.style.visibility = Visibility.Visible;
                }
                else
                {
                    thresholdSliderMinWarningLabel.style.visibility = Visibility.Hidden;
                }

                SettingsManager.Instance.SetAxisSetting(axis, tempSetting);
                isThresholdUpdating = false;
            };

            thresholdMaxFloatFieldCallback = evt =>
            {
                // Debug.Log("Changing THRESHOLD Max " + evt.newValue);
                if (isThresholdUpdating)
                {
                    return;
                }
                isThresholdUpdating = true;
                // Debug.Log($"{thresholdSlider.maxValue} - {(float)evt.newValue}");
                thresholdSlider.maxValue = (float)evt.newValue;
                tempSetting.ThrMaxSel = thresholdSlider.maxValue;
                // Debug.LogWarning($"{thresholdSlider.maxValue} - {tempSetting.ThrMaxSel}");

                // Debug.Log($"{(float)evt.newValue} et {thresholdSlider.maxValue} = {(float)evt.newValue < thresholdSlider.maxValue}");
                if ((float)evt.newValue > thresholdSlider.maxValue)
                {
                    thresholdSliderMaxWarningLabel.style.visibility = Visibility.Visible;
                }
                else
                {
                    thresholdSliderMaxWarningLabel.style.visibility = Visibility.Hidden;
                }

                SettingsManager.Instance.SetAxisSetting(axis, tempSetting);
                isThresholdUpdating = false;
            };

            thresholdSlider?.RegisterValueChangedCallback(thresholdSliderCallback);
            thresholdSliderMinFloatField?.RegisterValueChangedCallback(thresholdMinFloatFieldCallback);
            thresholdSliderMaxFloatField?.RegisterValueChangedCallback(thresholdMaxFloatFieldCallback);


            // SCALING
            List<string> scalingOptions = Enum.GetNames(typeof(ScalingType)).ToList();
            scalingDropdown.choices = scalingOptions;
            tempSetting.Scaling = Enum.TryParse<ScalingType>(tempSetting.Scaling, out var selectedType) ? selectedType.ToString() : ScalingType.Linear.ToString();
            scalingDropdown.value = tempSetting.Scaling;
            scalingDropdownCallback = evt =>
            {
                // Debug.Log("Changing SCALING " + evt.newValue);
                if (Enum.TryParse<ScalingType>(evt.newValue, out var selectedType))
                {
                    tempSetting.Scaling = selectedType.ToString();
                    SettingsManager.Instance.SetAxisSetting(axis, tempSetting);
                }
                else
                {
                    tempSetting.Scaling = ScalingType.Linear.ToString();
                }
            };
            scalingDropdown?.RegisterCallback(scalingDropdownCallback);
        }

        public void InitParamSettingsPanel(File file, Setting setting)
        {
            SettingsManager.Instance.SetSettings(Project.Id, file.Id);

            settingMode = SettingMode.Param;
            this.file = file;

            UnregisterSettingPanelEvents();


            originalSetting = setting;
            tempSetting = originalSetting.Clone();
            paramNameLabel.text = setting.Name;

            switch (setting.Mapping)
            {
                case null:
                    Debug.Log("InitParamSettingsPanel -> None");
                    InitNone();
                    break;
                case OPACITY:
                    Debug.Log("InitParamSettingsPanel -> Opacity");
                    InitOpacity();
                    break;
                case COLORMAP:
                    Debug.Log("InitParamSettingsPanel -> Colormap");
                    InitColormap();
                    break;
            }
            SetMappingDropdown();
        }

        private void InitNone(bool render = false)
        {
            SetNoneDisplayStyle();

            tempSetting.Mapping = null;

            if (render)
            {
                SettingsManager.Instance.SetParamSetting(tempSetting);
            }
        }

        private void InitOpacity(bool render = false)
        {
            SetOpacityDisplayStyle();

            SetThresholdSlider();
            SetScalingDropdown();
            SetInverseToggle();

            tempSetting.Mapping = OPACITY;

            if (render)
            {
                SettingsManager.Instance.SetParamSetting(tempSetting);
            }
        }

        private void InitColormap(bool render = false)
        {
            SetColormapDisplayStyle();

            SetThresholdSlider();
            SetColormapDropdown();
            SetScalingDropdown();
            SetInverseToggle();

            tempSetting.Mapping = COLORMAP;

            if (render)
            {
                SettingsManager.Instance.SetParamSetting(tempSetting);
            }
        }

        // === Setters ===
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

                SettingsManager.Instance.SetSettings(Project.Id, file.Id);

                switch (newMappingValue)
                {
                    case "None":
                        Debug.Log("Mapping type: None");
                        InitNone(true);
                        break;
                    case "Opacity":
                        Debug.Log("Mapping type: Opacity");
                        InitOpacity(true);
                        break;
                    case "Colormap":
                        Debug.Log("Mapping type: Colormap");
                        InitColormap(true);
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

            // mappingDropdown.value = value;

            mappingDropdown.SetValueWithoutNotify(value);

            if (mappingDropdownCallback != null)
            {
                mappingDropdown.RegisterValueChangedCallback(mappingDropdownCallback);
            }

            // mappingDropdown.schedule.Execute(() =>
            // {
            //     if (mappingDropdownCallback != null)
            //     {
            //         mappingDropdown.RegisterValueChangedCallback(mappingDropdownCallback);
            //     }
            // }).ExecuteLater(100);
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

                if ((float)evt.newValue < thresholdSlider.minValue)
                {
                    thresholdSliderMinWarningLabel.style.visibility = Visibility.Visible;
                }
                else
                {
                    thresholdSliderMinWarningLabel.style.visibility = Visibility.Hidden;
                }

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

                if ((float)evt.newValue > thresholdSlider.maxValue)
                {
                    thresholdSliderMaxWarningLabel.style.visibility = Visibility.Visible;
                }
                else
                {
                    thresholdSliderMaxWarningLabel.style.visibility = Visibility.Hidden;
                }

                SettingsManager.Instance.SetParamSetting(tempSetting);
                isThresholdUpdating = false;
            };

            thresholdSlider?.RegisterValueChangedCallback(thresholdSliderCallback);
            thresholdSliderMinFloatField?.RegisterValueChangedCallback(thresholdMinFloatFieldCallback);
            thresholdSliderMaxFloatField?.RegisterValueChangedCallback(thresholdMaxFloatFieldCallback);
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


            if (!Enum.TryParse<ColorMapEnum>(tempSetting.Colormap, true, out var colormap))
            {
                Debug.LogWarning($"[SetColormapDropdown] Invalid colormap '{tempSetting?.Colormap}'. Using default: {colormap}");
                colormap = ColorMapEnum.Autumn;
                tempSetting.Colormap = colormap.ToString();
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
            tempSetting.Scaling = Enum.TryParse<ScalingType>(tempSetting.Scaling, out var selectedType) ? selectedType.ToString() : ScalingType.Linear.ToString();
            scalingDropdown.value = tempSetting.Scaling;
            scalingDropdownCallback = evt =>
            {
                // Debug.Log("Changing SCALING " + evt.newValue);
                if (Enum.TryParse<ScalingType>(evt.newValue, out var selectedType))
                {
                    tempSetting.Scaling = selectedType.ToString();
                    if (settingMode == SettingMode.Axis)
                    {
                        SettingsManager.Instance.SetAxisSetting(axis, tempSetting);
                    }
                    else
                    {
                        SettingsManager.Instance.SetParamSetting(tempSetting);
                    }
                }
                else
                {
                    tempSetting.Scaling = ScalingType.Linear.ToString();
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
                // Debug.Log($"Invert toggled: {evt.newValue}");
                tempSetting.InvertMapping = evt.newValue;
                SettingsManager.Instance.SetParamSetting(tempSetting);
            };

            invertToggle.RegisterValueChangedCallback(invertToggleCallback);
        }

        // === Styles ===
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

        private void SetColormapDisplayStyle()
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
                applyButton.clicked -= () => applyButtonCallback?.Invoke(tempSetting);
            }
            if (cancelButtonCallback is not null)
            {
                cancelButton.clicked -= cancelButtonCallback;
            }
        }

    }

}