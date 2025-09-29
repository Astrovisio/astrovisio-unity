using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Astrovisio
{
    public class ProjectViewController
    {

        // === Dependencies ===
        public ProjectManager ProjectManager { get; }
        public UIManager UIManager { get; }
        public VisualElement Root { get; }
        public Project Project { get; }

        // === UI ===
        private Label projectNameLabel;
        private Label descriptionLabel;
        private Toggle favouriteToggle;
        private Button editButton;
        private Button deleteButton;
        private Button readMoreButton;
        private Label configFileNameLabel;
        private Toggle checkAllToggle;
        private Button headerNameButton;
        private ScrollView paramScrollView;

        // === Local ===
        private File currentFile;
        private float _nextAllowedUpdate;
        private ProjectFilesController projectFilesController;
        private readonly Dictionary<Axis, ParamRowController> selectedAxis = new();
        private readonly List<ParamRowController> paramControllers = new();

        private enum ScrollViewOrderType { None, AZ, ZA }
        private ScrollViewOrderType scrollViewOrderType = ScrollViewOrderType.None;


        public ProjectViewController(
            ProjectManager projectManager,
            UIManager uiManager,
            VisualElement root,
            Project project)
        {
            ProjectManager = projectManager;
            UIManager = uiManager;
            Root = root;
            Project = project;

            ProjectManager.ProjectUpdated += OnProjectUpdated;
            ProjectManager.FileProcessed += OnFileProcessed;
            ProjectManager.FileUpdated += OnFileUpdated;

            Init();
        }

        private void Init()
        {
            VisualElement topContainer = Root.Q<VisualElement>("TopContainer");
            VisualElement labelContainer = Root.Q<VisualElement>("LabelContainer");

            // Files
            VisualElement filesContainer = topContainer.Q<VisualElement>("FilesContainer");
            projectFilesController = new ProjectFilesController(
                ProjectManager,
                Project,
                filesContainer,
                UIManager.GetUIContext(),
                async () => await UpdateFileOrderCallback(),
                OnFileRowClicked
            );

            // Project Name
            projectNameLabel = topContainer.Q<Label>("ProjectNameLabel");
            projectNameLabel.text = Project.Name;

            // Project Description
            descriptionLabel = topContainer.Q<Label>("DescriptionLabel");
            descriptionLabel.text = Project.Description;

            // Project Buttons
            favouriteToggle = topContainer.Q<VisualElement>("FavouriteToggle").Q<Toggle>("CheckboxRoot");
            editButton = topContainer.Q<Button>("EditButton");
            deleteButton = topContainer.Q<Button>("DeleteButton");

            // Project Read More
            readMoreButton = topContainer.Q<Button>("ReadMoreButton");
            readMoreButton.clicked += () => UIManager.SetReadMoreViewVisibility(true, Project.Name, Project.Description);

            // Configuration file Label
            configFileNameLabel = labelContainer.Q<Label>("SubtitleLabel");

            // Scroll View
            VisualElement paramsContainer = Root.Q<VisualElement>("ParamsContainer");
            paramScrollView = paramsContainer.Q<ScrollView>("ParamScrollView");

            // Header Checkbox
            checkAllToggle = Root.Q<VisualElement>("AllCheckbox")?.Q<Toggle>("CheckboxRoot");
            if (checkAllToggle != null)
            {
                checkAllToggle.RegisterValueChangedCallback(evt =>
                {
                    bool isChecked = evt.newValue;
                    OnCheckAllToggled(isChecked);
                });
            }

            // Header Name
            headerNameButton = Root.Q<Button>("Name");
            headerNameButton.clicked += () =>
            {
                scrollViewOrderType = scrollViewOrderType switch
                {
                    ScrollViewOrderType.None => ScrollViewOrderType.AZ,
                    ScrollViewOrderType.AZ => ScrollViewOrderType.ZA,
                    _ => ScrollViewOrderType.None
                };
                ApplyScrollViewOrderType();
            };

            currentFile = Project.Files is { Count: > 0 } list ? list[0] : null;

            InitFileContainer();
            InitDeleteButton();
            InitEditButton();
            InitFavouriteToggle();
            InitScrollView();
            InitCheckAllToggle();
            UpdateConfigurationFileLabel();
        }

        public void Dispose()
        {
            ProjectManager.ProjectUpdated -= OnProjectUpdated;
            ProjectManager.FileProcessed -= OnFileProcessed;
            ProjectManager.FileUpdated -= OnFileUpdated;
        }

        private void OnFileRowClicked(FileState fileState)
        {
            if (fileState.file == null || fileState.file == currentFile)
            {
                return;
            }

            currentFile = fileState.file;
            Debug.Log("Is current clicked file processed? " + currentFile.Processed);

            InitScrollView();
            InitCheckAllToggle();
            UpdateConfigurationFileLabel();

            // Debug.Log($"[ProjectViewController] Showing file: {currentFile?.Name ?? "none"}");
            ProjectManager.NotifyFileSelected(Project, currentFile);
        }

        private void UpdateConfigurationFileLabel()
        {
            if (currentFile == null)
            {
                return;
            }

            configFileNameLabel.text = currentFile.Name;
        }

        private void OnCheckAllToggled(bool isChecked)
        {
            foreach (ParamRowController paramController in paramControllers)
            {
                paramController.SetSelected(isChecked);
            }
        }

        private void InitScrollView()
        {
            if (currentFile == null)
            {
                return;
            }


            foreach (ParamRowController paramRowController in paramControllers)
            {
                paramRowController.OnAxisChanged -= HandleOnAxisChanged;
                paramRowController.OnThresholdChanged -= HandleOnThresholdChanged;
                paramRowController.OnStateChanged -= HandleStateChanged;
            }
            paramControllers.Clear();
            paramScrollView.contentContainer.Clear();

            if (Project.Files.Count == 0)
            {
                Debug.LogWarning("No files to display.");
                return;
            }

            foreach (Variable variable in currentFile.Variables)
            {
                TemplateContainer paramRow = UIManager.GetUIContext().paramRowTemplate.CloneTree();
                VisualElement nameContainer = paramRow.Q<VisualElement>("NameContainer");
                nameContainer.Q<Label>("Label").text = variable.Name;

                ParamRowController paramRowController = new ParamRowController(paramRow, variable);
                paramControllers.Add(paramRowController);

                paramRowController.OnAxisChanged += HandleOnAxisChanged;
                paramRowController.OnThresholdChanged += HandleOnThresholdChanged;
                paramRowController.OnStateChanged += HandleStateChanged;

                paramScrollView.Add(paramRowController.Root);
            }

            InitSelectedAxes();
        }

        private void InitSelectedAxes()
        {
            foreach (ParamRowController paramController in paramControllers)
            {
                if (paramController.Variable.XAxis)
                {
                    selectedAxis[Axis.X] = paramController;
                }
                if (paramController.Variable.YAxis)
                {
                    selectedAxis[Axis.Y] = paramController;
                }
                if (paramController.Variable.ZAxis)
                {
                    selectedAxis[Axis.Z] = paramController;
                }
            }
        }

        private void HandleOnAxisChanged(Axis? axis, ParamRowController newly)
        {
            if (axis == null)
            {
                foreach (var kvp in selectedAxis.ToList())
                {
                    if (kvp.Value == newly)
                    {
                        selectedAxis.Remove(kvp.Key);
                        switch (kvp.Key)
                        {
                            case Axis.X:
                                newly.Variable.XAxis = false;
                                break;
                            case Axis.Y:
                                newly.Variable.YAxis = false;
                                break;
                            case Axis.Z:
                                newly.Variable.ZAxis = false;
                                break;
                        }
                    }
                }
                return;
            }

            if (selectedAxis.TryGetValue(axis.Value, out var previous) && previous != newly)
            {
                previous.DeselectAxis(axis.Value);
                switch (axis.Value)
                {
                    case Axis.X:
                        previous.Variable.XAxis = false;
                        break;
                    case Axis.Y:
                        previous.Variable.YAxis = false;
                        break;
                    case Axis.Z:
                        previous.Variable.ZAxis = false;
                        break;
                }
            }

            selectedAxis[axis.Value] = newly;

            switch (axis.Value)
            {
                case Axis.X:
                    newly.Variable.XAxis = true;
                    newly.Variable.YAxis = false;
                    newly.Variable.ZAxis = false;
                    break;
                case Axis.Y:
                    newly.Variable.XAxis = false;
                    newly.Variable.YAxis = true;
                    newly.Variable.ZAxis = false;
                    break;
                case Axis.Z:
                    newly.Variable.XAxis = false;
                    newly.Variable.ZAxis = true;
                    newly.Variable.YAxis = false;
                    break;
            }

            UpdateFile();
        }

        private void HandleOnThresholdChanged(Threshold threshold, ParamRowController controller)
        {
            UpdateFile();
        }

        private void HandleStateChanged()
        {
            UpdateFile();
        }

        private void UpdateFile()
        {
            if (Time.unscaledTime < _nextAllowedUpdate)
            {
                return;
            }

            _nextAllowedUpdate = Time.unscaledTime + 0.05f; // 50ms

            if (currentFile != null)
            {
                currentFile.Processed = false;
                // Debug.Log("UpdateFile");
                ProjectManager.UpdateFile(Project.Id, currentFile);
            }

        }

        private void InitFileContainer()
        {
            foreach (File file in Project.Files.OrderBy(f => f.Order))
            {
                FileInfo fileInfo = new FileInfo(file.Path, file.Name, file.Size);
                FileState fileState = new FileState(fileInfo, file, file.Processed);
                projectFilesController.AddFile(fileState);
                // Debug.Log($"File /// Name: {file.Name} - Processed: {file.Processed}");
                // Debug.Log($"File added: {fileState.fileInfo.Name} ({fileState.fileInfo.Size} bytes) - {fileState.fileInfo.Path}");
            }
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

        private void InitEditButton()
        {
            editButton.RegisterCallback<ClickEvent>(evt =>
            {
                UIManager.SetEditProject(Project);
                evt.StopPropagation();
            });
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

        private void InitCheckAllToggle()
        {
            if (checkAllToggle == null)
            {
                return;
            }

            bool allSelected =
                currentFile?.Variables != null &&
                currentFile.Variables.Count > 0 &&
                currentFile.Variables.All(v => v.Selected);

            checkAllToggle.SetValueWithoutNotify(allSelected);
        }

        private void ApplyScrollViewOrderType()
        {
            if (paramScrollView == null || paramControllers.Count == 0)
            {
                return;
            }

            IEnumerable<ParamRowController> ordered = scrollViewOrderType switch
            {
                ScrollViewOrderType.AZ => paramControllers
                    .OrderBy(pc => pc.Variable?.Name, StringComparer.OrdinalIgnoreCase),
                ScrollViewOrderType.ZA => paramControllers
                    .OrderByDescending(pc => pc.Variable?.Name, StringComparer.OrdinalIgnoreCase),
                _ => paramControllers
            };

            UpdateOrderTypeLabel();

            paramScrollView.contentContainer.Clear();
            foreach (var pc in ordered)
            {
                paramScrollView.Add(pc.Root);
            }
        }

        private void UpdateOrderTypeLabel()
        {
            switch (scrollViewOrderType)
            {
                case ScrollViewOrderType.AZ:
                    headerNameButton.Q<Label>().text = "Name (A-Z)";
                    break;
                case ScrollViewOrderType.ZA:
                    headerNameButton.Q<Label>().text = "Name (Z-A)";
                    break;
                default:
                    headerNameButton.Q<Label>().text = "Name";
                    break;
            }
        }

        private async Task UpdateFileOrderCallback()
        {
            // Debug.Log("UpdateFileOrderCallback");

            List<FileState> uiList = projectFilesController.GetFileList();
            if (Project.Files == null || uiList == null)
            {
                return;
            }

            List<File> original = Project.Files;

            int limit = Math.Min(uiList.Count, original.Count);

            for (int i = 0; i < limit; i++)
            {
                File uiFile = uiList[i].file;
                if (uiFile == null)
                {
                    Debug.LogWarning($"UI slot {i} has null file; skipping.");
                    continue;
                }

                int j = original.FindIndex(f => f.Id == uiFile.Id);
                if (j < 0)
                {
                    Debug.LogWarning($"File with Id={uiFile.Id} not found in Project.Files; skipping.");
                    continue;
                }
                if (j == i)
                {
                    continue;
                }
                (original[i], original[j]) = (original[j], original[i]);
            }

            int[] orderIds = new int[original.Count];
            for (int i = 0; i < original.Count; i++)
            {
                // original[i].Order = i;
                // ProjectManager.UpdateFile(Project.Id, original[i]);

                orderIds[i] = original[i].Id;
            }

            await ProjectManager.UpdateFileOrder(Project.Id, orderIds);

            // // Check order
            // for (int i = 0; i < Project.Files.Count; i++)
            // {
            //     Debug.Log(Project.Files[i].Name);
            // }

            // Debug.Log($"Project.Files reordered to match UI order (first {limit} items). Count={original.Count}");
        }

        private void OnProjectUpdated(Project project)
        {
            if (Project.Id != project.Id)
            {
                return;
            }

            projectNameLabel.text = project.Name;
            descriptionLabel.text = project.Description;
        }

        private void OnFileProcessed(Project project, File file, DataPack pack)
        {
            if (project == null || project.Id != Project.Id)
            {
                return;
            }

            // Debug.Log($"Project {project.Name}, file {file.Name}, processed {file.Processed}.");
            projectFilesController.SetFileState(file, true);
        }

        private void OnFileUpdated(Project project, File file)
        {
            if (project == null || project.Id != Project.Id)
            {
                return;
            }

            Debug.Log($"Project {project.Name}, file {file.Name}, updated.");
            projectFilesController.SetFileState(file, false);
        }

    }

}
