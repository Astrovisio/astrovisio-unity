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
        }

        private void OnDisable()
        {
            DisableNavbar();
        }

        private void EnableNavbar()
        {
            var navbarRoot = uiDocument.rootVisualElement.Q<VisualElement>("NavbarRoot");
            navbarController = new NavbarController(projectManager, projectButtonTemplate, navbarRoot);
        }

        private void DisableNavbar()
        {
            if (navbarController != null)
            {
                navbarController.Dispose();
            }
        }

    }
}
