using System.Collections.Generic;
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


        public ProjectSidebarController(ProjectManager projectManager, VisualTreeAsset sidebarParamRowTemplate, Project project, VisualElement root)
        {
            this.projectManager = projectManager;
            this.sidebarParamRowTemplate = sidebarParamRowTemplate;
            Project = project;
            Root = root;

            InitializeUI();
            // CheckAll();

            warningLabel.text = project.Name; // TODO: Remove
        }

        private void InitializeUI()
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

            if (Project.ConfigProcess?.Params == null)
            {
                Debug.LogWarning("No variables to display.");
                return;
            }

            foreach (var kvp in Project.ConfigProcess.Params)
            {
                var paramName = kvp.Key;
                var param = kvp.Value;

                var row = sidebarParamRowTemplate.CloneTree();

                var nameContainer = row.Q<VisualElement>("LabelContainer");
                nameContainer.Q<Label>("LabelParam").text = paramName;

                var labelChip = row.Q<VisualElement>("LabelChip");
                labelChip.style.display = DisplayStyle.None;

                paramScrollView.Add(row);
            }
        }

        private void CheckAll()
        {
            Debug.Log(dataSettingsButton);
            Debug.Log(warningLabel);
            Debug.Log(paramScrollView);
            Debug.Log(actualSizeLabel);
            Debug.Log(processDataButton);

            Debug.Log(renderSettingsButton);
            Debug.Log(downsamplingDropdown);

            Debug.Log(goToVRButton);
        }

    }
}
