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
        Cubic,
        Logaritmic
    }

    public class RenderSettings
    {
        public string Name { get; set; }
        public float ThresholdMin { get; set; }
        public float ThresholdMax { get; set; }
        public float ThresholdMinSelected { get; set; }
        public float ThresholdMaxSelected { get; set; }
        public MappingType Mapping { get; set; }
        public IMappingSettings MappingSettings { get; set; }

        public RenderSettings(
            string name,
            float thresholdMin,
            float thresholdMax,
            float thresholdMinSelected,
            float thresholdMaxSelected,
            MappingType mapping,
            IMappingSettings mappingSettings = null
        )
        {
            Name = name;
            ThresholdMinSelected = thresholdMinSelected;
            ThresholdMaxSelected = thresholdMaxSelected;
            ThresholdMin = thresholdMin;
            ThresholdMax = thresholdMax;
            Mapping = mapping;
            MappingSettings = mappingSettings;
        }
    }

    public interface IMappingSettings { }

    public class OpacitySettings : IMappingSettings
    {
        // public float OpacityMultiplier { get; set; }
        // public bool Invert { get; set; }
    }

    public class ColorMapSettings : IMappingSettings
    {
        public ColorMapEnum ColorMap { get; set; }
        public ScalingType ScalingType { get; set; }
        public float ThresholdMin { get; set; }
        public float ThresholdMax { get; set; }
        public bool Invert { get; set; }

        public ColorMapSettings(
            ColorMapEnum colorMap,
            ScalingType scalingType,
            float thresholdMin,
            float thresholdMax,
            bool invert
        ) =>
        (ColorMap, ScalingType, ThresholdMin, ThresholdMax, Invert) =
        (colorMap, scalingType, thresholdMin, thresholdMax, invert);
    }

    public class SoundSettings : IMappingSettings
    {
        // public float Volume { get; set; }
        // public AudioClip SoundEffect { get; set; }
    }

    public class HapticsSettings : IMappingSettings
    {
        // public float Intensity { get; set; }
        // public float Duration { get; set; }
    }

}
