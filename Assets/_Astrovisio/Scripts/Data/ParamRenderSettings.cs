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
using CatalogData;
using UnityEngine;

namespace Astrovisio
{
    public enum MappingType
    {
        None,
        Opacity,
        Colormap,
        Sound,
        Haptics
    }

    public class ParamRenderSettings : ICloneable
    {
        public string Name { get; set; }
        public MappingType Mapping { get; set; }
        public IMappingSettings MappingSettings { get; set; }

        public ParamRenderSettings(string name, MappingType mapping = MappingType.None, IMappingSettings mappingSettings = null)
        {
            Name = name;
            Mapping = mapping;
            MappingSettings = mappingSettings;
        }

        public object Clone()
        {
            return new ParamRenderSettings(
                Name,
                Mapping,
                MappingSettings is ICloneable cloneable ? cloneable.Clone() as IMappingSettings : null
            );
        }

    }

    public interface IMappingSettings
    {
        public float ThresholdMin { get; set; }
        public float ThresholdMax { get; set; }
        public float ThresholdMinSelected { get; set; }
        public float ThresholdMaxSelected { get; set; }
        public ScalingType ScalingType { get; set; }
        public bool Invert { get; set; }
    }

    public class OpacitySettings : IMappingSettings, ICloneable
    {
        public float ThresholdMin { get; set; }
        public float ThresholdMax { get; set; }
        public float ThresholdMinSelected { get; set; }
        public float ThresholdMaxSelected { get; set; }
        public ScalingType ScalingType { get; set; }
        public bool Invert { get; set; }

        public OpacitySettings() { }

        public OpacitySettings(
            float thresholdMin,
            float thresholdMax,
            float thresholdMinSelected,
            float thresholdMaxSelected,
            ScalingType scalingType,
            bool invert
        ) =>
        (ThresholdMin, ThresholdMax, ThresholdMinSelected, ThresholdMaxSelected, ScalingType, Invert) =
        (thresholdMin, thresholdMax, thresholdMinSelected, thresholdMaxSelected, scalingType, invert);

        public object Clone()
        {
            return new OpacitySettings
            {
                ThresholdMin = this.ThresholdMin,
                ThresholdMax = this.ThresholdMax,
                ThresholdMinSelected = this.ThresholdMinSelected,
                ThresholdMaxSelected = this.ThresholdMaxSelected,
                ScalingType = this.ScalingType,
                Invert = this.Invert
            };
        }

    }

    public class ColorMapSettings : IMappingSettings, ICloneable
    {
        public ColorMapEnum ColorMap { get; set; }
        public float ThresholdMin { get; set; }
        public float ThresholdMax { get; set; }
        public float ThresholdMinSelected { get; set; }
        public float ThresholdMaxSelected { get; set; }
        public ScalingType ScalingType { get; set; }
        public bool Invert { get; set; }

        public ColorMapSettings() { }

        public ColorMapSettings(
            ColorMapEnum colorMap,
            float thresholdMin,
            float thresholdMax,
            float thresholdMinSelected,
            float thresholdMaxSelected,
            ScalingType scalingType,
            bool invert
        ) =>
        (ColorMap, ThresholdMin, ThresholdMax, ThresholdMinSelected, ThresholdMaxSelected, ScalingType, Invert) =
        (colorMap, thresholdMin, thresholdMax, thresholdMinSelected, thresholdMaxSelected, scalingType, invert);

        public object Clone()
        {
            return new ColorMapSettings
            {
                ColorMap = this.ColorMap,
                ThresholdMin = this.ThresholdMin,
                ThresholdMax = this.ThresholdMax,
                ThresholdMinSelected = this.ThresholdMinSelected,
                ThresholdMaxSelected = this.ThresholdMaxSelected,
                ScalingType = this.ScalingType,
                Invert = this.Invert
            };
        }

    }

}
