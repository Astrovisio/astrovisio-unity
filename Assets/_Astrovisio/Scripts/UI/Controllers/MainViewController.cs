using UnityEngine.UIElements;

namespace Astrovisio
{
    public class MainViewController
    {
        private VisualElement mainViewRoot;

        public MainViewController(VisualElement root)
        {
            mainViewRoot = root.Q<VisualElement>("MainViewRoot");
        }

        public void SetBackground(bool state)
        {
            if (state)
            {
                mainViewRoot.AddToClassList("active");
            }
            else
            {
                mainViewRoot.RemoveFromClassList("active");
            }
        }

    }

}