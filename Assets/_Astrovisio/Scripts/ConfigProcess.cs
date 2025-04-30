using Newtonsoft.Json;
using System.Collections.Generic;

namespace Astrovisio
{
    public class ConfigProcess
    {
        [JsonProperty("downsampling")]
        public float Downsampling { get; set; }

        [JsonProperty("variables")]
        public Dictionary<string, ConfigParam> Params { get; set; }
        
    }
}
