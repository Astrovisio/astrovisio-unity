using UnityEngine;
using UnityEngine.UIElements;
using SFB;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

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
        private NewProjectFilesController newProjectfilesController;

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
            newProjectfilesController = new NewProjectFilesController(
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
                continueButton.clicked += OnContinueClickedHandler;
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
                continueButton.clicked -= OnContinueClickedHandler;
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
                    if (newProjectfilesController.Items.Any(file => file.Path == path))
                    {
                        Debug.Log($"File already added: {path}");
                        continue;
                    }

                    System.IO.FileInfo sysInfo = new System.IO.FileInfo(path);
                    FileInfo fileInfo = new FileInfo(path, sysInfo.Name, sysInfo.Length);
                    newProjectfilesController.AddFile(fileInfo);
                    // Debug.Log($"File added: {fileInfo.name} ({fileInfo.size} bytes) - {fileInfo.path}");
                }
            }

            UpdateFilesSizeLabel();
        }

        private void UpdateFilesSizeLabel()
        {
            filesSizeLabel.text = newProjectfilesController.GetFormattedTotalSize();

            bool overLimit = IsSizeOverLimit();

            continueButton.SetEnabled(!overLimit);
            filesSizeLabel.EnableInClassList("over", overLimit);
        }

        private bool IsSizeOverLimit()
        {
            const long maxSize = 5L * 1024 * 1024 * 1024;
            return newProjectfilesController.GetTotalSizeBytes() > maxSize;
        }

        private async void OnContinueClickedHandler()
        {
            await OnContinueClicked();
        }

        private async Task OnContinueClicked()
        {
            // Debug.LogWarning("OnContinueClicked()");
            string name = projectNameField?.value ?? "<empty>";
            string description = projectDescriptionField?.value ?? "<empty>";
            string[] paths = newProjectfilesController.Items
                .Select(file => $"data/{Path.GetFileName(file.Path)}")
                .ToArray();

            try
            {
                Project createdProject = await ProjectManager.CreateProject(name, description, paths);
                if (createdProject == null)
                {
                    return;
                }

                // TODO: Back-end should popolate order, not front-end
                List<FileInfo> fileList = newProjectfilesController.GetFileList();
                // Debug.Log("=== createdProject.Files ===");
                // foreach (var f in createdProject.Files)
                //     Debug.Log($"{f.Name} | {f.Path} | {f.Size} | Order={f.Order}");

                // Debug.Log("=== fileList ===");
                // foreach (var f in fileList)
                //     Debug.Log($"{f.Name} | {f.Path} | {f.Size}");

                // int[] fileIdOrder = new int[createdProject.Files.Count];

                if (createdProject.Files != null && createdProject.Files.Count > 0 && fileList.Count == createdProject.Files.Count)
                {
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        // Debug.Log($"{createdProject.Files.Count} @ {createdProject.Files[i].Order} @ {createdProject.Files[i].Name}");
                        File file = createdProject.Files.FirstOrDefault(f => (f.Name + "." + f.Type) == fileList[i].Name); //  && f.Size == fileList[i].Size
                        if (file != null)
                        {
                            file.Order = i;
                            ProjectManager.UpdateFile(createdProject.Id, file);
                            // Debug.Log("Done: " + file.Order + " - " + file.Name);


                            // fileIdOrder[i] = file.Id;
                        }
                    }
                }

                // createdProject.order
                // _ = ProjectManager.UpdateProject(createdProject.Id, createdProject);

                OnExit();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OnContinueClicked] {ex.Message}");
                throw;
            }
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
