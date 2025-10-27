/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hjg.Pngcs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Astrovisio
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager Instance { get; set; }
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
        private DataInspectorController dataInspectorController;
        private GizmoTransformController gizmoTransformController;
        private SettingsViewController settingsViewController;
        private LoaderController loaderController;
        private AboutViewController aboutViewController;
        private ReadMoreViewController readMoreViewController;

        // === Event System ===
        [SerializeField] private InputSystemUIInputModule desktopInputModule;
        [SerializeField] private XRUIInputModule xrInputModule;

        // === Local ===
        private bool uiVisibility = true;
        private bool clickStartedOnUI = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of UIManager found. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

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

            // Deprecated ?
            VisualElement dataInspector = uiDocument.rootVisualElement.Q<VisualElement>("DataInspector");
            dataInspectorController = new DataInspectorController(dataInspector);

            VisualElement gizmoTransform = uiDocument.rootVisualElement.Q<VisualElement>("GizmoTransform");
            gizmoTransformController = new GizmoTransformController(gizmoTransform);

            VisualElement settingsView = uiDocument.rootVisualElement.Q<VisualElement>("SettingsView");
            settingsViewController = new SettingsViewController(settingsView, this);

            VisualElement loaderView = uiDocument.rootVisualElement.Q<VisualElement>("LoaderView");
            loaderController = new LoaderController(loaderView);

            VisualElement aboutView = uiDocument.rootVisualElement.Q<VisualElement>("AboutView");
            aboutViewController = new AboutViewController(aboutView, this);

            VisualElement readMoreView = uiDocument.rootVisualElement.Q<VisualElement>("ReadMoreView");
            readMoreViewController = new ReadMoreViewController(readMoreView, this);

            projectManager.ProjectCreated += OnProjectCreated;
            projectManager.ProjectDeleted += OnProjectDeleted;
            projectManager.ApiError += OnApiError;

            projectManager.FetchAllProjects();
        }

        private void Update()
        {
            CheckClickStart();
        }

        public ProjectManager GetProjectManager()
        {
            return projectManager;
        }
        
        public UIContextSO GetUIContext()
        {
            return uiContextSO;
        }

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

        public void CheckClickStart()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame)
            {
                clickStartedOnUI = IsPointerOverVisibleUI();
            }

            if (!Mouse.current.leftButton.isPressed &&
                !Mouse.current.rightButton.isPressed &&
                !Mouse.current.middleButton.isPressed)
            {
                clickStartedOnUI = false;
            }
        }

        public bool HasClickStartedOnUI()
        {
            return clickStartedOnUI;
        }

        public bool IsPointerOverVisibleUI()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null || uiDocument.rootVisualElement.panel == null)
            {
                if (uiDocument)
                {
                    Debug.Log("rootVisualElement or panel is null");
                }

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
            settingsViewController.SetSettingsVisibility(!state);
            SetGizmoTransformVisibility(false);

            if (state == true)
            {
                RenderManager.Instance.ClearDataContainer();
            }
        }

        public void SetUIVisibility(bool state)
        {
            uiVisibility = state;

            mainViewController.SetHeaderVisibility(state);
            mainViewController.SetSideVisibility(state);
            // mainViewController.SetContentVisibility(state);
            // mainViewController.SetBackgroundVisibility(state);

            SetGizmoTransformVisibility(state);
            // SetDataInspectorVisibility(state);
        }

        public bool GetUIVisibility()
        {
            return uiVisibility;
        }

        public void SetLoadingView(bool state, LoaderType loaderType = LoaderType.Spinner)
        {
            VisualElement loaderView = uiDocument.rootVisualElement.Q<VisualElement>("LoaderView");
            // VisualElement loadingSpinner = loaderView.Q<VisualElement>("LoadingSpinner");
            // VisualElement loadingBar = loaderView.Q<VisualElement>("LoadingBar");

            if (state)
            {
                loaderView.AddToClassList("active");

                if (loaderType == LoaderType.Spinner)
                {
                    // loadingSpinner.style.display = DisplayStyle.Flex;
                    // loadingBar.style.display = DisplayStyle.None;
                    loaderController.SetSpinnerVisibility(true);
                    loaderController.SetBarVisibility(false);
                }
                else
                {
                    // loadingSpinner.style.display = DisplayStyle.None;
                    // loadingBar.style.display = DisplayStyle.Flex;
                    loaderController.SetSpinnerVisibility(false);
                    loaderController.SetBarVisibility(true);
                }
            }
            else
            {
                loaderView.RemoveFromClassList("active");
            }
        }

        public static void SetLoadingView(bool active)
        {
            Instance.SetLoadingView(active);
        }

        public void SetLoadingBarProgress(float value, string text = "", bool visibility = true)
        {
            loaderController.SetBarProgress(value, text, visibility);
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

        public void SetDataInspectorVisibility(bool state)
        {
            dataInspectorController.SetVisibility(state);
        }

        public void SetDataInspector(string[] header, float[] dataInfo)
        {
            dataInspectorController.SetData(header, dataInfo);
        }

        public void SetGizmoTransformVisibility(bool state)
        {
            gizmoTransformController.SetVisibility(state);
        }

        public void SetAboutViewVisibility(bool state)
        {
            if (state)
            {
                aboutViewController.Open();
            }
            else
            {
                aboutViewController.Close();
            }
        }

        public void SetReadMoreViewVisibility(bool state, string title, string description)
        {
            if (state)
            {
                readMoreViewController.Open(title, description);
            }
            else
            {
                readMoreViewController.Close();
            }
        }

        public async Task TakeScreenshot(bool uiVisibility = false)
        {
            Project currentProject = projectManager.GetCurrentProject();
            Settings settings = SettingsManager.Instance.GetCurrentFileSettings();
            File file = projectManager.GetCurrentProject().Files.Find(i => i.Id == ReelManager.Instance.GetReelCurrentFileId(projectManager.GetCurrentProject().Id));
            settings.Path = file.Path;

            await ScreenshotUtils.TakeScreenshotWithJson(currentProject.Name, file, Camera.main, renderManager.DataRenderer.GetAstrovidioDataSetRenderer().gameObject, settings, uiVisibility);
        }
    }

}
