using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    [Serializable]
    public class ScreenshotMetadata
    {
        private Project project;
        private Settings fileSettings;
        private ObjectTransform cameraSettings;
        private ObjectTransform cubeSettings;

        [JsonProperty("project")]
        public Project Project
        {
            get => project;
            set
            {
                if (project != value)
                {
                    project = value;
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

        public ScreenshotMetadata(Project project, GameObject camera, GameObject dataCube, Settings settings = null)
        {
            Project = project;
            CameraSettings = new ObjectTransform(camera);
            CubeSettings = new ObjectTransform(dataCube);
            if (settings != null)
                FileSettings = settings;
        }

    }
}
