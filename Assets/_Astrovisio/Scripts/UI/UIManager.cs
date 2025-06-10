using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class UIManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private RenderManager renderManager;
        [SerializeField] private UIContextSO uiContextSO;

        // === References ===
        private UIDocument uiDocument;
        private MainViewController mainViewController;
        private ErrorVRViewController errorVRViewController;
        private ToastMessageController toastMessageController;
        private DeleteProjectViewController deleteProjectViewController;

        private void Start()
        {
            uiDocument = GetComponent<UIDocument>();

            VisualElement mainView = uiDocument.rootVisualElement.Q<VisualElement>("MainView");
            mainViewController = new MainViewController(mainView);

            VisualElement errorVRView = uiDocument.rootVisualElement.Q<VisualElement>("ErrorVrView");
            errorVRViewController = new ErrorVRViewController(errorVRView);

            VisualElement toastMessage = uiDocument.rootVisualElement.Q<VisualElement>("ToastMessageView");
            toastMessageController = new ToastMessageController(toastMessage);

            VisualElement deleteProjectView = uiDocument.rootVisualElement.Q<VisualElement>("DeleteProjectView");
            deleteProjectViewController = new DeleteProjectViewController(projectManager, deleteProjectView);

            projectManager.ProjectCreated += OnProjectCreated;
            projectManager.ProjectDeleted += OnProjectDeleted;

            projectManager.FetchAllProjects();
        }

        public ProjectManager GetProjectManager() => projectManager;
        public RenderManager GetRenderManager() => renderManager;
        public UIContextSO getUIContext() => uiContextSO;

        public void SetSceneVisibility(bool state)
        {
            mainViewController.SetContentVisibility(state);
            mainViewController.SetBackgroundVisibility(state);
        }

        public void SetLoading(bool state)
        {
            VisualElement loaderView = uiDocument.rootVisualElement.Q<VisualElement>("LoaderView");

            if (state)
            {
                loaderView.AddToClassList("active");
            }
            else
            {
                loaderView.RemoveFromClassList("active");
            }
        }

        public void SetErrorVR(bool state)
        {
            if (state)
            {
                errorVRViewController.Open();
            }
            else
            {
                errorVRViewController.Close();
            }
        }

        private void OnProjectCreated(Project project)
        {
            toastMessageController.SetToastSuccessMessage($"Project {project.Name} has been created.");
        }

        private void OnProjectDeleted(Project project)
        {
            toastMessageController.SetToastSuccessMessage($"Project {project.Name} has been deleted.");
        }

        // public void SetToastSuccessMessage(string title, string message)
        // {
        //     toastMessageController.SetToastSuccessMessage(title, message);
        // }

        // public void SetToastErrorMessage(string message)
        // {
        //     toastMessageController.SetToastErrorMessage(message);
        // }

        public void SetDeleteProject(Project project)
        {
            VisualElement root = uiDocument.rootVisualElement;
            VisualElement deleteProjectView = root.Q<VisualElement>("DeleteProjectView");
            deleteProjectViewController.SetProjectToDelete(project);
            deleteProjectView.AddToClassList("active");
        }

    }

}
