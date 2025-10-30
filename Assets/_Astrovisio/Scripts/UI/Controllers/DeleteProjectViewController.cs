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

    public class DeleteProjectViewController
    {
        public ProjectManager ProjectManager { get; }
        public VisualElement Root { get; }

        private Button deleteButton;
        private Button cancelButton;
        private Project projectToDelete;

        public DeleteProjectViewController(ProjectManager projectManager, VisualElement root)
        {
            ProjectManager = projectManager;
            Root = root;

            deleteButton = Root.Q<VisualElement>("ContinueButton")?.Q<Button>();
            cancelButton = Root.Q<VisualElement>("CancelButton")?.Q<Button>();

            deleteButton.clicked += OnDeleteClicked;
            cancelButton.clicked += OnCancelClicked;
        }

        public void SetProjectToDelete(Project project)
        {
            projectToDelete = project;
        }

        private void OnDeleteClicked()
        {
            // Debug.Log("OnDeleteClicked");
            foreach (File file in projectToDelete.Files)
            {
                SettingsManager.Instance.RemoveSettings(projectToDelete.Id, file.Id);
            }
            ReelManager.Instance.RemoveReel(projectToDelete.Id);
            ProjectManager.DeleteProject(projectToDelete.Id, projectToDelete);
            Root.RemoveFromClassList("active");
        }

        private void OnCancelClicked()
        {
            // Debug.Log("OnCancelClicked");
            projectToDelete = null;
            Root.RemoveFromClassList("active");
        }

    }

}