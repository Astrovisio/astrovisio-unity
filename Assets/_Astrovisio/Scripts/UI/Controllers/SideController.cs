using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class SideController : MonoBehaviour
    {

        // === Dependencies ===
        private UIDocument uiDocument;
        private UIManager uiManager;

        // === References ===
        private ProjectManager projectManager;
        private UIContextSO uiContextSO;

        // === Controllers ===
        private HomeSidebarController sidebarController; // TODO ?
        private Dictionary<int, ProjectSidebarController> projectSidebarControllerDictionary = new();


        // === Containers ===
        private VisualElement sideContainer;
        private VisualElement sidebarContainer;
        private List<VisualElement> projectSidebarContainerList = new List<VisualElement>();


        private void Awake()
        {
            uiDocument = GetComponentInParent<UIDocument>();
            uiManager = GetComponentInParent<UIManager>();

            projectManager = uiManager.GetProjectManager();
            uiContextSO = uiManager.GetUIContext();

            if (projectManager == null)
            {
                Debug.LogError("ProjectManager not found.");
            }
        }

        private void OnEnable()
        {
            sideContainer = uiDocument.rootVisualElement.Q<VisualElement>("Side");

            EnableHomeSidebar();

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

        private void EnableHomeSidebar()
        {
            sidebarContainer = sideContainer.Q<VisualElement>("Sidebar");
            sidebarController = new HomeSidebarController(projectManager, sidebarContainer, uiManager, uiContextSO);
        }

        public HomeSidebarController GetHomeSidebarController()
        {
            return sidebarController;
        }

        private void OnProjectOpened(Project project)
        {
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

            // VisualElement projectSidebarInstance = projectSidebarTemplate.CloneTree();
            VisualElement projectSidebarInstance = uiContextSO.projectSidebarTemplate.CloneTree();
            sideContainer.Add(projectSidebarInstance);

            // var newProjectViewController = new ProjectSidebarController(projectManager, sidebarParamRowTemplate, projectManager.GetFakeProject(), projectSidebarInstance);
            // Debug.Log("@@@" + (projectManager.GetProject(project.Id) == project));
            var newProjectViewController = new ProjectSidebarController(uiManager, projectManager, uiContextSO, project, projectSidebarInstance);
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