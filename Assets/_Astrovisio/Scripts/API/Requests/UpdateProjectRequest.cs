using Newtonsoft.Json;

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

        [JsonProperty("order")]
        public int[] Order { get; set; }

    }
    
}
