using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class MainView : MonoBehaviour
    {

        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private UIDocument mainViewUIDocument;


        private NewProjectViewController newProjectController;
        private Button newProjectButton;


        private void Start()
        {
            if (projectManager)
            {
                newProjectController = new NewProjectViewController(projectManager);
            }
            else
            {
                Debug.LogError("ProjectManager is missing on MainView.");
            }
        }

        private void OnEnable()
        {
            VisualElement root = mainViewUIDocument.rootVisualElement;


            VisualElement newProjectButtonInstance = root.Q<VisualElement>("NewProjectButton");
            newProjectButton = newProjectButtonInstance?.Q<Button>();

            if (newProjectButton != null)
            {
                Debug.Log("Button trovato");
                newProjectButton.RegisterCallback<ClickEvent>(OnNewProjectClicked);
            }
            else
            {
                Debug.LogWarning("Button NON trovato all'interno di NewProjectButton");
            }

        }

        private void OnDisable()
        {
            if (newProjectButton != null)
            {
                newProjectButton.UnregisterCallback<ClickEvent>(OnNewProjectClicked);
            }
        }

        private void OnNewProjectClicked(ClickEvent evt)
        {
            VisualElement root = mainViewUIDocument.rootVisualElement;
            VisualElement newProjectViewInstance = root.Q<VisualElement>("NewProjectView");

            if (newProjectViewInstance != null)
            {
                newProjectViewInstance.style.display = DisplayStyle.Flex;
                newProjectController?.Dispose();
                newProjectController.Initialize(newProjectViewInstance);
            }
        }


    }

}
