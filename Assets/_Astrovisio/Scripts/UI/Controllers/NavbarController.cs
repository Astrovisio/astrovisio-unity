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
        public ProjectManager ProjectManager { get; set; }
        public UIManager UIManager { get; set; }
        public VisualElement Root { private set; get; }

        // === Local ===
        private VisualElement homeContainer;
        private VisualElement projectTabContainer;
        private Button homeButton;

        private readonly Dictionary<int, ProjectTabInfo> projectTabDictionary = new();

        public NavbarController(
            ProjectManager projectManager,
            UIManager uiManager,
            VisualElement root)
        {
            ProjectManager = projectManager;
            UIManager = uiManager;
            Root = root;

            Init();
        }

        public void Init()
        {
            homeContainer = Root.Q<VisualElement>("HomeContainer");
            homeButton = homeContainer.Q<Button>();
            if (homeButton != null)
            {
                homeButton.RegisterCallback<ClickEvent>(_ =>
                {
                    SetActiveHome();
                    ProjectManager.UnselectProject();
                });
                homeButton.AddToClassList("active");
            }

            projectTabContainer = Root.Q<VisualElement>("ProjectContainer");
            projectTabContainer.Clear();
            projectTabDictionary.Clear();

            ProjectManager.ProjectOpened += OnProjectOpened;
            ProjectManager.ProjectUpdated += OnProjectUpdated;
            ProjectManager.ProjectDeleted += OnProjectDeleted;
        }

        private void OnProjectOpened(Project project)
        {
            if (projectTabDictionary.ContainsKey(project.Id))
            {
                SetActiveTab(project.Id);
                return;
            }

            VisualElement tabElement = CreateProjectTab(project);
            ProjectTabInfo tabInfo = new ProjectTabInfo(project, tabElement);
            projectTabDictionary[project.Id] = tabInfo;

            projectTabContainer.Add(tabElement);
            SetActiveTab(project.Id);
            UpdateTabMargins();
        }

        private void OnProjectUpdated(Project updatedProject)
        {
            if (projectTabDictionary.TryGetValue(updatedProject.Id, out ProjectTabInfo projectTabInfo))
            {
                projectTabInfo.TabElement.Q<Label>("Label").text = updatedProject.Name;
            }
        }

        private void OnProjectDeleted(Project project)
        {
            RemoveProjectTab(project.Id);
        }

        private VisualElement CreateProjectTab(Project project)
        {
            TemplateContainer projectTab = UIManager.GetUIContext().projectButtonTemplate.CloneTree();
            projectTab.name = $"ProjectTab_{project.Id}";
            projectTab.Q<Label>("Label").text = project.Name;

            // Left Click
            projectTab.Q<Button>().RegisterCallback<ClickEvent>(_ =>
            {
                SetActiveTab(project.Id);
                ProjectManager.OpenProject(project.Id);
            });

            // MiddleMouse Click
            projectTab.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.MiddleMouse)
                {
                    RemoveProjectTab(project.Id);
                    // evt.StopPropagation();
                    ProjectManager.CloseProject(project.Id);
                }
            });

            // Close Button
            projectTab.Q<Button>("CloseButton").RegisterCallback<ClickEvent>(evt =>
            {
                RemoveProjectTab(project.Id);
                // evt.StopPropagation();
                ProjectManager.CloseProject(project.Id);
            });

            return projectTab;
        }


        private void RemoveProjectTab(int projectId)
        {
            // Debug.Log("RemoveProjectTab " + projectId);

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

            List<VisualElement> after = projectTabContainer.Children().ToList();
            if (after.Any())
            {
                int newIndex = removedIndex < after.Count ? removedIndex : after.Count - 1;
                var newTab = after.ElementAt(newIndex);
                int newProjectId = projectTabDictionary
                    .First(kvp => kvp.Value.TabElement == newTab)
                    .Key;

                SetActiveTab(newProjectId);
                ProjectManager.OpenProject(newProjectId);
            }
            else
            {
                SetActiveHome();
                ProjectManager.UnselectProject();
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
            UIManager.SetSceneVisibility(true);
        }

        public void Dispose()
        {
            ProjectManager.ProjectOpened -= OnProjectOpened;
            ProjectManager.ProjectUpdated -= OnProjectUpdated;
            ProjectManager.ProjectDeleted -= OnProjectDeleted;
        }


    }
}
