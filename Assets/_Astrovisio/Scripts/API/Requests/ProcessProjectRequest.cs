using System.Collections.Generic;
using Newtonsoft.Json;

namespace Astrovisio
{
    public class ProcessProjectRequest
    {
        [JsonProperty("downsampling")]
        public float Downsampling { get; set; }

        [JsonProperty("variables")]
        public Dictionary<string, ConfigParam> ConfigParam { get; set; }
    }
}