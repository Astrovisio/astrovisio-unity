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

using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    public class Variable
    {
        private string name;
        private double thrMin;
        private double thrMax;
        private double? thrMinSel;
        private double? thrMaxSel;
        private bool selected;
        private string unit;
        private bool xAxis;
        private bool yAxis;
        private bool zAxis;

        [JsonProperty("var_name")]
        public string Name
        {
            get => name;
            set => name = value;
        }

        [JsonProperty("thr_min")]
        public double ThrMin
        {
            get => thrMin;
            set => thrMin = value;
        }

        [JsonProperty("thr_min_sel")]
        public double? ThrMinSel
        {
            get => thrMinSel;
            set => thrMinSel = value;
        }

        [JsonProperty("thr_max")]
        public double ThrMax
        {
            get => thrMax;
            set => thrMax = value;
        }

        [JsonProperty("thr_max_sel")]
        public double? ThrMaxSel
        {
            get => thrMaxSel;
            set => thrMaxSel = value;
        }

        [JsonProperty("selected")]
        public bool Selected
        {
            get => selected;
            set
            {
                if (selected != value)
                {
                    selected = value;
                }
            }
        }

        [JsonProperty("unit")]
        public string Unit
        {
            get => unit;
            set => unit = value;
        }

        [JsonProperty("x_axis")]
        public bool XAxis
        {
            get => xAxis;
            set
            {
                if (xAxis != value)
                {
                    xAxis = value;
                }
            }
        }

        [JsonProperty("y_axis")]
        public bool YAxis
        {
            get => yAxis;
            set
            {
                if (yAxis != value)
                {
                    yAxis = value;
                }
            }
        }

        [JsonProperty("z_axis")]
        public bool ZAxis
        {
            get => zAxis;
            set
            {
                if (zAxis != value)
                {
                    zAxis = value;
                }
            }
        }

        public void UpdateFrom(Variable other)
        {
            if (other == null)
            {
                return;
            }

            Name = other.name;
            ThrMin = other.ThrMin;
            ThrMinSel = other.ThrMinSel;
            ThrMax = other.ThrMax;
            ThrMaxSel = other.ThrMaxSel;
            Selected = other.Selected;
            Unit = other.Unit;
            XAxis = other.XAxis;
            YAxis = other.YAxis;
            ZAxis = other.ZAxis;
        }

        public Variable DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Variable>(json);
        }

    }

}
