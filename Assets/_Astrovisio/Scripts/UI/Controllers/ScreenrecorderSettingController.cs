/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

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
