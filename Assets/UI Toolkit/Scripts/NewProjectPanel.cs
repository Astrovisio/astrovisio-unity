using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class NewProjectPanel : MonoBehaviour
    {

        [SerializeField] private APIManager apiManager;
        [SerializeField] private UIDocument newProjectView;

        private VisualElement newProjectPanel;
        private TextField projectNameField;
        private TextField projectDescriptionField;
        private Button continueButton;

        private void Start()
        {
            Debug.Log("NewProjectPanel");
        }

        private void OnEnable()
        {
            VisualElement root = newProjectView.rootVisualElement;

            // Trova l'istanza del template
            var continueButtonInstance = root.Q<VisualElement>("ContinueButton");

            // Trova il vero button all'interno dell'istanza
            continueButton = continueButtonInstance.Q<Button>();

            if (continueButton != null)
            {
                Debug.Log("Button trovato");
                // Best practice: utilizza RegisterCallback per gestire l'evento click
                continueButton.RegisterCallback<ClickEvent>(OnContinueClicked);
            }
            else
            {
                Debug.LogWarning("Button NON trovato all'interno di ContinueButton");
            }
        }

        private void OnDisable()
        {
            // Rimuove il callback per evitare memory leak; controlla che continueButton sia non null.
            if (continueButton != null)
            {
                continueButton.UnregisterCallback<ClickEvent>(OnContinueClicked);
            }
        }

        private void OnContinueClicked(ClickEvent evt)
        {
            Debug.Log("test se funziona");
            // string projectName = projectNameField.value;
            // string projectDescription = projectDescriptionField.value;

            // Debug.Log("Project Name: " + projectName);
            // Debug.Log("Project Description: " + projectDescription);

            // Project newProject = new Project
            // {
            //     Name = projectName,
            //     Favourite = false,
            //     Description = projectDescription,
            //     Paths = new string[0]
            // };

            // StartCoroutine(APIManager.Instance.PostProject(newProject));
        }

    }
}
