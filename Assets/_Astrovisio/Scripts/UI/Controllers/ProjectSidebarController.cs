using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ParamRow
    {
        public VisualElement VisualElement { get; set; }
        public ParamRowSettingsController ParamRowSettingsController { get; set; }
    }

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
        private DataContainer dataContainer;

        public ProjectSidebarController(UIManager uiManager, ProjectManager projectManager, UIContextSO uiContextSO, Project project, VisualElement root)
        {
            UIManager = uiManager;
            ProjectManager = projectManager;
            UIContextSO = uiContextSO;
            Project = project;
            Root = root;

            ProjectManager.ProjectProcessed += OnProjectProcessed;
            ProjectManager.ProjectOpened += OnProjectOpened;

            Init();
            SetActiveStep(ProjectSidebarStep.Data);
        }

        private void Init()
        {
            // Data
            dataSettingsContainer = Root.Q<VisualElement>("DataSettingsContainer");
            dataSettingsButton = dataSettingsContainer.Q<Button>("AccordionHeader");
            dataSettingsButton.clicked += OnDataSettingsButtonClicked;
            projectSidebarDataController = new ProjectSidebarDataController(this, UIManager, ProjectManager, UIContextSO, Project, dataSettingsContainer);

            // Render
            renderSettingsContainer = Root.Q<VisualElement>("RenderSettingsContainer");
            renderSettingsButton = renderSettingsContainer.Q<Button>("AccordionHeader");
            renderSettingsButton.clicked += OnRenderSettingsButtonClicked;
            renderSettingsButton.SetEnabled(false);
            projectSidebarRenderController = new ProjectSidebarRenderController(this, UIManager, ProjectManager, UIContextSO, Project, renderSettingsContainer);

            // VR
            goToVRButton = Root.Q<Button>("GoToVRButton");
            goToVRButton.SetEnabled(false);
            goToVRButton.clicked += OnGoToVRButtonClicked;
        }

        private void OnProjectOpened(Project project)
        {
            SetActiveStep(ProjectSidebarStep.Data);
        }

        private void OnProjectProcessed(DataPack data)
        {
            dataContainer = new DataContainer(data, Project);
            // Debug.Log(dataContainer.PointCount);
            // SetProcessDataButton(true);
            SetNextStepButtons(true);
        }

        private void OnDataSettingsButtonClicked()
        {
            SetActiveStep(ProjectSidebarStep.Data);
        }

        private void OnRenderSettingsButtonClicked()
        {
            SetActiveStep(ProjectSidebarStep.Render);
            RenderManager.Instance.RenderDataContainer(dataContainer); // To remove/change
        }

        private void OnGoToVRButtonClicked()
        {
            Debug.Log("OnGoToVRButtonClicked");
            VRManager.Instance.EnterVR();
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

    }

}