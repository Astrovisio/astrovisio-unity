using System.Collections.Generic;
using UnityEngine.UIElements;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

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
        private VisualElement dataSettingsContainer;
        private Label warningLabel;
        private ScrollView paramsScrollView;
        private DropdownField downsamplingDropdown;
        private Label actualSizeLabel;
        private Button processDataButton;
        private int chipLabelCounter;
        private bool isReadyToProcessData;
        private Dictionary<string, VisualElement> paramRowVisualElement = new();


        public ProjectSidebarDataController(
            ProjectSidebarController projectSidebarController,
            UIManager uiManager,
            ProjectManager projectManager,
            UIContextSO uiContextSO,
            Project project,
            VisualElement root)
        {
            ProjectSidebarController = projectSidebarController;
            UIManager = uiManager;
            ProjectManager = projectManager;
            UIContextSO = uiContextSO;
            Project = project;
            Root = root;

            ProjectManager.ProjectOpened += OnProjectOpened;
            ProjectManager.ProjectUpdated += OnProjectUpdated;

            Init();
        }

        private void Init()
        {
            dataSettingsContainer = Root.Q<VisualElement>("DataSettingsContainer");

            processDataButton = dataSettingsContainer.Q<Button>("ProcessDataButton");
            processDataButton.clicked += OnProcessDataClicked;

            InitWarningLabel();
            InitParamsScrollView();
            InitActualSizeLabel();
            InitDownsamplingDropdown();
        }

        private void InitParamsScrollView()
        {
            paramsScrollView = dataSettingsContainer.Q<ScrollView>("ParamsScrollView");

            UpdateParamsScrollView();
            UpdateChipLabel();
        }

        private void InitDownsamplingDropdown()
        {
            downsamplingDropdown = dataSettingsContainer.Q<DropdownField>("DropdownField");
            downsamplingDropdown.choices.Clear();
            downsamplingDropdown.choices.Add("0%");
            downsamplingDropdown.choices.Add("25%");
            downsamplingDropdown.choices.Add("50%");
            downsamplingDropdown.choices.Add("75%");
            downsamplingDropdown.value = ((1 - Project.Files[0].Downsampling) * 100).ToString("0") + "%"; // GB
            downsamplingDropdown?.RegisterValueChangedCallback(evt =>
            {
                string percentageText = evt.newValue.Replace("%", "");
                if (float.TryParse(percentageText, out float percentage))
                {
                    float value = 1 - (percentage / 100f);
                    Project.Files[0].Downsampling = value;
                    UpdateProcessDataButton();
                }
                else
                {
                    Debug.LogWarning("Invalid percentage format.");
                }
            });
        }

        private void InitActualSizeLabel()
        {
            actualSizeLabel = dataSettingsContainer.Q<Label>("ActualSize");
        }

        private void InitWarningLabel()
        {
            warningLabel = dataSettingsContainer.Q<Label>("Warning");
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Debug.Log($"Property changed: {e.PropertyName}");

            UpdateParamsScrollView();
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
            chipLabelCounter = Project.Files[0].Variables // GB
                .Count(p => p.XAxis || p.YAxis || p.ZAxis);

            // 2) Check which axes have been selected
            bool xAxis = Project.Files[0].Variables.Any(p => p.XAxis); // GB
            bool yAxis = Project.Files[0].Variables.Any(p => p.YAxis); // GB
            bool zAxis = Project.Files[0].Variables.Any(p => p.ZAxis); // GB

            // 3) If at least 3 parameters are active → OK, otherwise show warning
            if (chipLabelCounter >= 3)
            {
                isReadyToProcessData = true;
                UpdateProcessDataButton();
                warningLabel.style.display = DisplayStyle.None;
                return;
            }
            isReadyToProcessData = false;
            UpdateProcessDataButton();

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

        private void UpdateProcessDataButton()
        {
            if (isReadyToProcessData)
            {
                SetProcessDataButton(true);
            }
            else
            {
                SetProcessDataButton(false);
            }
        }

        private void OnProcessDataClicked()
        {
            SetProcessDataButton(false);
            ProjectManager.ProcessFile(Project.Id, Project.Files[0].Id); // GB
            // UpdateRenderingParams();
        }

        private void UpdateChipLabel()
        {
            ClearChipLabel();

            foreach (Variable variable in Project.Files[0].Variables)
            {
                if (!paramRowVisualElement.TryGetValue(variable.Name, out VisualElement row))
                {
                    // Debug.LogWarning($"Row not found for param: {paramName}");
                    continue;
                }

                VisualElement labelChip = row.Q<VisualElement>("LabelChip");
                Label labelChipLetter = labelChip.Q<Label>("Letter");

                if (variable.XAxis)
                {
                    labelChip.style.display = DisplayStyle.Flex;
                    labelChipLetter.text = "X";
                }
                else if (variable.YAxis)
                {
                    labelChip.style.display = DisplayStyle.Flex;
                    labelChipLetter.text = "Y";
                }
                else if (variable.ZAxis)
                {
                    labelChip.style.display = DisplayStyle.Flex;
                    labelChipLetter.text = "Z";
                }

                // Debug.Log($"Param: {paramName}, X: {param.XAxis}, Y: {param.YAxis}, Z: {param.ZAxis}");
            }

            HandleAxisControll();
        }

        private void UpdateParamsScrollView()
        {
            File file = Project.Files?[0]; // GB

            if (file.Variables == null)
            {
                Debug.LogWarning("No variables to display.");
                return;
            }

            foreach (Variable variable in file.Variables)
            {
                variable.PropertyChanged -= OnPropertyChanged;
            }

            paramsScrollView.contentContainer.Clear();
            paramRowVisualElement.Clear();

            if (file.Variables == null)
            {
                Debug.LogWarning("No variables to display.");
                return;
            }

            foreach (Variable variable in file.Variables)
            {
                variable.PropertyChanged += OnPropertyChanged;

                if (!variable.Selected)
                {
                    continue;
                }

                TemplateContainer paramRow = UIContextSO.sidebarParamRowTemplate.CloneTree();

                VisualElement nameContainer = paramRow.Q<VisualElement>("LabelContainer");
                nameContainer.Q<Label>("LabelParam").text = variable.Name;

                VisualElement labelChip = paramRow.Q<VisualElement>("LabelChip");
                labelChip.style.display = DisplayStyle.None;

                paramRowVisualElement.Add(variable.Name, paramRow);
                paramsScrollView.Add(paramRow);
            }
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

        private void OnProjectOpened(Project project)
        {
            if (Project.Id != project.Id)
            {
                // Debug.Log("RETURNED");
                return;
            }
            else
            {
                // Debug.Log("OPENED");
                // Project = project;
                UpdateChipLabel();
            }
        }

        private void OnProjectUpdated(Project project)
        {
            if (Project.Id != project.Id)
            {
                // Debug.Log("RETURNED");
                return;
            }
            else
            {
                // Debug.Log("UPDATED");
                // Project = project;
                UpdateChipLabel();
            }
        }

    }

}
