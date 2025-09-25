using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    public enum ProjectSidebarStep
    {
        Data,
        Render
    }

    public class ProjectSidebarController
    {
        // === Dependencies ===
        public UIManager UIManager { private set; get; }
        public ProjectManager ProjectManager { private set; get; }
        public UIContextSO UIContextSO { private set; get; }
        public Project Project { private set; get; }
        public VisualElement Root { private set; get; }

        // === Other ===
        private ProjectSidebarDataController projectSidebarDataController;
        private ProjectSidebarRenderController projectSidebarRenderController;

        private VisualElement dataSettingsContainer;
        private VisualElement renderSettingsContainer;

        private Button dataSettingsButton;
        private Button renderSettingsButton;
        private Button goToVRButton;
        // private DataContainer dataContainer;

        public ProjectSidebarController(UIManager uiManager, ProjectManager projectManager, UIContextSO uiContextSO, Project project, VisualElement root)
        {
            UIManager = uiManager;
            ProjectManager = projectManager;
            UIContextSO = uiContextSO;
            Project = project;
            Root = root;

            RenderManager.Instance.OnProjectRenderReady += OnProjectReadyToGetRendered;
            ProjectManager.ProjectOpened += OnProjectOpened;

            Init();
            SetActiveStep(ProjectSidebarStep.Data);

            // Debug.Log("###" + (projectManager.GetProject(Project.Id) == Project));
        }

        private void Init()
        {
            bool anyFileProcessed = Project?.Files?.Any(f => f.Processed) ?? false;

            // Data
            dataSettingsContainer = Root.Q<VisualElement>("DataSettingsContainer");
            dataSettingsButton = dataSettingsContainer.Q<Button>("AccordionHeader");
            dataSettingsButton.clicked += OnDataSettingsButtonClicked;

            // Render
            renderSettingsContainer = Root.Q<VisualElement>("RenderSettingsContainer");
            renderSettingsButton = renderSettingsContainer.Q<Button>("AccordionHeader");
            renderSettingsButton.clicked += OnRenderSettingsButtonClicked;
            renderSettingsButton.SetEnabled(anyFileProcessed);

            // VR
            goToVRButton = Root.Q<Button>("GoToVRButton");
            goToVRButton.SetEnabled(false);
            goToVRButton.clicked += OnGoToVRButtonClicked;

            // Init
            projectSidebarDataController = new ProjectSidebarDataController(this, UIManager, ProjectManager, UIContextSO, Project, dataSettingsContainer);
            projectSidebarRenderController = new ProjectSidebarRenderController(this, UIManager, ProjectManager, UIContextSO, Project, renderSettingsContainer);
        }

        private void OnDataSettingsButtonClicked()
        {
            SetActiveStep(ProjectSidebarStep.Data);
            UIManager.SetDataInspectorVisibility(false);
            UIManager.SetGizmoTransformVisibility(false);
        }

        private void OnRenderSettingsButtonClicked()
        {
            // Debug.Log($"[Sidebar] Render clicked for Project id={Project?.Id}, name='{Project?.Name}'");

            if (Project?.Files == null)
            {
                Debug.LogWarning("[Sidebar] Project.Files is null");
                return;
            }

            // Debug.Log($"[Sidebar] Files count = {Project.Files.Count}");

            // Log dettagliato di ogni file e presenza del DataContainer in RenderManager
            foreach (File f in Project.Files.OrderBy(f => f.Order))
            {
                bool hasDC = RenderManager.Instance.TryGetDataContainer(Project, f, out var _);
                // Debug.Log($"[Sidebar] File id={f.Id}, name='{f.Name}', processed={f.Processed}, order={f.Order}, processedPath='{f.ProcessedPath}', hasDataContainer={hasDC}");
            }

            SetActiveStep(ProjectSidebarStep.Render);

            // 1) prova standard: primo file con Processed == true
            File fileToRender = Project.Files.FirstOrDefault(f => f.Processed);

            // 2) fallback: se nessun file è marcato Processed, ma il DataContainer esiste già in RenderManager
            if (fileToRender == null)
            {
                fileToRender = Project.Files.FirstOrDefault(f => RenderManager.Instance.TryGetDataContainer(Project, f, out var _));
                if (fileToRender != null)
                {
                    Debug.Log($"[Sidebar] Nessun file con Processed=true, ma trovato DataContainer per '{fileToRender.Name}'. Lo userò come fallback.");
                }
            }

            if (fileToRender != null)
            {
                // Debug.Log($"[Sidebar] Rendering file id={fileToRender.Id}, name='{fileToRender.Name}'");
                RenderManager.Instance.RenderFile(Project, fileToRender);
                UIManager.SetGizmoTransformVisibility(true);
            }
            else
            {
                Debug.LogWarning($"[Sidebar] Nessun file processato trovato e nessun DataContainer registrato per Project id={Project.Id}, name='{Project.Name}'. Vedi log sopra.");
            }
        }


        private void OnGoToVRButtonClicked()
        {
            // Debug.Log("OnGoToVRButtonClicked");

            if (!XRManager.Instance.IsVRActive)
            {
                XRManager.Instance.EnterVR(() => SetGoToVRButtonState(true));
            }
            else
            {
                XRManager.Instance.ExitVR();
                SetGoToVRButtonState(false);
            }
            // UIManager.SetErrorVR(true);
        }

        private void SetActiveStep(ProjectSidebarStep projectSidebarStep)
        {
            switch (projectSidebarStep)
            {
                case ProjectSidebarStep.Data:
                    renderSettingsContainer.RemoveFromClassList("active");
                    dataSettingsContainer.AddToClassList("active");
                    UIManager.SetSceneVisibility(true);
                    break;
                case ProjectSidebarStep.Render:
                    dataSettingsContainer.RemoveFromClassList("active");
                    renderSettingsContainer.AddToClassList("active");
                    UIManager.SetSceneVisibility(false);
                    projectSidebarRenderController.Update();
                    break;
            }
        }

        public void SetNextStepButtons(bool state)
        {
            renderSettingsButton.SetEnabled(state);
            goToVRButton.SetEnabled(state);

            if (state)
            {
                renderSettingsButton.style.opacity = 1.0f;
                goToVRButton.style.opacity = 1.0f;
            }
            else
            {
                renderSettingsButton.style.opacity = 0.5f;
                goToVRButton.style.opacity = 0.5f;
            }
        }

        private void SetGoToVRButtonState(bool active)
        {
            Label label = goToVRButton.Q<Label>("Label");
            if (active)
            {
                label.text = "Exit VR";
            }
            else
            {
                label.text = "Go To VR";
            }
            goToVRButton.Blur();
        }

        private void OnProjectReadyToGetRendered(Project project)
        {
            if (Project.Id != project.Id)
            {
                return;
            }
            else
            {
                Project = project;
            }

            SetNextStepButtons(true);
        }

        private void OnProjectOpened(Project project)
        {
            if (Project.Id != project.Id)
            {
                return;
            }

            SetActiveStep(ProjectSidebarStep.Data);
        }

    }

}