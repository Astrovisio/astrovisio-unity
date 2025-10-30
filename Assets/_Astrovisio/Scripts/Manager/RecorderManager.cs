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

using SilverTau.NSR.Recorders.Video;
using UnityEngine;
using System;
using SFB;

namespace Astrovisio
{
    public class RecorderManager : MonoBehaviour
    {
        public static RecorderManager Instance { get; private set; }

        [SerializeField] private UIManager uiManager;
        [SerializeField] private UniversalVideoRecorder universalVideoRecorder;

        private string outputDir = "";
        private float recordingTime = 0f;

        public bool IsRecording { get; private set; }
        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (IsRecording && !IsPaused)
            {
                recordingTime += Time.deltaTime;
            }
        }

        [ContextMenu("Start Recording")]
        public void StartRecording()
        {
            if (IsRecording)
            {
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            outputDir = StandaloneFileBrowser.SaveFilePanel("Save Recording", "", $"{timestamp}.mp4", "mp4");

            if (string.IsNullOrEmpty(outputDir))
            {
                return;
            }

            universalVideoRecorder.StartRecorder(outputDir);

            recordingTime = 0f;
            IsRecording = true;
            IsPaused = false;
        }

        [ContextMenu("Pause Recording")]
        public void PauseRecording()
        {
            if (!IsRecording || IsPaused)
            {
                return;
            }

            universalVideoRecorder.PauseRecorder();
            IsPaused = true;
        }

        [ContextMenu("Resume Recording")]
        public void ResumeRecording()
        {
            if (!IsRecording || !IsPaused)
            {
                return;
            }

            universalVideoRecorder.ResumeRecorder();
            IsPaused = false;
        }

        [ContextMenu("Stop Recording")]
        public void StopRecording()
        {
            if (!IsRecording)
            {
                return;
            }

            universalVideoRecorder.StopRecorder();
            uiManager.SetToastSuccessMessage("Video saved in: " + outputDir);
            Debug.Log("Video saved in: " + outputDir);

            IsRecording = false;
            IsPaused = false;
        }

        public string GetRecordingTime()
        {
            return TimeSpan.FromSeconds(recordingTime).ToString(@"hh\:mm\:ss");
        }

    }

}
