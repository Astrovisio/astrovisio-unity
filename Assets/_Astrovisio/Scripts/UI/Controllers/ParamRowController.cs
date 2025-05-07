using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public enum Axis { X, Y, Z }

    public enum Threshold { Min, Max }

    public class ParamRowController
    {
        // === Dependencies ===
        private readonly VisualElement paramRow;

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
        private FloatField minInputField;
        private FloatField maxInputField;
        private Label minErrorLabel;
        private Label maxErrorLabel;
        private Button resetButton;
        private Toggle checkbox;

        // === Data ===
        public string ParamName { get; set; }
        public ConfigParam Param { get; set; }

        public ParamRowController(VisualElement paramRow, string paramName, ConfigParam param)
        {
            this.paramRow = paramRow;
            ParamName = paramName;
            Param = param;

            Init();
        }

        private void Init()
        {
            rootButton = paramRow.Q<Button>("RootButton");

            // Name
            nameContainer = paramRow.Q<VisualElement>("NameContainer");
            nameLabel = nameContainer.Q<Label>("Label");
            filesLabel = nameContainer.Q<VisualElement>("FilesContainer")?.Q<Label>("Label");
            InitName();

            // XYZ Axes
            xyzAxisContainer = paramRow.Q<VisualElement>("XYZAxisContainer");
            xChipButton = xyzAxisContainer.Q<VisualElement>("ChipX")?.Q<Button>("ChipRoot");
            yChipButton = xyzAxisContainer.Q<VisualElement>("ChipY")?.Q<Button>("ChipRoot");
            zChipButton = xyzAxisContainer.Q<VisualElement>("ChipZ")?.Q<Button>("ChipRoot");
            xChipButton?.RegisterCallback<ClickEvent>(_ => HandleAxisChipClick(Axis.X));
            yChipButton?.RegisterCallback<ClickEvent>(_ => HandleAxisChipClick(Axis.Y));
            zChipButton?.RegisterCallback<ClickEvent>(_ => HandleAxisChipClick(Axis.Z));
            InitAxis();

            // Threshold
            thresholdContainer = paramRow.Q<VisualElement>("ThresholdContainer");
            minInputField = thresholdContainer.Q<FloatField>("MinInputField");
            maxInputField = thresholdContainer.Q<FloatField>("MaxInputField");
            minErrorLabel = thresholdContainer.Q<Label>("MinErrorLabel");
            maxErrorLabel = thresholdContainer.Q<Label>("MaxErrorLabel");
            minInputField.RegisterValueChangedCallback(evt =>
            {
                Param.ThrMinSel = evt.newValue;
                // Debug.Log($"[UI] {ParamName} → ThrMin aggiornato a {evt.newValue}");
            });
            maxInputField.RegisterValueChangedCallback(evt =>
            {
                Param.ThrMaxSel = evt.newValue;
                // Debug.Log($"[UI] {ParamName} → ThrMax aggiornato a {evt.newValue}");
            });
            resetButton = thresholdContainer.Q<Button>("ResetButton");
            resetButton.clicked += OnResetButtonClicked;
            InitThresholds();

            // Select
            checkbox = paramRow.Q<Toggle>("CheckboxRoot");
            checkbox?.RegisterValueChangedCallback(evt =>
            {
                Param.Selected = evt.newValue;
                // Debug.Log($"Checkbox toggled for {ParamName}: {Param.Selected}");
                SetSelected(Param.Selected);
            });
            SetSelected(Param.Selected);

        }

        private void InitName()
        {
            filesLabel.text = Param.Files.Length.ToString();
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
            ResetThresholds();
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

            OnStateChanged?.Invoke();
            // Debug.Log(ParamName + " " + value);
        }

    }
}
