using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;


namespace Astrovisio
{
    public class ProjectViewController
    {

        // === Dependencies ===
        public ProjectManager ProjectManager { get; }
        public UIManager UIManager { get; }
        public VisualElement Root { get; }
        public Project Project { get; }
        public File File { get; }

        // === UI ===
        private Label projectNameLabel;
        private Label descriptionLabel;
        private Toggle favouriteToggle;
        private Button editButton;
        private Button deleteButton;
        private Button readMoreButton;
        private Toggle checkAllToggle;
        private Button headerNameButton;
        private ScrollView paramScrollView;

        // === Local ===
        private float _nextAllowedUpdate;
        private readonly Dictionary<Axis, ParamRowController> selectedAxis = new();
        private readonly List<ParamRowController> paramControllers = new();

        private enum ScrollViewOrderType { None, AZ, ZA }
        private ScrollViewOrderType scrollViewOrderType = ScrollViewOrderType.None;


        public ProjectViewController(
            ProjectManager projectManager,
            UIManager uiManager,
            VisualElement root,
            Project project,
            File file)
        {
            ProjectManager = projectManager;
            UIManager = uiManager;
            Root = root;
            Project = project;
            File = file;

            ProjectManager.ProjectUpdated += OnProjectUpdated;

            Init();
        }

        private void Init()
        {
            VisualElement topContainer = Root.Q<VisualElement>("TopContainer");

            // Files
            VisualElement filesContainer = topContainer.Q<VisualElement>("FilesContainer");
            _ = new FilesController<FileState>(filesContainer, UIManager.GetUIContext());

            // Project Name
            projectNameLabel = topContainer.Q<Label>("ProjectNameLabel");
            projectNameLabel.text = Project.Name;

            // Project Description
            descriptionLabel = topContainer.Q<Label>("DescriptionLabel");
            descriptionLabel.text = Project.Description;


            favouriteToggle = topContainer.Q<VisualElement>("FavouriteToggle").Q<Toggle>("CheckboxRoot");
            editButton = topContainer.Q<Button>("EditButton");
            deleteButton = topContainer.Q<Button>("DeleteButton");


            // Project Read More
            readMoreButton = topContainer.Q<Button>("ReadMoreButton");
            readMoreButton.clicked += () => UIManager.SetReadMoreViewVisibility(true, Project.Name, Project.Description);

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

            InitDeleteButton();
            InitEditButton();
            InitFavouriteToggle();
            InitScrollView();
            InitCheckAllToggle();
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

        private void OnCheckAllToggled(bool isChecked)
        {
            foreach (ParamRowController paramController in paramControllers)
            {
                paramController.SetSelected(isChecked);
            }
        }

        private void InitScrollView()
        {
            VisualElement paramsContainer = Root.Q<VisualElement>("ParamsContainer");
            paramScrollView = paramsContainer.Q<ScrollView>("ParamScrollView");
            paramScrollView.contentContainer.Clear();
            paramControllers.Clear();

            if (Project.Files.Count == 0)
            {
                Debug.LogWarning("No files to display.");
                return;
            }

            if (File == null)
            {
                Debug.LogWarning("No variables to display.");
                return;
            }

            foreach (Variable variable in File.Variables)
            {
                TemplateContainer paramRow = UIManager.GetUIContext().paramRowTemplate.CloneTree();
                VisualElement nameContainer = paramRow.Q<VisualElement>("NameContainer");
                nameContainer.Q<Label>("Label").text = variable.Name;

                ParamRowController paramRowController = new ParamRowController(paramRow, variable);
                paramControllers.Add(paramRowController);

                paramRowController.OnAxisChanged += HandleOnAxisChanged;
                paramRowController.OnThresholdChanged += HandleOnThresholdChanged;

                paramScrollView.Add(paramRowController.Root);
            }

            InitializeSelectedAxes();
        }

        private void InitializeSelectedAxes()
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

            UpdateProject();
        }

        private void HandleOnThresholdChanged(Threshold threshold, ParamRowController controller)
        {
            UpdateProject();
        }

        private void UpdateProject()
        {
            if (Time.unscaledTime < _nextAllowedUpdate)
            {
                return;
            }

            _nextAllowedUpdate = Time.unscaledTime + 0.05f; // 50ms
            ProjectManager.UpdateProject(Project.Id, Project);
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
                ProjectManager.UpdateProject(Project.Id, Project);
            });
        }

        private void InitCheckAllToggle()
        {
            if (checkAllToggle == null)
            {
                return;
            }

            bool allSelected =
                File?.Variables != null &&
                File.Variables.Count > 0 &&
                File.Variables.All(v => v.Selected);

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

    }

}
