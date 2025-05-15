using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class UIController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private RenderManager renderManager;

        // --- Local
        private UIDocument uiDocument;
        private MainViewController mainViewController;

        private void Start()
        {
            uiDocument = GetComponent<UIDocument>();

            var mainViewRoot = uiDocument.rootVisualElement.Q<VisualElement>("MainView");
            mainViewController = new MainViewController(mainViewRoot);

            projectManager.FetchAllProjects();


            projectManager.ProjectProcessed += OnProjectProcessed;
        }

        private void OnProjectProcessed(ProcessedData data)
        {
            mainViewController.SetBackground(false);
        }

        public ProjectManager GetProjectManager()
        {
            return projectManager;
        }

        public RenderManager GetRenderManager()
        {
            return renderManager;
        }



    }
}
