using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class MainViewController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private UIDocument mainViewUIDocument;

        [Header("UI Templates")]
        [SerializeField] private VisualTreeAsset projectRawHeaderTemplate;
        [SerializeField] private VisualTreeAsset projectRawTemplate;


        private HomeViewController homeProjectViewController;
        private NewProjectViewController newProjectController;
        private Button newProjectButton;

        private void Start()
        {
            if (projectManager)
            {
                homeProjectViewController = new HomeViewController(projectManager);
                newProjectController = new NewProjectViewController(projectManager);

                StartHomeView();
            }
            else
            {
                Debug.LogError("ProjectManager is missing on MainView.");
            }
        }

        private void OnEnable()
        {
            projectManager.ProjectsFetched += OnProjectsFetched;
            projectManager.ApiError += OnApiError;
            projectManager.FetchAllProjects();

            EnableNewProjectButton();
        }

        private void OnDisable()
        {

            projectManager.ProjectsFetched -= OnProjectsFetched;
            projectManager.ApiError -= OnApiError;

            DisableNewProjectButton();
        }

        private void EnableNewProjectButton()
        {
            VisualElement root = mainViewUIDocument.rootVisualElement;

            VisualElement newProjectButtonInstance = root.Q<VisualElement>("NewProjectButton");
            newProjectButton = newProjectButtonInstance?.Q<Button>();

            if (newProjectButton != null)
            {
                newProjectButton.RegisterCallback<ClickEvent>(OnNewProjectClicked);
            }
            else
            {
                Debug.LogWarning("Button NON trovato all'interno di NewProjectButton");
            }
        }

        private void DisableNewProjectButton()
        {
            if (newProjectButton != null)
            {
                newProjectButton.UnregisterCallback<ClickEvent>(OnNewProjectClicked);
            }
        }

        private void OnNewProjectClicked(ClickEvent evt)
        {
            VisualElement root = mainViewUIDocument.rootVisualElement;
            VisualElement newProjectViewInstance = root.Q<VisualElement>("NewProjectView");

            if (newProjectViewInstance != null)
            {
                newProjectViewInstance.AddToClassList("active");
                newProjectController?.Dispose();
                newProjectController.Initialize(newProjectViewInstance);
            }
        }

        private void OnProjectsFetched(List<Project> projects)
        {
            // Now you can access the fetched list of projects
            // Debug.Log("Projects fetched: " + projects.Count);
        }

        private void OnApiError(string error)
        {
            Debug.LogError($"[MainViewController] API error: {error}");
            // Optionally show an error message in UI
        }


        public string[] periodHeaderLabel = new string[3] { "Last week", "Last month", "Older" };

        private void StartHomeView()
        {
            VisualElement root = mainViewUIDocument.rootVisualElement;
            VisualElement homeViewInstance = root.Q<VisualElement>("HomeView");
            VisualElement projectScrollView = homeViewInstance.Q<ScrollView>("ProjectScrollView");
            projectScrollView.Clear();


            DateTime now = DateTime.UtcNow;
            var projectList = projectManager.GetProjectList();
            // projectList = projectManager.GetFakeProjectList();


            var lastWeekProjectList = projectList
                .Where(p => p.LastOpened.HasValue && (now - p.LastOpened.Value).TotalDays <= 7)
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


            // Debug.Log("Last week " + lastWeekProjectList.Count);
            if (lastWeekProjectList.Any())
            {
                var header = projectRawHeaderTemplate.CloneTree();
                header.Q<Label>("HeaderLabel").text = periodHeaderLabel[0];
                projectScrollView.Add(header);
                foreach (var project in lastWeekProjectList)
                {
                    var item = projectRawTemplate.CloneTree();
                    item.Q<Label>("ProjectNameLabel").text = project.Name;
                    projectScrollView.Add(item);
                }
            }

            // Debug.Log("Last month " + lastMonthProjectList.Count);
            if (lastMonthProjectList.Any())
            {
                var header = projectRawHeaderTemplate.CloneTree();
                header.Q<Label>("HeaderLabel").text = periodHeaderLabel[1];
                projectScrollView.Add(header);
                foreach (var project in lastMonthProjectList)
                {
                    var item = projectRawTemplate.CloneTree();
                    item.Q<Label>("ProjectNameLabel").text = project.Name;
                    projectScrollView.Add(item);
                }
            }

            // Debug.Log("Older " + olderProjectList.Count);
            if (olderProjectList.Any())
            {
                var header = projectRawHeaderTemplate.CloneTree();
                header.Q<Label>("HeaderLabel").text = periodHeaderLabel[2];
                projectScrollView.Add(header);
                foreach (var project in olderProjectList)
                {
                    var item = projectRawTemplate.CloneTree();
                    item.Q<Label>("ProjectNameLabel").text = project.Name;
                    projectScrollView.Add(item);
                }
            }

        }
    }
}
