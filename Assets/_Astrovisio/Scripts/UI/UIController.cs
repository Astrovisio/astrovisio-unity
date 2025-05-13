using UnityEngine;

namespace Astrovisio
{
    public class UIController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ProjectManager projectManager;
        [SerializeField] private RenderManager renderManager;

        private void Start()
        {
            projectManager.FetchAllProjects();
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
