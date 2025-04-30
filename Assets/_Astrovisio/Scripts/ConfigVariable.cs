using Newtonsoft.Json;

namespace Astrovisio
{
    public class ConfigParam
    {
        [JsonProperty("thr_min")]
        public int ThrMin { get; set; }

        [JsonProperty("thr_min_sel")]
        public int ThrMinSel { get; set; }

        [JsonProperty("thr_max")]
        public int ThrMax { get; set; }

        [JsonProperty("thr_max_sel")]
        public int ThrMaxSel { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("x_axis")]
        public bool XAxis { get; set; }

        [JsonProperty("y_axis")]
        public bool YAxis { get; set; }

        [JsonProperty("z_axis")]
        public bool ZAxis { get; set; }

        [JsonProperty("files")]
        public string[] Files { get; set; }
    }
}
