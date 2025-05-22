using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ProjectRowController
    {
        // === Dependencies ===
        private readonly VisualElement projectRow;

        // === Events ===

        // === UI ===
        private Label projectNameLabel;
        private Label filesLabel;
        private Label lastOpenedLabel;
        private Label createLabel;
        private Button favouriteButton;
        private Button vrButton;
        private Button moreButton;

        // === Data ===
        public string ProjectName { get; set; }
        public Project Project { get; set; }

        public ProjectRowController(VisualElement projectRow, string projectName, Project project)
        {
            this.projectRow = projectRow;
            ProjectName = projectName;
            Project = project;

            Init();
        }

        private void Init()
        {
            projectNameLabel = projectRow.Q<Label>("ProjectNameLabel");
            filesLabel = projectRow.Q<Label>("FilesLabel");
            lastOpenedLabel = projectRow.Q<Label>("LastOpenedLabel");
            createLabel = projectRow.Q<Label>("CreatedLabel");
            favouriteButton = projectRow.Q<Button>("FavouriteButton");
            vrButton = projectRow.Q<Button>("VRButton");
            moreButton = projectRow.Q<Button>("MoreButton");

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

            favouriteButton.SetEnabled(false);
            vrButton.SetEnabled(false);
            moreButton.SetEnabled(false);
        }

        private string FormatDateTime(DateTime dateTime)
        {
            DateTime utcTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            DateTime localTime = utcTime.ToLocalTime();
            string formatted = localTime.ToString("dd/MM/yyyy HH:mm");
            return formatted;
        }

    }

}
