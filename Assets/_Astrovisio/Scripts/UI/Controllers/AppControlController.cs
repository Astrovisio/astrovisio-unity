using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class AppControlController
    {
        public VisualElement Root { get; }

        private Button fullscreenButton;
        private Button closeButton;

        private int windowedWidth;
        private int windowedHeight;
        private bool windowedSizeStored = false;

        public AppControlController(VisualElement root)
        {
            Root = root;

            fullscreenButton = root.Q<Button>("FullscreenButton");
            closeButton = root.Q<Button>("CloseButton");

            if (fullscreenButton != null)
            {
                fullscreenButton.clicked += OnFullscreenClicked;
            }

            if (closeButton != null)
            {
                closeButton.clicked += OnCloseClicked;
            }

            if (!Screen.fullScreen)
            {
                windowedWidth = Screen.width;
                windowedHeight = Screen.height;
                windowedSizeStored = true;
            }
        }

        private void OnFullscreenClicked()
        {
#if UNITY_EDITOR
            Debug.Log("Fullscreen toggle is only active in build.");
#else
            if (!Screen.fullScreen)
            {
                windowedWidth = Screen.width;
                windowedHeight = Screen.height;
                windowedSizeStored = true;

                int width = Display.main.systemWidth;
                int height = Display.main.systemHeight;

                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.SetResolution(width, height, FullScreenMode.ExclusiveFullScreen);
                Screen.fullScreen = true;
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;

                if (windowedSizeStored && windowedWidth > 0 && windowedHeight > 0)
                {
                    Screen.SetResolution(windowedWidth, windowedHeight, FullScreenMode.Windowed);
                }

                Screen.fullScreen = false;
            }

            if (fullscreenButton != null)
            {
                if (Screen.fullScreen)
                {
                    fullscreenButton.RemoveFromClassList("active");
                }
                else
                {
                    fullscreenButton.AddToClassList("active");
                }
            }
#endif
        }

        private void OnCloseClicked()
        {
            UIManager.Instance.SetCloseViewVisibility(true);
        }

        public void Dispose()
        {
            if (fullscreenButton != null)
            {
                fullscreenButton.clicked -= OnFullscreenClicked;
            }

            if (closeButton != null)
            {
                closeButton.clicked -= OnCloseClicked;
            }
        }
    }
    
}
