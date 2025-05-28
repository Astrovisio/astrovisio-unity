using UnityEngine;

namespace Astrovisio
{
    public class RenderSettings
    {

        public string ParamName { get; set; }
        public string ColorMapName { get; set; }
        public ColorMapEnum ColorMap { get; set; }
        public float LowLimit { get; set; }
        public float HighLimit { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        public RenderSettings(
            string paramName,
            string colorMapName,
            ColorMapEnum colorMap,
            float lowLimit,
            float highLimit,
            float minValue,
            float maxValue
        )
        {
            ParamName = paramName;
            ColorMapName = colorMapName;
            ColorMap = colorMap;
            LowLimit = lowLimit;
            HighLimit = highLimit;
            MinValue = minValue;
            MaxValue = maxValue;
        }

    }

}
