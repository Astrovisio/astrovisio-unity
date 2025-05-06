using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ProjectSidebarController
    {
        // === Dependencies ===
        private readonly ProjectManager projectManager;
        private readonly VisualTreeAsset sidebarParamRowTemplate;

        // --- Data Settings
        private Button dataSettingsButton;
        private Label warningLabel;
        private ScrollView paramScrollView;
        private Label actualSizeLabel;
        private Button processDataButton;

        // --- Render Settings
        private Button renderSettingsButton;
        private DropdownField downsamplingDropdown;

        // --- Go To VR
        private Button goToVRButton;

        // === Local ===
        public Project Project { get; }
        public VisualElement Root { get; }
        private Dictionary<string, VisualElement> paramRowVisualElement = new();


        public ProjectSidebarController(ProjectManager projectManager, VisualTreeAsset sidebarParamRowTemplate, Project project, VisualElement root)
        {
            this.projectManager = projectManager;
            this.sidebarParamRowTemplate = sidebarParamRowTemplate;
            Project = project;
            Root = root;

            Init();

            warningLabel.text = Project.Name; // TODO: Remove

            // foreach (var kvp in Project.ConfigProcess.Params)
            // {
            //     string paramName = kvp.Key;
            //     ConfigParam param = kvp.Value;

            //     Debug.Log($"Param: {paramName}, XAxis: {param.XAxis}, Hash: {param.GetHashCode()}");
            //     Debug.Log($"Param: {paramName}, YAxis: {param.YAxis}, Hash: {param.GetHashCode()}");
            //     Debug.Log($"Param: {paramName}, ZAxis: {param.ZAxis}, Hash: {param.GetHashCode()}");
            // }
        }

        private void Init()
        {
            var dataSettingsContainer = Root.Q<VisualElement>("DataSettingsContainer");
            var renderSettingsContainer = Root.Q<VisualElement>("RenderSettingsContainer");

            dataSettingsButton = dataSettingsContainer.Q<Button>("AccordionHeader");
            warningLabel = dataSettingsContainer.Q<Label>("Warning");

            paramScrollView = dataSettingsContainer.Q<ScrollView>("ParamsScrollView");
            actualSizeLabel = dataSettingsContainer.Q<Label>("ActualSize");
            processDataButton = dataSettingsContainer.Q<Button>("ProcessDataButton");

            renderSettingsButton = renderSettingsContainer.Q<Button>("AccordionHeader");
            downsamplingDropdown = renderSettingsContainer.Q<DropdownField>("DownsamplingDropdown"); // TODO

            goToVRButton = Root.Q<Button>("GoToVRButton");

            PopulateScrollView();
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

                var paramRow = sidebarParamRowTemplate.CloneTree();

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
