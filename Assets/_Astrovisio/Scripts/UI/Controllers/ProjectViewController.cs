using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

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
        private Toggle checkAllToggle;
        private Button headerNameButton;

        // === Local ===
        private readonly Dictionary<Axis, ParamRowController> selectedAxis = new();
        private readonly Dictionary<string, ParamRowController> paramControllers = new();
        private enum ScrollViewOrderType
        {
            None,
            AZ,
            ZA
        }
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
            VisualElement topContainer = Root.Query<VisualElement>("TopContainer");

            // Files Scroll View
            VisualElement filesContainer = topContainer.Q<VisualElement>("FilesContainer");
            FilesController<FileState> filesController = new FilesController<FileState>(filesContainer, UIManager.GetUIContext());
            // ScrollView projectScrollView = filesContainer.Q<ScrollView>("ScrollFiles");
            // projectScrollView.Clear();
            // filesContainer.SetEnabled(false);

            // Project Name
            projectNameLabel = topContainer.Q<Label>("ProjectNameLabel");
            projectNameLabel.text = Project.Name;

            // Project Description
            descriptionLabel = topContainer.Q<Label>("DescriptionLabel");
            descriptionLabel.text = Project.Description;

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
            Debug.Log(headerNameButton);
            headerNameButton.clicked += ChangeScrollViewOrderType;

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
            foreach (var kvp in paramControllers.ToList())
            {
                kvp.Value.SetSelected(isChecked);
            }
        }

        private void InitScrollView()
        {
            VisualElement paramsContainer = Root.Query<VisualElement>("ParamsContainer");

            ScrollView paramScrollView = paramsContainer.Q<ScrollView>("ParamScrollView");
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

                ParamRowController controller = new ParamRowController(ProjectManager, paramRow, paramName, param);
                paramControllers[paramName] = controller;

                controller.OnAxisChanged += HandleOnAxisChanged;
                controller.OnThresholdChanged += HandleOnThresholdChanged;

                // controller.OnParamStateChanged += () => Debug.Log($"{paramName} disabled."); // TODO: update project request?

                paramScrollView.Add(paramRow);
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
                    // Debug.Log($"[Init] Axis X set to {controller.ParamName}");
                }
                if (param.YAxis)
                {
                    selectedAxis[Axis.Y] = controller;
                    // Debug.Log($"[Init] Axis Y set to {controller.ParamName}");
                }
                if (param.ZAxis)
                {
                    selectedAxis[Axis.Z] = controller;
                    // Debug.Log($"[Init] Axis Z set to {controller.ParamName}");
                }
            }
        }

        private void HandleOnAxisChanged(Axis? axis, ParamRowController newly)
        {
            if (axis == null)
            {
                // Caso: l'utente ha cliccato su un chip già attivo → lo sta deselezionando
                // Verifica se 'newly' era registrato in uno degli assi attivi, e rimuovilo
                foreach (var kvp in selectedAxis.ToList())
                {
                    if (kvp.Value == newly)
                    {
                        selectedAxis.Remove(kvp.Key);
                        // Debug.Log($"[AxisSelection] Deselected axis {kvp.Key} from {newly.ParamName}");

                        // Aggiorna il modello
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

            // 1) se c’è già un selezionato per quell’asse, deselezionalo
            if (selectedAxis.TryGetValue(axis.Value, out var previous) && previous != newly)
            {
                // Debug.Log($"[AxisSelection] New axis selected: {axis} → {newly.ParamName}");
                // if (previous != null)
                // Debug.Log($"[AxisSelection] Previous selection for {axis} was: {previous.ParamName}");

                previous.DeselectAxis(axis.Value);

                // Aggiorna il modello dell'asse precedente
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

            // 2) registra il nuovo selezionato
            selectedAxis[axis.Value] = newly;

            // 3) aggiorna il modello del nuovo selezionato
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
            ProjectManager.UpdateProject(Project.Id, Project);
        }

        private void InitCheckAllToggle()
        {

            bool areAllSelected = true;
            foreach (var kvp in Project.ConfigProcess.Params)
            {
                var paramName = kvp.Key;
                var param = kvp.Value;

                if (param.Selected == false)
                {
                    areAllSelected = false;
                }
                // Debug.Log($"{paramName} - {param.Selected}");
            }

            checkAllToggle.value = areAllSelected;
        }

        private void ChangeScrollViewOrderType()
        {
            switch (scrollViewOrderType)
            {
                case ScrollViewOrderType.None:
                    scrollViewOrderType = ScrollViewOrderType.AZ;
                    break;
                case ScrollViewOrderType.AZ:
                    scrollViewOrderType = ScrollViewOrderType.ZA;
                    break;
                case ScrollViewOrderType.ZA:
                    scrollViewOrderType = ScrollViewOrderType.None;
                    break;
            }

            // Debug.Log("New order type: " + scrollViewOrderType);
            ApplyScrollViewOrderType();
        }

        private void ApplyScrollViewOrderType()
        {
            VisualElement paramsContainer = Root.Q<VisualElement>("ParamsContainer");
            ScrollView paramScrollView = paramsContainer.Q<ScrollView>("ParamScrollView");

            if (paramScrollView == null || Project.ConfigProcess?.Params == null)
            {
                return;
            }

            paramScrollView.contentContainer.Clear();

            IEnumerable<KeyValuePair<string, ConfigParam>> ordered;

            switch (scrollViewOrderType)
            {
                case ScrollViewOrderType.AZ:
                    ordered = Project.ConfigProcess.Params.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
                    break;

                case ScrollViewOrderType.ZA:
                    ordered = Project.ConfigProcess.Params.OrderByDescending(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
                    break;

                default: // None
                    ordered = Project.ConfigProcess.Params;
                    break;
            }

            UpdateOrderTypeLabel();

            foreach (var kvp in ordered)
            {
                string paramName = kvp.Key;
                ConfigParam param = kvp.Value;

                TemplateContainer paramRow = UIManager.GetUIContext().paramRowTemplate.CloneTree();
                VisualElement nameContainer = paramRow.Q<VisualElement>("NameContainer");
                nameContainer.Q<Label>("Label").text = paramName;

                ParamRowController controller = new ParamRowController(ProjectManager, paramRow, paramName, param);
                paramControllers[paramName] = controller;

                controller.OnAxisChanged += HandleOnAxisChanged;
                controller.OnThresholdChanged += HandleOnThresholdChanged;

                paramScrollView.Add(paramRow);
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

                default: // None
                    headerNameButton.text = "Name";
                    break;
            }
        }


    }

}
