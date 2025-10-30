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
