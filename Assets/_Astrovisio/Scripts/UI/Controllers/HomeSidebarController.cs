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

        private ScrollView favouritesScrollView;

        public HomeSidebarController(ProjectManager projectManager, VisualElement root, UIContextSO uiContextSO)
        {
            ProjectManager = projectManager;
            Root = root;
            UIContextSO = uiContextSO;

            ProjectManager.ProjectsFetched += OnProjectFetched;
            ProjectManager.ProjectCreated += OnProjectCreated;
            ProjectManager.ProjectUpdated += OnProjectUpdated;

            InitFavouriteScrollView();
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

    }

}
