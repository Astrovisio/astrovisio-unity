using UnityEngine;
using UnityEngine.UIElements;


namespace Astrovisio
{
    public class HomeViewController
    {

        private readonly ProjectManager projectManager;
        private VisualElement root;

        public HomeViewController(ProjectManager projectManager)
        {
            this.projectManager = projectManager;
        }

        public void Initialize(VisualElement root)
        {
            this.root = root;
        }

    }

}