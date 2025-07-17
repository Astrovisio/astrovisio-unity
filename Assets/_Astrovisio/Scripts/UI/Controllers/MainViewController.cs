using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class MainViewController
    {
        private VisualElement mainViewRoot;
        private VisualElement body;
        private VisualElement header;
        private VisualElement side;
        private VisualElement content;

        public MainViewController(VisualElement root)
        {
            mainViewRoot = root.Q<VisualElement>("MainViewRoot");
            header = root.Q<VisualElement>("Header");
            body = root.Q<VisualElement>("Body");
            side = body.Q<VisualElement>("Side");
            content = body.Q<VisualElement>("Content");

            root.pickingMode = PickingMode.Ignore;
            mainViewRoot.pickingMode = PickingMode.Ignore;
            header.pickingMode = PickingMode.Position;
            body.pickingMode = PickingMode.Ignore;
        }

        public void SetBackgroundVisibility(bool state)
        {
            // Debug.Log("SetBackgroundVisibility");
            if (state)
            {
                mainViewRoot.AddToClassList("active");
            }
            else
            {
                mainViewRoot.RemoveFromClassList("active");
            }
        }

        public void SetHeaderVisibility(bool state)
        {
            // Debug.Log("SetHeaderVisibility");
            if (state)
            {
                header.style.visibility = Visibility.Visible;
            }
            else
            {
                header.style.visibility = Visibility.Hidden;
            }
        }

        public void SetSideVisibility(bool state)
        {
            // Debug.Log("SetSideVisibility");
            if (state)
            {
                side.style.visibility = Visibility.Visible;
            }
            else
            {
                side.style.visibility = Visibility.Hidden;
            }
        }

        public void SetContentVisibility(bool state)
        {
            // Debug.Log("SetContentVisibility");
            if (state)
            {
                content.style.visibility = Visibility.Visible;
            }
            else
            {
                content.style.visibility = Visibility.Hidden;
            }
        }

    }

}