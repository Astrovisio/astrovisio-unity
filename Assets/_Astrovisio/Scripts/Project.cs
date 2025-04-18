using System;
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
        public DateTime? Created { get; set; }

        [JsonProperty("last_opened")]
        public DateTime? LastOpened { get; set; }

        [JsonProperty("paths")]
        public string[] Paths { get; set; }

        [JsonProperty("config_process")]
        public ConfigProcess ConfigProcess { get; set; }

        public Project(string name, string description, bool favourite = false, string[] paths = null)
        {
            Name = name;
            Description = description;
            Favourite = favourite;
            Paths = paths ?? new string[0];
        }

    }
}
