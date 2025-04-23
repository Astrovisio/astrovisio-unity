using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class SideController : MonoBehaviour
    {

        [Header("UI Templates")]
        [SerializeField] private VisualTreeAsset favouriteButtonTemplate;

        // === References ===
        private UIDocument uiDocument;
        private UIController uiController;
        private ProjectManager projectManager;

        // === Controllers ===
        

        private void Awake()
        {
            uiDocument = GetComponentInParent<UIDocument>();
            uiController = GetComponentInParent<UIController>();
            projectManager = uiController.GetProjectManager();

            if (projectManager == null)
            {
                Debug.LogError("ProjectManager not found.");
            }
        }

    }

}