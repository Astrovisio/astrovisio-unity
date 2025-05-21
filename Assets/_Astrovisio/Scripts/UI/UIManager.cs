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

        private void Start()
        {
            uiDocument = GetComponent<UIDocument>();

            var mainViewRoot = uiDocument.rootVisualElement.Q<VisualElement>("MainView");
            mainViewController = new MainViewController(mainViewRoot);

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
            var loaderView = uiDocument.rootVisualElement.Q<VisualElement>("LoaderView");

            if (state)
            {
                loaderView.AddToClassList("active");
            }
            else
            {
                loaderView.RemoveFromClassList("active");
            }

        }

    }

}
