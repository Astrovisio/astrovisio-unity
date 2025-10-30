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
        private Button loadJSONButton;

        // === Controllers ===
        private NewProjectViewController newProjectController;
        private HomeViewController homeViewController;
        private Dictionary<int, ProjectViewController> projectViewControllerDictionary = new();

        // === Containers ===
        private VisualElement contentContainer;
        private VisualElement homeViewContainer;
        // private List<VisualElement> projectViewContainerList = new List<VisualElement>();

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
            projectManager.ProjectDeleted += OnProjectDeleted;
        }

        private void Start()
        {
            EnableNewProjectButton();
            EnableLoadJSONButton();
            EnableHomeView();
        }

        private void OnDisable()
        {
            DisableNewProjectButton();
            DisableLoadJSONButton();

            projectManager.ProjectOpened -= OnProjectOpened;
            projectManager.ProjectUnselected -= OnProjectUnselected;
            projectManager.ProjectClosed -= OnProjectClosed;
            projectManager.ProjectDeleted -= OnProjectDeleted;
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

        private void EnableLoadJSONButton()
        {
            VisualElement root = uiDocument.rootVisualElement;
            VisualElement loadJSONButtonInstance = root.Q<VisualElement>("LoadProjectButton");
            loadJSONButton = loadJSONButtonInstance?.Q<Button>();

            if (loadJSONButton != null)
            {
                loadJSONButton.RegisterCallback<ClickEvent>(OnLoadJSONClicked);
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

        private void DisableLoadJSONButton()
        {
            if (loadJSONButton != null)
            {
                loadJSONButton.UnregisterCallback<ClickEvent>(OnLoadJSONClicked);
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

        private void OnLoadJSONClicked(ClickEvent evt)
        {
            projectManager.LoadProjectFromJSON();
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
                // Debug.Log("Existing controller for " + project.Name);
                return;
            }

            VisualElement projectViewInstance = uiContextSO.projectViewTemplate.CloneTree();
            projectViewInstance.name = project.Id.ToString() + "-" + project.Name.ToString();
            contentContainer.Add(projectViewInstance);

            // var newProjectViewController = new ProjectViewController(projectManager, projectViewInstance, projectManager.GetFakeProject(), uiContextSO.paramRowTemplate);
            ProjectViewController newProjectViewController = new ProjectViewController(
                projectManager,
                uiManager,
                projectViewInstance,
                project);
            projectViewControllerDictionary[project.Id] = newProjectViewController;
            // Debug.Log("Created new controller for " + project.Name);
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
            SafeDisposeAndRemove(project);
        }
        private void OnProjectDeleted(Project project)
        {
            SafeDisposeAndRemove(project);
        }

        private void SafeDisposeAndRemove(Project project)
        {
            if (projectViewControllerDictionary.Remove(project.Id, out var controller) && controller != null)
            {
                if (controller.Root != null)
                    contentContainer.Remove(controller.Root);
                controller.Dispose();
            }
        }

    }

}
