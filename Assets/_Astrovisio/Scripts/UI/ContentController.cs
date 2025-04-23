using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ContentController : MonoBehaviour
    {
        [Header("UI Templates")]
        [SerializeField] private VisualTreeAsset projectRowHeaderTemplate;
        [SerializeField] private VisualTreeAsset projectRowTemplate;
        [SerializeField] private VisualTreeAsset projectViewTemplate;

        // === References ===
        private UIDocument uiDocument;
        private UIController uiController;
        private ProjectManager projectManager;
        private Button newProjectButton;

        // === Controllers ===
        private NewProjectViewController newProjectController;
        private HomeViewController homeViewController;
        private List<ProjectViewController> projectViewControllerList = new List<ProjectViewController>();

        // === Containers ===
        private VisualElement contentContainer;
        private VisualElement homeViewContainer;
        private List<VisualElement> projectViewContainerList = new List<VisualElement>();

        private void Awake()
        {
            uiDocument = GetComponentInParent<UIDocument>();
            uiController = GetComponentInParent<UIController>();
            projectManager = uiController.GetProjectManager();

            if (projectManager == null)
            {
                Debug.LogError("ProjectManager not found.");
            }
        }

        private void OnEnable()
        {
            contentContainer = uiDocument.rootVisualElement.Q<VisualElement>("Content");

            EnableNewProjectButton();
            EnableHomeView();

            projectManager.ProjectOpened += OnProjectOpened;
            projectManager.ProjectUnselected += OnProjectUnselected;
        }

        private void OnDisable()
        {
            DisableNewProjectButton();
        }

        private void EnableNewProjectButton()
        {
            newProjectController = new NewProjectViewController(projectManager);
            VisualElement root = uiDocument.rootVisualElement;
            VisualElement newProjectButtonInstance = root.Q<VisualElement>("NewProjectButton");
            newProjectButton = newProjectButtonInstance?.Q<Button>();

            if (newProjectButton != null)
            {
                newProjectButton.RegisterCallback<ClickEvent>(OnNewProjectClicked);
            }
            else
            {
                Debug.LogWarning("Button not found.");
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
            VisualElement root = uiDocument.rootVisualElement;
            VisualElement newProjectViewInstance = root.Q<VisualElement>("NewProjectView");

            if (newProjectViewInstance != null)
            {
                newProjectViewInstance.AddToClassList("active");
                newProjectController?.Dispose();
                newProjectController.Initialize(newProjectViewInstance);
            }
        }

        private void EnableHomeView()
        {
            homeViewController = new HomeViewController(projectManager, projectRowHeaderTemplate, projectRowTemplate);
            homeViewController.Initialize(contentContainer);
            homeViewContainer = contentContainer.Q<VisualElement>("HomeView");
        }

        private void OnProjectOpened(Project project)
        {
            homeViewContainer.style.display = DisplayStyle.None;

            // Verifica se esiste già una view per questo progetto
            // var existingController = projectViewControllerList.FirstOrDefault(c => c.GetProject().Id == project.Id);
            // if (existingController != null)
            // {
            //     foreach (var view in projectViewContainerList)
            //         view.style.display = DisplayStyle.None;

            //     existingController.Root.style.display = DisplayStyle.Flex;
            //     return;
            // }

            // Altrimenti crea e aggiunge
            VisualElement projectViewInstance = projectViewTemplate.CloneTree();
            contentContainer.Add(projectViewInstance);

            var controller = new ProjectViewController(projectManager, project);
            controller.Initialize(projectViewInstance);
            // controller.Root = projectViewInstance;

            projectViewControllerList.Add(controller);
            projectViewContainerList.Add(projectViewInstance);
        }

        private void OnProjectUnselected()
        {
            homeViewContainer.style.display = DisplayStyle.Flex;

            foreach (var view in projectViewContainerList)
            {
                view.style.display = DisplayStyle.None;
            }
        }
        
    }
}
