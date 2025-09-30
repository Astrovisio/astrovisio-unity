using System.Collections.Generic;
using UnityEngine.UIElements;
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
        private File currentFile;
        private EventCallback<ChangeEvent<string>> _downsamplingCallback;


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
            ProjectManager.ProjectClosed += OnProjectClosed;
            ProjectManager.FileSelected += OnFileSelected;
            ProjectManager.FileUpdated += OnFileUpdated;


            Init();
        }

        private void Init()
        {
            dataSettingsContainer = Root.Q<VisualElement>("DataSettingsContainer");

            processDataButton = dataSettingsContainer.Q<Button>("ProcessDataButton");
            processDataButton.clicked += OnProcessDataClicked;

            currentFile = Project.Files is { Count: > 0 } list ? list[0] : null;

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
            if (downsamplingDropdown == null)
            {
                Debug.LogError("DropdownField not found.");
                return;
            }

            if (_downsamplingCallback != null)
            {
                downsamplingDropdown.UnregisterValueChangedCallback(_downsamplingCallback);
            }

            downsamplingDropdown.choices.Clear();
            downsamplingDropdown.choices.AddRange(new[] { "0%", "25%", "50%", "75%" });

            if (currentFile == null)
            {
                Debug.LogWarning("No current file, downsampling dropdown disabled.");
                downsamplingDropdown.SetEnabled(false);
                downsamplingDropdown.SetValueWithoutNotify("0%");
                return;
            }

            downsamplingDropdown.SetEnabled(true);

            string pctText = ((1f - currentFile.Downsampling) * 100f).ToString("0") + "%";
            downsamplingDropdown.SetValueWithoutNotify(pctText);

            if (_downsamplingCallback == null)
            {
                _downsamplingCallback = evt =>
                {
                    string percentageText = evt.newValue.Replace("%", "");
                    if (float.TryParse(
                            percentageText,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out float percentage))
                    {
                        float value = 1f - (percentage / 100f);
                        currentFile.Downsampling = value;

                        Debug.Log($"[Downsampling] Set to {percentage:0}% → value={value:0.##}");
                        ProjectManager.UpdateFile(Project.Id, currentFile);
                        UpdateProcessDataButton();
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid percentage format: '{evt.newValue}'");
                    }
                };
            }

            downsamplingDropdown.RegisterValueChangedCallback(_downsamplingCallback);
        }

        private void InitActualSizeLabel()
        {
            actualSizeLabel = dataSettingsContainer.Q<Label>("ActualSize");
        }

        private void InitWarningLabel()
        {
            warningLabel = dataSettingsContainer.Q<Label>("Warning");
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

        private void HandleAxisControl()
        {
            if (currentFile?.Variables == null || currentFile.Variables.Count == 0)
            {
                isReadyToProcessData = false;
                UpdateProcessDataButton();
                warningLabel.style.display = DisplayStyle.None;
                return;
            }

            // 1) Count the active parameters
            chipLabelCounter = currentFile.Variables
                .Count(p => p.XAxis || p.YAxis || p.ZAxis);

            // 2) Check which axes have been selected
            bool xAxis = currentFile.Variables.Any(p => p.XAxis);
            bool yAxis = currentFile.Variables.Any(p => p.YAxis);
            bool zAxis = currentFile.Variables.Any(p => p.ZAxis);

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
            // SetProcessDataButton(false);
            ProjectManager.ProcessFile(Project.Id, currentFile.Id);
            // UpdateRenderingParams();
        }

        private void UpdateChipLabel()
        {
            if (currentFile?.Variables == null || currentFile.Variables.Count == 0)
            {
                ClearChipLabel();
                warningLabel.style.display = DisplayStyle.None;
                return;
            }

            ClearChipLabel();

            foreach (Variable variable in currentFile.Variables)
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

            HandleAxisControl();
        }

        private void UpdateParamsScrollView()
        {
            paramsScrollView.contentContainer.Clear();
            paramRowVisualElement.Clear();

            // Guard on the CURRENT file
            if (currentFile?.Variables == null || currentFile.Variables.Count == 0)
            {
                Debug.LogWarning("No variables to display.");
                return;
            }

            foreach (Variable variable in currentFile.Variables)
            {
                if (!variable.Selected)
                {
                    continue;
                }
                // Debug.Log("Name: " + variable.Name + " - Selected: " + variable.Selected + " - X: " + variable.XAxis + " - Y: " + variable.YAxis + " - Z: " + variable.ZAxis);

                TemplateContainer row = UIContextSO.sidebarParamRowTemplate.CloneTree();
                row.Q<VisualElement>("LabelContainer").Q<Label>("LabelParam").text = variable.Name;
                VisualElement labelChip = row.Q<VisualElement>("LabelChip");
                labelChip.style.display = DisplayStyle.None;

                // Protect against duplicate keys
                if (!paramRowVisualElement.ContainsKey(variable.Name))
                {
                    paramRowVisualElement.Add(variable.Name, row);
                }

                paramsScrollView.Add(row);
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


        // === Events ===
        private void OnProjectOpened(Project project)
        {
            if (Project.Id != project.Id)
            {
                // Debug.Log("RETURNED");
                return;
            }

            UpdateParamsScrollView();
            UpdateChipLabel();
        }

        private void OnProjectUpdated(Project project)
        {
            if (Project.Id != project.Id)
            {
                // Debug.Log("RETURNED");
                return;
            }

            UpdateParamsScrollView();
            UpdateChipLabel();
        }

        private void OnProjectClosed(Project project)
        {
            if (project == null || project.Id != Project.Id)
            {
                return;
            }

            // ProjectManager.ProjectOpened += OnProjectOpened;
            // ProjectManager.ProjectUpdated -= OnProjectUpdated;
            ProjectManager.FileSelected -= OnFileSelected;
            ProjectManager.ProjectClosed -= OnProjectClosed;
        }

        private void OnFileSelected(Project project, File file)
        {
            // Debug.Log("I'm: " + Project.Name + " @ " + project.Name + " - " + file.Name);

            if (project == null || file == null || project.Id != Project.Id)
            {
                return;
            }

            if (ReferenceEquals(currentFile, file))
            {
                return;
            }

            currentFile = file;

            InitWarningLabel();
            InitParamsScrollView();
            InitActualSizeLabel();
            InitDownsamplingDropdown();
        }

        private void OnFileUpdated(Project project, File file)
        {
            if (project == null || file == null)
            {
                return;
            }

            // Debug.Log(ReferenceEquals(Project, project));
            // Debug.Log(ReferenceEquals(currentFile, file));
            if (Project.Id == project.Id && currentFile.Id == file.Id)
            {
                Debug.Log($"Project name: {Project.Name} - File name: {currentFile.Name} updated.");
                UpdateParamsScrollView();
                UpdateChipLabel();
            }

        }

    }

}
