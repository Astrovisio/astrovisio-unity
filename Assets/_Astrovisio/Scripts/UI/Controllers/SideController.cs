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
            projectManager.ProjectDeleted += OnProjectDeleted;
        }

        private void OnDisable()
        {
            projectManager.ProjectOpened -= OnProjectOpened;
            projectManager.ProjectUnselected -= OnProjectUnselected;
            projectManager.ProjectClosed -= OnProjectClosed;
            projectManager.ProjectDeleted -= OnProjectDeleted;
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
            projectSidebarInstance.name = project.Id.ToString() + "-" + project.Name.ToString();
            sideContainer.Add(projectSidebarInstance);

            // var newProjectViewController = new ProjectSidebarController(projectManager, sidebarParamRowTemplate, projectManager.GetFakeProject(), projectSidebarInstance);
            // Debug.Log("@@@" + (projectManager.GetProject(project.Id) == project));
            ProjectSidebarController newProjectViewController = new ProjectSidebarController(uiManager, projectManager, uiContextSO, project, projectSidebarInstance);
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
            SafeDisposeAndRemove(project);
        }

        private void OnProjectDeleted(Project project)
        {
            SafeDisposeAndRemove(project);
        }

        private void SafeDisposeAndRemove(Project project)
        {
            if (projectSidebarControllerDictionary.Remove(project.Id, out var controller) && controller != null)
            {
                if (controller.Root != null)
                    sideContainer.Remove(controller.Root);
                controller.Dispose();
            }
        }

    }

}