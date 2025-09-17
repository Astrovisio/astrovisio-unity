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
            lastOpenedLabel = Root.Q<Label>("LastOpenedLabel");
            createLabel = Root.Q<Label>("CreatedLabel");
            favouriteToggle = Root.Q<VisualElement>("FavouriteToggle")?.Q<Toggle>();
            editButton = Root.Q<Button>("EditButton");
            duplicateButton = Root.Q<Button>("DuplicateButton");
            deleteButton = Root.Q<Button>("DeleteButton");

            projectNameLabel.text = Project.Name;

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
                ProjectManager.UpdateProject(Project.Id, Project);
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
