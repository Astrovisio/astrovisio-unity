using Newtonsoft.Json;

namespace Astrovisio
{
    public class Project
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("favourite")]
        public bool Favourite { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("last_opened")]
        public string LastOpened { get; set; }

        [JsonProperty("paths")]
        public string[] Paths { get; set; }

        [JsonProperty("config_process")]
        public ConfigProcess ConfigProcess { get; set; }

    }
}
