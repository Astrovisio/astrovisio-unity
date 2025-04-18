using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class MainViewController : MonoBehaviour
    {

        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private UIDocument mainViewUIDocument;

        [SerializeField] private VisualTreeAsset projectRawHeaderTemplate;
        [SerializeField] private VisualTreeAsset projectRawTemplate;

        private HomeViewController homeProjectViewController;
        private NewProjectViewController newProjectController;
        private Button newProjectButton;

        private void Start()
        {
            if (projectManager)
            {
                homeProjectViewController = new HomeViewController(projectManager);
                newProjectController = new NewProjectViewController(projectManager);

                StartHomeView();
            }
            else
            {
                Debug.LogError("ProjectManager is missing on MainView.");
            }
        }

        private void OnEnable()
        {
            EnableNewProjectButton();
        }

        private void OnDisable()
        {
            DisableNewProjectButton();
        }

        private void EnableNewProjectButton()
        {
            VisualElement root = mainViewUIDocument.rootVisualElement;

            VisualElement newProjectButtonInstance = root.Q<VisualElement>("NewProjectButton");
            newProjectButton = newProjectButtonInstance?.Q<Button>();

            if (newProjectButton != null)
            {
                // Debug.Log("Button trovato");
                newProjectButton.RegisterCallback<ClickEvent>(OnNewProjectClicked);
            }
            else
            {
                Debug.LogWarning("Button NON trovato all'interno di NewProjectButton");
            }
        }

        private void DisableNewProjectButton()
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
                // newProjectViewInstance.style.display = DisplayStyle.Flex;
                newProjectViewInstance.AddToClassList("active");
                newProjectController?.Dispose();
                newProjectController.Initialize(newProjectViewInstance);
            }
        }

        public string[] periodHeaderLabel = new string[3] { "Last week", "Last month", "Older" };

        private void StartHomeView()
        {
            VisualElement root = mainViewUIDocument.rootVisualElement;
            VisualElement homeViewInstance = root.Q<VisualElement>("HomeView");
            VisualElement projectScrollView = homeViewInstance.Q<ScrollView>("ProjectScrollView");
            projectScrollView.Clear();

            projectManager.FetchAllProjects();

            if (true) // Last week
            {
                VisualElement projectRawHeaderInstance = projectRawHeaderTemplate.CloneTree();
                Label headerLabel = projectRawHeaderInstance.Q<Label>("HeaderLabel");
                headerLabel.text = periodHeaderLabel[0];
                projectScrollView.Add(projectRawHeaderInstance);
                for (int i = 0; i < 3; i++)
                {
                    VisualElement projectRawInstance = projectRawTemplate.CloneTree();
                    projectScrollView.Add(projectRawInstance);
                }
            }

            if (true) // Last month
            {
                VisualElement projectRawHeaderInstance = projectRawHeaderTemplate.CloneTree();
                Label headerLabel = projectRawHeaderInstance.Q<Label>("HeaderLabel");
                headerLabel.text = periodHeaderLabel[1];
                projectScrollView.Add(projectRawHeaderInstance);
                for (int i = 0; i < 3; i++)
                {
                    VisualElement projectRawInstance = projectRawTemplate.CloneTree();
                    projectScrollView.Add(projectRawInstance);
                }
            }

            if (true) // Older
            {
                VisualElement projectRawHeaderInstance = projectRawHeaderTemplate.CloneTree();
                Label headerLabel = projectRawHeaderInstance.Q<Label>("HeaderLabel");
                headerLabel.text = periodHeaderLabel[2];
                projectScrollView.Add(projectRawHeaderInstance);
                for (int i = 0; i < 20; i++)
                {
                    VisualElement projectRawInstance = projectRawTemplate.CloneTree();
                    projectScrollView.Add(projectRawInstance);
                }
            }

        }


    }

}
