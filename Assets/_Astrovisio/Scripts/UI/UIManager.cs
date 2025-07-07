using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.UI;

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
        private EditProjectViewController editProjectViewController;
        private DuplicateProjectViewController duplicateProjectViewController;
        private DeleteProjectViewController deleteProjectViewController;

        // Event System
        [SerializeField] private InputSystemUIInputModule desktopInputModule;
        [SerializeField] private XRUIInputModule xrInputModule;

        // === Local ===
        private bool isInteractingWithUI = false;


        private void Start()
        {
            uiDocument = GetComponent<UIDocument>();

            VisualElement mainView = uiDocument.rootVisualElement.Q<VisualElement>("MainView");
            mainViewController = new MainViewController(mainView);

            VisualElement errorVRView = uiDocument.rootVisualElement.Q<VisualElement>("ErrorVrView");
            errorVRViewController = new ErrorVRViewController(errorVRView);

            VisualElement toastMessage = uiDocument.rootVisualElement.Q<VisualElement>("ToastMessageView");
            toastMessageController = new ToastMessageController(toastMessage);

            VisualElement editProjectView = uiDocument.rootVisualElement.Q<VisualElement>("EditProjectView");
            editProjectViewController = new EditProjectViewController(projectManager, editProjectView);
            duplicateProjectViewController = new DuplicateProjectViewController(projectManager, editProjectView);

            VisualElement deleteProjectView = uiDocument.rootVisualElement.Q<VisualElement>("DeleteProjectView");
            deleteProjectViewController = new DeleteProjectViewController(projectManager, deleteProjectView);

            projectManager.ProjectCreated += OnProjectCreated;
            projectManager.ProjectDeleted += OnProjectDeleted;
            projectManager.ApiError += OnApiError;

            projectManager.FetchAllProjects();
        }

        public ProjectManager GetProjectManager() => projectManager;
        public RenderManager GetRenderManager() => renderManager;
        public UIContextSO getUIContext() => uiContextSO;

        public void SwitchEventSystemToVR()
        {
            desktopInputModule.enabled = false;
            xrInputModule.enabled = true;
        }

        public void SwitchEventSystemToDesktop()
        {
            desktopInputModule.enabled = true;
            xrInputModule.enabled = false;
        }

        public bool IsInteractingWithUI()
        {
            isInteractingWithUI = IsPointerOverVisibleUI();
            return isInteractingWithUI;
        }

        private bool IsPointerOverVisibleUI()
        {
            if (uiDocument.rootVisualElement == null || uiDocument.rootVisualElement.panel == null)
            {
                Debug.Log("rootVisualElement or panel is null");
                return false;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            mousePosition.y = Screen.height - mousePosition.y;
            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(uiDocument.rootVisualElement.panel, mousePosition);
            VisualElement picked = uiDocument.rootVisualElement.panel.Pick(panelPosition);

            if (picked == null)
            {
                // Debug.Log("No UI");
                return false;
            }

            // Debug.Log($"UI: {picked.name}, pickingMode: {picked.pickingMode}, visibility: {picked.resolvedStyle.visibility}, display: {picked.resolvedStyle.display}");

            while (picked != null)
            {
                var style = picked.resolvedStyle;
                if (
                    picked.pickingMode == PickingMode.Position &&
                    style.visibility == Visibility.Visible &&
                    style.display != DisplayStyle.None &&
                    style.opacity > 0.01f &&
                    style.width > 0 && style.height > 0
                )
                {
                    return true;
                }

                picked = picked.parent;
            }

            return false;
        }

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

        private void OnApiError(string message)
        {
            toastMessageController.SetToastErrorMessage($"{message}");
        }

        public void SetEditProject(Project project)
        {
            // Debug.Log("SetEditProject");
            VisualElement root = uiDocument.rootVisualElement;
            VisualElement editProjectView = root.Q<VisualElement>("EditProjectView");
            Label titleLabel = editProjectView.Q<Label>("TitleLabel");
            titleLabel.text = "Edit title and description";
            editProjectViewController.SetProjectToEdit(project);
            editProjectView.AddToClassList("active");
        }

        public void SetDuplicateProject(Project project)
        {
            // Debug.Log("SetDuplicateProject");
            VisualElement root = uiDocument.rootVisualElement;
            VisualElement duplicateProjectView = root.Q<VisualElement>("EditProjectView");
            Label titleLabel = duplicateProjectView.Q<Label>("TitleLabel");
            titleLabel.text = "Duplicate project";
            duplicateProjectViewController.SetProjectToDuplicate(project);
            duplicateProjectView.AddToClassList("active");
            // Debug.Log("End SetDuplicateProject");
        }

        public void SetDeleteProject(Project project)
        {
            VisualElement root = uiDocument.rootVisualElement;
            VisualElement deleteProjectView = root.Q<VisualElement>("DeleteProjectView");
            deleteProjectViewController.SetProjectToDelete(project);
            deleteProjectView.AddToClassList("active");
        }

    }

}
