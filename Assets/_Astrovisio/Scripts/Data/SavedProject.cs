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

namespace Astrovisio
{
    [Serializable]
    public class SavedProject
    {
        private Project project;
        private List<Settings> filesSettings;

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

        [JsonProperty("filesSettings")]
        public List<Settings> FilesSettings
        {
            get => filesSettings;
            set
            {
                if (filesSettings != value)
                {
                    filesSettings = value;
                }
            }
        }
        
        public string[] GetFilePaths()
        {
            List<string> paths = new List<string>();
            foreach (File file in Project.Files)
            {
                paths.Add(file.Path);
            }

            return paths.ToArray();
        }

    }
}
