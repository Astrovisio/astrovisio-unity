/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
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
using UnityEngine;
using UnityEngine.UIElements;


namespace Astrovisio
{
    public class HomeSidebarController
    {

        // === Dependencies ===
        private ProjectManager ProjectManager { get; }
        private VisualElement Root { get; }
        private UIManager UIManager { get; }
        private UIContextSO UIContextSO { get; }

        public event Action<string> SearchValueChanged;

        private TextField searchTextField;
        private Button clearSearchButton;
        private ScrollView favouritesScrollView;
        private Button creditsButton;

        public HomeSidebarController(ProjectManager projectManager, VisualElement root, UIManager uiManager, UIContextSO uiContextSO)
        {
            ProjectManager = projectManager;
            Root = root;
            UIManager = uiManager;
            UIContextSO = uiContextSO;

            ProjectManager.ProjectsFetched += OnProjectFetched;
            ProjectManager.ProjectCreated += OnProjectCreated;
            ProjectManager.ProjectUpdated += OnProjectUpdated;
            ProjectManager.ProjectDeleted += OnProjectDeleted;

            InitSearchField();
            InitFavouriteScrollView();
            InitCreditsButton();
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

        private void InitCreditsButton()
        {
            creditsButton = Root.Q<Button>("Credits");
            creditsButton.text = "<u>Astrovisio v " + Application.version + " - Credits</u>";

            creditsButton.clicked += () => UIManager.SetAboutViewVisibility(true);
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
                    _ = ProjectManager.OpenProject(project.Id);
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
