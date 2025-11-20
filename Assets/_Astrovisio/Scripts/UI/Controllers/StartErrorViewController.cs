/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
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

using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class StartErrorViewController
    {
        public VisualElement Root { get; }
        public ProjectManager ProjectManager { get; }

        private Button exitButton;
        private Button retryButton;

        public StartErrorViewController(ProjectManager projectManager, VisualElement root)
        {
            ProjectManager = projectManager;
            Root = root;

            exitButton = root.Q<VisualElement>("ExitButton").Q<Button>();
            retryButton = root.Q<VisualElement>("RetryButton").Q<Button>();

            if (exitButton != null)
            {
                exitButton.clicked += OnExitClicked;
            }

            if (retryButton != null)
            {
                retryButton.clicked += OnRetryClicked;
            }
        }

        public void Open()
        {
            Root.AddToClassList("active");
        }

        public void Close()
        {
            Root.RemoveFromClassList("active");
        }

        private void OnExitClicked()
        {
            // Debug.Log("OnExitClicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnRetryClicked()
        {
            Debug.Log("OnRetryClicked");
            Root.RemoveFromClassList("active");
            ProjectManager.FetchAllProjects();
        }

    }

}