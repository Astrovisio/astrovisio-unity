using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public enum Axis { X, Y, Z }

    public enum Threshold { Min, Max }

    public class ParamRowController
    {
        // === Dependencies ===
        public VisualElement Root { get; }

        // === Events ===
        public event Action<Axis?, ParamRowController> OnAxisChanged;
        public event Action<Threshold, ParamRowController> OnThresholdChanged;
        public event Action OnStateChanged;

        // === UI ===
        private Button rootButton;
        private VisualElement nameContainer;
        private Label nameLabel;
        private Label filesLabel;
        private VisualElement xyzAxisContainer;
        private Button xChipButton;
        private Button yChipButton;
        private Button zChipButton;
        private VisualElement thresholdContainer;
        private DoubleField minInputField;
        private DoubleField maxInputField;
        private Label minErrorLabel;
        private Label maxErrorLabel;
        private Button resetButton;
        private Toggle checkbox;

        // === Data ===
        public string ParamName { get; set; }
        public ConfigParam Param { get; set; }

        public ParamRowController(VisualElement root, string paramName, ConfigParam param)
        {
            Root = root;
            ParamName = paramName;
            Param = param;

            Init();
        }

        private void Init()
        {
            rootButton = Root.Q<Button>("RootButton");

            // Name
            nameContainer = Root.Q<VisualElement>("NameContainer");
            nameLabel = nameContainer.Q<Label>("Label");

            // XYZ Axes
            xyzAxisContainer = Root.Q<VisualElement>("XYZAxisContainer");
            xChipButton = xyzAxisContainer.Q<VisualElement>("ChipX")?.Q<Button>("ChipRoot");
            yChipButton = xyzAxisContainer.Q<VisualElement>("ChipY")?.Q<Button>("ChipRoot");
            zChipButton = xyzAxisContainer.Q<VisualElement>("ChipZ")?.Q<Button>("ChipRoot");
            xChipButton?.RegisterCallback<ClickEvent>(_ => HandleAxisChipClick(Axis.X));
            yChipButton?.RegisterCallback<ClickEvent>(_ => HandleAxisChipClick(Axis.Y));
            zChipButton?.RegisterCallback<ClickEvent>(_ => HandleAxisChipClick(Axis.Z));
            InitAxis();

            // Threshold
            thresholdContainer = Root.Q<VisualElement>("ThresholdContainer");
            VisualElement histogramSlider = thresholdContainer.Q<VisualElement>("HistogramSlider");
            HistogramController histogramController = new HistogramController(histogramSlider);
            MinMaxSlider minMaxSlider = histogramSlider.Q<MinMaxSlider>("MinMaxHistogramSlider");
            minInputField = histogramSlider.Q<DoubleField>("MinFloatField");
            maxInputField = histogramSlider.Q<DoubleField>("MaxFloatField");
            // minInputField.SetValueWithoutNotify(double.Parse(minInputField.value.ToString("E5")));
            // maxInputField.SetValueWithoutNotify(double.Parse(maxInputField.value.ToString("E5")));

            minErrorLabel = histogramSlider.Q<Label>("MinErrorLabel");
            maxErrorLabel = histogramSlider.Q<Label>("MaxErrorLabel");
            Param.ThrMinSel = Param.ThrMinSel ?? Param.ThrMin;
            Param.ThrMaxSel = Param.ThrMaxSel ?? Param.ThrMax;
            minInputField.RegisterValueChangedCallback(evt =>
            {
                Param.ThrMinSel = evt.newValue;
                UpdateWarningLabel(Threshold.Min);
                OnThresholdChanged?.Invoke(Threshold.Min, this);
                Debug.Log($"[UI] {ParamName} → ThrMin aggiornato a {evt.newValue}");
            });
            maxInputField.RegisterValueChangedCallback(evt =>
            {
                Param.ThrMaxSel = evt.newValue;
                UpdateWarningLabel(Threshold.Max);
                OnThresholdChanged?.Invoke(Threshold.Max, this);
                Debug.Log($"[UI] {ParamName} → ThrMax aggiornato a {evt.newValue}");
            });
            resetButton = thresholdContainer.Q<Button>("ResetButton");
            resetButton.clicked += OnResetButtonClicked;
            InitThresholds();

            // Select
            checkbox = Root.Q<Toggle>("CheckboxRoot");
            checkbox?.RegisterValueChangedCallback(evt =>
            {
                Param.Selected = evt.newValue;
                // Debug.Log($"Checkbox toggled for {ParamName}: {Param.Selected}");
                SetSelected(Param.Selected);
            });
            SetSelected(Param.Selected);

        }

        private void InitAxis()
        {
            if (Param.XAxis)
            {
                HandleAxisChipClick(Axis.X);
            }
            if (Param.YAxis)
            {
                HandleAxisChipClick(Axis.Y);
            }
            if (Param.ZAxis)
            {
                HandleAxisChipClick(Axis.Z);
            }
        }

        private void HandleAxisChipClick(Axis axis)
        {
            // Debug.Log($"[UI] Axis chip clicked: {clickedAxis} for {ParamName}");

            switch (axis)
            {
                case Axis.X:
                    if (xChipButton.ClassListContains("active"))
                    {
                        xChipButton.RemoveFromClassList("active");
                        yChipButton.RemoveFromClassList("active");
                        zChipButton.RemoveFromClassList("active");
                        OnAxisChanged?.Invoke(null, this);
                        // Debug.Log($"[UI] Deactivating all axis for {ParamName} (axis {clickedAxis} was already active)");
                    }
                    else
                    {
                        yChipButton.RemoveFromClassList("active");
                        zChipButton.RemoveFromClassList("active");
                        xChipButton.AddToClassList("active");
                        OnAxisChanged?.Invoke(Axis.X, this);
                        // Debug.Log($"[UI] Activating axis {clickedAxis} for {ParamName}");
                    }
                    break;
                case Axis.Y:
                    if (yChipButton.ClassListContains("active"))
                    {
                        xChipButton.RemoveFromClassList("active");
                        yChipButton.RemoveFromClassList("active");
                        zChipButton.RemoveFromClassList("active");
                        OnAxisChanged?.Invoke(null, this);
                        // Debug.Log($"[UI] Deactivating all axis for {ParamName} (axis {clickedAxis} was already active)");
                    }
                    else
                    {
                        xChipButton.RemoveFromClassList("active");
                        zChipButton.RemoveFromClassList("active");
                        yChipButton.AddToClassList("active");
                        OnAxisChanged?.Invoke(Axis.Y, this);
                        // Debug.Log($"[UI] Activating axis {clickedAxis} for {ParamName}");
                    }
                    break;
                case Axis.Z:
                    if (zChipButton.ClassListContains("active"))
                    {
                        xChipButton.RemoveFromClassList("active");
                        yChipButton.RemoveFromClassList("active");
                        zChipButton.RemoveFromClassList("active");
                        OnAxisChanged?.Invoke(null, this);
                        // Debug.Log($"[UI] Deactivating all axis for {ParamName} (axis {clickedAxis} was already active)");
                    }
                    else
                    {
                        xChipButton.RemoveFromClassList("active");
                        yChipButton.RemoveFromClassList("active");
                        zChipButton.AddToClassList("active");
                        OnAxisChanged?.Invoke(Axis.Z, this);
                        // Debug.Log($"[UI] Activating axis {clickedAxis} for {ParamName}");
                    }
                    break;
            }

        }

        public void DeselectAxis(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    xChipButton.RemoveFromClassList("active");
                    Param.XAxis = false;
                    break;
                case Axis.Y:
                    yChipButton.RemoveFromClassList("active");
                    Param.YAxis = false;
                    break;
                case Axis.Z:
                    zChipButton.RemoveFromClassList("active");
                    Param.ZAxis = false;
                    break;
            }

            // Debug.Log($"[DeselectAxis] Deselected axis {axis} for {ParamName}");
        }

        private void InitThresholds()
        {
            minInputField.value = Param.ThrMinSel ?? Param.ThrMin;
            maxInputField.value = Param.ThrMaxSel ?? Param.ThrMax;
            UpdateWarningLabel(Threshold.Min);
            UpdateWarningLabel(Threshold.Max);
        }

        private void OnResetButtonClicked()
        {
            ResetThresholds();
        }

        private void ResetThresholds()
        {
            minInputField.value = Param.ThrMin;
            maxInputField.value = Param.ThrMax;
            Param.ThrMinSel = Param.ThrMin;
            Param.ThrMaxSel = Param.ThrMax;
        }

        public void SetSelected(bool value)
        {
            checkbox.value = value;
            Param.Selected = value;

            if (rootButton != null)
            {
                rootButton.style.opacity = value ? 1f : 0.5f;
            }

            nameContainer.SetEnabled(value);
            xyzAxisContainer.SetEnabled(value);
            thresholdContainer.SetEnabled(value);

            if (value == false)
            {
                DeselectAxis(Axis.X);
                DeselectAxis(Axis.Y);
                DeselectAxis(Axis.Z);
            }

            OnStateChanged?.Invoke();
            // Debug.Log(ParamName + " " + value);
        }

        private void UpdateWarningLabel(Threshold threshold)
        {
            if (threshold == Threshold.Min)
            {
                if (Param.ThrMinSel < Param.ThrMin)
                {
                    minErrorLabel.style.visibility = Visibility.Visible;
                    minErrorLabel.text = "Value too low";
                }
                else if (Param.ThrMinSel > Param.ThrMax || Param.ThrMinSel > Param.ThrMaxSel)
                {
                    minErrorLabel.style.visibility = Visibility.Visible;
                    minErrorLabel.text = "Value too low";
                }
                else
                {
                    minErrorLabel.style.visibility = Visibility.Hidden;
                }
            }
            else if (threshold == Threshold.Max)
            {
                if (Param.ThrMaxSel > Param.ThrMax)
                {
                    maxErrorLabel.style.visibility = Visibility.Visible;
                    maxErrorLabel.text = "Value too high";
                }
                else if (Param.ThrMaxSel < Param.ThrMin || Param.ThrMaxSel < Param.ThrMinSel)
                {
                    maxErrorLabel.style.visibility = Visibility.Visible;
                    maxErrorLabel.text = "Value too low";
                }
                else
                {
                    maxErrorLabel.style.visibility = Visibility.Hidden;
                }
            }
        }

    }
}
