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

using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public class EditProjectViewController
    {
        public ProjectManager ProjectManager { get; }
        public VisualElement Root { get; }

        private TextField nameTextLabel;
        private TextField descriptionTextLabel;
        private Button continueButton;
        private Button cancelButton;
        private Project projectToEdit;

        public EditProjectViewController(ProjectManager projectManager, VisualElement root)
        {
            ProjectManager = projectManager;
            Root = root;

            nameTextLabel = Root.Q<VisualElement>("ProjectNameInputField")?.Q<TextField>();
            descriptionTextLabel = Root.Q<VisualElement>("ProjectDescriptionInputField")?.Q<TextField>();
            continueButton = Root.Q<VisualElement>("ContinueButton")?.Q<Button>();
            cancelButton = Root.Q<VisualElement>("CancelButton")?.Q<Button>();
        }

        public void SetProjectToEdit(Project project)
        {
            continueButton.clicked += OnContinueClicked;
            cancelButton.clicked += OnCancelClicked;
            projectToEdit = project;
            nameTextLabel.value = project.Name;
            descriptionTextLabel.value = project.Description;
        }

        private void OnContinueClicked()
        {
            // Debug.Log("OnContinueClicked");
            projectToEdit.Name = nameTextLabel.value;
            projectToEdit.Description = descriptionTextLabel.value;
            _ = ProjectManager.UpdateProject(projectToEdit.Id, projectToEdit);
            Root.RemoveFromClassList("active");
            Dispose();
        }

        private void OnCancelClicked()
        {
            // Debug.Log("OnCancelClicked");
            projectToEdit = null;
            Root.RemoveFromClassList("active");
            Dispose();
        }

        private void Dispose()
        {
            continueButton.clicked -= OnContinueClicked;
            cancelButton.clicked -= OnCancelClicked;
        }

    }

}