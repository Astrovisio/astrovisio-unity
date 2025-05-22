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
        private readonly UIManager uiManager;
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
        private VisualElement settingsPanel;
        private Dictionary<string, ParamSettingsData> paramSettingsDatas = new();

        // @@@ Go To VR
        private Button goToVRButton;

        // === Local ===
        private ProjectSidebarStep projectSidebarStep = ProjectSidebarStep.Data;
        private DataContainer dataContainer;
        public Project Project { get; }
        public VisualElement Root { get; }
        public UIContextSO UIContextSO { get; }


        public ProjectSidebarController(UIManager uiManager, ProjectManager projectManager, UIContextSO uiContextSO, Project project, VisualElement root)
        {
            this.uiManager = uiManager;
            this.projectManager = projectManager;
            UIContextSO = uiContextSO;
            Project = project;
            Root = root;

            projectManager.ProjectProcessed += OnProjectProcessed;
            projectManager.ProjectOpened += OnProjectOpened;

            Init();
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

            var restFrequencyButton = renderSettingsContainer.Q<Button>("RestFrequencyButton");
            restFrequencyButton.RemoveFromClassList("active");
            var restFrequencyPanel = renderSettingsContainer.Q<VisualElement>("RestFrequencyPanel");
            restFrequencyPanel.RemoveFromClassList("active");

            var xLabel = renderSettingsContainer.Q<Label>("XParamLabel");
            var yLabel = renderSettingsContainer.Q<Label>("YParamLabel");
            var zLabel = renderSettingsContainer.Q<Label>("ZParamLabel");
            xLabel.text = "";
            yLabel.text = "";
            zLabel.text = "";

            settingsPanel = renderSettingsContainer.Q<VisualElement>("SettingsPanel");
            settingsPanel.RemoveFromClassList("active");

            paramSettingsScrollView = renderSettingsContainer.Q<ScrollView>("ParamSettingsScrollView");
            paramSettingsScrollView.Clear();

            renderSettingsButton = renderSettingsContainer.Q<Button>("AccordionHeader");
            renderSettingsButton.SetEnabled(false);
            renderSettingsButton.clicked += OnRenderSettingsButtonClicked;

            // @@@ VR
            goToVRButton = Root.Q<Button>("GoToVRButton");
            goToVRButton.SetEnabled(false);
            goToVRButton.clicked += OnGoToVRButtonClicked;

            PopulateScrollView();

            SetActiveStep(ProjectSidebarStep.Data);
        }

        private void OnGoToVRButtonClicked()
        {
            Debug.Log("OnGoToVRButtonClicked");
            VRManager.Instance.StartVRMode();
            // uiManager.SetErrorVR(true);
        }

        private void OnRenderSettingsButtonClicked()
        {
            SetActiveStep(ProjectSidebarStep.Render);
            RenderManager.Instance.RenderDataContainer(dataContainer); // To remove/change
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
                    uiManager.SetSceneVisibility(true);
                    break;
                case ProjectSidebarStep.Render:
                    dataSettingsContainer.RemoveFromClassList("active");
                    renderSettingsContainer.AddToClassList("active");
                    uiManager.SetSceneVisibility(false);
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

                var paramRow = UIContextSO.sidebarParamRowTemplate.CloneTree();

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
                SetNextStepButtons(false);
            }
        }

        private void SetNextStepButtons(bool state)
        {
            renderSettingsButton.SetEnabled(state);
            goToVRButton.SetEnabled(state);

            if (state)
            {
                renderSettingsButton.style.opacity = 1.0f;
                goToVRButton.style.opacity = 1.0f;
            }
            else
            {
                renderSettingsButton.style.opacity = 0.5f;
                goToVRButton.style.opacity = 0.5f;
            }
        }

        private void OnProcessDataClicked()
        {
            SetProcessDataButton(false);
            projectManager.ProcessProject(Project.Id, Project.ConfigProcess);
            UpdateRenderingParams();
        }

        private void OnProjectProcessed(DataPack data)
        {
            dataContainer = new DataContainer(data);
            SetProcessDataButton(true);
            SetNextStepButtons(true);
        }

        private void OnProjectOpened(Project project)
        {
            SetActiveStep(ProjectSidebarStep.Data);
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
        }

        private void UpdateSettingsPanel(ParamRowSettingsController paramRowSettingsController)
        {
            Debug.Log(paramRowSettingsController.ParamName);
        }

        private void UpdateRenderingParams()
        {
            // Labels
            var xLabel = renderSettingsContainer.Q<Label>("XParamLabel");
            var yLabel = renderSettingsContainer.Q<Label>("YParamLabel");
            var zLabel = renderSettingsContainer.Q<Label>("ZParamLabel");
            xLabel.text = Project.ConfigProcess.Params.FirstOrDefault(p => p.Value.XAxis).Key ?? "";
            yLabel.text = Project.ConfigProcess.Params.FirstOrDefault(p => p.Value.YAxis).Key ?? "";
            zLabel.text = Project.ConfigProcess.Params.FirstOrDefault(p => p.Value.ZAxis).Key ?? "";

            // ScrollView
            paramSettingsScrollView = renderSettingsContainer.Q<ScrollView>("ParamSettingsScrollView");
            paramSettingsScrollView.Clear();
            foreach (var kvp in Project.ConfigProcess.Params)
            {
                var paramName = kvp.Key;
                var configVariable = kvp.Value;


                if (!configVariable.Selected || configVariable.XAxis || configVariable.YAxis || configVariable.ZAxis)
                {
                    continue;
                }

                VisualElement paramRowSettings = UIContextSO.paramRowSettingsTemplate.CloneTree();

                var nameLabel = paramRowSettings.Q<Label>("ParamLabel");
                if (nameLabel != null)
                {
                    nameLabel.text = paramName;
                }

                ParamSettingsData paramSettingsData = new ParamSettingsData
                {
                    VisualElement = paramRowSettings,
                    ParamRowSettingsController = new ParamRowSettingsController(Project, paramName, UIContextSO)
                };

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
                        settingsPanel.ToggleInClassList("active");
                        UpdateSettingsPanel(paramSettingsData.ParamRowSettingsController);
                    }
                };

                paramSettingsDatas.Add(paramName, paramSettingsData);
                paramSettingsScrollView.Add(paramRowSettings);
            }

        }

    }

}
