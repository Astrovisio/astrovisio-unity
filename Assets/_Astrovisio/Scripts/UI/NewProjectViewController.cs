using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class NewProjectViewController
    {
        private readonly ProjectManager projectManager;
        private VisualElement root;
        private TextField projectNameField;
        private TextField projectDescriptionField;
        private Button continueButton;
        private Button cancelButton;

        public NewProjectViewController(ProjectManager projectManager)
        {
            this.projectManager = projectManager;
        }

        public void Initialize(VisualElement root)
        {
            this.root = root;
            projectNameField = root.Q<VisualElement>("ProjectNameInputField")?.Q<TextField>();
            projectDescriptionField = root.Q<VisualElement>("ProjectDescriptionInputField")?.Q<TextField>();
            continueButton = root.Q<VisualElement>("ContinueButton")?.Q<Button>();
            cancelButton = root.Q<VisualElement>("CancelButton")?.Q<Button>();

            if (continueButton != null)
            {
                continueButton.RegisterCallback<ClickEvent>(OnContinueClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.RegisterCallback<ClickEvent>(OnCancelClicked);
            }
        }

        public void Dispose()
        {
            if (continueButton != null)
            {
                continueButton.UnregisterCallback<ClickEvent>(OnContinueClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.UnregisterCallback<ClickEvent>(OnCancelClicked);
            }
        }

        private void OnContinueClicked(ClickEvent evt)
        {
            string name = projectNameField?.value ?? "<vuoto>";
            string description = projectDescriptionField?.value ?? "<vuoto>";
            Debug.Log($"Create project: {name}, {description}");
            projectManager.CreateProject(name, description, new string[0]);
        }

        private void OnCancelClicked(ClickEvent evt)
        {
            root.style.display = DisplayStyle.None;
            Dispose();
        }
    }

}
