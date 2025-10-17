using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class DeleteProjectViewController
    {
        public ProjectManager ProjectManager { get; }
        public VisualElement Root { get; }

        private Button deleteButton;
        private Button cancelButton;
        private Project projectToDelete;

        public DeleteProjectViewController(ProjectManager projectManager, VisualElement root)
        {
            ProjectManager = projectManager;
            Root = root;

            deleteButton = Root.Q<VisualElement>("ContinueButton")?.Q<Button>();
            cancelButton = Root.Q<VisualElement>("CancelButton")?.Q<Button>();

            deleteButton.clicked += OnDeleteClicked;
            cancelButton.clicked += OnCancelClicked;
        }

        public void SetProjectToDelete(Project project)
        {
            projectToDelete = project;
        }

        private void OnDeleteClicked()
        {
            // Debug.Log("OnDeleteClicked");
            foreach (File file in projectToDelete.Files)
            {
                SettingsManager.Instance.RemoveSettings(projectToDelete.Id, file.Id);
            }
            ReelManager.Instance.RemoveReel(projectToDelete.Id);
            ProjectManager.DeleteProject(projectToDelete.Id, projectToDelete);
            Root.RemoveFromClassList("active");
        }

        private void OnCancelClicked()
        {
            // Debug.Log("OnCancelClicked");
            projectToDelete = null;
            Root.RemoveFromClassList("active");
        }

    }

}