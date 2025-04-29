using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ProjectSidebarController
    {
        private VisualElement root;

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


        private List<ConfigVariable> configVariable;
        private List<ConfigProcess> configProcess;


        public ProjectSidebarController(VisualElement root, List<ConfigVariable> configVariable, List<ConfigProcess> configProcess)
        {
            this.root = root;
            this.configVariable = configVariable;
            this.configProcess = configProcess;

            InitializeUI();
            CheckAll();
        }

        private void InitializeUI()
        {
            var dataSettingsContainer = root.Q<VisualElement>("DataSettingsContainer");
            var renderSettingsContainer = root.Q<VisualElement>("RenderSettingsContainer");

            dataSettingsButton = dataSettingsContainer.Q<Button>("AccordionHeader");
            warningLabel = dataSettingsContainer.Q<Label>("Warning");
            paramScrollView = dataSettingsContainer.Q<ScrollView>("ParamsScrollView");
            actualSizeLabel = dataSettingsContainer.Q<Label>("ActualSize");
            processDataButton = dataSettingsContainer.Q<Button>("ProcessDataButton");

            renderSettingsButton = renderSettingsContainer.Q<Button>("AccordionHeader");
            downsamplingDropdown = renderSettingsContainer.Q<DropdownField>("DownsamplingDropdown"); // TODO

            goToVRButton = root.Q<Button>("GoToVRButton");
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
