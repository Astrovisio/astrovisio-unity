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
        private Dictionary<int, ProjectViewController> projectViewControllerDictionary = new();

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
            projectManager.ProjectClosed += OnProjectClosed;
            projectManager.ProjectUnselected += OnProjectUnselected;
        }

        private void OnDisable()
        {
            DisableNewProjectButton();

            projectManager.ProjectOpened -= OnProjectOpened;
            projectManager.ProjectClosed -= OnProjectClosed;
            projectManager.ProjectUnselected -= OnProjectUnselected;
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
            foreach (var controller in projectViewControllerDictionary.Values)
            {
                controller.Root.style.display = DisplayStyle.None;
            }

            if (projectViewControllerDictionary.TryGetValue(project.Id, out var existingController))
            {
                existingController.Root.style.display = DisplayStyle.Flex;
                return;
            }

            VisualElement projectViewInstance = projectViewTemplate.CloneTree();
            contentContainer.Add(projectViewInstance);

            var newProjectViewController = new ProjectViewController(projectManager, project, projectViewInstance);
            projectViewControllerDictionary[project.Id] = newProjectViewController;
        }

        private void OnProjectUnselected()
        {
            homeViewContainer.style.display = DisplayStyle.Flex;

            foreach (var controller in projectViewControllerDictionary.Values)
            {
                controller.Root.style.display = DisplayStyle.None;
            }
        }

        private void OnProjectClosed(Project project)
        {
            Debug.Log("OnProjectClosed");
            foreach (var controller in projectViewControllerDictionary.Values)
            {
                controller.Root.style.display = DisplayStyle.None;
            }
            homeViewContainer.style.display = DisplayStyle.Flex;

            projectViewControllerDictionary.Remove(project.Id);
        }



    }
}
