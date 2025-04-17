using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class NewProjectView : MonoBehaviour
    {
        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private UIDocument newProjectUIDocument;

        private TextField projectNameField;
        private TextField projectDescriptionField;
        private Button continueButton;
        private Button cancelButton;

        private void Start()
        {
            Debug.Log("NewProjectPanel");
        }

        private void OnEnable()
        {
            VisualElement root = newProjectUIDocument.rootVisualElement;

            VisualElement nameInputContainer = root.Q<VisualElement>("ProjectNameInputField");
            VisualElement descriptionInputContainer = root.Q<VisualElement>("ProjectDescriptionInputField");

            projectNameField = nameInputContainer?.Q<TextField>();
            projectDescriptionField = descriptionInputContainer?.Q<TextField>();

            if (projectNameField == null)
            {
                Debug.LogWarning("ProjectNameField NON trovato");
            }

            if (projectDescriptionField == null)
            {
                Debug.LogWarning("ProjectDescriptionField NON trovato");
            }

            VisualElement continueButtonInstance = root.Q<VisualElement>("ContinueButton");
            continueButton = continueButtonInstance?.Q<Button>();

            if (continueButton != null)
            {
                Debug.Log("Button trovato");
                continueButton.RegisterCallback<ClickEvent>(OnContinueClicked);
            }
            else
            {
                Debug.LogWarning("Button NON trovato all'interno di ContinueButton");
            }

            VisualElement cancelButtonInstance = root.Q<VisualElement>("CancelButton");
            cancelButton = cancelButtonInstance?.Q<Button>();

            if (cancelButton != null)
            {
                Debug.Log("Button trovato");
                cancelButton.RegisterCallback<ClickEvent>(OnCancelClicked);
            }
            else
            {
                Debug.LogWarning("Button NON trovato all'interno di CancelButton");
            }
        }

        private void OnDisable()
        {
            if (continueButton != null)
            {
                continueButton.UnregisterCallback<ClickEvent>(OnContinueClicked);
            }
        }

        private void OnContinueClicked(ClickEvent evt)
        {
            string projectName = projectNameField?.value ?? "<vuoto>";
            string projectDescription = projectDescriptionField?.value ?? "<vuoto>";
            string[] paths = new string[0];
            Debug.Log($"Project Name: {projectName} - Project Description: {projectDescription}");

            projectManager.CreateProject(projectName, projectDescription, paths);
        }

        private void OnCancelClicked(ClickEvent evt)
        {
            gameObject.SetActive(false);
        }

    }
}
