using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;

namespace Astrovisio
{
    public class HomeViewController
    {
        // === Dependencies ===
        public ProjectManager ProjectManager { get; }
        public UIManager UIManager { get; }
        public VisualElement Root { get; }

        private readonly VisualTreeAsset projectRowHeaderTemplate;
        private readonly VisualTreeAsset projectRowTemplate;

        // === Local ===
        private readonly string[] periodHeaderLabel = new string[3] { "Last week", "Last month", "Older" };
        private readonly Dictionary<int, ProjectRowController> projectControllers = new();

        private VisualElement projectScrollView;

        public HomeViewController(ProjectManager projectManager, UIManager uiManager, VisualElement root, VisualTreeAsset projectRowHeaderTemplate, VisualTreeAsset projectRowTemplate)
        {
            ProjectManager = projectManager;
            UIManager = uiManager;
            Root = root;
            this.projectRowHeaderTemplate = projectRowHeaderTemplate;
            this.projectRowTemplate = projectRowTemplate;

            projectScrollView = Root.Q<ScrollView>("ProjectScrollView");
            projectScrollView.Clear();

            projectManager.ProjectOpened += OnProjectsOpened;
            projectManager.ProjectsFetched += OnProjectsFetched;
            projectManager.ProjectCreated += OnProjectCreated;
            projectManager.ProjectUpdated += OnProjectUpdated;
            projectManager.ProjectDeleted += OnProjectDeleted;
        }

        private void OnProjectsOpened(Project project)
        {
            UpdateHomeView();
        }

        private void OnProjectsFetched(List<Project> list)
        {
            UpdateHomeView();
        }

        private void OnProjectCreated(Project project)
        {
            UpdateHomeView();
        }

        private void OnProjectUpdated(Project project)
        {
            UpdateHomeView();
        }

        private void OnProjectDeleted(Project project)
        {
            UpdateHomeView();
        }

        private void UpdateHomeView()
        {
            projectScrollView.Clear();

            DateTime now = DateTime.UtcNow;
            List<Project> projectList = ProjectManager.GetProjectList();
            // projectList = projectManager.GetFakeProjectList();

            // UnityEngine.Debug.Log(projectList.Count);

            List<Project> lastWeekProjectList = projectList
                .Where(p => (p.LastOpened.HasValue && (now - p.LastOpened.Value).TotalDays <= 7) || (p.Created.HasValue && (now - p.Created.Value).TotalDays <= 7))
                .ToList();

            List<Project> lastMonthProjectList = projectList
                .Where(p => p.LastOpened.HasValue)
                .Where(p =>
                {
                    var daysAgo = (now - p.LastOpened.Value).TotalDays;
                    return daysAgo > 7 && daysAgo <= 30;
                })
                .ToList();

            List<Project> olderProjectList = projectList
                .Where(p => p.LastOpened.HasValue && (now - p.LastOpened.Value).TotalDays > 30)
                .ToList();

            if (lastWeekProjectList.Any())
            {
                AddProjectRows(projectScrollView, lastWeekProjectList, periodHeaderLabel[0]);
            }

            if (lastMonthProjectList.Any())
            {
                AddProjectRows(projectScrollView, lastMonthProjectList, periodHeaderLabel[1]);
            }

            if (olderProjectList.Any())
            {
                AddProjectRows(projectScrollView, olderProjectList, periodHeaderLabel[2]);
            }

        }

        private void AddProjectRows(VisualElement target, List<Project> projectList, string projectHeader)
        {
            TemplateContainer header = projectRowHeaderTemplate.CloneTree();
            header.Q<Label>("HeaderLabel").text = projectHeader;
            target.Add(header);

            projectList.Sort((p1, p2) =>
            {
                DateTime p1DateTime = (DateTime)(p1.LastOpened ?? p1.Created);
                DateTime p2DateTime = (DateTime)(p2.LastOpened ?? p2.Created);
                return p2DateTime.CompareTo(p1DateTime);
            });

            foreach (Project project in projectList)
            {
                TemplateContainer projectRow = projectRowTemplate.CloneTree();

                // Debug.Log("AddProjectRows " + project.Name + " " + project.Id);
                ProjectRowController controller = new ProjectRowController(ProjectManager, UIManager, project, projectRow);
                projectControllers[project.Id] = controller;

                projectRow.RegisterCallback<ClickEvent>(_ => ProjectManager.OpenProject(project.Id));
                target.Add(projectRow);
            }
        }

    }
}
