using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace Astrovisio
{
    public class HomeSidebarController
    {

        // === Dependencies ===
        private ProjectManager ProjectManager { get; }
        private VisualElement Root { get; }
        private UIContextSO UIContextSO { get; }

        public event Action<string> SearchValueChanged;

        private TextField searchTextField;
        private Button clearSearchButton;
        private ScrollView favouritesScrollView;
        private Label versionLabel;

        public HomeSidebarController(ProjectManager projectManager, VisualElement root, UIContextSO uiContextSO)
        {
            ProjectManager = projectManager;
            Root = root;
            UIContextSO = uiContextSO;

            ProjectManager.ProjectsFetched += OnProjectFetched;
            ProjectManager.ProjectCreated += OnProjectCreated;
            ProjectManager.ProjectUpdated += OnProjectUpdated;
            ProjectManager.ProjectDeleted += OnProjectDeleted;

            InitSearchField();
            InitFavouriteScrollView();
            InitVersionLabel();
        }

        private void InitSearchField()
        {
            searchTextField = Root.Q<VisualElement>("SearchContainer")?.Q<TextField>();
            clearSearchButton = Root.Q<VisualElement>("SearchContainer")?.Q<Button>();

            if (clearSearchButton != null && searchTextField != null)
            {
                clearSearchButton.clicked += () =>
                {
                    searchTextField.value = string.Empty;
                };

                searchTextField.RegisterValueChangedCallback(evt =>
                {
                    OnSearchValueChanged(evt.newValue);
                });
            }
        }

        private void OnSearchValueChanged(string newValue)
        {
            // Debug.Log("Search updated: " + newValue);
            SearchValueChanged?.Invoke(newValue);
        }

        private void InitVersionLabel()
        {
            versionLabel = Root.Q<Label>("AstrovisioLabel");
            versionLabel.text = "<u>©INAF Astrovisio v " + Application.version + "</u>";
        }

        private void InitFavouriteScrollView()
        {
            var favouritesContainer = Root.Q<VisualElement>("FavouritesContainer");
            favouritesScrollView = favouritesContainer.Q<ScrollView>("ProjectScrollView");

            favouritesScrollView.Clear();
            UpdateFavouriteScrollView();
        }

        private void UpdateFavouriteScrollView()
        {
            List<Project> projectList = ProjectManager.GetProjectList();

            favouritesScrollView.Clear();

            foreach (Project project in projectList)
            {

                if (!project.Favourite)
                {
                    continue;
                }

                // Debug.Log("Checking project " + project.Name);

                TemplateContainer favouriteProjectTemplate = UIContextSO.favouriteProjectButton.CloneTree();

                Button favouriteButton = favouriteProjectTemplate.Q<Button>();
                favouriteButton.text = project.Name;

                favouriteButton.clicked += () =>
                {
                    // Debug.Log(project.Name);
                    ProjectManager.OpenProject(project.Id);
                };

                favouritesScrollView.Add(favouriteProjectTemplate);
            }
        }


        private void OnProjectUpdated(Project project)
        {
            // Debug.Log("OnProjectUpdated");
            UpdateFavouriteScrollView();
        }

        private void OnProjectCreated(Project project)
        {
            // Debug.Log("OnProjectCreated");
            UpdateFavouriteScrollView();
        }

        private void OnProjectFetched(List<Project> list)
        {
            // Debug.Log("OnProjectFetched");
            UpdateFavouriteScrollView();
        }

        private void OnProjectDeleted(Project project)
        {
            // Debug.Log("OnProjectDeleted");
            UpdateFavouriteScrollView();
        }

    }

}
