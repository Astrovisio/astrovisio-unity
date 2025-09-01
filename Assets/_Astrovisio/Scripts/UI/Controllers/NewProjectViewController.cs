using UnityEngine;
using UnityEngine.UIElements;
using SFB;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Astrovisio
{

    public class NewProjectViewController
    {
        public ProjectManager ProjectManager { get; }
        private UIManager UIManager { get; }

        // private readonly VisualTreeAsset listItemFileTemplate;

        private VisualElement root;

        // === UI ===
        private TextField projectNameField;
        private TextField projectDescriptionField;
        private Button addFileButton;
        // private ListView filesListView;
        private Label filesSizeLabel;
        private Button continueButton;
        private Button cancelButton;

        // === Local ===
        private FilesController<FileInfo> filesController;
        private List<FileInfo> selectedFiles;

        public NewProjectViewController(ProjectManager projectManager, UIManager uiManager)
        {
            ProjectManager = projectManager;
            UIManager = uiManager;

            // this.listItemFileTemplate = listItemFileTemplate;
        }

        public void Initialize(VisualElement root)
        {
            this.root = root;
            projectNameField = root.Q<VisualElement>("ProjectNameInputField")?.Q<TextField>();
            projectDescriptionField = root.Q<VisualElement>("ProjectDescriptionInputField")?.Q<TextField>();
            addFileButton = root.Q<VisualElement>("AddFileButton").Q<Button>();


            VisualElement filesContainer = this.root.Q<VisualElement>("FilesContainer");
            filesController = new FilesController<FileInfo>(filesContainer, UIManager.GetUIContext());
            selectedFiles = new();
            UpdateFilesContainer();

            filesSizeLabel = root.Q<VisualElement>("MemorySizeInfo")?.Q<Label>("SizeLabel");
            continueButton = root.Q<VisualElement>("ContinueButton")?.Q<Button>();
            cancelButton = root.Q<VisualElement>("CancelButton")?.Q<Button>();

            projectNameField.value = string.Empty;
            projectDescriptionField.value = string.Empty;

            if (addFileButton != null)
            {
                addFileButton.clicked += OnAddFileClicked;
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
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select file", "", "", true);
            if (paths.Length > 0)
            {
                foreach (string path in paths)
                {
                    if (!File.Exists(path))
                    {
                        Debug.LogWarning($"File not found: {path}");
                        continue;
                    }

                    // Check if the file extension is supported (.hdf5 or .fits)
                    string extension = Path.GetExtension(path).ToLowerInvariant();
                    if (extension != ".hdf5" && extension != ".fits")
                    {
                        Debug.LogWarning($"Unsupported file format: {path}");
                        continue;
                    }

                    // Skip if the file is already in the list
                    if (selectedFiles.Any(file => file.path == path))
                    {
                        Debug.Log($"File already added: {path}");
                        continue;
                    }

                    System.IO.FileInfo sysInfo = new System.IO.FileInfo(path);
                    FileInfo fileInfo = new FileInfo(path, sysInfo.Name, sysInfo.Length);
                    selectedFiles.Add(fileInfo);
                    Debug.Log($"File added: {fileInfo.name} ({fileInfo.size} bytes) - {fileInfo.path}");
                }

                UpdateFilesContainer();
            }
        }

        private void OnContinueClicked()
        {
            string name = projectNameField?.value ?? "<empty>";
            string description = projectDescriptionField?.value ?? "<empty>";
            string[] paths = selectedFiles.Select(file => $"data/{Path.GetFileName(file.path)}").ToArray();
            // Debug.Log($"Create project: {name}, {description} with {paths.Length} files.");
            ProjectManager.CreateProject(name, description, paths);
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
            for (int i = 0; i < selectedFiles.Count; i++)
            {
                FileInfo fileInfo = selectedFiles[i];
                filesController.AddFile(fileInfo);

                Debug.Log(fileInfo.Name + " " + fileInfo.Size + " " + fileInfo.Path);

                // Button fileItemButton = fileItem.Q<Button>();
                // fileItemButton.clicked += () =>
                // {
                //     selectedFiles.Remove(fileInfo);
                //     UpdateFilesContainer();
                //     UpdateFilesSizeLabel();
                // };
            }
        }

    }

}
