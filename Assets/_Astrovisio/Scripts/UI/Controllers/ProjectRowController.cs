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
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ProjectRowController
    {
        // === Dependencies ===
        public ProjectManager ProjectManager { get; set; }
        public UIManager UIManager { get; set; }
        public Project Project { get; set; }
        public VisualElement Root { private set; get; }

        // === UI ===
        private Label projectNameLabel;
        private Label filesLabel;
        private Label lastOpenedLabel;
        private Label createLabel;
        private Toggle favouriteToggle;
        private Button editButton;
        private Button duplicateButton;
        private Button deleteButton;


        public ProjectRowController(
            ProjectManager projectManager,
            UIManager uiManager,
            Project project,
            VisualElement root)
        {
            ProjectManager = projectManager;
            UIManager = uiManager;
            Project = project;
            Root = root;

            // ProjectManager.ProjectOpened += OnProjectOpened;
            // ProjectManager.ProjectCreated += ProjectCreated;

            Init();
        }

        // private void ProjectCreated(Project project)
        // {
        //     throw new NotImplementedException();
        // }

        private void Init()
        {
            projectNameLabel = Root.Q<Label>("ProjectNameLabel");
            filesLabel = Root.Q<Label>("FilesLabel");
            lastOpenedLabel = Root.Q<Label>("LastOpenedLabel");
            createLabel = Root.Q<Label>("CreatedLabel");
            favouriteToggle = Root.Q<VisualElement>("FavouriteToggle")?.Q<Toggle>();
            editButton = Root.Q<Button>("EditButton");
            duplicateButton = Root.Q<Button>("DuplicateButton");
            deleteButton = Root.Q<Button>("DeleteButton");

            projectNameLabel.text = Project.Name;
            filesLabel.text = (Project.Files.Count == 1) ? "1 file" : Project.Files.Count + " files";

            if (Project.LastOpened is null && Project.Created is null)
            {
                lastOpenedLabel.text = "null";
                createLabel.text = "null";
            }
            else if (Project.LastOpened is null && Project.Created is not null)
            {
                lastOpenedLabel.text = FormatDateTime((DateTime)Project.Created);
                createLabel.text = FormatDateTime((DateTime)Project.Created);
            }
            else
            {
                lastOpenedLabel.text = FormatDateTime((DateTime)Project.LastOpened);
                createLabel.text = FormatDateTime((DateTime)Project.Created);
            }

            InitFavouriteToggle();
            InitEditButton();
            InitDuplicateButton();
            InitDeleteButton();
        }

        // private void OnProjectOpened(Project project)
        // {
        //     if (Project.Id != project.Id)
        //     {
        //         return;
        //     }
        //     else
        //     {
        //         Project = project;
        //     }
        // }

        private string FormatDateTime(DateTime dateTime)
        {
            DateTime utcTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            DateTime localTime = utcTime.ToLocalTime();
            string formatted = localTime.ToString("dd/MM/yyyy HH:mm");
            return formatted;
        }

        private void InitFavouriteToggle()
        {
            favouriteToggle.value = Project.Favourite;

            favouriteToggle.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
            });

            favouriteToggle.RegisterValueChangedCallback(evt =>
            {
                Project.Favourite = evt.newValue;
                _ = ProjectManager.UpdateProject(Project.Id, Project);
            });
        }

        private void InitEditButton()
        {
            editButton.RegisterCallback<ClickEvent>(evt =>
            {
                UIManager.SetEditProject(Project);
                evt.StopPropagation();
                // Debug.Log("EditButton clicked");
            });
        }

        private void InitDuplicateButton()
        {
            duplicateButton.RegisterCallback<ClickEvent>(evt =>
            {
                UIManager.SetDuplicateProject(Project);
                evt.StopPropagation();
                // Debug.Log("DuplicateButton clicked");
            });
        }

        private void InitDeleteButton()
        {
            deleteButton.RegisterCallback<ClickEvent>(evt =>
            {
                UIManager.SetDeleteProject(Project);
                evt.StopPropagation();
                // Debug.Log("DeleteButton clicked");
            });
        }


    }

}
