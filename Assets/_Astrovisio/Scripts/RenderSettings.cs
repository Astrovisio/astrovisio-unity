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

    public class RenderSettings : ICloneable
    {
        public string Name { get; set; }
        public MappingType Mapping { get; set; }
        public IMappingSettings MappingSettings { get; set; }

        public RenderSettings(string name, MappingType mapping = MappingType.None, IMappingSettings mappingSettings = null)
        {
            Name = name;
            Mapping = mapping;
            MappingSettings = mappingSettings;
        }

        public object Clone()
        {
            return new RenderSettings(
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
