using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class NavbarController
    {
        private readonly ProjectManager projectManager;
        private readonly VisualTreeAsset projectButtonTemplate;

        private VisualElement root;
        private VisualElement homeContainer;
        private VisualElement projectContainer;
        private Button homeButton;
        private readonly List<Project> openProjects = new List<Project>();

        public NavbarController(ProjectManager projectManager, VisualTreeAsset projectButtonTemplate)
        {
            this.projectManager = projectManager;
            this.projectButtonTemplate = projectButtonTemplate;
        }

        public void Initialize(VisualElement root)
        {
            this.root = root;

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

            projectContainer = root.Q<VisualElement>("ProjectContainer");
            projectContainer.Clear();
            openProjects.Clear();

            projectManager.ProjectOpened += OnProjectOpened;
            projectManager.ProjectDeleted += OnProjectDeleted;
        }

        private void OnProjectOpened(Project project)
        {
            if (openProjects.Any(p => p.Id == project.Id))
            {
                SetActiveTab(project.Id);
                return;
            }

            openProjects.Add(project);
            AddProjectButton(root.Q<VisualElement>("ProjectContainer"), project);
            SetActiveTab(project.Id);
        }

        private void OnProjectDeleted(int projectId)
        {
            RemoveProjectTab(projectId);
        }

        private void AddProjectButton(VisualElement container, Project project)
        {
            var tab = projectButtonTemplate.CloneTree();
            tab.name = $"ProjectTab_{project.Id}";
            tab.Q<Label>("Label").text = project.Name;

            var mainBtn = tab.Q<Button>();

            mainBtn.RegisterCallback<ClickEvent>(_ =>
            {
                SetActiveTab(project.Id);
                projectManager.OpenProject(project.Id);
            });

            var closeBtn = tab.Q<Button>("CloseButton");
            closeBtn.RegisterCallback<ClickEvent>(_ =>
            {
                RemoveProjectTab(project.Id);
            });

            container.Add(tab);
            UpdateTabMargins(container);
        }

        private void RemoveProjectTab(int projectId)
        {
            openProjects.RemoveAll(p => p.Id == projectId);

            var container = root.Q<VisualElement>("ProjectContainer");
            var tab = container.Q<VisualElement>($"ProjectTab_{projectId}");
            if (tab != null)
            {
                bool wasActive = tab.Q<Button>().ClassListContains("active");
                tab.RemoveFromHierarchy();

                if (wasActive)
                {
                    if (openProjects.Any())
                        SetActiveTab(openProjects.Last().Id);
                    else
                        SetActiveHome();
                }
            }

            UpdateTabMargins(container);
        }

        private void UpdateTabMargins(VisualElement container)
        {
            var children = container.Children().ToList();
            for (int i = 0; i < children.Count; i++)
            {
                children[i].style.marginRight = (i < children.Count - 1) ? 8 : 0;
            }
        }

        private void SetActiveTab(int projectId)
        {
            var container = root.Q<VisualElement>("ProjectContainer");

            // Disattiva il pulsante home
            homeButton?.RemoveFromClassList("active");

            foreach (var tab in container.Children())
            {
                var button = tab.Q<Button>();
                if (tab.name == $"ProjectTab_{projectId}")
                    button.AddToClassList("active");
                else
                    button.RemoveFromClassList("active");
            }
        }

        private void SetActiveHome()
        {
            var container = root.Q<VisualElement>("ProjectContainer");

            // Disattiva tutti i project button
            foreach (var tab in container.Children())
            {
                var button = tab.Q<Button>();
                button.RemoveFromClassList("active");
            }

            // Attiva il pulsante home
            homeButton?.AddToClassList("active");
        }

        public void Dispose()
        {
            projectManager.ProjectOpened -= OnProjectOpened;
            projectManager.ProjectDeleted -= OnProjectDeleted;
        }
    }
}
