using UnityEngine;

namespace Astrovisio
{
    public class RenderSettings
    {

        public string ParamName { get; set; }
        public string ColorMapName { get; set; }
        public Sprite ColorMap { get; set; }
        public float LowLimit { get; set; }
        public float HighLimit { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        public RenderSettings(
            string ParamName,
            string ColorMapName,
            float LowLimit,
            float HighLimit,
            float MinValue,
            float MaxValue
        )
        {

        }

    }

}
