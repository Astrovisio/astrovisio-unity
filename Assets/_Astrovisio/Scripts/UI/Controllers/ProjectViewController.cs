using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ProjectViewController
    {

        // === Dependencies ===
        private readonly ProjectManager projectManager;
        private readonly Project project;

        // === Local ===
        private VisualElement root;

        public ProjectViewController(ProjectManager projectManager, Project project)
        {
            this.projectManager = projectManager;
            this.project = project;
        }

        public void Initialize(VisualElement root)
        {
            this.root = root;

            var projectNameLabel = this.root.Q<Label>("ProjectNameLabel");
            projectNameLabel.text = project.Name;
        }


    }
}
