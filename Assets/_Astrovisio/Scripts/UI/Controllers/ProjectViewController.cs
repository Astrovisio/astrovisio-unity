using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using NUnit.Framework.Internal;

namespace Astrovisio
{
    public class ProjectViewController
    {

        // === Dependencies ===
        public ProjectManager ProjectManager { get; }
        public UIManager UIManager { get; }

        // === Local ===
        public Project Project { get; }
        public VisualElement Root { get; }

        // === UI ===
        private Label projectNameLabel;
        private Label descriptionLabel;
        private Button favouriteButton;
        private Button editButton;
        private Button deleteButton;
        private Button readMoreButton;
        private Toggle checkAllToggle;
        private Button headerNameButton;
        private ScrollView paramScrollView;

        // === Local ===
        private float _nextAllowedUpdate;
        private readonly Dictionary<Axis, ParamRowController> selectedAxis = new();
        private readonly Dictionary<string, ParamRowController> paramControllers = new();

        private enum ScrollViewOrderType { None, AZ, ZA }
        private ScrollViewOrderType scrollViewOrderType = ScrollViewOrderType.None;


        public ProjectViewController(ProjectManager projectManager, UIManager uiManager, VisualElement root, Project project)
        {
            ProjectManager = projectManager;
            UIManager = uiManager;
            Root = root;
            Project = project;

            ProjectManager.ProjectUpdated += OnProjectUpdated;

            Init();
        }

        private void Init()
        {
            VisualElement topContainer = Root.Q<VisualElement>("TopContainer");

            // Files (se servisse altrove)
            VisualElement filesContainer = topContainer.Q<VisualElement>("FilesContainer");
            _ = new FilesController<FileState>(filesContainer, UIManager.GetUIContext());

            // Project Name
            projectNameLabel = topContainer.Q<Label>("ProjectNameLabel");
            projectNameLabel.text = Project.Name;

            // Project Description
            descriptionLabel = topContainer.Q<Label>("DescriptionLabel");
            descriptionLabel.text = Project.Description;


            favouriteButton = topContainer.Q<Button>("FavouriteButton");
            editButton = topContainer.Q<Button>("EditButton");
            deleteButton = topContainer.Q<Button>("DeleteButton");

            Debug.Log("------------------");
            Debug.Log(favouriteButton);
            Debug.Log(editButton);
            Debug.Log(deleteButton);
            Debug.Log("------------------");

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
            foreach (var kvp in paramControllers)
            {
                kvp.Value.SetSelected(isChecked);
            }
        }

        private void InitScrollView()
        {
            VisualElement paramsContainer = Root.Q<VisualElement>("ParamsContainer");
            paramScrollView = paramsContainer.Q<ScrollView>("ParamScrollView");
            paramScrollView.contentContainer.Clear();
            paramControllers.Clear();

            if (Project.ConfigProcess?.Params == null)
            {
                Debug.LogWarning("No variables to display.");
                return;
            }

            foreach (var kvp in Project.ConfigProcess.Params)
            {
                string paramName = kvp.Key;
                ConfigParam param = kvp.Value;

                TemplateContainer paramRow = UIManager.GetUIContext().paramRowTemplate.CloneTree();
                VisualElement nameContainer = paramRow.Q<VisualElement>("NameContainer");
                nameContainer.Q<Label>("Label").text = paramName;

                ParamRowController paramRowController = new ParamRowController(paramRow, paramName, param);
                paramControllers[paramName] = paramRowController;

                paramRowController.OnAxisChanged += HandleOnAxisChanged;
                paramRowController.OnThresholdChanged += HandleOnThresholdChanged;

                paramScrollView.Add(paramRowController.Root);
            }

            InitializeSelectedAxes();
        }

        private void InitializeSelectedAxes()
        {
            foreach (var kvp in paramControllers)
            {
                var controller = kvp.Value;
                var param = controller.Param;

                if (param.XAxis)
                {
                    selectedAxis[Axis.X] = controller;
                }
                if (param.YAxis)
                {
                    selectedAxis[Axis.Y] = controller;
                }
                if (param.ZAxis)
                {
                    selectedAxis[Axis.Z] = controller;
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
                                newly.Param.XAxis = false;
                                break;
                            case Axis.Y:
                                newly.Param.YAxis = false;
                                break;
                            case Axis.Z:
                                newly.Param.ZAxis = false;
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
                        previous.Param.XAxis = false;
                        break;
                    case Axis.Y:
                        previous.Param.YAxis = false;
                        break;
                    case Axis.Z:
                        previous.Param.ZAxis = false;
                        break;
                }
            }

            selectedAxis[axis.Value] = newly;

            switch (axis.Value)
            {
                case Axis.X:
                    newly.Param.XAxis = true;
                    newly.Param.YAxis = false;
                    newly.Param.ZAxis = false;
                    break;
                case Axis.Y:
                    newly.Param.XAxis = false;
                    newly.Param.YAxis = true;
                    newly.Param.ZAxis = false;
                    break;
                case Axis.Z:
                    newly.Param.XAxis = false;
                    newly.Param.ZAxis = true;
                    newly.Param.YAxis = false;
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

        private void InitCheckAllToggle()
        {
            checkAllToggle.value = Project.ConfigProcess?.Params != null &&
                                   Project.ConfigProcess.Params.All(kv => kv.Value.Selected);
        }

        private void ApplyScrollViewOrderType()
        {
            if (paramScrollView == null || Project.ConfigProcess?.Params == null)
            {
                return;
            }

            IEnumerable<KeyValuePair<string, ConfigParam>> ordered = scrollViewOrderType switch
            {
                ScrollViewOrderType.AZ => Project.ConfigProcess.Params.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase),
                ScrollViewOrderType.ZA => Project.ConfigProcess.Params.OrderByDescending(k => k.Key, StringComparer.OrdinalIgnoreCase),
                _ => Project.ConfigProcess.Params
            };

            UpdateOrderTypeLabel();

            paramScrollView.contentContainer.Clear();
            foreach (var kvp in ordered)
            {
                paramScrollView.Add(paramControllers[kvp.Key].Root);
            }
        }

        private void UpdateOrderTypeLabel()
        {
            switch (scrollViewOrderType)
            {
                case ScrollViewOrderType.AZ:
                    headerNameButton.text = "Name (A-Z)";
                    break;
                case ScrollViewOrderType.ZA:
                    headerNameButton.text = "Name (Z-A)";
                    break;
                default:
                    headerNameButton.text = "Name";
                    break;
            }
        }

    }

}
