using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class HeaderController : MonoBehaviour
    {

        [Header("UI Templates")]
        [SerializeField] private VisualTreeAsset projectButtonTemplate;

        // === References ===
        private UIDocument uiDocument;
        private UIController uiController;
        private ProjectManager projectManager;

        // === Controllers ===
        private NavbarController navbarController;

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
            EnableNavbar();

            projectManager.ProjectOpened += OnProjectOpened;
            projectManager.ProjectDeleted += OnProjectDeleted;
        }

        private void OnDisable()
        {
            projectManager.ProjectOpened -= OnProjectOpened;
            projectManager.ProjectDeleted -= OnProjectDeleted;
        }

        private void OnProjectOpened(Project project)
        {
            Debug.Log($"[NavbarController] Progetto aggiunto alla navbar: ID {project.Id}");
            throw new NotImplementedException();
        }

        private void OnProjectDeleted(int projectId)
        {
            Console.WriteLine($"[NavbarController] Progetto rimosso dalla navbar: ID {projectId}");
        }

        private void EnableNavbar()
        {
            navbarController = new NavbarController(projectManager, projectButtonTemplate);
            var headerRoot = uiDocument.rootVisualElement.Q<VisualElement>("Header");
            Debug.Log("EnableNavbar");
            navbarController.Initialize(headerRoot);
        }

    }
}
