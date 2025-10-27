/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
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
