using SilverTau.NSR.Recorders.Video;
using UnityEngine;

namespace Astrovisio
{
    public class RecorderManager : MonoBehaviour
    {
        public static RecorderManager Instance { get; private set; }

        [SerializeField] private UniversalVideoRecorder universalVideoRecorder;

        string videoFolderFilePath = Application.dataPath + "/Recordings";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of RecorderManager found. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        [ContextMenu("Start Recording")]
        public void StartRecording()
        {
            if (!System.IO.Directory.Exists(videoFolderFilePath))
            {
                System.IO.Directory.CreateDirectory(videoFolderFilePath);
            }

            universalVideoRecorder.StartRecorder(videoFolderFilePath);
        }

        [ContextMenu("Pause Recording")]
        public void PauseRecording()
        {
            universalVideoRecorder.PauseRecorder();
        }

        [ContextMenu("Stop Recording")]
        public void StopRecording()
        {
            universalVideoRecorder.StopRecorder();
            Debug.Log("Video recording saved in: " + videoFolderFilePath);
        }

    }

}
