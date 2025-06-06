using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ProjectRowController
    {
        // === Dependencies ===
        public ProjectManager ProjectManager { get; set; }
        public Project Project { get; set; }
        public VisualElement Root { private set; get; }

        // === Events ===

        // === UI ===
        private Label projectNameLabel;
        private Label filesLabel;
        private Label lastOpenedLabel;
        private Label createLabel;
        private Toggle favouriteToggle;
        private Button vrButton;
        private Button moreButton;


        public ProjectRowController(ProjectManager projectManager, Project project, VisualElement root)
        {
            ProjectManager = projectManager;
            Project = project;
            Root = root;

            Init();
        }

        private void Init()
        {
            projectNameLabel = Root.Q<Label>("ProjectNameLabel");
            filesLabel = Root.Q<Label>("FilesLabel");
            lastOpenedLabel = Root.Q<Label>("LastOpenedLabel");
            createLabel = Root.Q<Label>("CreatedLabel");
            favouriteToggle = Root.Q<VisualElement>("FavouriteToggle")?.Q<Toggle>();
            vrButton = Root.Q<Button>("VRButton");
            moreButton = Root.Q<Button>("MoreButton");

            projectNameLabel.text = Project.Name;
            filesLabel.text = Project.Paths.Length.ToString() + " files";

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
            InitVRButton();
            InitMoreButton();
        }

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

        private void InitVRButton()
        {
            vrButton.SetEnabled(false);
            vrButton.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                Debug.Log("vrButton clicked");
            });
        }

        private void InitMoreButton()
        {
            moreButton.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                Debug.Log("moreButton clicked");
            });
        }



    }

}
