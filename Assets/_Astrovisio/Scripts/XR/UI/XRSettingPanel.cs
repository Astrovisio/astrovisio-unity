using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using TS.DoubleSlider;
using static Astrovisio.SettingsPanelController;
using System.Collections.Generic;
using System;
using CatalogData;
using System.Linq;

namespace Astrovisio
{
    public class XRSettingPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleTMP;

        [Header("Mapping")]
        [SerializeField] private GameObject mappingContainer;
        [SerializeField] private TMP_Dropdown mappingDropdown;

        [Header("Colormap")]
        [SerializeField] private GameObject colormapContainer;
        [SerializeField] private TMP_Dropdown colormapDropdown;

        [Header("Threshold")]
        [SerializeField] private GameObject thresholdContainer;
        [SerializeField] private DoubleSlider thresholdDoubleSlider;

        [Header("Scaling")]
        [SerializeField] private GameObject scalingContainer;
        [SerializeField] private TMP_Dropdown scalingDropdown;

        [Header("Invert")]
        [SerializeField] private GameObject invertContainer;
        [SerializeField] private Toggle invertToggle;

        [Header("Others")]
        [SerializeField] private GameObject line1;
        [SerializeField] private GameObject line2;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button applyButton;



        // === Constants ===
        private const string OPACITY = "Opacity";
        private const string COLORMAP = "Colormap";

        // === Callbacks ===
        private UnityAction<int> mappingDropdownAction;
        private UnityAction<int> colormapDropdownAction;
        private UnityAction<float, float> thresholdDoubleSliderAction;
        private UnityAction<int> scalingDropdownAction;
        private UnityAction<bool> invertToggleAction;
        private UnityAction cancelAction;
        private UnityAction applyAction;

        // === Local ===
        private Project project;
        private File file;
        private Axis axis;
        private Setting originalSetting;
        private Setting tempSetting;
        private SettingMode settingMode;

        private void Start()
        {
            // Colormap
            List<string> colormapNames = Enum.GetNames(typeof(ColorMapEnum)).ToList();
            colormapDropdown.ClearOptions();
            colormapDropdown.AddOptions(colormapNames);
            ColorMapEnum defaultValue = ColorMapEnum.Autumn;
            int idx = colormapNames.IndexOf(defaultValue.ToString());
            colormapDropdown.SetValueWithoutNotify(idx >= 0 ? idx : 0);
            colormapDropdown.RefreshShownValue();
        }

        public void InitAxisSettingsPanel(Project project, File file, Axis axis, Setting setting)
        {
            SettingsManager.Instance.SetSettings(project.Id, file.Id);

            settingMode = SettingMode.Axis;
            this.project = project;
            this.file = file;
            this.axis = axis;

            UnregisterActions();
            SetAxisDisplayStyle();

            originalSetting = setting;
            tempSetting = originalSetting.Clone();
            titleTMP.text = setting.Name;

            SetThresholdSlider();
            SetScalingDropdown();
        }

        public void InitParamSettingsPanel(Project project, File file, Setting setting)
        {
            SettingsManager.Instance.SetSettings(project.Id, file.Id);

            settingMode = SettingMode.Param;
            this.project = project;
            this.file = file;

            UnregisterActions();

            originalSetting = setting;
            tempSetting = originalSetting.Clone();
            titleTMP.text = setting.Name;

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


        public void SetOnApplyAction(UnityAction onApplyAction)
        {
            if (applyAction != null)
            {
                applyButton.onClick.RemoveListener(applyAction);
            }
            applyAction = onApplyAction;
            applyButton.onClick.AddListener(onApplyAction);
        }

        public void SetOnCancelAction(UnityAction onCancelAction)
        {
            if (onCancelAction != null)
            {
                cancelButton.onClick.RemoveListener(onCancelAction);
            }
            cancelAction = onCancelAction;
            cancelButton.onClick.AddListener(onCancelAction);
        }

        // === Setters ===
        private void SetMappingDropdown()
        {
            if (mappingDropdown == null)
            {
                return;
            }

            if (mappingDropdownAction != null)
            {
                mappingDropdown.onValueChanged.RemoveListener(mappingDropdownAction);
                mappingDropdownAction = null;
            }

            List<string> choices = new List<string> { "None", OPACITY, COLORMAP };
            mappingDropdown.options.Clear();
            foreach (string c in choices)
            {
                mappingDropdown.options.Add(new TMP_Dropdown.OptionData(c));
            }

            string currentMapping =
                string.IsNullOrEmpty(tempSetting?.Mapping) ? "None" :
                (string.Equals(tempSetting.Mapping, OPACITY, StringComparison.OrdinalIgnoreCase) ? OPACITY :
                (string.Equals(tempSetting.Mapping, COLORMAP, StringComparison.OrdinalIgnoreCase) ? COLORMAP : "None"));

            int initialIndex = choices.IndexOf(currentMapping);
            if (initialIndex < 0)
            {
                initialIndex = 0;
            }

            mappingDropdown.SetValueWithoutNotify(initialIndex);
            mappingDropdown.RefreshShownValue();

            string prevMappingValue = choices[initialIndex];

            mappingDropdownAction = (int index) =>
            {
                if (project == null || file == null)
                {
                    Debug.LogWarning("[XRSettingPanel] Mapping change ignored: missing project/file.");
                    return;
                }

                string newMappingValue = (index >= 0 && index < choices.Count) ? choices[index] : "None";

                switch (prevMappingValue)
                {
                    case OPACITY:
                        SettingsManager.Instance.RemoveOpacity();
                        break;
                    case COLORMAP:
                        SettingsManager.Instance.RemoveColormap();
                        break;
                }

                SettingsManager.Instance.SetSettings(project.Id, file.Id);

                switch (newMappingValue)
                {
                    case "None":
                        Debug.Log("[XRSettingPanel] Mapping type: None");
                        InitNone(true);
                        break;

                    case OPACITY:
                        Debug.Log("[XRSettingPanel] Mapping type: Opacity");
                        InitOpacity(true);
                        break;

                    case COLORMAP:
                        Debug.Log("[XRSettingPanel] Mapping type: Colormap");
                        InitColormap(true);
                        break;

                    default:
                        Debug.LogWarning($"[XRSettingPanel] Unknown mapping: {newMappingValue}");
                        InitNone(true);
                        break;
                }

                prevMappingValue = newMappingValue;
            };

            mappingDropdown.onValueChanged.AddListener(mappingDropdownAction);
        }


        private void SetMappingDropdownValue(int index)
        {
            if (mappingDropdownAction != null)
            {
                mappingDropdown.onValueChanged.RemoveListener(mappingDropdownAction);
            }

            mappingDropdown.SetValueWithoutNotify(index);

            if (mappingDropdownAction != null)
            {
                mappingDropdown.onValueChanged.AddListener(mappingDropdownAction);
            }
        }

        private void SetThresholdSlider()
        {
            if (tempSetting == null || thresholdDoubleSlider == null)
            {
                return;
            }

            if (thresholdDoubleSliderAction != null)
            {
                thresholdDoubleSlider.OnValueChanged.RemoveListener(thresholdDoubleSliderAction);
                thresholdDoubleSliderAction = null;
            }

            float hardMin = (float)tempSetting.ThrMin;
            float hardMax = (float)tempSetting.ThrMax;

            float selMin = (float)(tempSetting.ThrMinSel ?? tempSetting.ThrMin);
            float selMax = (float)(tempSetting.ThrMaxSel ?? tempSetting.ThrMax);

            if (Mathf.Approximately(hardMin, hardMax))
            {
                thresholdContainer.SetActive(false);
                Debug.LogWarning($"[XRSettingPanel] Threshold disabled: ThrMin == ThrMax ({hardMin}).");
                return;
            }
            else
            {
                thresholdContainer.SetActive(true);
            }

            selMin = Mathf.Clamp(selMin, hardMin, hardMax);
            selMax = Mathf.Clamp(selMax, hardMin, hardMax);
            if (selMin > selMax) (selMin, selMax) = (selMax, selMin);


            thresholdDoubleSlider.Setup(hardMin, hardMax, selMin, selMax);

            thresholdDoubleSliderAction = (float newMin, float newMax) =>
            {
                tempSetting.ThrMinSel = newMin;
                tempSetting.ThrMaxSel = newMax;

                if (settingMode == SettingMode.Axis)
                {
                    SettingsManager.Instance.SetAxisSetting(axis, tempSetting);
                }
                else
                {
                    SettingsManager.Instance.SetParamSetting(tempSetting);
                }
            };

            thresholdDoubleSlider.OnValueChanged.AddListener(thresholdDoubleSliderAction);
        }

        private void SetColormapDropdown()
        {
            if (tempSetting == null || colormapDropdown == null)
            {
                return;
            }
            if (colormapDropdownAction != null)
            {
                colormapDropdown.onValueChanged.RemoveListener(colormapDropdownAction);
            }

            if (!Enum.TryParse<ColorMapEnum>(tempSetting.Colormap, true, out var colormap))
            {
                colormap = ColorMapEnum.Autumn;
                tempSetting.Colormap = colormap.ToString();
            }

            List<string> names = Enum.GetNames(typeof(ColorMapEnum)).ToList();
            colormapDropdown.options.Clear();
            for (int i = 0; i < names.Count; i++)
            {
                colormapDropdown.options.Add(new TMP_Dropdown.OptionData(names[i]));
            }

            int index = names.IndexOf(colormap.ToString());
            if (index < 0)
            {
                index = 0;
            }

            colormapDropdown.SetValueWithoutNotify(index);
            colormapDropdown.RefreshShownValue();

            colormapDropdownAction = (int i) =>
            {
                string selected = colormapDropdown.options[i].text;
                if (Enum.TryParse<ColorMapEnum>(selected, out var selectedColormap))
                {
                    Debug.Log($"Colormap selected: {selected}");
                    tempSetting.Colormap = selectedColormap.ToString();
                    SettingsManager.Instance.SetParamSetting(tempSetting);
                }
            };

            colormapDropdown.onValueChanged.AddListener(colormapDropdownAction);
        }

        private void SetScalingDropdown()
        {
            if (tempSetting == null || scalingDropdown == null)
            {
                return;
            }

            if (scalingDropdownAction != null)
            {
                scalingDropdown.onValueChanged.RemoveListener(scalingDropdownAction);
            }

            var scalingOptions = Enum.GetNames(typeof(ScalingType)).ToList();
            scalingDropdown.options.Clear();
            for (int i = 0; i < scalingOptions.Count; i++)
            {
                scalingDropdown.options.Add(new TMP_Dropdown.OptionData(scalingOptions[i]));
            }

            string current = Enum.TryParse<ScalingType>(tempSetting.Scaling, out var parsed) ? parsed.ToString() : ScalingType.Linear.ToString();
            int selectedIndex = scalingOptions.IndexOf(current);
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            scalingDropdown.SetValueWithoutNotify(selectedIndex);
            scalingDropdown.RefreshShownValue();

            scalingDropdownAction = (int index) =>
            {
                string newValue = scalingDropdown.options[index].text;
                Debug.Log($"Scaling selected: {newValue}");
                if (Enum.TryParse<ScalingType>(newValue, out var selectedType))
                {
                    tempSetting.Scaling = selectedType.ToString();
                }
                else
                {
                    tempSetting.Scaling = ScalingType.Linear.ToString();
                }

                if (settingMode == SettingMode.Axis)
                {
                    SettingsManager.Instance.SetAxisSetting(axis, tempSetting);
                }
                else
                {
                    SettingsManager.Instance.SetParamSetting(tempSetting);
                }
            };

            scalingDropdown.onValueChanged.AddListener(scalingDropdownAction);
        }

        private void SetInverseToggle()
        {
            if (tempSetting is null || invertToggle is null)
            {
                return;
            }
            if (invertToggleAction is not null)
            {
                invertToggle?.onValueChanged.RemoveListener(invertToggleAction);
            }

            invertToggle.SetIsOnWithoutNotify(tempSetting.InvertMapping);
            Debug.Log($"Invert selected: {tempSetting.InvertMapping}");

            invertToggleAction = isOn =>
            {
                Debug.Log($"Invert selected: {isOn}");
                tempSetting.InvertMapping = isOn;
                SettingsManager.Instance.SetParamSetting(tempSetting);
            };

            invertToggle.onValueChanged.AddListener(invertToggleAction);
        }


        // === Styles ===
        [ContextMenu("SetAxisDisplayStyle")]
        public void SetAxisDisplayStyle()
        {
            mappingContainer.SetActive(false);
            line1.SetActive(false);
            colormapContainer.SetActive(false);
            thresholdContainer.SetActive(true);
            scalingContainer.SetActive(true);
            invertContainer.SetActive(false);
            line2.SetActive(true);
        }

        [ContextMenu("SetNoneDisplayStyle")]
        public void SetNoneDisplayStyle()
        {
            mappingContainer.SetActive(true);
            line1.SetActive(false);
            colormapContainer.SetActive(false);
            thresholdContainer.SetActive(false);
            scalingContainer.SetActive(false);
            invertContainer.SetActive(false);
            line2.SetActive(true);
        }

        [ContextMenu("SetOpacityDisplayStyle")]
        public void SetOpacityDisplayStyle()
        {
            mappingContainer.SetActive(true);
            line1.SetActive(true);
            colormapContainer.SetActive(false);
            thresholdContainer.SetActive(true);
            scalingContainer.SetActive(true);
            invertContainer.SetActive(true);
            line2.SetActive(true);
        }

        [ContextMenu("SetColormapDisplayStyle")]
        public void SetColormapDisplayStyle()
        {
            mappingContainer.SetActive(true);
            line1.SetActive(true);
            colormapContainer.SetActive(true);
            thresholdContainer.SetActive(true);
            scalingContainer.SetActive(true);
            invertContainer.SetActive(true);
            line2.SetActive(true);
        }


        public void CloseSettingsPanel()
        {
            UnregisterActions();
            gameObject.SetActive(false);
        }

        private void UnregisterActions()
        {
            if (mappingDropdown != null && mappingDropdownAction != null)
            {
                mappingDropdown.onValueChanged.RemoveListener(mappingDropdownAction);
                mappingDropdownAction = null;
            }

            if (colormapDropdown != null && colormapDropdownAction != null)
            {
                colormapDropdown.onValueChanged.RemoveListener(colormapDropdownAction);
                colormapDropdownAction = null;
            }

            if (thresholdDoubleSlider != null && thresholdDoubleSliderAction != null)
            {
                thresholdDoubleSlider.OnValueChanged.RemoveListener(thresholdDoubleSliderAction);
                thresholdDoubleSliderAction = null;
            }

            if (scalingDropdown != null && scalingDropdownAction != null)
            {
                scalingDropdown.onValueChanged.RemoveListener(scalingDropdownAction);
                scalingDropdownAction = null;
            }

            if (invertToggle != null && invertToggleAction != null)
            {
                invertToggle.onValueChanged.RemoveListener(invertToggleAction);
                invertToggleAction = null;
            }

            if (cancelButton != null && cancelAction != null)
            {
                cancelButton.onClick.RemoveListener(cancelAction);
                cancelAction = null;
            }

            if (applyButton != null && applyAction != null)
            {
                applyButton.onClick.RemoveListener(applyAction);
                applyAction = null;
            }
        }


        public Setting GetTempSetting()
        {
            return tempSetting;
        }

        internal SettingMode GetSettingMode()
        {
            return settingMode;
        }

    }

}
