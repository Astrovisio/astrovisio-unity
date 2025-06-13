using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class DuplicateProjectViewController
    {
        public ProjectManager ProjectManager { get; }
        public VisualElement Root { get; }

        private TextField nameTextLabel;
        private TextField descriptionTextLabel;
        private Button continueButton;
        private Button cancelButton;
        private Project projectToDuplicate;

        public DuplicateProjectViewController(ProjectManager projectManager, VisualElement root)
        {
            ProjectManager = projectManager;
            Root = root;

            nameTextLabel = Root.Q<VisualElement>("ProjectNameInputField")?.Q<TextField>();
            descriptionTextLabel = Root.Q<VisualElement>("ProjectDescriptionInputField")?.Q<TextField>();
            continueButton = Root.Q<VisualElement>("ContinueButton")?.Q<Button>();
            cancelButton = Root.Q<VisualElement>("CancelButton")?.Q<Button>();
        }

        public void SetProjectToDuplicate(Project project)
        {
            continueButton.clicked += OnContinueClicked;
            cancelButton.clicked += OnCancelClicked;
            projectToDuplicate = project;
            nameTextLabel.value = project.Name;
            descriptionTextLabel.value = project.Description;
        }

        private void OnContinueClicked()
        {
            // Debug.Log("OnContinueClicked");
            ProjectManager.DuplicateProject(nameTextLabel.value, descriptionTextLabel.value, projectToDuplicate);
            Root.RemoveFromClassList("active");
            Dispose();
        }

        private void OnCancelClicked()
        {
            // Debug.Log("OnCancelClicked");
            projectToDuplicate = null;
            Root.RemoveFromClassList("active");
            Dispose();
        }

        private void Dispose()
        {
            continueButton.clicked -= OnContinueClicked;
            cancelButton.clicked -= OnCancelClicked;
        }

    }

}