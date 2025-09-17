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
        public UIContextSO UIContextSO { get; }
        public SideController SideController { get; }

        // === Local ===
        private readonly string[] periodHeaderLabel = new string[3] { "Last week", "Last month", "Older" };
        private readonly Dictionary<int, ProjectRowController> projectControllers = new();

        private VisualElement projectScrollView;
        private bool isSearching = false;
        private string searchValue = "";

        public HomeViewController(
            ProjectManager projectManager,
            UIManager uiManager,
            VisualElement root,
            UIContextSO uiContextSO,
            SideController sideController)
        {
            ProjectManager = projectManager;
            UIManager = uiManager;
            Root = root;
            UIContextSO = uiContextSO;
            SideController = sideController;

            projectScrollView = Root.Q<ScrollView>("ProjectScrollView");
            projectScrollView.Clear();

            ProjectManager.ProjectOpened += OnProjectsOpened;
            ProjectManager.ProjectsFetched += OnProjectsFetched;
            ProjectManager.ProjectCreated += OnProjectCreated;
            ProjectManager.ProjectUpdated += OnProjectUpdated;
            ProjectManager.ProjectDeleted += OnProjectDeleted;
            SideController.GetHomeSidebarController().SearchValueChanged += OnSearchValueChanged;
        }

        private void OnSearchValueChanged(string obj)
        {
            // Debug.Log(obj);
            isSearching = obj.Length > 0;
            searchValue = obj;
            UpdateHomeView();
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
            if (isSearching)
            {
                UpdateSearchView();
            }
            else
            {
                UpdateProjectView();
            }
        }

        private void AddProjectRows(VisualElement target, List<Project> projectList, string projectHeader)
        {
            if (!string.IsNullOrEmpty(projectHeader))
            {
                TemplateContainer header = UIContextSO.projectRowHeaderTemplate.CloneTree();
                header.Q<Label>("HeaderLabel").text = projectHeader;
                target.Add(header);
            }

            projectList.Sort((p1, p2) =>
            {
                DateTime p1DateTime = (DateTime)(p1.LastOpened ?? p1.Created);
                DateTime p2DateTime = (DateTime)(p2.LastOpened ?? p2.Created);
                return p2DateTime.CompareTo(p1DateTime);
            });

            foreach (Project project in projectList)
            {
                TemplateContainer projectRow = UIContextSO.projectRowTemplate.CloneTree();

                // Debug.Log("AddProjectRows " + project.Name + " " + project.Id);
                ProjectRowController projectRowController = new ProjectRowController(ProjectManager, UIManager, project, projectRow);
                projectControllers[project.Id] = projectRowController;

                projectRow.RegisterCallback<ClickEvent>(_ => ProjectManager.OpenProject(project.Id));
                target.Add(projectRow);
            }
        }

        private void UpdateProjectView()
        {
            projectScrollView.Clear();

            DateTime now = DateTime.UtcNow;
            List<Project> projectList = ProjectManager.GetProjectList();
            // Debug.Log(string.Join(", ", projectList.Select(p => p.Name)));
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

        private void UpdateSearchView()
        {
            projectScrollView.Clear();

            DateTime now = DateTime.UtcNow;
            List<Project> projectList = ProjectManager.GetProjectList();

            List<Project> filteredProjects = projectList
                .Where(p => p.Name != null && p.Name.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            // foreach (var project in filteredProjects)
            // {
            //     Debug.Log(project.Name);
            // }

            AddProjectRows(projectScrollView, filteredProjects, "");
        }

    }
}
