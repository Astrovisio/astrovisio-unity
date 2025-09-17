using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ContentController : MonoBehaviour
    {

        // === Dependencies ===
        private UIDocument uiDocument;
        private UIManager uiManager;
        private SideController sideController;

        // === References ===
        private ProjectManager projectManager;
        private UIContextSO uiContextSO;
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
            uiManager = GetComponentInParent<UIManager>();
            sideController = transform.parent.GetComponentInChildren<SideController>();

            projectManager = uiManager.GetProjectManager();
            uiContextSO = uiManager.GetUIContext();
        }

        private void OnEnable()
        {
            contentContainer = uiDocument.rootVisualElement.Q<VisualElement>("Content");

            // EnableNewProjectButton();
            // EnableHomeView();

            projectManager.ProjectOpened += OnProjectOpened;
            projectManager.ProjectUnselected += OnProjectUnselected;
            projectManager.ProjectClosed += OnProjectClosed;
        }

        private void Start()
        {
            EnableNewProjectButton();
            EnableHomeView();
        }

        private void OnDisable()
        {
            DisableNewProjectButton();

            projectManager.ProjectOpened -= OnProjectOpened;
            projectManager.ProjectUnselected -= OnProjectUnselected;
            projectManager.ProjectClosed -= OnProjectClosed;
        }

        private void EnableNewProjectButton()
        {
            newProjectController = new NewProjectViewController(projectManager, uiManager);
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
                newProjectController.Init(newProjectViewInstance);
            }
        }

        private void EnableHomeView()
        {
            homeViewController = new HomeViewController(projectManager, uiManager, contentContainer, uiContextSO, sideController);
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

            VisualElement projectViewInstance = uiContextSO.projectViewTemplate.CloneTree();
            contentContainer.Add(projectViewInstance);

            // var newProjectViewController = new ProjectViewController(projectManager, projectViewInstance, projectManager.GetFakeProject(), uiContextSO.paramRowTemplate);
            ProjectViewController newProjectViewController = new ProjectViewController(
                projectManager,
                uiManager,
                projectViewInstance,
                project,
                project.Files[0]); // GB
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
            foreach (var controller in projectViewControllerDictionary.Values)
            {
                controller.Root.style.display = DisplayStyle.None;
            }
            homeViewContainer.style.display = DisplayStyle.Flex;

            projectViewControllerDictionary.Remove(project.Id);
        }

    }
    
}
