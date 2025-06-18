using UnityEngine;
using UnityEngine.UIElements;
using SFB;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Astrovisio
{

    public class NewProjectViewController
    {
        private readonly ProjectManager projectManager;
        private readonly VisualTreeAsset listItemFileTemplate;
        private VisualElement root;

        // === UI ===
        private TextField projectNameField;
        private TextField projectDescriptionField;
        private Button addFileButton;
        private ScrollView filesScrollView;
        private Label filesSizeLabel;
        private Button continueButton;
        private Button cancelButton;

        // === Local ===
        private List<FileInfo> selectedFiles;

        public NewProjectViewController(ProjectManager projectManager, VisualTreeAsset listItemFileTemplate)
        {
            this.projectManager = projectManager;
            this.listItemFileTemplate = listItemFileTemplate;
        }

        public void Initialize(VisualElement root)
        {
            this.root = root;
            projectNameField = root.Q<VisualElement>("ProjectNameInputField")?.Q<TextField>();
            projectDescriptionField = root.Q<VisualElement>("ProjectDescriptionInputField")?.Q<TextField>();
            addFileButton = root.Q<VisualElement>("AddFileButton").Q<Button>();
            filesScrollView = root.Q<ScrollView>("FilesScrollView");
            filesSizeLabel = root.Q<VisualElement>("MemorySizeInfo")?.Q<Label>("SizeLabel");
            continueButton = root.Q<VisualElement>("ContinueButton")?.Q<Button>();
            cancelButton = root.Q<VisualElement>("CancelButton")?.Q<Button>();

            projectNameField.value = string.Empty;
            projectDescriptionField.value = string.Empty;

            if (addFileButton != null)
            {
                addFileButton.clicked += OnAddFileClicked;
            }

            if (filesScrollView != null)
            {
                selectedFiles = new();
                UpdateFilesContainer();
            }

            if (filesSizeLabel != null)
            {
                filesSizeLabel.text = "";
            }

            if (continueButton != null)
            {
                continueButton.clicked += OnContinueClicked;
            }

            if (cancelButton != null)
            {
                cancelButton.clicked += OnCancelClicked;
            }
        }

        public void Dispose()
        {
            if (addFileButton != null)
            {
                addFileButton.clicked -= OnAddFileClicked;
            }

            if (continueButton != null)
            {
                continueButton.clicked -= OnContinueClicked;
            }

            if (cancelButton != null)
            {
                cancelButton.clicked -= OnCancelClicked;
            }
        }

        private void OnAddFileClicked()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Select file", "", "", true);
            if (paths.Length > 0)
            {
                selectedFiles.Clear();
                foreach (var path in paths)
                {
                    if (File.Exists(path))
                    {
                        var sysInfo = new System.IO.FileInfo(path);
                        FileInfo fileInfo = new FileInfo(path, sysInfo.Name, sysInfo.Length);
                        selectedFiles.Add(fileInfo);
                        Debug.Log($"File selezionato: {fileInfo.name} ({fileInfo.size} byte) - {fileInfo.path}");
                    }
                    else
                    {
                        Debug.LogWarning($"File non trovato: {path}");
                    }
                }

                UpdateFilesContainer();
                UpdateFilesSizeLabel();
            }
        }

        private void UpdateFilesSizeLabel()
        {
            long totalSize = selectedFiles.Sum(file => file.size);

            if (filesSizeLabel != null)
            {
                filesSizeLabel.text = $"{FormatFileSize(totalSize)}";
            }
        }


        private void OnContinueClicked()
        {
            string name = projectNameField?.value ?? "<empty>";
            string description = projectDescriptionField?.value ?? "<empty>";
            string[] paths = selectedFiles.Select(file => $"data/{Path.GetFileName(file.path)}").ToArray();
            // Debug.Log($"Create project: {name}, {description} with {paths.Length} files.");
            projectManager.CreateProject(name, description, paths);
            OnExit();
        }

        private void OnCancelClicked()
        {
            OnExit();
        }

        private void OnExit()
        {
            Dispose();
            root.RemoveFromClassList("active");
            root.schedule.Execute(() =>
            {
                projectNameField?.SetValueWithoutNotify(string.Empty);
                projectDescriptionField?.SetValueWithoutNotify(string.Empty);
                // projectManager.FetchAllProjects();
            }).StartingIn(400);
        }

        private void UpdateFilesContainer()
        {
            filesScrollView.Clear();

            // const int maxPerRow = 3;
            // int count = 0;

            // VisualElement rowContainer = null;

            for (int i = 0; i < selectedFiles.Count; i++)
            {
                // if (count == 0)
                // {
                //     rowContainer = new VisualElement();
                //     rowContainer.style.flexDirection = FlexDirection.Row;
                //     rowContainer.style.marginBottom = 8;
                //     filesScrollView.Add(rowContainer);
                // }

                FileInfo file = selectedFiles[i];
                TemplateContainer fileItem = listItemFileTemplate.CloneTree();

                // Name
                Label nameLabel = fileItem.Q<Label>("NameLabel");
                if (nameLabel != null)
                {
                    nameLabel.text = Path.GetFileName(file.path);
                }

                // Size
                Label sizeLabel = fileItem.Q<Label>("SizeLabel");
                if (sizeLabel != null)
                {
                    sizeLabel.text = FormatFileSize(file.size);
                }

                Button fileItemButton = fileItem.Q<Button>();
                fileItemButton.clicked += () =>
                {
                    // Debug.Log("Removing " + nameLabel.text);
                    selectedFiles.Remove(file);
                    UpdateFilesContainer();
                    UpdateFilesSizeLabel();
                };

                fileItem.AddToClassList("ColListItem");
                filesScrollView.Add(fileItem);
                // count++;

                // if (count == maxPerRow)
                // {
                //     count = 0;
                // }
            }

            // Remove margin-bottom to the last row
            // if (filesScrollView.childCount > 0)
            // {
            //     filesScrollView.ElementAt(filesScrollView.childCount - 1).style.marginBottom = 0;
            // }
        }

        private string FormatFileSize(long sizeInBytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (sizeInBytes >= GB)
            {
                return $"{sizeInBytes / (float)GB:F2}GB";
            }
            else if (sizeInBytes >= MB)
            {
                return $"{sizeInBytes / (float)MB:F2}MB";
            }
            else
            {
                return $"{sizeInBytes / (float)KB:F2}KB";
            }
        }



    }

}
