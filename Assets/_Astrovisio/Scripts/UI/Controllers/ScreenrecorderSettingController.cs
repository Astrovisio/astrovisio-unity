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
        private IVisualElementScheduledItem timerSchedule;

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

            playButton.clicked += ToggleRecording;
            undoButton.clicked += Reset;
        }

        private void ToggleRecording()
        {
            if (!RecorderManager.Instance.IsRecording)
            {
                Debug.Log("Recording started.");
                RecorderManager.Instance.StartRecording();
                playButton.AddToClassList("active");

                if (timerSchedule == null)
                    timerSchedule = Root.schedule.Execute(UpdateTimerLabel).Every(500);
                else
                    timerSchedule.Resume();

                return;
            }

            if (!RecorderManager.Instance.IsPaused)
            {
                Debug.Log("Recording paused.");
                RecorderManager.Instance.PauseRecording();
                playButton.RemoveFromClassList("active");
                timerSchedule?.Pause();
            }
            else
            {
                Debug.Log("Recording resumed.");
                RecorderManager.Instance.ResumeRecording();
                playButton.AddToClassList("active");
                timerSchedule?.Resume();
            }
        }

        private void Reset()
        {
            if (RecorderManager.Instance.IsRecording)
            {
                Debug.Log("Recording stopped.");
                RecorderManager.Instance.StopRecording();
            }

            playButton.RemoveFromClassList("active");
            timerLabel.text = "00:00:00";
            timerSchedule?.Pause();
        }

        private void UpdateTimerLabel()
        {
            if (RecorderManager.Instance.IsRecording && !RecorderManager.Instance.IsPaused)
            {
                timerLabel.text = RecorderManager.Instance.GetRecordingTime();
            }
        }

    }

}
