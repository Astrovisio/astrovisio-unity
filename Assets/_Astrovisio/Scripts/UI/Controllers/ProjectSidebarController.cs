using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public struct ParamSettingsData
    {
        public VisualElement VisualElement { get; set; }
        public ParamRowSettingsController ParamRowSettingsController { get; set; }
    }

    public enum ProjectSidebarStep
    {
        Data,
        Render
    }

    public class ProjectSidebarController
    {
        // === Dependencies ===
        private readonly ProjectManager projectManager;
        private readonly VisualTreeAsset sidebarParamRowTemplate;

        // @@@ Data Settings
        private VisualElement dataSettingsContainer;
        private Button dataSettingsButton;
        private Label warningLabel;
        private ScrollView paramScrollView;
        private DropdownField downsamplingDropdown;
        private Label actualSizeLabel;
        private Button processDataButton;
        private int chipLabelCounter;
        private bool isReadyToProcessData;
        private Dictionary<string, VisualElement> paramRowVisualElement = new();

        // @@@ Render Settings
        private VisualElement renderSettingsContainer;
        private Button renderSettingsButton;
        private ScrollView paramSettingsScrollView;
        private Dictionary<string, ParamSettingsData> paramSettingsDatas = new();
        // public Action OnRenderSettingsButtonActive;

        // @@@ Go To VR
        private Button goToVRButton;

        // === Local ===
        private ProjectSidebarStep projectSidebarStep = ProjectSidebarStep.Data;
        public Project Project { get; }
        public VisualElement Root { get; }
        public SideContextSO SideContextSO { get; }


        public ProjectSidebarController(ProjectManager projectManager, SideContextSO sideContextSO, Project project, VisualElement root)
        {
            this.projectManager = projectManager;
            SideContextSO = sideContextSO;
            Project = project;
            Root = root;

            projectManager.ProjectProcessed += OnProjectProcessed;

            Init();
        }

        private void OnProjectProcessed(ProcessedData data)
        {
            // Debug.Log("OnProjectProcessed: " + data.Columns.Length + " " + data.Rows.Length);
        }

        private void Init()
        {
            // @@@ Data
            dataSettingsContainer = Root.Q<VisualElement>("DataSettingsContainer");

            dataSettingsButton = dataSettingsContainer.Q<Button>("AccordionHeader");
            warningLabel = dataSettingsContainer.Q<Label>("Warning");

            paramScrollView = dataSettingsContainer.Q<ScrollView>("ParamsScrollView");
            actualSizeLabel = dataSettingsContainer.Q<Label>("ActualSize");
            downsamplingDropdown = dataSettingsContainer.Q<DropdownField>("DropdownField");
            downsamplingDropdown.choices.Clear();
            downsamplingDropdown.choices.Add("10%");
            downsamplingDropdown.choices.Add("20%");
            downsamplingDropdown.choices.Add("30%");
            downsamplingDropdown.choices.Add("40%");
            downsamplingDropdown.choices.Add("50%");
            downsamplingDropdown?.RegisterValueChangedCallback(evt =>
            {
                string percentageText = evt.newValue.Replace("%", "");
                if (float.TryParse(percentageText, out float percentage))
                {
                    float value = percentage / 100f;
                    // Debug.Log($"Converted value: {value}");
                    Project.ConfigProcess.Downsampling = percentage;
                }
                else
                {
                    Debug.LogWarning("Invalid percentage format.");
                }
            });

            processDataButton = dataSettingsContainer.Q<Button>("ProcessDataButton");
            processDataButton.clicked += OnProcessDataClicked;
            dataSettingsButton.clicked += OnDataSettingsButtonClicked;

            // @@@ Render
            renderSettingsContainer = Root.Q<VisualElement>("RenderSettingsContainer");
            paramSettingsScrollView = Root.Q<ScrollView>("ParamSettingsScrollView");
            paramSettingsScrollView.Clear();
            foreach (var kvp in Project.ConfigProcess.Params)
            {
                var paramName = kvp.Key;
                var configVariable = kvp.Value;

                if (configVariable.XAxis || configVariable.YAxis || configVariable.ZAxis)
                {
                    continue;
                }

                VisualElement paramRowSettings = SideContextSO.paramRowSettingsTemplate.CloneTree();

                var nameLabel = paramRowSettings.Q<Label>("ParamLabel");
                if (nameLabel != null)
                {
                    nameLabel.text = paramName;
                }

                var button = paramRowSettings.Q<Button>("Root");
                button.RemoveFromClassList("active");
                paramRowSettings.style.marginBottom = 8;
                button.clicked += () =>
                {
                    if (button.ClassListContains("active"))
                    {
                        UnselectAllParamSettingButton();
                    }
                    else
                    {
                        UnselectAllParamSettingButton();
                        button.ToggleInClassList("active");
                    }
                };
                // paramRowSettingsVisualElement.Add(paramName, paramRowSettings);
                // paramSettingsControllers.Add(paramName, new ParamRowSettingsController(Project, paramName, SideContextSO));

                ParamSettingsData paramSettingsData = new ParamSettingsData
                {
                    VisualElement = paramRowSettings,
                    ParamRowSettingsController = new ParamRowSettingsController(Project, paramName, SideContextSO)
                };
                paramSettingsDatas.Add(paramName, paramSettingsData);

                paramSettingsScrollView.Add(paramRowSettings);
            }

            renderSettingsButton = renderSettingsContainer.Q<Button>("AccordionHeader");
            renderSettingsButton.clicked += OnRenderSettingsButtonClicked;

            // @@@ VR
            goToVRButton = Root.Q<Button>("GoToVRButton");
            goToVRButton.clicked += OnGoToVRButtonClicked;

            PopulateScrollView();
        }

        private void OnGoToVRButtonClicked()
        {
            Debug.Log("OnGoToVRButtonClicked");
        }

        private void OnRenderSettingsButtonClicked()
        {
            SetActiveStep(ProjectSidebarStep.Render);
        }

        private void OnDataSettingsButtonClicked()
        {
            SetActiveStep(ProjectSidebarStep.Data);
        }

        private void SetActiveStep(ProjectSidebarStep projectSidebarStep)
        {
            switch (projectSidebarStep)
            {
                case ProjectSidebarStep.Data:
                    renderSettingsContainer.RemoveFromClassList("active");
                    dataSettingsContainer.AddToClassList("active");
                    break;
                case ProjectSidebarStep.Render:
                    dataSettingsContainer.RemoveFromClassList("active");
                    renderSettingsContainer.AddToClassList("active");
                    break;
            }
        }

        private void PopulateScrollView()
        {
            paramScrollView.contentContainer.Clear();
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

                var paramRow = SideContextSO.sidebarParamRowTemplate.CloneTree();

                var nameContainer = paramRow.Q<VisualElement>("LabelContainer");
                nameContainer.Q<Label>("LabelParam").text = paramName;

                var labelChip = paramRow.Q<VisualElement>("LabelChip");
                labelChip.style.display = DisplayStyle.None;
                var labelChipLetter = labelChip.Q<Label>("Letter");

                param.PropertyChanged += OnPropertyChanged;

                paramRowVisualElement.Add(paramName, paramRow);
                paramScrollView.Add(paramRow);
            }

            UpdateChipLabel();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateChipLabel();
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
                SetProcessData(true);
                warningLabel.style.display = DisplayStyle.None;
                return;
            }
            SetProcessData(false);

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

        private void SetProcessData(bool state)
        {
            processDataButton.SetEnabled(state);

            if (state)
            {
                processDataButton.style.opacity = 1.0f;
                // Debug.Log("ready to process data");
            }
            else
            {
                processDataButton.style.opacity = 0.5f;
                // Debug.Log("NOT ready to process data");
            }

            isReadyToProcessData = state;
        }

        private void OnProcessDataClicked()
        {
            projectManager.ProcessProject(Project.Id, Project.ConfigProcess);
        }

        private void UnselectAllParamSettingButton()
        {
            foreach (var paramSettingsData in paramSettingsDatas.Values)
            {
                var button = paramSettingsData.VisualElement.Q<Button>("Root");
                if (button != null)
                {
                    button.RemoveFromClassList("active");
                }
            }
        }

    }
}
