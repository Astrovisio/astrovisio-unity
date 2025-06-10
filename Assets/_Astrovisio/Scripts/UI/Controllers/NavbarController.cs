using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class NavbarController
    {
        // === Dependencies ===
        private readonly UIManager uiManager;
        private readonly ProjectManager projectManager;
        private readonly VisualTreeAsset projectButtonTemplate;
        private readonly VisualElement root;

        // === Local ===
        private VisualElement homeContainer;
        private VisualElement projectTabContainer;
        private Button homeButton;

        private readonly Dictionary<int, ProjectTabInfo> projectTabDictionary = new();

        public NavbarController(UIManager uiManager, ProjectManager projectManager, VisualTreeAsset projectButtonTemplate, VisualElement root)
        {
            this.uiManager = uiManager;
            this.projectManager = projectManager;
            this.projectButtonTemplate = projectButtonTemplate;
            this.root = root;

            Init();
        }

        public void Init()
        {
            homeContainer = root.Q<VisualElement>("HomeContainer");
            homeButton = homeContainer.Q<Button>();
            if (homeButton != null)
            {
                homeButton.RegisterCallback<ClickEvent>(_ =>
                {
                    SetActiveHome();
                    projectManager.UnselectProject();
                });
                homeButton.AddToClassList("active");
            }

            projectTabContainer = root.Q<VisualElement>("ProjectContainer");
            projectTabContainer.Clear();
            projectTabDictionary.Clear();

            projectManager.ProjectOpened += OnProjectOpened;
            projectManager.ProjectDeleted += OnProjectDeleted;
        }

        private void OnProjectOpened(Project project)
        {
            if (projectTabDictionary.ContainsKey(project.Id))
            {
                SetActiveTab(project.Id);
                return;
            }

            var tabElement = CreateProjectTab(project);
            var tabInfo = new ProjectTabInfo(project, tabElement);
            projectTabDictionary[project.Id] = tabInfo;

            projectTabContainer.Add(tabElement);
            SetActiveTab(project.Id);
            UpdateTabMargins();
        }

        private void OnProjectDeleted(Project project)
        {
            RemoveProjectTab(project.Id);
        }

        private VisualElement CreateProjectTab(Project project)
        {
            var projectTab = projectButtonTemplate.CloneTree();
            projectTab.name = $"ProjectTab_{project.Id}";
            projectTab.Q<Label>("Label").text = project.Name;

            // Left Click
            projectTab.Q<Button>().RegisterCallback<ClickEvent>(_ =>
            {
                SetActiveTab(project.Id);
                projectManager.OpenProject(project.Id);
            });

            // MiddleMouse Click
            projectTab.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.MiddleMouse)
                {
                    RemoveProjectTab(project.Id);
                    evt.StopPropagation();
                }
            });

            // Close Button
            projectTab.Q<Button>("CloseButton").RegisterCallback<ClickEvent>(_ =>
            {
                RemoveProjectTab(project.Id);
            });

            return projectTab;
        }


        private void RemoveProjectTab(int projectId)
        {
            if (!projectTabDictionary.TryGetValue(projectId, out var tabInfo))
            {
                return;
            }

            bool wasActive = tabInfo.TabElement.Q<Button>().ClassListContains("active");

            var before = projectTabContainer.Children().ToList();
            int removedIndex = before.IndexOf(tabInfo.TabElement);

            tabInfo.TabElement.RemoveFromHierarchy();
            projectTabDictionary.Remove(projectId);

            UpdateTabMargins();

            if (!wasActive)
            {
                return;
            }

            var after = projectTabContainer.Children().ToList();
            if (after.Any())
            {
                int newIndex = removedIndex < after.Count ? removedIndex : after.Count - 1;
                var newTab = after.ElementAt(newIndex);
                int newProjectId = projectTabDictionary
                    .First(kvp => kvp.Value.TabElement == newTab)
                    .Key;

                SetActiveTab(newProjectId);
                projectManager.OpenProject(newProjectId);
            }
            else
            {
                SetActiveHome();
                projectManager.UnselectProject();
            }
        }

        private void UpdateTabMargins()
        {
            var children = projectTabContainer.Children().ToList();
            for (int i = 0; i < children.Count; i++)
            {
                children[i].style.marginRight = (i < children.Count - 1) ? 8 : 0;
            }
        }

        private void SetActiveTab(int projectId)
        {
            homeButton?.RemoveFromClassList("active");

            foreach (var tab in projectTabContainer.Children())
            {
                var button = tab.Q<Button>();
                if (tab.name == $"ProjectTab_{projectId}")
                {
                    button.AddToClassList("active");
                }
                else
                {
                    button.RemoveFromClassList("active");
                }
            }
        }

        private void SetActiveHome()
        {
            foreach (var tab in projectTabContainer.Children())
            {
                tab.Q<Button>().RemoveFromClassList("active");
            }

            homeButton?.AddToClassList("active");
            uiManager.SetSceneVisibility(true);
        }

        public void Dispose()
        {
            projectManager.ProjectOpened -= OnProjectOpened;
            projectManager.ProjectDeleted -= OnProjectDeleted;
        }


    }
}
