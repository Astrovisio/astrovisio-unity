using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class HeaderController : MonoBehaviour
    {

        // === Dependencies ===
        private UIDocument uiDocument;
        private UIManager uiManager;

        // === References ===
        private ProjectManager projectManager;

        // === Controllers ===
        private NavbarController navbarController;

        private void Awake()
        {
            uiDocument = GetComponentInParent<UIDocument>();
            uiManager = GetComponentInParent<UIManager>();

            projectManager = uiManager.GetProjectManager();
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
            navbarController = new NavbarController(projectManager, uiManager, navbarRoot);
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
