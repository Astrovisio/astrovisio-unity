using UnityEngine;

namespace Astrovisio
{
    public class UIController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ProjectManager projectManager;

        private void Start()
        {
            projectManager.FetchAllProjects();
        }

        public ProjectManager GetProjectManager()
        {
            return projectManager;
        }

    }
}
