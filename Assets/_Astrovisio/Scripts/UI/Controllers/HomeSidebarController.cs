using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace Astrovisio
{
    public class HomeSidebarController
    {

        // === Dependencies ===
        private readonly ProjectManager projectManager;

        // === Local ===
        public VisualElement Root { get; }

        public HomeSidebarController(ProjectManager projectManager, VisualElement root, UIContextSO uiContextSO)
        {
            this.projectManager = projectManager;
            Root = root;

            InitFavouriteScrollView();
        }

        private void InitFavouriteScrollView()
        {
            var favouritesContainer = Root.Q<VisualElement>("FavouritesContainer");
            var projectScrollView = favouritesContainer.Q<ScrollView>("ProjectScrollView");

            projectScrollView.Clear();
        }
    }

}
