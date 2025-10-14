using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    [Serializable]
    public class ScreenshotMetadata
    {
        private string projectName;
        private File file;
        private Settings fileSettings;
        private ObjectTransform cameraSettings;
        private ObjectTransform cubeSettings;

        [JsonProperty("projectName")]
        public string ProjectName
        {
            get => projectName;
            set
            {
                if (projectName != value)
                {
                    projectName = value;
                }
            }
        }

        [JsonProperty("file")]
        public File File
        {
            get => file;
            set
            {
                if (file != value)
                {
                    file = value;
                }
            }
        }

        [JsonProperty("fileSettings")]
        public Settings FileSettings
        {
            get => fileSettings;
            set
            {
                if (fileSettings != value)
                {
                    fileSettings = value;
                }
            }
        }

        [JsonProperty("cameraSettings")]
        public ObjectTransform CameraSettings
        {
            get => cameraSettings;
            set
            {
                if (cameraSettings != value)
                {
                    cameraSettings = value;
                }
            }
        }

        [JsonProperty("cubeSettings")]
        public ObjectTransform CubeSettings
        {
            get => cubeSettings;
            set
            {
                if (cubeSettings != value)
                {
                    cubeSettings = value;
                }
            }
        }

        public ScreenshotMetadata(string projectName, File file, GameObject camera, GameObject dataCube, Settings settings = null)
        {
            ProjectName = projectName;
            File = file;
            CameraSettings = new ObjectTransform(camera);
            CubeSettings = new ObjectTransform(dataCube);
            if (settings != null)
                FileSettings = settings;
        }

    }
}
