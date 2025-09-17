using UnityEngine;
using UnityEngine.UIElements;
using SFB;
using System.IO;
using System.Linq;

namespace Astrovisio
{

    public class NewProjectViewController
    {
        // === Dependencies ===
        public ProjectManager ProjectManager { get; }
        private UIManager UIManager { get; }

        // === UI ===
        private TextField projectNameField;
        private TextField projectDescriptionField;
        private Button uploadFileButton;
        private Label filesSizeLabel;
        private Button continueButton;
        private Button cancelButton;

        // === Local ===
        private VisualElement root;
        private FilesController<FileInfo> filesController;

        public NewProjectViewController(ProjectManager projectManager, UIManager uiManager)
        {
            ProjectManager = projectManager;
            UIManager = uiManager;
        }

        public void Init(VisualElement root)
        {
            this.root = root;
            projectNameField = root.Q<VisualElement>("ProjectNameInputField")?.Q<TextField>();
            projectDescriptionField = root.Q<VisualElement>("ProjectDescriptionInputField")?.Q<TextField>();
            uploadFileButton = root.Q<VisualElement>("AddFileButton").Q<Button>();


            VisualElement filesContainer = this.root.Q<VisualElement>("FilesContainer");
            filesController = new FilesController<FileInfo>(
                filesContainer,
                UIManager.GetUIContext(),
                () => UpdateFilesSizeLabel()
            );

            filesSizeLabel = root.Q<VisualElement>("MemorySizeInfo")?.Q<Label>("SizeLabel");
            continueButton = root.Q<VisualElement>("ContinueButton")?.Q<Button>();
            cancelButton = root.Q<VisualElement>("CancelButton")?.Q<Button>();

            projectNameField.value = string.Empty;
            projectDescriptionField.value = string.Empty;

            if (uploadFileButton != null)
            {
                uploadFileButton.clicked += OnUploadFileClicked;
            }

            if (filesSizeLabel != null)
            {
                filesSizeLabel.text = "0.00KB";
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
            if (uploadFileButton != null)
            {
                uploadFileButton.clicked -= OnUploadFileClicked;
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

        private void OnUploadFileClicked()
        {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select file", "", "", true);
            if (paths.Length > 0)
            {
                foreach (string path in paths)
                {
                    if (!System.IO.File.Exists(path))
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
                    if (filesController.Items.Any(file => file.path == path))
                    {
                        Debug.Log($"File already added: {path}");
                        continue;
                    }

                    System.IO.FileInfo sysInfo = new System.IO.FileInfo(path);
                    FileInfo fileInfo = new FileInfo(path, sysInfo.Name, sysInfo.Length);
                    filesController.AddFile(fileInfo);
                    Debug.Log($"File added: {fileInfo.name} ({fileInfo.size} bytes) - {fileInfo.path}");
                }
            }

            UpdateFilesSizeLabel();
        }

        private void UpdateFilesSizeLabel()
        {
            filesSizeLabel.text = filesController.GetFormattedTotalSize();

            bool overLimit = IsSizeOverLimit();

            continueButton.SetEnabled(!overLimit);
            filesSizeLabel.EnableInClassList("over", overLimit);
        }

        private bool IsSizeOverLimit()
        {
            const long maxSize = 5L * 1024 * 1024 * 1024;
            return filesController.GetTotalSizeBytes() > maxSize;
        }

        private void OnContinueClicked()
        {
            string name = projectNameField?.value ?? "<empty>";
            string description = projectDescriptionField?.value ?? "<empty>";
            string[] paths = filesController.Items.Select(file => $"data/{Path.GetFileName(file.path)}").ToArray();
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

    }

}
