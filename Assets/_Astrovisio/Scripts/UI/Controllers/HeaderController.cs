/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

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
        private AppControlController appControlController;

        private void Awake()
        {
            uiDocument = GetComponentInParent<UIDocument>();
            uiManager = GetComponentInParent<UIManager>();

            projectManager = uiManager.GetProjectManager();
        }

        private void OnEnable()
        {
            EnableNavbar();
            EnableAppControl();
        }

        private void OnDisable()
        {
            DisableNavbar();
            DisableAppControl();
        }

        private void EnableNavbar()
        {
            VisualElement navbarRoot = uiDocument.rootVisualElement.Q<VisualElement>("NavbarRoot");
            navbarController = new NavbarController(projectManager, uiManager, navbarRoot);
        }

        private void DisableNavbar()
        {
            if (navbarController != null)
            {
                navbarController.Dispose();
            }
        }

        private void EnableAppControl()
        {
            VisualElement appControlRoot = uiDocument.rootVisualElement.Q<VisualElement>("AppControls");
            appControlController = new AppControlController(appControlRoot);
        }

        private void DisableAppControl()
        {
            if (appControlController != null)
            {
                appControlController.Dispose();
            }
        }

    }
}
