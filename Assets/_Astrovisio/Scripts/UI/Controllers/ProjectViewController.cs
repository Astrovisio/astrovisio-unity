using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ProjectViewController
    {

        // === Dependencies ===
        private readonly ProjectManager projectManager;

        // === Local ===
        public Project Project { get; }
        public VisualElement Root { get; }

        public ProjectViewController(ProjectManager projectManager, Project project, VisualElement root)
        {
            this.projectManager = projectManager;
            Project = project;
            Root = root;

            var projectNameLabel = Root.Q<Label>("ProjectNameLabel");
            projectNameLabel.text = project.Name;
        }

    }
}
