using Newtonsoft.Json;

namespace Astrovisio
{
    public class DuplicateProjectRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

    }
}
