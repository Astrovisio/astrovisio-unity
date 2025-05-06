using Newtonsoft.Json;
using System.Collections.Generic;

namespace Astrovisio
{
    public class UpdateProjectRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("favourite")]
        public bool Favourite { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("paths")]
        public string[] Paths { get; set; }

        [JsonProperty("config_process")]
        public ConfigProcess ConfigProcess { get; set; }
    }
}
