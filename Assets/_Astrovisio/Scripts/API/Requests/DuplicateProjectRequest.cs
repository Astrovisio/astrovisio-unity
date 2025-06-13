using Newtonsoft.Json;

namespace Astrovisio
{
    public class DuplicateProjectRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("paths")]
        public string[] Paths { get; set; }

        [JsonProperty("config_process")]
        public ConfigProcess ConfigProcess { get; set; }

    }
}
