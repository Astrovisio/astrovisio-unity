using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Astrovisio
{

    public class ProjectViewController
    {
        // === Dependencies ===
        private readonly ProjectManager projectManager;
        private readonly VisualTreeAsset paramRowTemplate;

        // === Local ===
        public Project Project { get; }
        public VisualElement Root { get; }

        private readonly Dictionary<Axis, ParamRowController> selectedAxis = new Dictionary<Axis, ParamRowController>();

        private ScrollView paramScrollView;
        private Label projectNameLabel;

        private Dictionary<string, ParamRowController> paramControllers = new();

        public ProjectViewController(ProjectManager projectManager, VisualElement root, Project project, VisualTreeAsset paramRowTemplate)
        {
            this.projectManager = projectManager;
            Root = root;
            Project = project;
            this.paramRowTemplate = paramRowTemplate;

            Init();
        }

        private void Init()
        {
            var rootContainer = Root.Children().First();
            var leftContainer = rootContainer.Children().FirstOrDefault(child => child.name == "LeftContainer");
            var rightContainer = rootContainer.Children().FirstOrDefault(child => child.name == "RightContainer");

            paramScrollView = leftContainer.Q<ScrollView>("ParamScrollView");

            projectNameLabel = rightContainer.Q<Label>("ProjectNameLabel");
            projectNameLabel.text = Project.Name;

            PopulateScrollView();
        }

        private void PopulateScrollView()
        {
            paramScrollView.contentContainer.Clear();
            paramControllers.Clear();

            if (Project.ConfigProcess?.Params == null)
            {
                Debug.LogWarning("No variables to display.");
                return;
            }

            foreach (var kvp in Project.ConfigProcess.Params)
            {
                var paramName = kvp.Key;
                var param = kvp.Value;

                var paramRow = paramRowTemplate.CloneTree();
                var nameContainer = paramRow.Q<VisualElement>("NameContainer");
                nameContainer.Q<Label>("Label").text = paramName;

                var controller = new ParamRowController(paramRow, paramName, param);
                paramControllers[paramName] = controller;

                controller.OnAxisChanged += HandleAxisSelected;

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

        private void HandleAxisSelected(Axis? axis, ParamRowController newly)
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

        private void UpdateProject()
        {
            var updateProjectRequest = new UpdateProjectRequest
            {
                Name = Project.Name,
                Favourite = Project.Favourite,
                Description = Project.Description,
                Paths = Project.Paths,
                ConfigProcess = Project.ConfigProcess
            };

            // foreach (var kvp in Project.ConfigProcess.Params)
            // {
            //     var paramName = kvp.Key;
            //     var param = kvp.Value;
            //     Debug.Log($"[Param Debug] {paramName} → X: {param.XAxis}, Y: {param.YAxis}, Z: {param.ZAxis}");
            // }

            // projectManager.UpdateProject(Project.Id, updateProjectRequest);
        }

    }

}
