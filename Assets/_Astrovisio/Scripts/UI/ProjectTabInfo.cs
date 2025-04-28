using UnityEngine.UIElements;

namespace Astrovisio
{

    public class ProjectTabInfo
    {
        public Project Project { get; private set; }
        public VisualElement TabElement { get; private set; }

        public ProjectTabInfo(Project project, VisualElement tabElement)
        {
            Project = project;
            TabElement = tabElement;
        }
    }

}
