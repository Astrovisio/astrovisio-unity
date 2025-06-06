using System;
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

    public enum ScalingType
    {
        Linear,
        Quadratic,
        Cubic,
        Exponential,
        Logaritmic
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

    public interface IMappingSettings { }

    public class OpacitySettings : IMappingSettings, ICloneable
    {
        public float Opacity { get; set; }
        public bool Invert { get; set; }

        public object Clone()
        {
            return new OpacitySettings
            {
                Opacity = this.Opacity,
                Invert = this.Invert
            };
        }
    }

    public class ColorMapSettings : IMappingSettings, ICloneable
    {
        public ColorMapEnum ColorMap { get; set; }
        public ScalingType ScalingType { get; set; }
        public float ThresholdMin { get; set; }
        public float ThresholdMax { get; set; }
        public float ThresholdMinSelected { get; set; }
        public float ThresholdMaxSelected { get; set; }
        public bool Invert { get; set; }

        public ColorMapSettings() { }

        public ColorMapSettings(
            ColorMapEnum colorMap,
            ScalingType scalingType,
            float thresholdMin,
            float thresholdMax,
            float thresholdMinSelected,
            float thresholdMaxSelected,
            bool invert
        ) =>
        (ColorMap, ScalingType, ThresholdMin, ThresholdMax, ThresholdMinSelected, ThresholdMaxSelected, Invert) =
        (colorMap, scalingType, thresholdMin, thresholdMax, thresholdMinSelected, thresholdMaxSelected, invert);

        public object Clone()
        {
            return new ColorMapSettings
            {
                ColorMap = this.ColorMap,
                ScalingType = this.ScalingType,
                ThresholdMin = this.ThresholdMin,
                ThresholdMax = this.ThresholdMax,
                ThresholdMinSelected = this.ThresholdMinSelected,
                ThresholdMaxSelected = this.ThresholdMaxSelected,
                Invert = this.Invert
            };
        }
    }

    public class SoundSettings : IMappingSettings, ICloneable
    {
        // Aggiungi i campi se servono
        public object Clone()
        {
            return new SoundSettings();
        }
    }

    public class HapticsSettings : IMappingSettings, ICloneable
    {
        // Aggiungi i campi se servono
        public object Clone()
        {
            return new HapticsSettings();
        }
    }

}
