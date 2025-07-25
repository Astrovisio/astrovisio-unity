using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class ScreenrecorderSettingController
    {
        public VisualElement Root { get; }

        private Button playButton;
        private Button undoButton;
        private Label timerLabel;

        public ScreenrecorderSettingController(VisualElement root)
        {
            Root = root;
            Init();
        }

        private void Init()
        {
            playButton = Root.Q<Button>("RecButton");
            undoButton = Root.Q<Button>("ResetButton");
            timerLabel = Root.Q<Label>("TimerLabel");

            timerLabel.text = "00:00:00";

            // Debug.Log(playButton);
            // Debug.Log(undoButton);
            // Debug.Log(undoButton);
        }

        private void Reset()
        {

        }

    }

}
