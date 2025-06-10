using System.Collections.Generic;
using UnityEngine.UIElements;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using System;

namespace Astrovisio
{
    public class ProjectSidebarDataController
    {
        // === Dependencies ===
        public ProjectSidebarController ProjectSidebarController { private set; get; }
        public UIManager UIManager { private set; get; }
        public ProjectManager ProjectManager { private set; get; }
        public UIContextSO UIContextSO { private set; get; }
        public Project Project { private set; get; }
        public VisualElement Root { private set; get; }

        // === Other ===
        // private VisualElement dataSettingsContainer;
        private Label warningLabel;
        private ScrollView paramsScrollView;
        private DropdownField downsamplingDropdown;
        private Label actualSizeLabel;
        private Button processDataButton;
        private int chipLabelCounter;
        private bool isReadyToProcessData;
        private Dictionary<string, VisualElement> paramRowVisualElement = new();

        public ProjectSidebarDataController(ProjectSidebarController projectSidebarController, UIManager uiManager, ProjectManager projectManager, UIContextSO uiContextSO, Project project, VisualElement root)
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
            var dataSettingsContainer = Root.Q<VisualElement>("DataSettingsContainer");

            processDataButton = dataSettingsContainer.Q<Button>("ProcessDataButton");
            processDataButton.clicked += OnProcessDataClicked;

            InitWarningLabel(dataSettingsContainer);
            InitParamsScrollView(dataSettingsContainer);
            InitActualSizeLabel(dataSettingsContainer);
            InitDownsamplingDropdown(dataSettingsContainer);
        }

        private void InitParamsScrollView(VisualElement dataSettingsContainer)
        {
            paramsScrollView = dataSettingsContainer.Q<ScrollView>("ParamsScrollView");
            paramsScrollView.contentContainer.Clear();
            paramRowVisualElement.Clear();

            if (Project.ConfigProcess?.Params == null)
            {
                Debug.LogWarning("No variables to display.");
                return;
            }

            foreach (var kvp in Project.ConfigProcess.Params)
            {
                var paramName = kvp.Key;
                var param = kvp.Value;

                var paramRow = UIContextSO.sidebarParamRowTemplate.CloneTree();

                var nameContainer = paramRow.Q<VisualElement>("LabelContainer");
                nameContainer.Q<Label>("LabelParam").text = paramName;

                var labelChip = paramRow.Q<VisualElement>("LabelChip");
                labelChip.style.display = DisplayStyle.None;
                var labelChipLetter = labelChip.Q<Label>("Letter");

                param.PropertyChanged += OnPropertyChanged;

                paramRowVisualElement.Add(paramName, paramRow);
                paramsScrollView.Add(paramRow);
            }

            UpdateChipLabel();
        }

        private void InitDownsamplingDropdown(VisualElement dataSettingsContainer)
        {
            downsamplingDropdown = dataSettingsContainer.Q<DropdownField>("DropdownField");
            downsamplingDropdown.choices.Clear();
            downsamplingDropdown.choices.Add("0%");
            downsamplingDropdown.choices.Add("25%");
            downsamplingDropdown.choices.Add("50%");
            downsamplingDropdown.choices.Add("75%");
            downsamplingDropdown.value = ((1 - Project.ConfigProcess.Downsampling) * 100).ToString("0") + "%";
            downsamplingDropdown?.RegisterValueChangedCallback(evt =>
            {
                string percentageText = evt.newValue.Replace("%", "");
                if (float.TryParse(percentageText, out float percentage))
                {
                    float value = 1 - (percentage / 100f);
                    Project.ConfigProcess.Downsampling = value;
                    SetProcessDataButton(true);
                }
                else
                {
                    Debug.LogWarning("Invalid percentage format.");
                }
            });
        }

        private void InitActualSizeLabel(VisualElement dataSettingsContainer)
        {
            actualSizeLabel = dataSettingsContainer.Q<Label>("ActualSize");
        }

        private void InitWarningLabel(VisualElement dataSettingsContainer)
        {
            warningLabel = dataSettingsContainer.Q<Label>("Warning");
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateChipLabel();
        }

        private void SetProcessDataButton(bool state)
        {
            processDataButton.SetEnabled(state);

            if (state)
            {
                processDataButton.style.opacity = 1.0f;
            }
            else
            {
                processDataButton.style.opacity = 0.5f;
                ProjectSidebarController.SetNextStepButtons(false);
            }
        }

        private void HandleAxisControll()
        {
            // 1) Count the active parameters
            chipLabelCounter = Project.ConfigProcess.Params
                .Values
                .Count(p => p.XAxis || p.YAxis || p.ZAxis);

            // 2) Check which axes have been selected
            bool xAxis = Project.ConfigProcess.Params.Values.Any(p => p.XAxis);
            bool yAxis = Project.ConfigProcess.Params.Values.Any(p => p.YAxis);
            bool zAxis = Project.ConfigProcess.Params.Values.Any(p => p.ZAxis);

            // 3) If at least 3 parameters are active → OK, otherwise show warning
            if (chipLabelCounter >= 3)
            {
                isReadyToProcessData = true;
                SetProcessDataButton(true);
                warningLabel.style.display = DisplayStyle.None;
                return;
            }
            isReadyToProcessData = false;
            SetProcessDataButton(false);

            // 4) Count how many distinct axes have been selected
            int axesCount = (xAxis ? 1 : 0) + (yAxis ? 1 : 0) + (zAxis ? 1 : 0);

            // 5) Determine the warning message
            switch (axesCount)
            {
                case 0:
                    warningLabel.text = "Select the axes";
                    break;

                case 1:
                    if (xAxis)
                    {
                        warningLabel.text = "Select the Y and Z axes";
                    }
                    else if (yAxis)
                    {
                        warningLabel.text = "Select the X and Z axes";
                    }
                    else if (zAxis)
                    {
                        warningLabel.text = "Select the X and Y axes";
                    }
                    break;

                case 2:
                    if (!xAxis)
                    {
                        warningLabel.text = "Select the X axis";
                    }
                    else if (!yAxis)
                    {
                        warningLabel.text = "Select the Y axis";
                    }
                    else if (!zAxis)
                    {
                        warningLabel.text = "Select the Z axis";
                    }
                    break;

                default:
                    warningLabel.text = string.Empty;
                    break;
            }

            // 6) Show the warning if needed
            warningLabel.style.display =
                string.IsNullOrEmpty(warningLabel.text)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
        }

        private void OnProcessDataClicked()
        {
            SetProcessDataButton(false);
            ProjectManager.ProcessProject(Project.Id, Project.ConfigProcess);
            // UpdateRenderingParams();
        }

        private void UpdateChipLabel()
        {
            ClearChipLabel();

            foreach (var kvp in Project.ConfigProcess.Params)
            {
                string paramName = kvp.Key;
                ConfigParam param = kvp.Value;

                if (!paramRowVisualElement.TryGetValue(paramName, out VisualElement row))
                {
                    Debug.LogWarning($"Row not found for param: {paramName}");
                    continue;
                }

                var labelChip = row.Q<VisualElement>("LabelChip");
                var labelChipLetter = labelChip.Q<Label>("Letter");

                if (param.XAxis)
                {
                    labelChip.style.display = DisplayStyle.Flex;
                    labelChipLetter.text = "X";
                }
                else if (param.YAxis)
                {
                    labelChip.style.display = DisplayStyle.Flex;
                    labelChipLetter.text = "Y";
                }
                else if (param.ZAxis)
                {
                    labelChip.style.display = DisplayStyle.Flex;
                    labelChipLetter.text = "Z";
                }

                // Debug.Log($"Param: {paramName}, X: {param.XAxis}, Y: {param.YAxis}, Z: {param.ZAxis}");
            }

            HandleAxisControll();
        }

        private void ClearChipLabel()
        {
            foreach (var kvp in paramRowVisualElement)
            {
                // string paramName = kvp.Key;
                VisualElement row = kvp.Value;

                var labelChip = row.Q<VisualElement>("LabelChip");
                labelChip.style.display = DisplayStyle.None;
            }
        }

    }

}
