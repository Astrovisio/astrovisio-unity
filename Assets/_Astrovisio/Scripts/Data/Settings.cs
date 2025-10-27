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

using System.Collections.Generic;
using CatalogData;
using Newtonsoft.Json;

namespace Astrovisio
{
    public class Settings
    {

        private List<Setting> variables;
        private string path;
        private float noise;

        [JsonProperty("variables")]
        public List<Setting> Variables
        {
            get => variables;
            set => variables = value;
        }

        [JsonProperty("path")]
        public string Path
        {
            get => path;
            set => path = value;
        }

        [JsonProperty("noise")]
        public float Noise
        {
            get =>  noise;
            set => noise = value;
        }

        public void SetDefaults(File file)
        {
            Noise = 0f;

            variables.Clear();

            foreach (Variable variable in file.Variables)
            {
                if (!variable.Selected)
                {
                    continue;
                }

                Setting setting = new Setting
                {
                    Name = variable.Name,
                    Mapping = null,
                    ThrMin = variable.ThrMin,
                    ThrMax = variable.ThrMax,
                    ThrMinSel = variable.ThrMin,
                    ThrMaxSel = variable.ThrMax,
                    Scaling = ScalingType.Linear.ToString(),
                    Colormap = ColorMapEnum.Autumn.ToString(),
                    InvertMapping = false,
                };

                variables.Add(setting);
            }
        }

    }

}
