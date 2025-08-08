using System;
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

            RenderManager.Instance.OnProjectReadyToGetRendered += OnProjectReadyToGetRendered;
            ProjectManager.ProjectOpened += OnProjectOpened;

            Init();
            SetActiveStep(ProjectSidebarStep.Data);

            // Debug.Log("###" + (projectManager.GetProject(Project.Id) == Project));
        }

        private void Init()
        {
            // Data
            dataSettingsContainer = Root.Q<VisualElement>("DataSettingsContainer");
            dataSettingsButton = dataSettingsContainer.Q<Button>("AccordionHeader");
            dataSettingsButton.clicked += OnDataSettingsButtonClicked;

            // Render
            renderSettingsContainer = Root.Q<VisualElement>("RenderSettingsContainer");
            renderSettingsButton = renderSettingsContainer.Q<Button>("AccordionHeader");
            renderSettingsButton.clicked += OnRenderSettingsButtonClicked;
            renderSettingsButton.SetEnabled(false);

            // VR
            goToVRButton = Root.Q<Button>("GoToVRButton");
            goToVRButton.SetEnabled(false);
            goToVRButton.clicked += OnGoToVRButtonClicked;

            // Init
            projectSidebarDataController = new ProjectSidebarDataController(this, UIManager, ProjectManager, UIContextSO, Project, dataSettingsContainer);
            projectSidebarRenderController = new ProjectSidebarRenderController(this, UIManager, ProjectManager, UIContextSO, Project, renderSettingsContainer);
        }

        private void OnProjectOpened(Project project)
        {
            if (Project.Id != project.Id)
            {
                return;
            }

            SetActiveStep(ProjectSidebarStep.Data);
        }

        private void OnProjectReadyToGetRendered(Project project)
        {
            // Debug.Log($"Returned {Project.Id} {Project.Name} - {project.Id} {project.Name}");
            // Debug.Log(Project == project);
            // Debug.Log(ReferenceEquals(project, Project));

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

        private void OnDataSettingsButtonClicked()
        {
            SetActiveStep(ProjectSidebarStep.Data);
            UIManager.SetDataInspectorVisibility(false);
            UIManager.SetGizmoTransformVisibility(false);
        }

        private void OnRenderSettingsButtonClicked()
        {
            // Debug.Log("OnRenderSettingsButtonClicked " + Project.Name);
            SetActiveStep(ProjectSidebarStep.Render);
            RenderManager.Instance.RenderDataContainer(Project);
            UIManager.SetGizmoTransformVisibility(true);
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

    }

}