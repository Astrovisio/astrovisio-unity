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
