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

using System;
using CatalogData;

namespace Astrovisio
{
    public class AxisRenderSettings : ICloneable
    {
        public string Name { get; set; }
        public Axis Axis { get; set; }
        public float ThresholdMin { get; set; }
        public float ThresholdMax { get; set; }
        public float ThresholdMinSelected { get; set; }
        public float ThresholdMaxSelected { get; set; }
        public ScalingType ScalingType { get; set; }

        public AxisRenderSettings(string name, Axis axis, float thresholdMin, float thresholdMax, float thresholdMinSelected, float thresholdMaxSelected, ScalingType scalingType)
        {
            Name = name;
            Axis = axis;
            ThresholdMin = thresholdMin;
            ThresholdMax = thresholdMax;
            ThresholdMinSelected = thresholdMinSelected;
            ThresholdMaxSelected = thresholdMaxSelected;
            ScalingType = scalingType;
        }

        public object Clone()
        {
            return new AxisRenderSettings(
                Name,
                Axis,
                ThresholdMin,
                ThresholdMax,
                ThresholdMinSelected,
                ThresholdMaxSelected,
                ScalingType
            );
        }

    }
    
}
