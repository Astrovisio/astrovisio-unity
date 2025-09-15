using Newtonsoft.Json;

namespace Astrovisio
{
    public class DuplicateProjectRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("files")]
        public string[] Paths { get; set; }

        [JsonProperty("config_process")]
        public File ConfigProcess { get; set; }

    }
}
