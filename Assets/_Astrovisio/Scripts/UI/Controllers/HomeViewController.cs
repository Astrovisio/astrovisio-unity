using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;

namespace Astrovisio
{
    public class HomeViewController
    {
        // === Dependencies ===
        private readonly ProjectManager projectManager;
        private readonly VisualTreeAsset projectRowHeaderTemplate;
        private readonly VisualTreeAsset projectRowTemplate;

        // === Local ===
        private VisualElement root;
        private readonly string[] periodHeaderLabel = new string[3] { "Last week", "Last month", "Older" };

        public HomeViewController(ProjectManager projectManager, VisualTreeAsset projectRowHeaderTemplate, VisualTreeAsset projectRowTemplate)
        {
            this.projectManager = projectManager;
            this.projectRowHeaderTemplate = projectRowHeaderTemplate;
            this.projectRowTemplate = projectRowTemplate;
        }

        public void Initialize(VisualElement root)
        {
            this.root = root;
            UpdateHomeView();

            projectManager.ProjectsFetched += OnProjectsFetched;
        }

        private void OnProjectsFetched(List<Project> list)
        {
            UpdateHomeView();
        }

        private void UpdateHomeView()
        {
            VisualElement projectScrollView = root.Q<ScrollView>("ProjectScrollView");
            projectScrollView.Clear();

            DateTime now = DateTime.UtcNow;
            var projectList = projectManager.GetProjectList();
            // projectList = projectManager.GetFakeProjectList();

            UnityEngine.Debug.Log(projectList.Count);

            var lastWeekProjectList = projectList
                .Where(p => (p.LastOpened.HasValue && (now - p.LastOpened.Value).TotalDays <= 7) || (p.Created.HasValue && (now - p.Created.Value).TotalDays <= 7))
                .ToList();

            var lastMonthProjectList = projectList
                .Where(p => p.LastOpened.HasValue)
                .Where(p =>
                {
                    var daysAgo = (now - p.LastOpened.Value).TotalDays;
                    return daysAgo > 7 && daysAgo <= 30;
                })
                .ToList();

            var olderProjectList = projectList
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
            var header = projectRowHeaderTemplate.CloneTree();
            header.Q<Label>("HeaderLabel").text = projectHeader;
            target.Add(header);

            projectList.Sort((p1, p2) =>
            {
                DateTime p1DateTime = (DateTime)(p1.LastOpened ?? p1.Created);
                DateTime p2DateTime = (DateTime)(p2.LastOpened ?? p2.Created);
                return p2DateTime.CompareTo(p1DateTime);
            });

            foreach (var project in projectList)
            {
                var item = projectRowTemplate.CloneTree();
                item.Q<Label>("ProjectNameLabel").text = project.Name;
                item.RegisterCallback<ClickEvent>(_ => projectManager.OpenProject(project.Id));
                target.Add(item);
            }
        }

    }

}
