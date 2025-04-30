using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class SideController : MonoBehaviour
    {

        [Header("UI Templates")]

        [Space(3)][Header("Home")]
        [SerializeField] private VisualTreeAsset toDo;

        [Space(3)][Header("Project")]
        [SerializeField] private VisualTreeAsset projectSidebarTemplate;
        [SerializeField] private VisualTreeAsset sidebarParamRowTemplate;

        // === References ===
        private UIDocument uiDocument;
        private UIController uiController;
        private ProjectManager projectManager;

        // === Controllers ===
        private SidebarController sidebarController; // TODO ?
        private Dictionary<int, ProjectSidebarController> projectSidebarControllerDictionary = new();


        // === Containers ===
        private VisualElement sideContainer;
        private VisualElement sidebarContainer;
        private List<VisualElement> projectSidebarContainerList = new List<VisualElement>();


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
            sideContainer = uiDocument.rootVisualElement.Q<VisualElement>("Side");

            EnableSidebar();

            projectManager.ProjectOpened += OnProjectOpened;
            projectManager.ProjectUnselected += OnProjectUnselected;
            projectManager.ProjectClosed += OnProjectClosed;
        }

        private void OnDisable()
        {
            projectManager.ProjectOpened -= OnProjectOpened;
            projectManager.ProjectUnselected -= OnProjectUnselected;
            projectManager.ProjectClosed -= OnProjectClosed;
        }

        private void EnableSidebar()
        {
            // Debug.Log("EnableSidebar");
            sidebarController = new SidebarController();
            sidebarContainer = sideContainer.Q<VisualElement>("Sidebar");
            // Debug.Log("Sidebar is: " + sidebarContainer);
        }

        private void OnProjectOpened(Project project)
        {
            // Debug.Log("OnProjectOpened");
            sidebarContainer.style.display = DisplayStyle.None;
            foreach (var controller in projectSidebarControllerDictionary.Values)
            {
                controller.Root.style.display = DisplayStyle.None;
            }

            if (projectSidebarControllerDictionary.TryGetValue(project.Id, out var existingController))
            {
                existingController.Root.style.display = DisplayStyle.Flex;
                return;
            }

            VisualElement projectSidebarInstance = projectSidebarTemplate.CloneTree();
            sideContainer.Add(projectSidebarInstance);

            // var newProjectViewController = new ProjectSidebarController(projectManager, sidebarParamRowTemplate, project, projectSidebarInstance);
            var newProjectViewController = new ProjectSidebarController(projectManager, sidebarParamRowTemplate, projectManager.GetFakeProject(), projectSidebarInstance);
            projectSidebarControllerDictionary[project.Id] = newProjectViewController;
        }

        private void OnProjectUnselected()
        {
            sidebarContainer.style.display = DisplayStyle.Flex;

            foreach (var controller in projectSidebarControllerDictionary.Values)
            {
                controller.Root.style.display = DisplayStyle.None;
            }
        }

        private void OnProjectClosed(Project project)
        {
            foreach (var controller in projectSidebarControllerDictionary.Values)
            {
                controller.Root.style.display = DisplayStyle.None;
            }
            sidebarContainer.style.display = DisplayStyle.Flex;

            projectSidebarControllerDictionary.Remove(project.Id);
        }

    }

}