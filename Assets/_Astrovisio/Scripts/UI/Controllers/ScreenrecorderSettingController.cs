/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
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

        private bool isRecording = false;
        private float recordingTime = 0f;

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
            isRecording = !isRecording;

            if (isRecording)
            {
                playButton.AddToClassList("active");
                Debug.Log("Recording started.");
                // TODO: Start recording logic here
            }
            else
            {
                playButton.RemoveFromClassList("active");
                Debug.Log("Recording stopped.");
                // TODO: Stop recording logic here
            }
        }

        private void Reset()
        {
            if (isRecording)
            {
                ToggleRecording();
            }

            recordingTime = 0f;
            timerLabel.text = "00:00:00";
            Debug.Log("Recording reset.");
        }

        // TODO ???
        public void UpdateTimer(float deltaTime)
        {
            if (!isRecording)
            {
                return;
            }

            recordingTime += deltaTime;

            int hours = Mathf.FloorToInt(recordingTime / 3600f);
            int minutes = Mathf.FloorToInt((recordingTime % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(recordingTime % 60f);

            timerLabel.text = $"{hours:00}:{minutes:00}:{seconds:00}";
        }

    }

}
