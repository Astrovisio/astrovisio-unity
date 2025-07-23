using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ScreenshotSettingController
    {
        public VisualElement Root { get; }

        private Button screenshotButton;
        private Button recordingButton;

        public ScreenshotSettingController(VisualElement root)
        {
            Root = root;
            Init();
        }

        private void Init()
        {
            screenshotButton = Root.Q<Button>("ScreenshotButton");
            recordingButton = Root.Q<Button>("RecordingButton");
        }

        private void Reset()
        {
            // ???
        }

    }

}
