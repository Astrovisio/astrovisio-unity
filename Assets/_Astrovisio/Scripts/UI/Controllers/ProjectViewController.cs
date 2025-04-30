using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Astrovisio
{
    public class ProjectViewController
    {
        // === Dependencies ===
        private readonly ProjectManager projectManager;
        private readonly VisualTreeAsset paramRowTemplate;

        // === Local ===
        public Project Project { get; }
        public VisualElement Root { get; }

        private ScrollView paramScrollView;
        private Label projectNameLabel;

        public ProjectViewController(ProjectManager projectManager, VisualElement root, Project project,  VisualTreeAsset paramRowTemplate)
        {
            this.projectManager = projectManager;
            Root = root;
            Project = project;
            this.paramRowTemplate = paramRowTemplate;

            InitializeUI();
        }

        private void InitializeUI()
        {
            var rootContainer = Root.Children().First();
            var leftContainer = rootContainer.Children().FirstOrDefault(child => child.name == "LeftContainer");
            var rightContainer = rootContainer.Children().FirstOrDefault(child => child.name == "RightContainer");

            paramScrollView = leftContainer.Q<ScrollView>("ParamScrollView");

            projectNameLabel = rightContainer.Q<Label>("ProjectNameLabel");
            projectNameLabel.text = Project.Name;

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

                var row = paramRowTemplate.CloneTree();

                var nameContainer = row.Q<VisualElement>("NameContainer");
                nameContainer.Q<Label>("Label").text = paramName;

                paramScrollView.Add(row);
            }
        }

    }
}
