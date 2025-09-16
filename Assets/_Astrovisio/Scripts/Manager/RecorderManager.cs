using SilverTau.NSR.Recorders.Video;
using UnityEngine;
using System;
using System.IO;

namespace Astrovisio
{
    public class RecorderManager : MonoBehaviour
    {
        public static RecorderManager Instance { get; private set; }

        [SerializeField] private UniversalVideoRecorder universalVideoRecorder;

        private string outputDir = "";
        private string currentFilePath = "";
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

            // cartella predefinita
            outputDir = Path.Combine(Application.dataPath, "Recordings");
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

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            currentFilePath = Path.Combine(outputDir, fileName);

            universalVideoRecorder.StartRecorder(currentFilePath);

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
            Debug.Log("Video saved in: " + outputDir);

            IsRecording = false;
            IsPaused = false;
            currentFilePath = "";
        }

        public string GetRecordingTime()
        {
            return TimeSpan.FromSeconds(recordingTime).ToString(@"hh\:mm\:ss");
        }

    }

}
